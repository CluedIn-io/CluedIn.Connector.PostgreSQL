using CluedIn.Connector.PostgreSqlServer.Connector;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public interface IBuildCreateIndexFeature
    {
        IEnumerable<PostgreSqlConnectorCommand> BuildCreateIndexSql(
            string tableName,
            IEnumerable<string> keys,
            ILogger logger,
            string indexName = null);
    }
}
