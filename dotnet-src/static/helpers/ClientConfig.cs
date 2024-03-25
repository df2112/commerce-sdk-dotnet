namespace Salesforce.CommerceCloud.Foundation
{

    /// <summary>
    /// Defines all the parameters that can be reused by the client.
    /// Headers can be overwritten when actual calls are made.
    /// 
    /// The following types are ignore in the dotnet version of SFCC:
    /// - ICacheManager
    /// - OperationOptions
    /// - RequestInit
    /// </summary>
    public class ClientConfig
    {
        public string? BaseUri { get; set; }
        // public ICacheManager? CacheManager { get; set; }
        public BasicHeaders? Headers { get; set; }
        public CommonParameters? Parameters { get; set; }
        // public OperationOptions? RetrySettings { get; set; }
        // public RequestInit? FetchOptions { get; set; }
    }
}
