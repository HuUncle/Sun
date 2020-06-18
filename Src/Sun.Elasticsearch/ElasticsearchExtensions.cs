using System;
using Microsoft.Extensions.DependencyInjection;

namespace Sun.Elasticsearch
{
    public static class ElasticsearchExtensions
    {
        public static IServiceCollection AddElasticsearch(this IServiceCollection services, Action<ElasticsearchOptions> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var options = new ElasticsearchOptions();
            action.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton<IElasticClientFactory, ElasticClientFactory>();
            services.AddSingleton<IIndexManager, IndexManager>();

            return services;
        }
    }
}