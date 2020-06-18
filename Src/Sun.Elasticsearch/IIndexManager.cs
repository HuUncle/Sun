using Nest;
using System;

namespace Sun.Elasticsearch
{
    public interface IIndexManager
    {
        void EnsureIndexWithMapping<T>(string indexName = null, Func<PutMappingDescriptor<T>, PutMappingDescriptor<T>> customMapping = null) where T : class;
    }
}