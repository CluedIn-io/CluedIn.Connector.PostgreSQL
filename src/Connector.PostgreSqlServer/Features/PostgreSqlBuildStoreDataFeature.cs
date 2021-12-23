using CluedIn.Connector.PostgreSqlServer.Connector;
using CluedIn.Core.Streams.Models;
using CluedIn.Connector.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public class PostgreSqlBuildStoreDataFeature : IBuildStoreDataFeature, IBuildStoreDataForMode
    {
        public IEnumerable<PostgreSqlConnectorCommand> BuildStoreDataSql(string containerName,
            IDictionary<string, object> data, IList<string> keys, ILogger logger)
        {
            return BuildStoreDataSql(containerName, data, keys, StreamMode.Sync, null, logger);
        }

        public virtual IEnumerable<PostgreSqlConnectorCommand> BuildStoreDataSql(
            string containerName,
            IDictionary<string, object> data,
            IList<string> keys,
            StreamMode streamMode,
            string correlationId,
            ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new InvalidOperationException("The containerName must be provided.");

            if (data == null || data.Count == 0)
                throw new InvalidOperationException("The data to specify columns must be provided.");

            if (keys == null || !keys.Any())
                throw new InvalidOperationException("No Key Fields have been specified");

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            //HACK we need to pull out Codes into a separate table
            var container = new Container(containerName, streamMode);
            if (data.TryGetValue("Codes", out var codes) && codes is IEnumerable codesEnumerable)
            {
                data.Remove("Codes");
                keys.Remove("Codes");

                // HACK: need a better way to source origin entity code
                var codesTable = container.Tables["Codes"];

                if (streamMode == StreamMode.Sync)
                    // need to delete from Codes table
                    yield return ComposeDelete(codesTable.Name,
                        new Dictionary<string, object>
                        {
                            ["OriginEntityCode"] = data["OriginEntityCode"]
                        }); // TODO: ROK: move string literals to constants

                // need to insert into Codes table
                var enumerator = codesEnumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var dictionary = new Dictionary<string, object>
                    {
                        ["OriginEntityCode"] = data["OriginEntityCode"], ["Code"] = enumerator.Current
                    };

                    if (streamMode == StreamMode.EventStream)
                        dictionary["CorrelationId"] = correlationId;

                    yield return ComposeInsert(codesTable.Name, dictionary, "Codes", streamMode);
                }
            }

            yield return ComposeInsert(container.PrimaryTable, data, "Data", streamMode);
        }

        protected virtual PostgreSqlConnectorCommand ComposeDelete(string tableName, IDictionary<string, object> fields)
        {
            var sqlBuilder = new StringBuilder($"DELETE FROM {tableName.SqlSanitize()} WHERE ");
            var clauses = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            foreach (var entry in fields)
            {
                var key = entry.Key.SqlSanitize();
                clauses.Add($"{key} = @{key}");
                parameters.Add(new NpgsqlParameter($"{key}", entry.Value));
            }

            sqlBuilder.AppendJoin(" AND ", clauses);
            sqlBuilder.Append(";");

            return new PostgreSqlConnectorCommand {Text = sqlBuilder.ToString(), Parameters = parameters};
        }

        protected virtual PostgreSqlConnectorCommand ComposeInsert(string tableName, IDictionary<string, object> fields,
            string context, StreamMode streamMode)
        {
            var columns = new List<string>();
            var parameters = new List<NpgsqlParameter>();

            var updateList = string.Join(",",
                (from dataType in fields select $"{dataType.Key.SqlSanitize()} = @{dataType.Key}").ToList());

            foreach (var entry in fields)
            {
                columns.Add($"{entry.Key.SqlSanitize()}");
                parameters.Add(new NpgsqlParameter($"{entry.Key.SqlSanitize()}", entry.Value));
            }

            var sqlBuilder = new StringBuilder($"INSERT INTO {tableName.SqlSanitize()} (");
            sqlBuilder.AppendJoin(",", columns);
            sqlBuilder.Append(") VALUES (");
            sqlBuilder.AppendJoin(",", parameters.Select(x => $"@{x.ParameterName}"));
            sqlBuilder.Append(")");

            if (context != "Data" || streamMode != StreamMode.Sync)
                return new PostgreSqlConnectorCommand {Text = sqlBuilder.ToString(), Parameters = parameters};

            sqlBuilder.AppendLine(" ON CONFLICT(OriginEntityCode)");
            sqlBuilder.AppendLine(" DO");
            sqlBuilder.AppendLine($" UPDATE SET {updateList}");

            return new PostgreSqlConnectorCommand {Text = sqlBuilder.ToString(), Parameters = parameters};
        }
    }
}
