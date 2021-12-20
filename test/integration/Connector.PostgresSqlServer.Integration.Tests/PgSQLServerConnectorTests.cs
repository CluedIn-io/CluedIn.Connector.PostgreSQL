using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CluedIn.Core.Connectors;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.DataStore;
using CluedIn.Connector.PostgreSqlServer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ExecutionContext = CluedIn.Core.ExecutionContext;
using CluedIn.Connector.PostgreSqlServer.Connector;
using Xunit.Abstractions;

namespace Cluedin.Connector.PostgresSqlServer.Integration.Tests
{
    public class PgSQLServerConnectorTests
    {
        private readonly ITestOutputHelper OutputHelper;

        public PgSQLServerConnectorTests( ITestOutputHelper outputHelper)
        {
            this.OutputHelper = outputHelper;
        }

        [Fact]
        public async Task<Task> ConnectionTest()
        {
            var configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepositoryMock
                .Setup(x => x.GetConfigurationById(It.IsAny<ExecutionContext>(), It.IsAny<Guid>()))
                .Returns(new Dictionary<string, object>
                {
                    {PostgreSqlServerConstants.KeyName.Username, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Password, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Host, "localhost"},
                    {PostgreSqlServerConstants.KeyName.DatabaseName, "postgres"},
                    //{PostgreSqlServerConstants.KeyName.PortNumber, 5432},
                    {PostgreSqlServerConstants.KeyName.Schema, "cluedin"}
                });
            var configCon = new Dictionary<string, object>
                {
                    {PostgreSqlServerConstants.KeyName.Username, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Password, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Host, "localhost"},
                    {PostgreSqlServerConstants.KeyName.DatabaseName, "postgres"},
                    //{PostgreSqlServerConstants.KeyName.PortNumber, 5432},
                    {PostgreSqlServerConstants.KeyName.Schema, "cluedin"}
                };



            var logger = Mock.Of<ILogger<PostgreSqlServerConnector>>();
            var PgSqlClient = new PostgreSqlClient();
            var providerId = PostgreSqlServerConstants.ProviderId;
            var executionContext = Mock.Of<ExecutionContext>();
            var providerDefinitionId = Guid.NewGuid();
            var sqlServerConnector = new PostgreSqlServerConnector(configurationRepositoryMock.Object, logger, PgSqlClient);
            var isConnectionOk = await sqlServerConnector.VerifyConnection(executionContext, configCon);
            return Task.CompletedTask;
        }

        [Fact]
        public async Task<Task> CheckSchemaTest()
        {
            var configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepositoryMock
                .Setup(x => x.GetConfigurationById(It.IsAny<ExecutionContext>(), It.IsAny<Guid>()))
                .Returns(new Dictionary<string, object>
                {
                    {PostgreSqlServerConstants.KeyName.Username, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Password, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Host, "localhost"},
                    {PostgreSqlServerConstants.KeyName.DatabaseName, "postgres"},
                    {PostgreSqlServerConstants.KeyName.PortNumber, 5432},
                    {PostgreSqlServerConstants.KeyName.Schema, "cluedin"}
                });
            var configCon = new Dictionary<string, object>
                {
                    {PostgreSqlServerConstants.KeyName.Username, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Password, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Host, "localhost"},
                    {PostgreSqlServerConstants.KeyName.DatabaseName, "postgres"},
                    {PostgreSqlServerConstants.KeyName.PortNumber, 5432},
                    {PostgreSqlServerConstants.KeyName.Schema, "1cluedin"}
                };



            var logger = Mock.Of<ILogger<PostgreSqlServerConnector>>();
            var PgSqlClient = new PostgreSqlClient();
            var providerId = PostgreSqlServerConstants.ProviderId;
            var executionContext = Mock.Of<ExecutionContext>();
            var providerDefinitionId = Guid.NewGuid();
            var sqlServerConnector = new PostgreSqlServerConnector(configurationRepositoryMock.Object, logger, PgSqlClient);
            var isConnectionOk = await sqlServerConnector.CheckSchemaAsync(configCon);
            //var isConnectionOk = await sqlServerConnector.SchemaTestAsync(configCon,"0cluedin");
            this.OutputHelper.WriteLine("Result:" + isConnectionOk);
            return Task.CompletedTask;
        }


        [Fact]
        public async Task<Task> GetTableTest()
        {
            var configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepositoryMock
                .Setup(x => x.GetConfigurationById(It.IsAny<ExecutionContext>(), It.IsAny<Guid>()))
                .Returns(new Dictionary<string, object>
                {
                    {PostgreSqlServerConstants.KeyName.Username, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Password, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Host, "localhost"},
                    {PostgreSqlServerConstants.KeyName.DatabaseName, "postgres"}
                });
            var configCon = new Dictionary<string, object>
                {
                    {PostgreSqlServerConstants.KeyName.Username, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Password, "postgres"},
                    {PostgreSqlServerConstants.KeyName.Host, "localhost"},
                    {PostgreSqlServerConstants.KeyName.DatabaseName, "postgres"}
                };



            var logger = Mock.Of<ILogger<PostgreSqlServerConnector>>();
            var PgSqlClient = new PostgreSqlClient();
            var providerId = PostgreSqlServerConstants.ProviderId;
            var executionContext = Mock.Of<ExecutionContext>();
            var providerDefinitionId = Guid.NewGuid();
            var sqlServerConnector = new PostgreSqlServerConnector(configurationRepositoryMock.Object, logger, PgSqlClient);
            var isConnectionOk = await sqlServerConnector.VerifyConnection(executionContext, configCon);
            return Task.CompletedTask;
        }
    }
}

