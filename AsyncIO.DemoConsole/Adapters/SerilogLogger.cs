using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncIO.DemoConsole.Adapters
{
    class SerilogLogger : ILogger
    {
        private readonly Logger logger;

        public SerilogLogger(Logger logger)
        {
            this.logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return this.logger.IsEnabled(ConvertLogLevel(logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.logger.Write(ConvertLogLevel(logLevel), exception, formatter?.Invoke(state, exception));
        }

        private LogEventLevel ConvertLogLevel(LogLevel logLevel)
        {
            if (logLevel != LogLevel.None)
            {
                return (LogEventLevel)(int)logLevel;
            }
            else
            {
                return 0;
            }
        }
    }
}
