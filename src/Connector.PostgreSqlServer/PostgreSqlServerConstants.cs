using CluedIn.Connector.Common.Configurations;
using CluedIn.Core.Providers;
using System;

namespace CluedIn.Connector.PostgreSqlServer
{
    public class PostgreSqlServerConstants : ConfigurationConstantsBase, IPostgreSqlServerConstants
    {
        public const string DefaultPgSQLSchema = "cluedin";
        public const string SSLMode = nameof(SSLMode);

        public PostgreSqlServerConstants() : base(Guid.Parse("838E4EA2-80E0-4D60-B1D1-F052BFCD0CAF"),
            providerName: "PostgreSql Server Connector",
            componentName: "PostgreSqlServerConnector",
            icon: "Resources.sqlserver.png",
            domain: "https://www.postgresql.org/docs/",
            about: "Supports publishing of data to external PostgreSql databases",
            PostgreSqlAuthMethods,
            guideDetails: "Provides connectivity to a PostgreSql Server database")
        {
        }

        private static AuthMethods PostgreSqlAuthMethods => new AuthMethods
        {
            token = new[]
            {
                new Control
                {
                    name = CommonConfigurationNames.Host,
                    displayName = CommonConfigurationNames.Host,
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = CommonConfigurationNames.DatabaseName,
                    displayName = CommonConfigurationNames.DatabaseName,
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = CommonConfigurationNames.Username,
                    displayName = CommonConfigurationNames.Username,
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = CommonConfigurationNames.Password,
                    displayName = CommonConfigurationNames.Password,
                    type = "password",
                    isRequired = true
                },
                new Control
                {
                    name = CommonConfigurationNames.PortNumber,
                    displayName = "Port Number (default: 5432)",
                    type = "input",
                    isRequired = false
                },
                new Control
                {
                    name = CommonConfigurationNames.Schema,
                    displayName = "Schema (default: cluedin)",
                    type = "input",
                    isRequired = false
                },
                new Control
                {
                    name = SSLMode,
                    displayName = "SSLMode (default: Require)",
                    type = "Input",
                    isRequired = false
                }
            }
        };
    }
}
