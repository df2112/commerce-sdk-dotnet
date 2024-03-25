namespace Salesforce.CommerceCloud.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public record BasicHeaders(Dictionary<string, string> Headers);
    public record PathParameters(Dictionary<string, string> Parameters);
    public record QueryParameters(Dictionary<string, object> Parameters);

    // public class BaseClient { }

    public record SdkFetchOptions
    {
        public required BaseClient Client { get; init; }
        public required string Path { get; init; }
        public PathParameters? PathParameters { get; init; }
        public QueryParameters? QueryParameters { get; init; }
        public BasicHeaders? Headers { get; init; }
        public bool? RawResponse { get; init; }
        // public OperationOptions? RetrySettings { get; init; }
        public HttpRequestMessage? FetchOptions { get; init; }
        public object? Body { get; init; }
    };

    public record SdkFetchOptionsNoBody : SdkFetchOptions
    {
        public SdkFetchOptionsNoBody(SdkFetchOptions options)
        {
            Client = options.Client;
            Path = options.Path;
            PathParameters = options.PathParameters;
            QueryParameters = options.QueryParameters;
            Headers = options.Headers;
            RawResponse = options.RawResponse;
            // RetrySettings = options.RetrySettings;
            FetchOptions = options.FetchOptions;
        }
    };

    public record SdkFetchOptionsWithBody : SdkFetchOptionsNoBody
    {
        // public object Body { get; init; }

        public SdkFetchOptionsWithBody(SdkFetchOptions options) : base(options)
        {
            Body = options.Body ?? throw new ArgumentNullException(nameof(options.Body));
        }
    };

    public class ResponseError : Exception
    {
        public HttpResponseMessage Response { get; }

        public ResponseError(HttpResponseMessage response) : base(response.ReasonPhrase)
        {
            Response = response;
        }
    }

    public static class FetchHelper
    {
        public static async Task<object> GetObjectFromResponse(HttpResponseMessage response)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static void LogFetch(string resource, HttpRequestMessage fetchOptions)
        {
            // Implementation goes here
        }

        public static void LogResponse(HttpResponseMessage response)
        {
            // Implementation goes here
        }

        public static BasicHeaders GetHeaders(BasicHeaders? options = null)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static BasicHeaders MergeHeaders(params BasicHeaders[] allHeaders)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static HttpContent TransformRequestBody(object body, HttpRequestMessage request)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static async Task<object> Get(SdkFetchOptionsNoBody options)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static async Task<object> Delete(SdkFetchOptionsNoBody options)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static async Task<object> Patch(SdkFetchOptionsWithBody options)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static async Task<object> Post(SdkFetchOptionsWithBody options)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        public static async Task<object> Put(SdkFetchOptionsWithBody options)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }
    }
}
