using System;
using log4net;
using Microsoft.Extensions.Logging;

namespace Sun.Log4net
{
    public class Log4netLogger : ILogger
    {
        private readonly ILog _logger;

        public Log4netLogger(string name)
        {
            _logger = LogManager.GetLogger(Log4netHelper.AssemblyName, name);
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return _logger.IsFatalEnabled;

                case LogLevel.Debug:
                case LogLevel.Trace:
                    return _logger.IsDebugEnabled;

                case LogLevel.Error:
                    return _logger.IsErrorEnabled;

                case LogLevel.Information:
                    return _logger.IsInfoEnabled;

                case LogLevel.Warning:
                    return _logger.IsWarnEnabled;

                case LogLevel.None:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!(string.IsNullOrEmpty(message) && exception == null))
            {
                switch (logLevel)
                {
                    case LogLevel.Critical:
                        _logger.Fatal(message, exception);
                        break;

                    case LogLevel.Debug:
                    case LogLevel.Trace:
                        _logger.Debug(message, exception);
                        break;

                    case LogLevel.Error:
                        _logger.Error(message, exception);
                        break;

                    case LogLevel.Information:
                        _logger.Info(message, exception);
                        break;

                    case LogLevel.Warning:
                        _logger.Warn(message, exception);
                        break;

                    default:
                        _logger.Warn($"Encountered unknown log level {logLevel}, writing out as Info.");
                        _logger.Info(message, exception);
                        break;
                }
            }
        }
    }
}