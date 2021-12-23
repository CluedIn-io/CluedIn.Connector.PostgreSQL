using CluedIn.Connector.PostgreSqlServer.Features;
using CluedIn.Core;
using CluedIn.Core.Connectors;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.DataStore;
using CluedIn.Core.Streams.Models;
using CluedIn.Connector.Common;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CluedIn.Connector.PostgreSqlServer.Connector
{
    public class PostgreSqlServerConnector : SqlConnectorBase<PostgreSqlServerConnector, IPostgreSqlClient>,
        IConnectorStreamModeSupport
    {
        private const string TimestampFieldName = "TimeStamp";
        private const string ChangeTypeFieldName = "ChangeType";
        private const string CorrelationIdFieldName = "CorrelationId";
        private readonly IList<string> _defaultKeyFields = new List<string> { "OriginEntityCode" };
        private readonly IFeatureStore _features;

        public PostgreSqlServerConnector(IConfigurationRepository repository, ILogger<PostgreSqlServerConnector> logger,
            IPostgreSqlClient client,
            ICommonServiceHolder serviceHolder, IPostgreSqlServerConstants constants) : base(repository, logger, client,
            serviceHolder, constants.ProviderId)
        {
            _features = new PostgreSqlFeatureStore();
        }

        public StreamMode StreamMode { get; private set; } = StreamMode.Sync;

        public virtual IList<StreamMode> GetSupportedModes()
        {
            return new List<StreamMode> { StreamMode.Sync, StreamMode.EventStream };
        }

        public virtual void SetMode(StreamMode mode)
        {
            StreamMode = mode;
        }

        public async Task StoreData(ExecutionContext executionContext, Guid providerDefinitionId, string containerName,
            string correlationId, DateTimeOffset timestamp, VersionChangeType changeType,
            IDictionary<string, object> data)
        {
            try
            {
                var dataToUse = new Dictionary<string, object>(data);
                if (StreamMode == StreamMode.EventStream)
                {
                    dataToUse.Add(TimestampFieldName, timestamp);
                    dataToUse.Add(ChangeTypeFieldName, changeType.ToString());
                    dataToUse.Add(CorrelationIdFieldName, correlationId);
                }
                else
                    dataToUse.Add(TimestampFieldName, timestamp);

                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                // feature start here

                var feature = _features.GetFeature<IBuildStoreDataFeature>();
                IEnumerable<PostgreSqlConnectorCommand> commands;
                if (feature is IBuildStoreDataForMode modeFeature)
                    commands = modeFeature.BuildStoreDataSql(containerName, dataToUse, _defaultKeyFields, StreamMode,
                        correlationId, _logger);
                else
                    commands = feature.BuildStoreDataSql(containerName, dataToUse, _defaultKeyFields, _logger);

                foreach (var command in commands)
                    await _client.ExecuteCommandAsync(config, command.Text, command.Parameters);
            }
            catch (Exception e)
            {
                var message =
                    $"Could not store data into Container '{containerName}' for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
                throw new StoreDataException(message);
            }
        }

        public Task<string> GetCorrelationId()
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public async Task StoreEdgeData(ExecutionContext executionContext, Guid providerDefinitionId,
            string containerName, string originEntityCode, string correlationId, DateTimeOffset timestamp,
            VersionChangeType changeType, IEnumerable<string> edges)
        {
            try
            {
                var edgeTableName = $"{containerName.SqlSanitize()}Edges".ToLowerInvariant();
                if (await CheckTableExists(executionContext, providerDefinitionId, edgeTableName))
                {
                    var sql = BuildEdgeStoreDataSql(edgeTableName, originEntityCode, correlationId, edges,
                        out var param);

                    if (!string.IsNullOrWhiteSpace(sql))
                    {
                        _logger.LogDebug($"Sql Server Connector - Store Edge Data - Generated query: {sql}");

                        var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);
                        await _client.ExecuteCommandAsync(config, sql, param);
                    }
                }
            }
            catch (Exception e)
            {
                var message =
                    $"Could not store edge data into Container '{containerName}' for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
                throw new StoreDataException(message);
            }
        }

        public override async Task CreateContainer(ExecutionContext executionContext, Guid providerDefinitionId,
            CreateContainerModel model)
        {
            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                await CheckDbSchemaAsync(config.Authentication);

                async Task createTable(string tableName, IEnumerable<ConnectionDataType> columns,
                    IEnumerable<string> keys, string context)
                {
                    try
                    {
                        var commands = _features.GetFeature<IBuildCreateContainerFeature>()
                            .BuildCreateContainerSql(tableName, columns, keys, context, StreamMode, _logger);

                        // Do not create IDX on PrimaryTable if we are in SyncMode. It has a PK
                        if (context != "Data" || context == "Data" && StreamMode == StreamMode.EventStream)
                        {
                            var indexCommands = _features.GetFeature<IBuildCreateIndexFeature>()
                                .BuildCreateIndexSql(tableName, _defaultKeyFields, _logger);

                            commands = commands.Union(indexCommands);
                        }

                        foreach (var command in commands)
                        {
                            _logger.LogDebug(
                                "Sql Server Connector - Create Container[{Context}] - Generated query: {sql}", context,
                                command.Text);

                            await _client.ExecuteCommandAsync(config, command.Text, command.Parameters);
                        }
                    }
                    catch (Exception e)
                    {
                        var message = $"Could not create Container {tableName} for Connector {providerDefinitionId}";
                        _logger.LogError(e, message);
                        throw new CreateContainerException(message);
                    }
                }

                var container = new Container(model.Name, StreamMode);
                var codesTable = container.Tables["Codes"];

                var connectionDataTypes = model.DataTypes;

                if (StreamMode == StreamMode.EventStream)
                {
                    connectionDataTypes.Add(new ConnectionDataType
                    {
                        Name = TimestampFieldName,
                        Type = VocabularyKeyDataType.DateTime
                    });
                    connectionDataTypes.Add(new ConnectionDataType
                    {
                        Name = ChangeTypeFieldName,
                        Type = VocabularyKeyDataType.Text
                    });
                    connectionDataTypes.Add(new ConnectionDataType
                    {
                        Name = CorrelationIdFieldName,
                        Type = VocabularyKeyDataType.Text
                    });
                }
                else
                    connectionDataTypes.Add(new ConnectionDataType
                    {
                        Name = TimestampFieldName,
                        Type = VocabularyKeyDataType.DateTime
                    });

                var tasks = new List<Task>
                {
                    // Primary table
                    createTable(container.PrimaryTable, connectionDataTypes, _defaultKeyFields, "Data"),

                    // Codes table
                    createTable(codesTable.Name, codesTable.Columns, codesTable.Keys, "Codes")
                };

                // We optionally build an edges table
                if (model.CreateEdgeTable)
                {
                    var edgesTable = container.Tables["Edges"];
                    tasks.Add(createTable(edgesTable.Name, edgesTable.Columns, edgesTable.Keys, "Edges"));
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                var message = $"Could not create Container {model.Name} for Connector {providerDefinitionId}";
                _logger.LogError(e, message);
                throw new CreateContainerException(message);
            }
        }

        public override async Task<string> GetValidContainerName(ExecutionContext executionContext,
            Guid providerDefinitionId, string name)
        {
            return await _commonServiceHolder.GetValidContainerName(executionContext, providerDefinitionId, name,
                CheckTableExists);
        }

        private async Task<bool> CheckTableExists(ExecutionContext executionContext, Guid providerDefinitionId,
            string name)
        {
            return await _commonServiceHolder.CheckTableExists(executionContext, providerDefinitionId, name, _client,
                this, _logger);
        }

        public override Task StoreData(ExecutionContext executionContext, Guid providerDefinitionId,
            string containerName, IDictionary<string, object> data)
        {
            return StoreData(executionContext, providerDefinitionId, containerName, null, DateTimeOffset.Now,
                VersionChangeType.NotSet, data);
        }

        public override async Task ArchiveContainer(ExecutionContext executionContext, Guid providerDefinitionId,
            string oldTableName)
        {
            if (oldTableName is null)
                throw new ArgumentNullException(nameof(oldTableName));

            try
            {
                var config = await base.GetAuthenticationDetails(executionContext, providerDefinitionId);

                var newName = await GetValidContainerName(executionContext, providerDefinitionId,
                    $"{oldTableName}{DateTime.Now:yyyyMMddHHmmss}");
                var sql = BuildRenameContainerSql(oldTableName, newName);

                _logger.LogDebug($"PostgreSql Server Connector - Archive Container - Generated query: {sql}");

                await _client.ExecuteCommandAsync(config, sql);
            }
            catch (Exception e)
            {
                var message = $"Could not archive Container {oldTableName}";
                _logger.LogError(e, message);

                throw new EmptyContainerException(message);
            }
        }

        private string BuildRenameContainerSql(string oldTableName, string newTableName)
        {
            return $"ALTER TABLE IF EXISTS {oldTableName.SqlSanitize()} RENAME TO {newTableName.SqlSanitize()}";
        }

        private string BuildRemoveContainerSql(string tableName)
        {
            return $"DROP TABLE  IF EXISTS {tableName.SqlSanitize()}";
        }

        public override Task RenameContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id,
            string newName)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<IConnectorContainer>> GetContainers(ExecutionContext executionContext,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<IConnectionDataType>> GetDataTypes(ExecutionContext executionContext,
            Guid providerDefinitionId, string containerId)
        {
            throw new NotImplementedException();
        }

        public override Task EmptyContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id)
        {
            throw new NotImplementedException();
        }

        //not used
        public override async Task RemoveContainer(ExecutionContext executionContext, Guid providerDefinitionId,
            string id)
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
                throw new EmptyContainerException(message);
            }
        }

        public override Task StoreEdgeData(ExecutionContext executionContext, Guid providerDefinitionId,
            string containerName, string originEntityCode, IEnumerable<string> edges)
        {
            return StoreEdgeData(executionContext, providerDefinitionId, containerName, originEntityCode, null,
                DateTimeOffset.Now, VersionChangeType.NotSet, edges);
        }

        protected override async Task<IDbConnection> GetDbConnection(IDictionary<string, object> config)
        {
            return await _client.GetConnection(config);
        }

        public async Task CheckDbSchemaAsync(IDictionary<string, object> config)
        {
            var schemaName = config.TryGetValue(CommonConfigurationNames.Schema, out var schema)
                ? schema.ToString()
                : PostgreSqlServerConstants.DefaultPgSQLSchema;

            await using var connection = await _client.GetConnection(config);
            var cmdDb =
                new NpgsqlCommand(
                    "SELECT schema_name FROM information_schema.schemata WHERE schema_name = @schemaName",
                    connection);

            cmdDb.Parameters.AddWithValue("schemaName", schemaName);

            var reader = cmdDb.ExecuteReader();

            if (!reader.HasRows)
                await CreateSchemaAsync(config, schemaName);
        }

        public async Task CreateSchemaAsync(IDictionary<string, object> config, string schemaName)
        {
            await using var connection = await _client.GetConnection(config);
            var sqlQuery = $"CREATE SCHEMA IF NOT EXISTS \"{schemaName.SqlSanitize()}\"";
            var cmdDb = new NpgsqlCommand(sqlQuery, connection);
            cmdDb.ExecuteNonQuery();
        }

        public string BuildEdgeStoreDataSql(string containerName, string originEntityCode, string correlationId,
            IEnumerable<string> edges, out List<NpgsqlParameter> param)
        {
            var originParam = new NpgsqlParameter { ParameterName = "OriginEntityCode", Value = originEntityCode };
            var correlationParam = new NpgsqlParameter { ParameterName = "CorrelationId", Value = correlationId };
            param = new List<NpgsqlParameter> { originParam, correlationParam };

            var builder = new StringBuilder();

            if (StreamMode == StreamMode.Sync)
                builder.AppendLine(
                    $"DELETE FROM {containerName.SqlSanitize()} WHERE OriginEntityCode = @{originParam.ParameterName}; ");

            var edgeValues = new List<string>();
            foreach (var edge in edges)
            {
                var edgeParam = new NpgsqlParameter { ParameterName = $"{edgeValues.Count}", Value = edge };
                param.Add(edgeParam);

                edgeValues.Add(StreamMode == StreamMode.EventStream
                    ? $"(@OriginEntityCode, @CorrelationId, @{edgeParam.ParameterName})"
                    : $"(@OriginEntityCode, @{edgeParam.ParameterName})");
            }

            if (edgeValues.Count <= 0)
                return builder.ToString();

            builder.AppendLine(
                StreamMode == StreamMode.EventStream
                    ? $"INSERT INTO {containerName.SqlSanitize()} (OriginEntityCode,CorrelationId,Code) VALUES"
                    : $"INSERT INTO {containerName.SqlSanitize()} (OriginEntityCode,Code) VALUES");

            builder.AppendJoin(", ", edgeValues);

            return builder.ToString();
        }
    }
}
