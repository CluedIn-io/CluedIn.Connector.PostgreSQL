using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Connectors;
using CluedIn.Core.Data.Vocabularies;   
using CluedIn.Core.DataStore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>")]
    public class PostgreSqlServerConnector : ConnectorBase
    {
        private readonly ILogger<PostgreSqlServerConnector> _logger;
        private readonly IPostgreSqlClient _client;

        public PostgreSqlServerConnector(IConfigurationRepository repo, ILogger<PostgreSqlServerConnector> logger, IPostgreSqlClient client) : base(repo)
        {
            ProviderId = PostgreSqlServerConstants.ProviderId;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public override async Task CreateContainer(ExecutionContext executionContext, Guid providerDefinitionId, CreateContainerModel model)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                var sql = BuildCreateContainerSql(model);

                _logger.LogDebug($"PostgreSql Server Connector - Create Container - Generated query: {sql}");

                await _client.ExecuteCommandAsync(config, sql);
            }
            catch (Exception e)
            {
                var message = $"Could not create Container {model.Name} for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
                throw new CreateContainerException(message);
            }
        }

        public string BuildCreateContainerSql(CreateContainerModel model)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"CREATE TABLE {Sanitize(model.Name)}(");

            var index = 0;
            var count = model.DataTypes.Count;
            foreach (var type in model.DataTypes)
            {
                builder.AppendLine($"{Sanitize(type.Name)} {GetDbType(type.Type)} NULL {(type.Name.ToLower().Equals("originentitycode") ? "PRIMARY KEY" : "")}{(index < count - 1 ? "," : "")} ");
                index++;
            }

            builder.AppendLine(") ");

            var sql = builder.ToString();
            return sql;
        }

        public override async Task EmptyContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                var sql = BuildEmptyContainerSql(id);

                _logger.LogDebug($"PostgreSql Server Connector - Empty Container - Generated query: {sql}");

                await _client.ExecuteCommandAsync(config, sql);
            }
            catch (Exception e)
            {
                var message = $"Could not empty Container {id}";
                _logger.LogError(e, message);

                // throw new EmptyContainerException(message);
            }
        }

        public string BuildEmptyContainerSql(string id)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"TRUNCATE TABLE {Sanitize(id)}");
            var sql = builder.ToString();
            return sql;
        }

        private string Sanitize(string str)
        {
            return str.Replace("--", "").Replace(";", "").Replace("'", "");       // Bare-bones sanitization to prevent Sql Injection. Extra info here http://sommarskog.se/dynamic_sql.html
        }

        public override Task<string> GetValidDataTypeName(ExecutionContext executionContext, Guid providerDefinitionId, string name)
        {
            // Strip non-alpha numeric characters
            var result = Regex.Replace(name, @"[^A-Za-z0-9]+", "");

            return Task.FromResult(result);
        }

        public override async Task<string> GetValidContainerName(ExecutionContext executionContext, Guid providerDefinitionId, string name)
        {
            // Strip non-alpha numeric characters
            var result = Regex.Replace(name, @"[^A-Za-z0-9]+", "");

            // Check if exists
            if (await CheckTableExists(executionContext, providerDefinitionId, result))
            {
                // If exists, append count like in windows explorer
                var count = 0;
                string newName;
                do
                {
                    count++;
                    newName = $"{result}{count}";
                } while (await CheckTableExists(executionContext, providerDefinitionId, newName));

                result = newName;
            }

            // return new name
            return result;
        }

        private async Task<bool> CheckTableExists(ExecutionContext executionContext, Guid providerDefinitionId, string name)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);
                var tables = await _client.GetTables(config.Authentication, name);

                return tables.Rows.Count > 0;
            }
            catch (Exception e)
            {
                var message = $"Error checking Container '{name}' exists for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
                throw new ConnectionException(message);
            }
        }

        public override async Task<IEnumerable<IConnectorContainer>> GetContainers(ExecutionContext executionContext, Guid providerDefinitionId)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);
                var tables = await _client.GetTables(config.Authentication);

                var result = from DataRow row in tables.Rows
                             select row["TABLE_NAME"] as string into tableName
                             select new PostgreSqlServerConnectorContainer { Id = tableName, Name = tableName };

                return result.ToList();
            }
            catch (Exception e)
            {
                var message = $"Could not get Containers for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
                throw new GetContainersException(message);
            }
        }

        public override async Task<IEnumerable<IConnectionDataType>> GetDataTypes(ExecutionContext executionContext, Guid providerDefinitionId, string containerId)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);
                var tables = await _client.GetTableColumns(config.Authentication, containerId);

                var result = from DataRow row in tables.Rows
                             let name = row["COLUMN_NAME"] as string
                             let rawType = row["DATA_TYPE"] as string
                             let type = GetVocabType(rawType)
                             select new PostgreSqlServerConnectorDataType
                             {
                                 Name = name,
                                 RawDataType = rawType,
                                 Type = type
                             };

                return result.ToList();
            }
            catch (Exception e)
            {
                var message = $"Could not get Data types for Container '{containerId}' for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
                throw new GetDataTypesException(message);
            }
        }

        private VocabularyKeyDataType GetVocabType(string rawType)
        {

            return rawType.ToLower() switch
            {
                "bigint" => VocabularyKeyDataType.Text,
                "int" => VocabularyKeyDataType.Text,
                "smallint" => VocabularyKeyDataType.Text,
                "tinyint" => VocabularyKeyDataType.Text,
                "bit" => VocabularyKeyDataType.Text,
                "decimal" => VocabularyKeyDataType.Text,
                "numeric" => VocabularyKeyDataType.Text,
                "float" => VocabularyKeyDataType.Text,
                "real" => VocabularyKeyDataType.Text,
                "money" => VocabularyKeyDataType.Text,
                "smallmoney" => VocabularyKeyDataType.Text,
                "datetime" => VocabularyKeyDataType.Text,
                "smalldatetime" => VocabularyKeyDataType.Text,
                "date" => VocabularyKeyDataType.Text,
                "datetimeoffset" => VocabularyKeyDataType.Text,
                "time" => VocabularyKeyDataType.Text,
                "char" => VocabularyKeyDataType.Text,
                "varchar" => VocabularyKeyDataType.Text,
                "text" => VocabularyKeyDataType.Text,
                "nchar" => VocabularyKeyDataType.Text,
                "nvarchar" => VocabularyKeyDataType.Text,
                "ntext" => VocabularyKeyDataType.Text,
                "binary" => VocabularyKeyDataType.Text,
                "varbinary" => VocabularyKeyDataType.Text,
                "image" => VocabularyKeyDataType.Text,
                "timestamp" => VocabularyKeyDataType.Text,
                "uniqueidentifier" => VocabularyKeyDataType.Guid,
                "XML" => VocabularyKeyDataType.Xml,
                "geometry" => VocabularyKeyDataType.Text,
                "geography" => VocabularyKeyDataType.GeographyLocation,
                _ => VocabularyKeyDataType.Text
            };
        }

        private string GetDbType(VocabularyKeyDataType type)
        {
            //return type switch //TODO: LJU: Temporary change until we get vocabularies resolved @ LHO.
            //{
            //    VocabularyKeyDataType.Integer => "bigint",
            //    VocabularyKeyDataType.Number => "decimal(18,4)",
            //    VocabularyKeyDataType.Money => "money",
            //    VocabularyKeyDataType.DateTime => "datetime2",
            //    VocabularyKeyDataType.Time => "time",
            //    VocabularyKeyDataType.Xml => "XML",
            //    VocabularyKeyDataType.Guid => "uniqueidentifier",
            //    VocabularyKeyDataType.GeographyLocation => "geography",
            //    _ => "nvarchar(max)"
            //};

            return "text";
        }

        public override async Task<bool> VerifyConnection(ExecutionContext executionContext, Guid providerDefinitionId)
        {
            var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);
            _logger.LogInformation("Getting Authentication Details");
            return await VerifyConnection(executionContext, config.Authentication);
        }

        public override async Task<bool> VerifyConnection(ExecutionContext executionContext, IDictionary<string, object> config)
        {
            try
            {
                var connection = await _client.GetConnection(config);
                _logger.LogDebug($"PostgreSQL State: {connection.State}");
                return connection.State == ConnectionState.Open;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error verifying connection");
                throw new ConnectionException();
            }
        }

        public override async Task StoreData(ExecutionContext executionContext, Guid providerDefinitionId, string containerName, IDictionary<string, object> data)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                var sql = BuildStoreDataSql(containerName, data, out var param);

                _logger.LogDebug($"PostgreSql Server Connector - Store Data - Generated query: {sql}");

                await _client.ExecuteCommandAsync(config, sql, param);
            }
            catch (Exception e)
            {
                var message = $"Could not store data into Container '{containerName}' for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
            }
        }

        public string BuildStoreDataSql(string containerName, IDictionary<string, object> data, out List<NpgsqlParameter> param)
        {
            var builder = new StringBuilder();

            var nameList = data.Select(n => Sanitize(n.Key)).ToList();
            var fieldList = string.Join(", ", nameList.Select(n => $"{n}"));

            var values = new List<string>();

            foreach (var dataType in data)
            {
                if (dataType.Value is List<object> dataTypeValueList)
                {
                    values.Add(string.Join(",", dataTypeValueList.Select(x => x?.ToString() ?? string.Empty)));
                }
                else
                {
                    values.Add($"'{dataType.Value ?? string.Empty}'");
                }
            }

            var list = new List<string>();
            foreach (var dataType in data)
            {
                string updateValue;

                if (dataType.Value is List<object> dataTypeValueList)
                {
                    updateValue = string.Join(",", dataTypeValueList.Select(x => x?.ToString() ?? string.Empty));
                }
                else
                {
                    updateValue = dataType.Value?.ToString() ?? string.Empty;
                }

                list.Add($"{Sanitize(dataType.Key)} = '{updateValue}'");
            }

            var updateList = string.Join(",", list);

            builder.AppendLine($"INSERT INTO {Sanitize(containerName)} ({fieldList})");
            builder.AppendLine($"VALUES ({string.Join(",", values)})");
            builder.AppendLine($"ON CONFLICT(OriginEntityCode)");
            builder.AppendLine($"DO");
            builder.AppendLine($"  UPDATE SET {updateList}");

            param = new List<NpgsqlParameter>();
            foreach (var dataType in data)
            {
                var name = Sanitize(dataType.Key);
                if (dataType.Value is List<object> dataTypeValueList)
                {
                    param.Add(new NpgsqlParameter
                    {
                        ParameterName = $"@{name}",
                        Value = string.Join(",", dataTypeValueList.Select(x => x?.ToString() ?? string.Empty))
                    });
                }
                else
                {
                    param.Add(new NpgsqlParameter {ParameterName = $"@{name}", Value = dataType.Value ?? string.Empty});
                }
            }

            return builder.ToString();
        }

        public override async Task ArchiveContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                var newName = await GetValidContainerName(executionContext, providerDefinitionId, $"{id}{DateTime.Now:yyyyMMddHHmmss}");
                var sql = BuildRenameContainerSql(id, newName, out var param);

                _logger.LogDebug($"PostgreSql Server Connector - Archive Container - Generated query: {sql}");

                await _client.ExecuteCommandAsync(config, sql, param);
            }
            catch (Exception e)
            {
                var message = $"Could not archive Container {id}";
                _logger.LogError(e, message);

                // throw new EmptyContainerException(message);
            }
        }

        private string BuildRenameContainerSql(string id, string newName, out List<NpgsqlParameter> param)
        {
            var result = $"ALTER TABLE IF EXISTS {Sanitize(id)} RENAME TO {Sanitize(newName)}";

            param = new List<NpgsqlParameter>
            {
                new NpgsqlParameter("@tableName", SqlDbType.NVarChar)
                {
                    Value = Sanitize(id)
                },
                new NpgsqlParameter("@newName", SqlDbType.NVarChar)
                {
                    Value = Sanitize(newName)
                }
            };

            return result;
        }

        private string BuildRemoveContainerSql(string id)
        {
            var result = $"DROP TABLE  IF EXISTS {Sanitize(id)}";
            return result;
        }

        public override async Task RenameContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id, string newName)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                var tempName = Sanitize(newName);

                var sql = BuildRenameContainerSql(id, tempName, out var param);

                _logger.LogDebug($"Sql Server Connector - Rename Container - Generated query: {sql}");

                await _client.ExecuteCommandAsync(config, sql, param);
            }
            catch (Exception e)
            {
                var message = $"Could not rename Container {id}";
                _logger.LogError(e, message);

                // throw new EmptyContainerException(message);
            }
        }

        public override async Task RemoveContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                var sql = BuildRemoveContainerSql(id);

                _logger.LogDebug($"PostgreSql Server Connector - Remove Container - Generated query: {sql}");

                await _client.ExecuteCommandAsync(config, sql);
            }
            catch (Exception e)
            {
                var message = $"Could not remove Container {id}";
                _logger.LogError(e, message);

                // throw new EmptyContainerException(message);
            }
        }

        public override Task StoreEdgeData(ExecutionContext executionContext, Guid providerDefinitionId, string containerName, string originEntityCode, IEnumerable<string> edges)
        {
            throw new NotImplementedException();
        }
    }
}
