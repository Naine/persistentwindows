using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Ninjacrab.PersistentWindows.Diagnostics
{
    public class Log
    {
        static Log()
        {
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            consoleTarget.Layout = @"${date:format=HH\\:mm\\:ss} ${logger} ${message}";
            fileTarget.FileName = "${basedir}/PersistentWindows.log";
            fileTarget.Layout = "${date:format=HH\\:mm\\:ss} ${logger} ${message}";

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(rule1);

            var rule2 = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Occurs when something is logged. STATIC EVENT!
        /// </summary>
        public static event Action<LogLevel, string>? LogEvent;

        private static Logger? _logger;
        private static Logger Logger
            => _logger ??= LogManager.GetLogger("Logger");

        public static void Trace(string message)
        {
            Logger.Trace(message);
            LogEvent?.Invoke(LogLevel.Trace, message);
        }

        public static void Info(string message)
        {
            Logger.Info(message);
            LogEvent?.Invoke(LogLevel.Info, message);
        }

        public static void Error(string message)
        {
            Logger.Error(message);
            LogEvent?.Invoke(LogLevel.Error, message);
        }
    }
}
