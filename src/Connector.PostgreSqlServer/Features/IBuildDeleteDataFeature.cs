using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core;
using CluedIn.Core.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public interface IBuildDeleteDataFeature
    {
        IEnumerable<PostgreSqlConnectorCommand> BuildDeleteDataSql(ExecutionContext executionContext,
            Guid providerDefinitionId,
            string containerName,
            string originEntityCode,
            IList<IEntityCode> codes,
            Guid? entityId,
            ILogger logger);
    }
}
