using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core.Streams.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public interface IBuildStoreDataForMode
    {
        IEnumerable<PostgreSqlConnectorCommand> BuildStoreDataSql(
            string containerName,
            IDictionary<string, object> data,
            IList<string> keys,
            StreamMode mode,
            string correlationId,
            ILogger logger);
    }
}
