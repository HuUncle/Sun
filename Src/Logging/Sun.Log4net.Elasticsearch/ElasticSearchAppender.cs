using System;
using log4net.Appender;
using log4net.Core;
using Nest;
using Sun.Elasticsearch;
using Sun.Logging;

namespace Sun.Log4net.Extensions
{
    public class ElasticSearchAppender : BufferingAppenderSkeleton
    {
        private readonly ElasticClient _elasticClient;

        public ElasticSearchAppender(IElasticClientFactory elasticClientFactory)
        {
            _elasticClient = elasticClientFactory?.ESClient ?? throw new ArgumentNullException(nameof(elasticClientFactory));
        }

        protected async override void SendBuffer(LoggingEvent[] events)
        {
            if (events == null || events.Length == 0)
                return;

            foreach (var le in events)
            {
                var logEntry = new LogEntry
                {
                    Level = le.Level.Name,
                    //ExceptionMessage = le.GetExceptionString(),
                    LoggerName = le.LoggerName,
                    Message = le.RenderedMessage,
                    TimeStamp = le.TimeStampUtc
                };

                await _elasticClient.IndexAsync(logEntry, idx => idx.Id(logEntry.Id));
            }
        }
    }
}