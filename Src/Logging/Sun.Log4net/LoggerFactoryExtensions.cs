using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sun.Log4net
{
    public static class LoggerFactoryExtensions
    {
        public static IApplicationBuilder UseLog4net(this IApplicationBuilder app, string file = "log4net.config")
        {
            var factory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

            if (null == factory)
                throw new ArgumentNullException(nameof(factory));

            factory.AddProvider(new Log4netLoggerProvider(file));

            return app;
        }

        public static ILoggingBuilder AddLog4net(this ILoggingBuilder builder, string file = "log4net.config")
        {
            builder.AddProvider(new Log4netLoggerProvider(file));

            return builder;
        }
    }
}