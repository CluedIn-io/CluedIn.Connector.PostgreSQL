using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core;
using CluedIn.Core.Data;
using Connector.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public class PostgreSqlBuildDeleteDataFeature : IBuildDeleteDataFeature
    {
        public IEnumerable<PostgreSqlConnectorCommand> BuildDeleteDataSql(
            ExecutionContext executionContext,
            Guid providerDefinitionId,
            string containerName,
            string originEntityCode,
            IList<IEntityCode> codes,
            Guid? entityId,
            ILogger logger)
        {
            if (executionContext == null)
                throw new ArgumentNullException(nameof(executionContext));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(containerName))
                throw new InvalidOperationException("The containerName must be provided.");

            if (!string.IsNullOrWhiteSpace(originEntityCode))
                return ComposeDelete(containerName,
                    new Dictionary<string, object> {["OriginEntityCode"] = originEntityCode});

            if (entityId.HasValue)
                return ComposeDelete(containerName, new Dictionary<string, object> {["Id"] = entityId.Value});

            if (codes != null)
                return codes.SelectMany(
                    x => ComposeDelete(containerName, new Dictionary<string, object> {["Code"] = x}));

            return Enumerable.Empty<PostgreSqlConnectorCommand>();
        }

        protected virtual IEnumerable<PostgreSqlConnectorCommand> ComposeDelete(string tableName,
            IDictionary<string, object> fields)
        {
            var sqlBuilder = new StringBuilder($"DELETE FROM {tableName.SqlSanitize()} WHERE ");
            var clauses = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            foreach (var entry in fields)
            {
                var key = entry.Key.SqlSanitize();
                clauses.Add($"{key} = @{key}");
                parameters.Add(new NpgsqlParameter(key, entry.Value));
            }

            sqlBuilder.AppendJoin(" AND ", clauses);
            sqlBuilder.Append(";");

            yield return new PostgreSqlConnectorCommand {Text = sqlBuilder.ToString(), Parameters = parameters};
        }
    }
}
