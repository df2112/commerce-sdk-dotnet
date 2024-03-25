namespace Salesforce.CommerceCloud.Foundation
{
    /// <summary>
    /// Common parameters are the set of values that are often shared across SDK
    /// client configurations.
    /// </summary>
    public record CommonParameterPositions
    {
        public List<string> BaseUriParameters { get; init; } = new() { "shortCode", "version" };
        public List<string> PathParameters { get; init; } = new() { "organizationId" };
        public List<string> QueryParameters { get; init; } = new() { "clientId", "siteId" };
    };
}
