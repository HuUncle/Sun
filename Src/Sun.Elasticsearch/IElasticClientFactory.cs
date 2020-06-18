using System;
using Nest;

namespace Sun.Elasticsearch
{
    public interface IElasticClientFactory
    {
        ElasticClient ESClient { get; set; }

        string DefaultIndex { get; set; }

        void EnsureIndexWithMapping<T>(string indexName = null,
            Func<PutMappingDescriptor<T>, PutMappingDescriptor<T>> customMapping = null)
            where T : class;
    }
}