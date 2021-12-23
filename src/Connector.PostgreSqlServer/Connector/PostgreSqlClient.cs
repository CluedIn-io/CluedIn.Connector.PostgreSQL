using CluedIn.Connector.Common.Clients;
using CluedIn.Connector.Common.Configurations;
using Npgsql;
using System;
using System.Collections.Generic;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public class PostgreSqlClient : ClientBase<NpgsqlConnection, NpgsqlParameter>, IPostgreSqlClient
    {
        public override string BuildConnectionString(IDictionary<string, object> config)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Password = (string)config[CommonConfigurationNames.Password],
                Username = (string)config[CommonConfigurationNames.Username],
                Database = (string)config[CommonConfigurationNames.DatabaseName],
                Host = (string)config[CommonConfigurationNames.Host]
            };

            connectionStringBuilder.Port = config.TryGetValue(CommonConfigurationNames.PortNumber, out var portEntry) &&
                                           portEntry != null && portEntry is int port
                ? port
                : NpgsqlConnection.DefaultPort;

            if (config.TryGetValue(CommonConfigurationNames.Schema, out var schemaNameEntry)
                && schemaNameEntry is string schemaName && !string.IsNullOrWhiteSpace(schemaName))
                connectionStringBuilder.SearchPath = schemaName;
            else
                connectionStringBuilder.SearchPath = PostgreSqlServerConstants.DefaultPgSQLSchema;

            connectionStringBuilder.SslMode =
                config.TryGetValue(PostgreSqlServerConstants.SSLMode, out var SSLModeEntry) && SSLModeEntry != null
                    ? (SslMode)Enum.Parse(typeof(SslMode), SSLModeEntry.ToString())
                    : SslMode.Require;

            //// Activate rule for internal debug / force SSLMode to be disabled:
            //connectionStringBuilder.SslMode = SslMode.Disable;

            return connectionStringBuilder.ToString();
        }
    }
}
