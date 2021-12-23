using System;
using System.Collections.Generic;
using System.Text;

namespace CluedIn.Connector.PostgreSqlServer.Features
{
    public interface IFeatureStore
    {
        void SetFeature<T>(T instance);

        T GetFeature<T>();
    }
}
