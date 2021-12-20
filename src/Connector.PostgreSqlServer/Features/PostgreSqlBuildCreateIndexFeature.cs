using CluedIn.Connector.PostgreSqlServer.Connector;
using Connector.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public class PostgreSqlBuildCreateIndexFeature : IBuildCreateIndexFeature
    {
        public virtual IEnumerable<PostgreSqlConnectorCommand> BuildCreateIndexSql(
            string tableName,
            IEnumerable<string> keys,
            ILogger logger,
            string indexName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new InvalidOperationException("The Table Name must be provided.");

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var builder = new StringBuilder();

            // TODO: Better define INDEX. Proper approach would be to search if it exists, and check the fields. IDX can have more than 1 column to index.
            if (string.IsNullOrWhiteSpace(indexName)) indexName = $"idx_{tableName.SqlSanitize()}";

            builder.AppendLine(
                $"CREATE INDEX IF NOT EXISTS {indexName} ON {tableName.SqlSanitize()}({string.Join(", ", keys)}); ");

            return new[] {new PostgreSqlConnectorCommand {Text = builder.ToString()}};
        }
    }
}
