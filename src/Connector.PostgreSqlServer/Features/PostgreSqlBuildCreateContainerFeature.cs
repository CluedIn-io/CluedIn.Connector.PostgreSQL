using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core.Connectors;
using CluedIn.Core.Streams.Models;
using Connector.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public class PostgreSqlBuildCreateContainerFeature : IBuildCreateContainerFeature
    {
        public virtual IEnumerable<PostgreSqlConnectorCommand> BuildCreateContainerSql(
            string tableName,
            IEnumerable<ConnectionDataType> columns,
            IEnumerable<string> keys,
            string context,
            StreamMode streamMode,
            ILogger logger)
        {
            var builder = new StringBuilder();

            var trimmedColumns = columns.Where(x => x.Name != "Codes").ToList();

            builder.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName.SqlSanitize()}(");

            var index = 0;

            var count = trimmedColumns.Count;

            foreach (var column in trimmedColumns)
            {
                builder.AppendLine($"{column.Name.SqlSanitize()} text NULL " +

                                   // TODO: appoint PK to valid column for StreamMode Event
                                   $"{(column.Name.ToLower().Equals("originentitycode") && context == "Data" && streamMode == StreamMode.Sync ? "PRIMARY KEY" : "")}" +
                                   $"{(index < count - 1 ? "," : "")} ");
                index++;
            }

            builder.AppendLine(");");

            return new[] {new PostgreSqlConnectorCommand {Text = builder.ToString()}};
        }
    }
}
