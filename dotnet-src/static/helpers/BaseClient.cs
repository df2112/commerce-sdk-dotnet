namespace Salesforce.CommerceCloud.Foundation
{

    /// <summary>
    /// A basic implementation of a client that all the Commerce API clients extend.
    /// </summary>
    public class BaseClient
    {
        public ClientConfig ClientConfig { get; set; }

        public BaseClient(ClientConfig? config = null)
        {
            ClientConfig = config ?? new ClientConfig();
        }
    }
}
