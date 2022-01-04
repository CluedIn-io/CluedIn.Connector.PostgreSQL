using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CluedIn.Core.DataStore;
using CluedIn.Connector.PostgreSqlServer;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ExecutionContext = CluedIn.Core.ExecutionContext;
using CluedIn.Connector.PostgreSqlServer.Connector;
using Xunit.Abstractions;
using CluedIn.Connector.Common.Configurations;

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
                    {CommonConfigurationNames.Username, "postgres"},
                    {CommonConfigurationNames.Password, "postgres"},
                    {CommonConfigurationNames.Host, "localhost"},
                    {CommonConfigurationNames.DatabaseName, "postgres"},
                    {CommonConfigurationNames.PortNumber,"5432"},
                    {CommonConfigurationNames.Schema,"cluedin"}

                });
            var configCon = new Dictionary<string, object>
                {
                    {CommonConfigurationNames.Username, "postgres"},
                    {CommonConfigurationNames.Password, "postgres"},
                    {CommonConfigurationNames.Host, "localhost"},
                    {CommonConfigurationNames.DatabaseName, "postgres"},
                    {CommonConfigurationNames.PortNumber,"5432"},
                    {CommonConfigurationNames.Schema,"cluedin"}

                };



            var logger = Mock.Of<ILogger<PostgreSqlServerConnector>>();
            var PgSqlClient = new PostgreSqlClient();
            var executionContext = Mock.Of<ExecutionContext>();
            var providerDefinitionId = Guid.NewGuid();
            var pgsqlServerConnector = new PostgreSqlServerConnector(configurationRepositoryMock.Object, logger, PgSqlClient, new PostgreSqlServerConstants());
            var isConnectionOk = await pgsqlServerConnector.VerifyConnection(executionContext, configCon);
            return Task.CompletedTask;
        }

        // [Fact]
        public async Task<Task> CheckSchemaTest()
        {
            var configurationRepositoryMock = new Mock<IConfigurationRepository>();
            configurationRepositoryMock
                .Setup(x => x.GetConfigurationById(It.IsAny<ExecutionContext>(), It.IsAny<Guid>()))
                .Returns(new Dictionary<string, object>
                {
                    {CommonConfigurationNames.Username, "postgres"},
                    {CommonConfigurationNames.Password, "postgres"},
                    {CommonConfigurationNames.Host, "localhost"},
                    {CommonConfigurationNames.DatabaseName, "postgres"},
                    {CommonConfigurationNames.PortNumber,"5432"},
                    {CommonConfigurationNames.Schema,"cluedin"}

                });
            var configCon = new Dictionary<string, object>
                {
                    {CommonConfigurationNames.Username, "postgres"},
                    {CommonConfigurationNames.Password, "postgres"},
                    {CommonConfigurationNames.Host, "localhost"},
                    {CommonConfigurationNames.DatabaseName, "postgres"},
                    {CommonConfigurationNames.PortNumber,"5432"},
                    {CommonConfigurationNames.Schema,"cluedin"}

                };



            var logger = Mock.Of<ILogger<PostgreSqlServerConnector>>();
            var PgSqlClient = new PostgreSqlClient();
            var executionContext = Mock.Of<ExecutionContext>();
            var providerDefinitionId = Guid.NewGuid();
            var pgsqlServerConnector = new PostgreSqlServerConnector(configurationRepositoryMock.Object, logger, PgSqlClient, new PostgreSqlServerConstants());
            await pgsqlServerConnector.CheckDbSchemaAsync(configCon);
            //var isConnectionOk = await sqlServerConnector.SchemaTestAsync(configCon,"0cluedin");
            //this.OutputHelper.WriteLine("Result:");
            return Task.CompletedTask;
        }


        
    }
}

