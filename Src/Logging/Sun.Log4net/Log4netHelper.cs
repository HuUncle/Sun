using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Config;

namespace Sun.Log4net
{
    public static class Log4netHelper
    {
        public static string AssemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? AppDomain.CurrentDomain.FriendlyName;

        private static readonly object ConfigInitLock = new object();

        public static int LogInit(string log4NetConfigFile)
        {
            if (string.IsNullOrEmpty(log4NetConfigFile))
                throw new ArgumentException($"log4NetConfigFile is not be null");

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, log4NetConfigFile);
            if (!File.Exists(path))
                throw new FileNotFoundException($"{path} not found");

            if (null == LogManager.GetAllRepositories()?.FirstOrDefault(p => p.Name == AssemblyName))
            {
                lock (ConfigInitLock)
                {
                    if (null == LogManager.GetAllRepositories()
                            ?.FirstOrDefault(p => p.Name == AssemblyName))
                    {
                        XmlConfigurator.ConfigureAndWatch(LogManager.CreateRepository(AssemblyName), new FileInfo(path));
#if NET45
                        XmlConfigurator.ConfigureAndWatch(new FileInfo(configFilePath));
#endif
                        return 1;
                    }
                }
            }
            return 0;
        }

        public static ILog GetLogger<TCategory>()
        {
            return LogManager.GetLogger(AssemblyName, typeof(TCategory));
        }

        public static ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(AssemblyName, type);
        }

        public static ILog GetLogger(string loggerName)
        {
            return LogManager.GetLogger(AssemblyName, loggerName);
        }
    }
}