using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Sun.Log4net
{
    [ProviderAlias("log4net")]
    public class Log4netLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, Log4netLogger> _loggers =
         new ConcurrentDictionary<string, Log4netLogger>(StringComparer.Ordinal);

        private const string DefaultLog4NetFileName = "log4net.config";

        public Log4netLoggerProvider()
            : this(DefaultLog4NetFileName)
        {
        }

        public Log4netLoggerProvider(string log4NetConfigFile)
        {
            Log4netHelper.LogInit(log4NetConfigFile);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, loggerName => new Log4netLogger(loggerName));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}