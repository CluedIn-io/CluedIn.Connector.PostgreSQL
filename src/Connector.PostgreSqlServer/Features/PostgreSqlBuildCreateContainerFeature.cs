using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core.Connectors;
using CluedIn.Core.Streams.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CluedIn.Connector.Common.Helpers;
using System;

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

            if (string.IsNullOrWhiteSpace(tableName))
                throw new InvalidOperationException("The tableName must be provided.");

            if (columns == null)
                throw new InvalidOperationException("The data to specify columns must be provided.");

            var builder = new StringBuilder();

            var trimmedColumns = columns.Where(x => x.Name != "Codes").ToList();

            builder.AppendLine($"CREATE TABLE IF NOT EXISTS {SqlStringSanitizer.Sanitize(tableName)}(");

            var index = 0;

            var count = trimmedColumns.Count;

            foreach (var column in trimmedColumns)
            {
                builder.AppendLine($"{SqlStringSanitizer.Sanitize(column.Name)} text NULL " +

                                   // TODO: appoint PK to valid column for StreamMode Event
                                   $"{(column.Name.ToLower().Equals("originentitycode") && context == "Data" && streamMode == StreamMode.Sync ? "PRIMARY KEY" : "")}" +
                                   $"{(index < count - 1 ? "," : "")} ");
                index++;
            }

            builder.AppendLine(");");

            return new[] { new PostgreSqlConnectorCommand { Text = builder.ToString() } };
        }
    }
}
