using CluedIn.Connector.Common;
using Npgsql;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public interface IPostgreSqlClient : IClientBase<NpgsqlConnection, NpgsqlParameter>
    {
    }
}
