using System.Security.Cryptography;
using System.Text;

namespace SFCC.Core;

public class Slas
{
    /**
    * Converts a string into Base64 encoding
    *
    * @param unencoded - A string to be encoded
    * @returns Base64 encoded string
    */
    public static string StringToBase64(string unencoded)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(unencoded);
        return Convert.ToBase64String(plainTextBytes);
    }

    /**
     * Parse out the code and usid from a redirect url
     *
     * @param urlString - A url that contains `code` and `usid` query parameters, typically returned when calling a Shopper Login endpoint
     * @returns An object containing the code and usid.
     */
    public static (string code, string usid) GetCodeAndUsidFromUrl(string urlString)
    {
        var uri = new Uri(urlString);
        var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var usid = queryParameters["usid"] ?? "";
        var code = queryParameters["code"] ?? "";

        return (code, usid);
    }

    /**
 * Creates a random string to use as a code verifier. This code is created by the client and sent with both the authorization request (as a code challenge) and the token request.
 *
 * @returns code verifier
 */
    public static string CreateCodeVerifier()
    {
        using (var rng = new RSACryptoServiceProvider())
        {
            var bytes = new byte[128 / 8]; // 128 bits
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    /**
     * Encodes a code verifier to a code challenge to send to the authorization endpoint
     *
     * @param codeVerifier - random string to use as a code verifier
     * @returns code challenge
     */
    public static string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var challenge = Convert.ToBase64String(challengeBytes);
            challenge = challenge.Replace('+', '-').Replace('/', '_').Replace("=", "");

            if (string.IsNullOrEmpty(challenge))
            {
                throw new Exception("Problem generating code challenge");
            }

            return challenge;
        }
    }



    /**
     * Wrapper for the authorization endpoint. For federated login (3rd party IDP non-guest), the caller should redirect the user to the url in the url field of the returned object. The url will be the login page for the 3rd party IDP and the user will be sent to the redirectURI on success. Guest sessions return the code and usid directly with no need to redirect.
     *
     * @param slasClient - a configured instance of the ShopperLogin SDK client
     * @param codeVerifier - random string created by client app to use as a secret in the request
     * @param parameters - Request parameters used by the `authorizeCustomer` endpoint.
     * @param parameters.redirectURI - the location the client will be returned to after successful login with 3rd party IDP. Must be registered in SLAS.
     * @param parameters.hint - optional string to hint at a particular IDP. Guest sessions are created by setting this to 'guest'
     * @param parameters.usid - optional saved SLAS user id to link the new session to a previous session
     * @returns login url, user id and authorization code if available
     */
    public async Task<(string code, string url, string usid)> Authorize(
        ISlasClient slasClient,
        string codeVerifier,
        (string redirectURI, string hint, string usid) parameters)
    {
        var codeChallenge = await GenerateCodeChallenge(codeVerifier);

        var options = new
        {
            parameters = new Dictionary<string, string>
        {
            { "client_id", slasClient.ClientConfig.Parameters.ClientId },
            { "code_challenge", codeChallenge },
            { "organizationId", slasClient.ClientConfig.Parameters.OrganizationId },
            { "redirect_uri", parameters.redirectURI },
            { "response_type", "code" },
        },
            fetchOptions = new { redirect = "manual" }
        };

        if (!string.IsNullOrEmpty(parameters.hint))
        {
            options.parameters.Add("hint", parameters.hint);
        }

        if (!string.IsNullOrEmpty(parameters.usid))
        {
            options.parameters.Add("usid", parameters.usid);
        }

        var response = await slasClient.AuthorizeCustomer(options, true);

        if (response.Status != HttpStatusCode.SeeOther)
        {
            throw new ResponseError(response);
        }

        var redirectUrl = response.Headers.Location?.ToString() ?? response.Url;

        return (GetCodeAndUsidFromUrl(redirectUrl), redirectUrl);
    }


    /**
     * A single function to execute the ShopperLogin Private Client Guest Login as described in the [API documentation](https://developer.salesforce.com/docs/commerce/commerce-api/guide/slas-private-client.html).
     *
     * @param slasClient - a configured instance of the ShopperLogin SDK client
     * @param credentials - client secret used for authentication
     * @param credentials.clientSecret - secret associated with client ID
     * @param usid - optional Unique Shopper Identifier to enable personalization
     * @returns TokenResponse
     */
    public async Task<TokenResponse> LoginGuestUserPrivate(
        ISlasClient slasClient,
        (string clientSecret) credentials,
        string usid = null)
    {
        var authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{slasClient.ClientConfig.Parameters.ClientId}:{credentials.clientSecret}"))}";

        var options = new
        {
            headers = new Dictionary<string, string> { { "Authorization", authorization } },
            body = new Dictionary<string, string> { { "grant_type", "client_credentials" } }
        };

        if (!string.IsNullOrEmpty(usid))
        {
            options.body.Add("usid", usid);
        }

        return await slasClient.GetAccessToken(options);
    }

    /**
     * A single function to execute the ShopperLogin Public Client Guest Login with proof key for code exchange flow as described in the [API documentation](https://developer.salesforce.com/docs/commerce/commerce-api/guide/slas-public-client.html).
     *
     * @param slasClient a configured instance of the ShopperLogin SDK client.
     * @param parameters - parameters to pass in the API calls.
     * @param parameters.redirectURI - Per OAuth standard, a valid app route. Must be listed in your SLAS configuration. On server, this will not be actually called
     * @param parameters.usid - Unique Shopper Identifier to enable personalization.
     * @returns TokenResponse
     */
    public async Task<TokenResponse> LoginGuestUser(
        ISlasClient slasClient,
        (string redirectURI, string usid) parameters)
    {
        var codeVerifier = CreateCodeVerifier();

        var authResponse = await Authorize(slasClient, codeVerifier, new { redirectURI = parameters.redirectURI, hint = "guest", usid = parameters.usid });

        var tokenBody = new TokenRequest
        {
            client_id = slasClient.ClientConfig.Parameters.ClientId,
            code = authResponse.code,
            code_verifier = codeVerifier,
            grant_type = "authorization_code_pkce",
            redirect_uri = parameters.redirectURI,
            usid = authResponse.usid,
        };

        return await slasClient.GetAccessToken(new { body = tokenBody });
    }


    /**
     * A single function to execute the ShopperLogin Private Client Registered User B2C Login as described in the [API documentation](https://developer.salesforce.com/docs/commerce/commerce-api/guide/slas-private-client.html).
     *
     * @param slasClient - a configured instance of the ShopperLogin SDK client.
     * @param credentials - the shopper username and password for login and client secret for additional authentication
     * @param credentials.username - the id of the user to login with
     * @param credentials.password - the password of the user to login with
     * @param credentials.clientSecret - secret associated with client ID
     * @param parameters - parameters to pass in the API calls.
     * @param parameters.redirectURI - Per OAuth standard, a valid app route. Must be listed in your SLAS configuration. On server, this will not be actually called
     * @param parameters.usid - optional Unique Shopper Identifier to enable personalization
     * @returns TokenResponse
     */
    public async Task<TokenResponse> LoginRegisteredUserB2CPrivate(
        ISlasClient slasClient,
        (string username, string password, string clientSecret) credentials,
        (string redirectURI, string usid) parameters)
    {
        var codeVerifier = CreateCodeVerifier();
        var codeChallenge = await GenerateCodeChallenge(codeVerifier);

        var authHeaderUserPass = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.username}:{credentials.password}"))}";

        var optionsLogin = new
        {
            headers = new Dictionary<string, string> { { "Authorization", authHeaderUserPass } },
            body = new Dictionary<string, string>
        {
            { "code_challenge", codeChallenge },
            { "channel_id", slasClient.ClientConfig.Parameters.SiteId },
            { "client_id", slasClient.ClientConfig.Parameters.ClientId },
            { "redirect_uri", parameters.redirectURI }
        },
            fetchOptions = new { redirect = "manual" }
        };

        if (!string.IsNullOrEmpty(parameters.usid))
        {
            optionsLogin.body.Add("usid", parameters.usid);
        }

        var response = await slasClient.AuthenticateCustomer(optionsLogin, true);

        if (response.Status != HttpStatusCode.SeeOther)
        {
            throw new ResponseError(response);
        }

        var redirectUrl = response.Headers.Location?.ToString() ?? response.Url;
        var authResponse = GetCodeAndUsidFromUrl(redirectUrl);

        var authHeaderIdSecret = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{slasClient.ClientConfig.Parameters.ClientId}:{credentials.clientSecret}"))}";

        var optionsToken = new
        {
            headers = new Dictionary<string, string> { { "Authorization", authHeaderIdSecret } },
            body = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code_pkce" },
            { "code_verifier", codeVerifier },
            { "code", authResponse.code },
            { "client_id", slasClient.ClientConfig.Parameters.ClientId },
            { "redirect_uri", parameters.redirectURI },
            { "usid", authResponse.usid }
        }
        };

        return await slasClient.GetAccessToken(optionsToken);
    }



    /**
     * A single function to execute the ShopperLogin Private Client Registered User B2C Login with proof key for code exchange flow as described in the [API documentation](https://developer.salesforce.com/docs/commerce/commerce-api/guide/slas-public-client.html).
     *
     * @param slasClient - a configured instance of the ShopperLogin SDK client.
     * @param credentials - the id and password to login with.
     * @param credentials.username - the id of the user to login with.
     * @param credentials.password - the password of the user to login with.
     * @param parameters - parameters to pass in the API calls.
     * @param parameters.redirectURI - Per OAuth standard, a valid app route. Must be listed in your SLAS configuration. On server, this will not be actually called. On browser, this will be called, but ignored.
     * @param parameters.usid - Unique Shopper Identifier to enable personalization.
     * @returns TokenResponse
     */
    public async Task<TokenResponse> LoginRegisteredUserB2C(
        ISlasClient slasClient,
        (string username, string password) credentials,
        (string redirectURI, string usid) parameters)
    {
        var codeVerifier = CreateCodeVerifier();
        var codeChallenge = await GenerateCodeChallenge(codeVerifier);

        var authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.username}:{credentials.password}"))}";

        var options = new
        {
            headers = new Dictionary<string, string> { { "Authorization", authorization } },
            parameters = new Dictionary<string, string> { { "organizationId", slasClient.ClientConfig.Parameters.OrganizationId } },
            body = new Dictionary<string, string>
        {
            { "redirect_uri", parameters.redirectURI },
            { "client_id", slasClient.ClientConfig.Parameters.ClientId },
            { "code_challenge", codeChallenge },
            { "channel_id", slasClient.ClientConfig.Parameters.SiteId }
        },
            fetchOptions = new { redirect = "manual" }
        };

        if (!string.IsNullOrEmpty(parameters.usid))
        {
            options.body.Add("usid", parameters.usid);
        }

        var response = await slasClient.AuthenticateCustomer(options, true);

        if (response.Status != HttpStatusCode.SeeOther)
        {
            throw new ResponseError(response);
        }

        var redirectUrl = response.Headers.Location?.ToString() ?? response.Url;
        var authResponse = GetCodeAndUsidFromUrl(redirectUrl);

        var tokenBody = new
        {
            grant_type = "authorization_code_pkce",
            code_verifier = codeVerifier,
            code = authResponse.code,
            client_id = slasClient.ClientConfig.Parameters.ClientId,
            redirect_uri = parameters.redirectURI,
            usid = authResponse.usid
        };

        return await slasClient.GetAccessToken(new { body = tokenBody });
    }


    /**
     * Exchange a refresh token for a new access token.
     *
     * @param slasClient - a configured instance of the ShopperLogin SDK client.
     * @param parameters - parameters to pass in the API calls.
     * @param parameters.refreshToken - a valid refresh token to exchange for a new access token (and refresh token).
     * @returns TokenResponse
     */
    public async Task<TokenResponse> RefreshAccessToken(
        ISlasClient slasClient,
        string refreshToken)
    {
        var body = new Dictionary<string, string>
    {
        { "grant_type", "refresh_token" },
        { "refresh_token", refreshToken },
        { "client_id", slasClient.ClientConfig.Parameters.ClientId }
    };

        return await slasClient.GetAccessToken(new { body });
    }

    /**
     * Exchange a refresh token for a new access token.
     *
     * @param slasClient - a configured instance of the ShopperLogin SDK client.
     * @param credentials - client secret used for authentication
     * @param credentials.clientSecret - secret associated with client ID
     * @param parameters - parameters to pass in the API calls.
     * @param parameters.refreshToken - a valid refresh token to exchange for a new access token (and refresh token).
     * @returns TokenResponse
     */
    public async Task<TokenResponse> RefreshAccessTokenPrivate(
        ISlasClient slasClient,
        string clientSecret,
        string refreshToken)
    {
        var authorization = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{slasClient.ClientConfig.Parameters.ClientId}:{clientSecret}"))}";

        var options = new
        {
            headers = new Dictionary<string, string> { { "Authorization", authorization } },
            body = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        }
        };

        return await slasClient.GetAccessToken(options);
    }



    /**
     * Logout a shopper. The shoppers access token and refresh token will be revoked and if the shopper authenticated with ECOM the OCAPI JWT will also be revoked.
     *
     * @param slasClient - a configured instance of the ShopperLogin SDK client.
     * @param parameters - parameters to pass in the API calls.
     * @param parameters.accessToken - a valid access token to exchange for a new access token (and refresh token).
     * @param parameters.refreshToken - a valid refresh token to exchange for a new access token (and refresh token).
     * @returns TokenResponse
     */
    public async Task<TokenResponse> Logout(
        ISlasClient slasClient,
        (string accessToken, string refreshToken) parameters)
    {
        var options = new
        {
            headers = new Dictionary<string, string> { { "Authorization", $"Bearer {parameters.accessToken}" } },
            parameters = new Dictionary<string, string>
        {
            { "refresh_token", parameters.refreshToken },
            { "client_id", slasClient.ClientConfig.Parameters.ClientId },
            { "channel_id", slasClient.ClientConfig.Parameters.SiteId }
        }
        };

        return await slasClient.LogoutCustomer(options);
    }


}
