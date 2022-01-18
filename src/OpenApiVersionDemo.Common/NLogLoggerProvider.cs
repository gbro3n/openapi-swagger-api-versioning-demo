using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
namespace OpenApiVersionDemo.Common
{
    public sealed class NLogLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, NLogLogger> _loggers = new();
        private readonly string _environmentName;

        public NLogLoggerProvider(string environmentName)
        {
            this._environmentName = environmentName;
        }

        public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new NLogLogger(_environmentName));


        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
