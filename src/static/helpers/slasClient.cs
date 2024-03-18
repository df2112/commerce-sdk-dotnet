namespace SFCC.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// public class OperationOptions { /* ... */ }
public class Response { /* ... */ }
// public class RequestInit { /* ... */ }

public class LoginRequest
{
    public required string client_id { get; set; }
    public required string response_type { get; set; }
    public required string redirect_uri { get; set; }
    public required string state { get; set; }
    public required string scope { get; set; }
    public required string usid { get; set; }
    public required string channel_id { get; set; }
    public required string code_challenge { get; set; }
    public required Dictionary<string, object> AdditionalData { get; set; }
}

public class TokenRequest
{
    public required string refresh_token { get; set; }
    public required string code { get; set; }
    public required string usid { get; set; }
    public required string grant_type { get; set; }
    public required string redirect_uri { get; set; }
    public required string code_verifier { get; set; }
    public required string client_id { get; set; }
    public required string channel_id { get; set; }
    public required Dictionary<string, object> AdditionalData { get; set; }
}

public class TokenResponse
{
    public required string access_token { get; set; }
    public required string id_token { get; set; }
    public required string refresh_token { get; set; }
    public int expires_in { get; set; }
    public required string token_type { get; set; }
    public required string usid { get; set; }
    public required string customer_id { get; set; }
    public required string enc_user_id { get; set; }
    public required string idp_access_token { get; set; }
    public required Dictionary<string, object> AdditionalData { get; set; }
}

public interface ISlasClient
{
    Task<Response> AuthenticateCustomer(
        Dictionary<string, object> parameters,
        // OperationOptions retrySettings,
        Dictionary<string, string> headers,
        // RequestInit fetchOptions,
        LoginRequest body,
        bool rawResponse
    );

    Task<Response> AuthorizeCustomer(
        Dictionary<string, object> parameters,
        // OperationOptions retrySettings,
        Dictionary<string, string> headers,
        // RequestInit fetchOptions,
        bool rawResponse
    );

    Task<object> GetAccessToken(
        Dictionary<string, object> parameters,
        // OperationOptions retrySettings,
        Dictionary<string, string> headers,
        // RequestInit fetchOptions,
        TokenRequest body,
        bool rawResponse
    );

    Task<object> LogoutCustomer(
        Dictionary<string, object> parameters,
        // OperationOptions retrySettings,
        Dictionary<string, string> headers,
        // RequestInit fetchOptions,
        bool rawResponse
    );

    ClientConfig ClientConfig { get; set; }
}