using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using CluedIn.Connector.PostgreSqlServer.Connector;

namespace CluedIn.Connector.PostgreSqlServer
{
    public class InstallComponents : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IPostgreSqlClient>().ImplementedBy<PostgreSqlClient>().OnlyNewServices());
        }
    }
}
