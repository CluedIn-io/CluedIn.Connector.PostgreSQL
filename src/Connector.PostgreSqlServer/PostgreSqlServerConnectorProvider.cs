using CluedIn.Connector.Common;
using CluedIn.Connector.Common.Configurations;
using CluedIn.Core;
using CluedIn.Core.Crawling;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CluedIn.Connector.PostgreSqlServer
{
    public class PostgreSqlServerConnectorProvider : ConnectorProviderBase<PostgreSqlServerConnectorProvider>
    {
        public PostgreSqlServerConnectorProvider([NotNull] ApplicationContext appContext, IPostgreSqlServerConstants configuration, ILogger<PostgreSqlServerConnectorProvider> logger)
            : base(appContext, configuration, logger)
        {
        }

        protected override IEnumerable<string> ProviderNameParts => new[] { CommonConfigurationNames.Host, CommonConfigurationNames.DatabaseName };

        public override string Schedule(DateTimeOffset relativeDateTime, bool webHooksEnabled)
            => $"{relativeDateTime.Minute} 0/23 * * *";

        public override Task<CrawlLimit> GetRemainingApiAllowance(ExecutionContext context, CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId)
            => Task.FromResult(new CrawlLimit(-1, TimeSpan.Zero));
    }
}
