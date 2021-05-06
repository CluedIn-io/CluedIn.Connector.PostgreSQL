using CluedIn.Core.Connectors;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public class PostgreSqlServerConnectorDataType : IConnectionDataType {
        public string Name { get; set; }
        public VocabularyKeyDataType Type { get; set; }
        public string RawDataType { get; set; }
    }
}
