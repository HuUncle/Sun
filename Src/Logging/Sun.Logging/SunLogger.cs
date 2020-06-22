using System;
using Microsoft.Extensions.Logging;

namespace Sun.Logging
{
    public class SunLogger : ILogger
    {
        private readonly ILogStore _logStore;

        public SunLogger(ILogStore logStore)
        {
            _logStore = logStore;
        }

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel
            , EventId eventId
            , TState state
            , Exception exception
            , Func<TState, Exception, string> formatter)
        {
            var message = formatter?.Invoke(state, exception);

            _logStore?.Post(
                new LogEntry
                {
                    Id = eventId.Id,
                    Message = message,
                    Level = logLevel.ToString(),
                    Exception = exception
                });
        }
    }
}