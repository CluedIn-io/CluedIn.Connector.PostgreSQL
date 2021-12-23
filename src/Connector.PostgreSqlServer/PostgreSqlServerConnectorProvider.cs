using CluedIn.Core;
using CluedIn.Connector.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CluedIn.Connector.PostgreSqlServer
{
    public class PostgreSqlServerConnectorProvider : ConnectorProviderBase<PostgreSqlServerConnectorProvider>
    {
        public PostgreSqlServerConnectorProvider([NotNull] ApplicationContext appContext, IPostgreSqlServerConstants configuration, ILogger<PostgreSqlServerConnectorProvider> logger)
            : base(appContext, configuration, logger)
        {
        }

        protected override IEnumerable<string> ProviderNameParts => new[] { CommonConfigurationNames.Host, CommonConfigurationNames.DatabaseName };
    }
}
