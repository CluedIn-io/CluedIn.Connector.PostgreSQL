using CluedIn.Connector.Common;
using System;
using System.Collections.Concurrent;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public class PostgreSqlFeatureStore : IFeatureStore
    {
        private readonly ConcurrentDictionary<Type, object> _store = new ConcurrentDictionary<Type, object>
        {
            [typeof(IBuildStoreDataFeature)] = new PostgreSqlBuildStoreDataFeature(),
            [typeof(IBuildCreateContainerFeature)] = new PostgreSqlBuildCreateContainerFeature(),
            [typeof(IBuildCreateIndexFeature)] = new PostgreSqlBuildCreateIndexFeature(),
            [typeof(IBuildDeleteDataFeature)] = new PostgreSqlBuildDeleteDataFeature()
        };

        public T GetFeature<T>()
        {
            return _store.TryGetValue(typeof(T), out var result) ? (T)result : default;
        }

        public void SetFeature<T>(T instance)
        {
            _store.AddOrUpdate(typeof(T), instance, (_, __) => instance);
        }
    }
}
