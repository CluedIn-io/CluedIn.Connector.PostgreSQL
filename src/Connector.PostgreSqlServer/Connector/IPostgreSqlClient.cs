using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CluedIn.Core.Connectors;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public interface IPostgreSqlClient
    {
        Task ExecuteCommandAsync(IConnectorConnection config, string commandText, IList<NpgsqlParameter> param = null);
        Task<NpgsqlConnection> GetConnection(IDictionary<string, object> config);
        Task<DataTable> GetTables(IDictionary<string, object> config, string name = null);
        Task<DataTable> GetTableColumns(IDictionary<string, object> config, string tableName);
    }
}
