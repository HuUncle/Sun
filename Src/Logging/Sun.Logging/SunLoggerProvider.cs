using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Sun.Logging
{
    [ProviderAlias("SunLogger")]
    public class SunLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, SunLogger> _loggers =
         new ConcurrentDictionary<string, SunLogger>(StringComparer.Ordinal);

        private readonly ILogStore _logStore;

        public SunLoggerProvider(ILogStore logStore)
        {
            _logStore = logStore;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, loggerName => new SunLogger(_logStore));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}