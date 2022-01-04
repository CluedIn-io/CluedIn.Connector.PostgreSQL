using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core.Connectors;
using CluedIn.Core.Streams.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public interface IBuildCreateContainerFeature
    {
        IEnumerable<PostgreSqlConnectorCommand> BuildCreateContainerSql(
            string tableName,
            IEnumerable<ConnectionDataType> columns,
            IEnumerable<string> keys,
            string context,
            StreamMode streamMode,
            ILogger logger);
    }
}
