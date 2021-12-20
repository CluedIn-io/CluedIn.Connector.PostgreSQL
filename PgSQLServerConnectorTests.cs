using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CluedIn.Core.Connectors;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.DataStore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ExecutionContext = CluedIn.Core.ExecutionContext;


namespace CluedIn.Connector.PostgreSqlServer.Integration.Tests
{

    public class PgSQLServerConnectorTests
    {
        public PgSQLServerConnectorTests()
        {
            [Fact]
            public async Task<Task> ConnectionTest()
            {
                var configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepositoryMock
                    .Setup(x => x.GetConfigurationById(It.IsAny<ExecutionContext>(), It.IsAny<Guid>()))
                    .Returns(new Dictionary<string, object>
                    {
                    {SqlServerConstants.KeyName.Username, "sa"},
                    {SqlServerConstants.KeyName.Password, "yourStrong(!)Password"},
                    {SqlServerConstants.KeyName.Host, "localhost"},
                    {SqlServerConstants.KeyName.DatabaseName, "Stream"}
                    });
                var configCon = new Dictionary<string, object>
                {
                    {SqlServerConstants.KeyName.Username, "sa"},
                    {SqlServerConstants.KeyName.Password, "yourStrong(!)Password"},
                    {SqlServerConstants.KeyName.Host, "localhost"},
                    {SqlServerConstants.KeyName.DatabaseName, "Stream"}
                };




                var logger = Mock.Of<ILogger<SqlServerConnector>>();
                var sqlClient = new SqlClient();
                var providerId = SqlServerConstants.ProviderId;
                var executionContext = Mock.Of<ExecutionContext>();
                var providerDefinitionId = Guid.NewGuid();
                var features = Mock.Of<IFeatureStore>();
                var sqlServerConnector = new SqlServerConnector(configurationRepositoryMock.Object, logger, sqlClient, features);
                var isConnectionOk = await sqlServerConnector.VerifyConnection(executionContext, configCon);
                return Task.CompletedTask;
            }
        }
    }
}
