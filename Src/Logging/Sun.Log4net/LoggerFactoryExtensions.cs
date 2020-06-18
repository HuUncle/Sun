using System;
using Microsoft.Extensions.Logging;

namespace Sun.Log4net
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddLog4net(this ILoggerFactory loggerFactory, string file = "log4net.config")
        {
            if (null == loggerFactory)
                throw new ArgumentNullException(nameof(loggerFactory));

            loggerFactory.AddProvider(new Log4netLoggerProvider(file));

            return loggerFactory;
        }

        public static ILoggingBuilder AddLog4net(this ILoggingBuilder builder, string file = "log4net.config")
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddProvider(new Log4netLoggerProvider(file));

            return builder;
        }
    }
}