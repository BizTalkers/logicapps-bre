using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ProcessManager.Tests
{
    public class LoggerStub<T> : ILogger<T>, IDisposable
    {
        public List<string> Logs { get; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state) => this;

        public void Dispose() { }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Logs.Add(formatter(state, exception));
        }
    }
}
