using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core.DataStore;
using Microsoft.Extensions.Logging;
using Moq;

namespace CluedIn.Connector.SqlServer.Unit.Tests
{
    public class SqlServerConnectorTestsBase
    {
        protected readonly PostgreSqlServerConnector Sut;
        protected readonly Mock<IConfigurationRepository> Repo = new Mock<IConfigurationRepository>();
        protected readonly Mock<ILogger<PostgreSqlServerConnector>> Logger = new Mock<ILogger<PostgreSqlServerConnector>>();
        protected readonly Mock<IPostgreSqlClient> Client = new Mock<IPostgreSqlClient>();
        protected readonly TestContext Context = new TestContext();

        public SqlServerConnectorTestsBase()
        {
            Sut = new PostgreSqlServerConnector(Repo.Object, Logger.Object, Client.Object);
        }
    }
}
