using System;
using System.Collections.Generic;
using CluedIn.Core.Net.Mail;
using CluedIn.Core.Providers;

namespace CluedIn.Connector.PostgreSqlServer
{
    public class PostgreSqlServerConstants
    {
        public struct KeyName
        {
            public const string Host = "host";
            public const string DatabaseName = "databaseName";
            public const string Username = "username";
            public const string Password = "password";
            public const string PortNumber = "portNumber";
        }

        public const string ConnectorName = "PostgreSqlServerConnector";
        public const string ConnectorComponentName = "PostgreSqlServerConnector";
        public const string ConnectorDescription = "Supports publishing of data to external PostgreSql databases.";
        public const string Uri = "https://www.postgresql.org/docs/";

        public static readonly Guid ProviderId = Guid.Parse("838E4EA2-80E0-4D60-B1D1-F052BFCD0CAF");
        public const string ProviderName = "PostgreSql Server Connector";
        public const bool SupportsConfiguration = false;
        public const bool SupportsWebHooks = false;
        public const bool SupportsAutomaticWebhookCreation = false;
        public const bool RequiresAppInstall = false;
        public const string AppInstallUrl = null;
        public const string ReAuthEndpoint = null;

        public static IList<string> ServiceType = new List<string> { "Connector" };
        public static IList<string> Aliases = new List<string> { "PostgreSqlServerConnector" };
        public const string IconResourceName = "Resources.sqlserver.png";
        public const string Instructions = "Provide authentication instructions here, if applicable";
        public const IntegrationType Type = IntegrationType.Connector;
        public const string Category = "Connectivity";
        public const string Details = "Provides connectivity to a PostgreSql Server database";

        public static AuthMethods AuthMethods = new AuthMethods
        {
            token = new Control[]
            {
                new Control
                {
                    name = KeyName.Host,
                    displayName = "Host",
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = KeyName.DatabaseName,
                    displayName = "DatabaseName",
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = KeyName.Username,
                    displayName = "Username",
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = KeyName.Password,
                    displayName = "Password",
                    type = "password",
                    isRequired = true
                },
                new Control
                {
                    name = KeyName.PortNumber,
                    displayName = "Port Number",
                    type = "input",
                    isRequired = false
                }
            }
        };

        public static IEnumerable<Control> Properties = new List<Control>
        {

        };

        public static readonly ComponentEmailDetails ComponentEmailDetails = new ComponentEmailDetails {
            Features = new Dictionary<string, string>
            {
                                       { "Connectivity",        "Expenses and Invoices against customers" }
                                   },
            Icon = ProviderIconFactory.CreateConnectorUri(ProviderId),
            ProviderName = ProviderName,
            ProviderId = ProviderId,
            Webhooks = SupportsWebHooks
        };

        public static IProviderMetadata CreateProviderMetadata()
        {
            return new ProviderMetadata {
                Id = ProviderId,
                ComponentName = ConnectorName,
                Name = ProviderName,
                Type = "Connector",
                SupportsConfiguration = SupportsConfiguration,
                SupportsWebHooks = SupportsWebHooks,
                SupportsAutomaticWebhookCreation = SupportsAutomaticWebhookCreation,
                RequiresAppInstall = RequiresAppInstall,
                AppInstallUrl = AppInstallUrl,
                ReAuthEndpoint = ReAuthEndpoint,
                ComponentEmailDetails = ComponentEmailDetails
            };
        }
    }
}