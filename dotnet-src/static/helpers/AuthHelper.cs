
namespace Salesforce.CommerceCloud.Foundation
{

    // TODO DAVE: Will probably need this https://github.com/AzureAD/microsoft-identity-web
    // instead of the following using statment:
    // using Microsoft.IdentityModel.Tokens.JWT;

    /// <summary>
    /// A public interface for auth tokens.
    /// </summary>
    public interface IAuthToken
    {
        string GetAuthToken();
    }

    public static class AuthHelper
    {
        /// <summary>
        /// Strip "Bearer " from the passed header.
        /// </summary>
        /// <param name="header">A Bearer token</param>
        /// <returns>The token after stripping "Bearer "</returns>
        public static string StripBearer(string header)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Implements ShopperJWT auth scheme. Gets ShopperJWT Bearer tokens of type
    /// `guest` and `credentials`.
    /// </summary>
    public class ShopperToken<T> : IAuthToken
    {
        public string RawToken { get; init; }

        // TODO DAVE: What is the correct .net type for NPM JwtPayload
        // public JwtPayload? DecodedToken { get; init; }

        // TODO DAVE: This is a temp hack due to issue immediately above
        public string? DecodedToken { get; init; }
        public T CustomerInfo { get; init; }

        public ShopperToken(T dto, string token)
        {
            CustomerInfo = dto;
            RawToken = token;
            // Decoding token logic goes here
        }

        /// <summary>
        /// Returns the JWT.
        /// </summary>
        /// <returns>JWT</returns>
        public string GetAuthToken()
        {
            return RawToken;
        }

        /// <summary>
        /// Returns a Bearer token i.e. `Bearer <JWT>`.
        /// </summary>
        /// <returns>The JWT with "Bearer " added to the front</returns>
        public string GetBearerHeader()
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the customer information.
        /// </summary>
        /// <returns>Customer information this object is instantiated with</returns>
        public T GetCustomerInfo()
        {
            return CustomerInfo;
        }
    }
}
