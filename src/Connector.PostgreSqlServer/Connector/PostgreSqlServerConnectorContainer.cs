using CluedIn.Core.Connectors;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public class PostgreSqlServerConnectorContainer : IConnectorContainer
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string FullyQualifiedName { get; set; }
    }
}
