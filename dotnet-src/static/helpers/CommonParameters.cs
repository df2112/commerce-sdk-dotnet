namespace Salesforce.CommerceCloud.Foundation
{
    /// <summary>
    /// Common parameters are the set of values that are often shared across SDK
    /// client configurations.
    /// </summary>
    public record CommonParameters
    {
        public string? ClientId { get; init; }
        public string? OrganizationId { get; init; }
        public string? ShortCode { get; init; }
        public string? SiteId { get; init; }
        public string? Version { get; init; }
    }
}
