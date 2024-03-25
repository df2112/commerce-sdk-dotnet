namespace Salesforce.CommerceCloud.Foundation
{
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
}
