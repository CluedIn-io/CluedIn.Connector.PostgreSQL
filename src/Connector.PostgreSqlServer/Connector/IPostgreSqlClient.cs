using CluedIn.Connector.Common.Clients;
using Npgsql;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public interface IPostgreSqlClient : IClientBase<NpgsqlConnection, NpgsqlParameter>
    {
    }
}
