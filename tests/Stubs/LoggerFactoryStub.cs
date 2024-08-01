using Microsoft.Extensions.Logging;

namespace ProcessManager.Tests
{
    public class LoggerFactoryStub : ILoggerFactory
    {
        private readonly LoggerStub<BreRunner> _loggerStub;

        public LoggerFactoryStub(LoggerStub<BreRunner> loggerStub)
        {
            _loggerStub = loggerStub;
        }

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => _loggerStub;

        public void Dispose() { }
    }
}