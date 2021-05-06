using System.Collections.Generic;
using CluedIn.Core.Crawling;

namespace CluedIn.Connector.PostgreSqlServer
{
    public class PostgreSqlServerConnectorJobData : CrawlJobData
    {
        public PostgreSqlServerConnectorJobData(IDictionary<string, object> configuration)
        {
            if (configuration == null)
            {
                return;
            }

            Username = GetValue<string>(configuration, PostgreSqlServerConstants.KeyName.Username);
            DatabaseName = GetValue<string>(configuration, PostgreSqlServerConstants.KeyName.DatabaseName);
            Host = GetValue<string>(configuration, PostgreSqlServerConstants.KeyName.Host);
            Password = GetValue<string>(configuration, PostgreSqlServerConstants.KeyName.Password);
            PortNumber = GetValue<int>(configuration, PostgreSqlServerConstants.KeyName.PortNumber);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object> {
                { PostgreSqlServerConstants.KeyName.Username, Username },
                { PostgreSqlServerConstants.KeyName.DatabaseName, DatabaseName },
                { PostgreSqlServerConstants.KeyName.Host, Host },
                { PostgreSqlServerConstants.KeyName.Password, Password },
                { PostgreSqlServerConstants.KeyName.PortNumber, PortNumber }
            };
        }

        public string Username { get; set; }

        public string DatabaseName { get; set; }

        public string Host { get; set; }

        public string Password { get; set; }

        public int PortNumber { get; set; }
    }
}
