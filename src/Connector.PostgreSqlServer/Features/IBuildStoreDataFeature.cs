using CluedIn.Connector.PostgreSqlServer.Connector;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public interface IBuildStoreDataFeature
    {
        IEnumerable<PostgreSqlConnectorCommand> BuildStoreDataSql(
            string containerName,
            IDictionary<string, object> data,
            IList<string> keys,
            ILogger logger);
    }
}
