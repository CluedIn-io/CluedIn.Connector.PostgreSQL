using CluedIn.Core;
using ComponentHost;
using Connector.Common;

namespace CluedIn.Connector.PostgreSqlServer
{
    [Component(nameof(PostgreSqlServerConnectorComponent), "Providers", ComponentType.Service,
        ServerComponents.ProviderWebApi, Components.Server, Components.DataStores,
        Isolation = ComponentIsolation.NotIsolated)]
    public sealed class PostgreSqlServerConnectorComponent : ComponentBase<InstallComponents>
    {
        public PostgreSqlServerConnectorComponent(ComponentInfo componentInfo) : base(componentInfo)
        {
        }
    }
}
