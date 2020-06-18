using Elasticsearch.Net;
using Nest;
using System;
using System.Linq;

namespace Sun.Elasticsearch
{
    public class ElasticClientFactory : IElasticClientFactory
    {
        public ElasticClientFactory(ElasticsearchOptions options)
        {
            var urls = options.Uri.Split(';').Select(x => new Uri(x)).ToArray();

            // _logger.LogInformation($"Butterfly.Storage.Elasticsearch initialized ElasticClient
            // with options: ElasticSearchHosts={elasticsearchHosts}.");

            var pool = new StaticConnectionPool(urls);
            var settings = new ConnectionSettings(pool).DefaultIndex(options.DefaultIndex);

            if (!String.IsNullOrEmpty(options.UserName) && !string.IsNullOrEmpty(options.Password))
            {
                settings.BasicAuthentication(options.UserName, options.Password);
            }
            ESClient = new ElasticClient(settings);
            DefaultIndex = options.DefaultIndex;
        }

        public ElasticClient ESClient { get; set; }

        public string DefaultIndex { get; set; }

        public void EnsureIndexWithMapping<T>(string indexName = null, Func<PutMappingDescriptor<T>, PutMappingDescriptor<T>> customMapping = null) where T : class
        {
            if (String.IsNullOrEmpty(indexName)) indexName = this.DefaultIndex;

            // Map type T to that index
            ESClient.ConnectionSettings.DefaultIndices.Add(typeof(T), indexName);

            // Does the index exists?
            var indexExistsResponse = ESClient.Indices.Exists(new IndexExistsRequest(indexName));
            if (!indexExistsResponse.IsValid) throw new InvalidOperationException(indexExistsResponse.DebugInformation);

            // If exists, return
            if (indexExistsResponse.Exists) return;
            // Otherwise create the index and the type mapping
            var createIndexRes = ESClient.Indices.Create(indexName);
            if (!createIndexRes.IsValid) throw new InvalidOperationException(createIndexRes.DebugInformation);

            var res = ESClient.Map<T>(m =>
            {
                m.AutoMap().Index(indexName);
                if (customMapping != null) m = customMapping(m);
                return m;
            });

            if (!res.IsValid) throw new InvalidOperationException(res.DebugInformation);
        }
    }
}