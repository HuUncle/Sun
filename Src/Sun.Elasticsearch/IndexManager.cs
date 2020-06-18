using Nest;
using System;

namespace Sun.Elasticsearch
{
    public class IndexManager : IIndexManager
    {
        private readonly IElasticClientFactory _elasticClientFactory;

        public IndexManager(IElasticClientFactory elasticClientFactory)
        {
            _elasticClientFactory = elasticClientFactory;
        }

        public void EnsureIndexWithMapping<T>(string indexName = null, Func<PutMappingDescriptor<T>, PutMappingDescriptor<T>> customMapping = null) where T : class
        {
            if (String.IsNullOrEmpty(indexName)) indexName = this._elasticClientFactory.DefaultIndex;

            // Map type T to that index
            this._elasticClientFactory.ESClient.ConnectionSettings.DefaultIndices.Add(typeof(T), indexName);

            // Does the index exists?
            var indexExistsResponse = this._elasticClientFactory.ESClient.Indices.Exists(new IndexExistsRequest(indexName));
            if (!indexExistsResponse.IsValid) throw new InvalidOperationException(indexExistsResponse.DebugInformation);

            // If exists, return
            if (indexExistsResponse.Exists) return;

            // Otherwise create the index and the type mapping
            var createIndexRes = this._elasticClientFactory.ESClient.Indices.Create(indexName);
            if (!createIndexRes.IsValid) throw new InvalidOperationException(createIndexRes.DebugInformation);

            var res = this._elasticClientFactory.ESClient.Map<T>(m =>
            {
                m.AutoMap().Index(indexName);
                if (customMapping != null) m = customMapping(m);
                return m;
            });

            if (!res.IsValid) throw new InvalidOperationException(res.DebugInformation);
        }
    }
}