using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core.DataStore;
using Microsoft.Extensions.Logging;
using Moq;


namespace CluedIn.Connector.PostgreSqlServer.Unit.Tests
{
    public class PostgreSqlServerConnectorTestsBase
    {
        protected readonly PostgreSqlServerConnector Sut;
        protected readonly Mock<IConfigurationRepository> Repo = new Mock<IConfigurationRepository>();
        protected readonly Mock<ILogger<PostgreSqlServerConnector>> Logger = new Mock<ILogger<PostgreSqlServerConnector>>();
        protected readonly Mock<IPostgreSqlClient> Client = new Mock<IPostgreSqlClient>();
        protected readonly TestContext Context = new TestContext();

        public PostgreSqlServerConnectorTestsBase()
        {

            Sut = new PostgreSqlServerConnector(Repo.Object, Logger.Object, Client.Object, new PostgreSqlServerConstants());
        }
    }
}
