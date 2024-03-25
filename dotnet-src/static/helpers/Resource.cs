namespace Salesforce.CommerceCloud.Foundation
{
    // public record BasicHeaders(Dictionary<string, string> XBasicHeaders);
    public record BaseUriParameters(Dictionary<string, string> XBaseUriParameters);
    // public record PathParameters(Dictionary<string, string> XPathParameters);
    // public record QueryParameters(Dictionary<string, object> XQueryParameters);

    /// <summary>
    /// A class to render a flattened URL from the parts including template
    /// parameters. Out of the various options to render an array in a query string,
    /// this class comma separates the value for each element of the array,
    /// i.e. {a: [1, 2]} => "?a=1,2". One exception is the 'refine' query param as
    /// SCAPI expects the repeated format, i.e. {refine: [1, 2]} => "?refine=1&refine=2"
    /// </summary>
    public class Resource
    {
        private string BaseUri { get; set; }
        private BaseUriParameters? BaseUriParameters { get; set; }
        private string? Path { get; set; }
        private PathParameters? PathParameters { get; set; }
        private QueryParameters? QueryParameters { get; set; }

        public Resource(string baseUri, BaseUriParameters? baseUriParameters = null, string? path = null, PathParameters? pathParameters = null, QueryParameters? queryParameters = null)
        {
            BaseUri = baseUri;
            BaseUriParameters = baseUriParameters;
            Path = path;
            PathParameters = pathParameters;
            QueryParameters = queryParameters;
        }

        /// <summary>
        /// Substitutes template parameters in the path with matching parameters.
        /// </summary>
        /// <param name="path">String containing template parameters</param>
        /// <param name="parameters">All the parameters that should substitute the template parameters</param>
        /// <returns>Path with actual parameters</returns>
        public string SubstitutePathParameters(string? path = null, PathParameters? parameters = null)
        {
            // Implementation goes here
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a url from a baseUri, path and query parameters.
        /// </summary>
        /// <returns>Rendered URL</returns>
        public override string ToString()
        {
            // Implementation goes here
            throw new NotImplementedException();
        }
    }
}
