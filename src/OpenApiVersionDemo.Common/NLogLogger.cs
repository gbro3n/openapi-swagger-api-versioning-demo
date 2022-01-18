using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog;

namespace OpenApiVersionDemo.Common
{
    public class NLogLogger : Microsoft.Extensions.Logging.ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class.
        /// </summary>
        /// <param name="webHostEnvironment">environmentName will be read from webHostEnvironment.EnvironmentName</param>
        public NLogLogger(IWebHostEnvironment webHostEnvironment)
        {
            var environmentName = webHostEnvironment.EnvironmentName;

            NLogLoggerInstance = Init(environmentName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class.
        /// </summary>
        /// <param name="environmentName">Set null for non environment specific NLog.config file</param>
        public NLogLogger(string environmentName)
        {
            NLogLoggerInstance = Init(environmentName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogLogger"/> class.
        /// </summary>
        /// <param name="nLogLogger">An existing NLog.ILogger instance</param>
        public NLogLogger(NLog.ILogger nLogLogger)
        {
            NLogLoggerInstance = nLogLogger;
        }

        private NLog.ILogger NLogLoggerInstance { get; }

        public IDisposable BeginScope<TState>(TState state)
        {
            // Not implementing this feature

            return new DisposableStub();
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            if (logLevel == Microsoft.Extensions.Logging.LogLevel.Trace)
            {
                return NLogLoggerInstance.IsTraceEnabled;
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Debug)
            {
                return NLogLoggerInstance.IsDebugEnabled;
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Information)
            {
                return NLogLoggerInstance.IsInfoEnabled;
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Warning)
            {
                return NLogLoggerInstance.IsWarnEnabled;
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Error)
            {
                return NLogLoggerInstance.IsErrorEnabled;
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Critical)
            {
                return NLogLoggerInstance.IsFatalEnabled;
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.None)
            {
                return false;
            }
            else
            {
                throw new InvalidOperationException($"Unhandled type of {nameof(Microsoft.Extensions.Logging.LogLevel)}");
            }
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == Microsoft.Extensions.Logging.LogLevel.Trace)
            {
                NLogLoggerInstance.Trace(formatter(state, exception));
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Debug)
            {
                NLogLoggerInstance.Debug(formatter(state, exception));
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Information)
            {
                NLogLoggerInstance.Info(exception, formatter(state, exception));
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Warning)
            {
                NLogLoggerInstance.Warn(exception, formatter(state, exception));
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Error)
            {
                NLogLoggerInstance.Error(exception, formatter(state, exception));
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.Critical)
            {
                NLogLoggerInstance.Fatal(exception, formatter(state, exception));
            }
            else if (logLevel == Microsoft.Extensions.Logging.LogLevel.None)
            {
                NLogLoggerInstance.Info(formatter(state, exception));
            }
        }

        private Logger Init(string environmentName)
        {
            string nlogConfigFileName;

            if (environmentName != null)
            {
                nlogConfigFileName = $"NLog.{environmentName}.config";
            }
            else
            {
                nlogConfigFileName = "NLog.config";
            }

            return LogManager.LoadConfiguration(nlogConfigFileName).GetCurrentClassLogger();
        }

        private class DisposableStub : IDisposable
        {
            public void Dispose()
            {
                // Do nothing
            }
        }
    }
}