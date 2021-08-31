using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core.Connectors;
using Microsoft.Data.SqlClient;
using Npgsql;
using Serilog;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public class PostgreSqlClient : IPostgreSqlClient
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
        public async Task ExecuteCommandAsync(IConnectorConnection config, string commandText, IList<NpgsqlParameter> param = null)
        {
            Log.Logger.Information(commandText);

            using (var connection = await GetConnection(config.Authentication))
            {
                var cmd = connection.CreateCommand();

                cmd.CommandText = commandText;

                if (param != null)
                    cmd.Parameters.AddRange(param.ToArray());

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<NpgsqlConnection> GetConnection(IDictionary<string, object> config)
        {
            var cnxString = new NpgsqlConnectionStringBuilder();
            cnxString.Password = (string)config[PostgreSqlServerConstants.KeyName.Password];
            cnxString.Username = (string)config[PostgreSqlServerConstants.KeyName.Username];
            cnxString.Database = (string)config[PostgreSqlServerConstants.KeyName.DatabaseName];
            cnxString.Host = (string)config[PostgreSqlServerConstants.KeyName.Host];
            cnxString.Port = 5432;
            cnxString.SslMode = SslMode.Require;
            cnxString.TrustServerCertificate = true;

            var result = new NpgsqlConnection(cnxString.ToString());
            await result.OpenAsync();
            return result;
        }

        public async Task<DataTable> GetTables(IDictionary<string, object> config, string name = null)
        {
            using (var connection = await GetConnection(config))
            {
                DataTable result;
                if (!string.IsNullOrEmpty(name))
                {
                    var restrictions = new string[4];
                    restrictions[2] = name;

                    result = connection.GetSchema("Tables", restrictions);
                }
                else
                {
                    result = connection.GetSchema("Tables");
                }

                return result;
            }
        }

        public async Task<DataTable> GetTableColumns(IDictionary<string, object> config, string tableName)
        {
            using (var connection = await GetConnection(config))
            {

                var restrictions = new string[4];
                restrictions[2] = tableName;

                var result = connection.GetSchema("Columns", restrictions);

                return result;
            }
        }
    }
}
