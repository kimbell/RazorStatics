using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace RazorStatics.Tests
{
    internal class TestScope : WebApplicationFactory<Program>
    {
        private readonly ITestOutputHelper _output;
        private readonly TestOutputLoggerProvider _logger;

        public bool UsePathBaseNet7Workaround { get; set; }
        public bool UsePathBase { get; set; }

        public TestScope(ITestOutputHelper output)
        {
            _output = output;
            _logger = new TestOutputLoggerProvider(output); ;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(l =>
            {
                l.ClearProviders();
                l.AddProvider(_logger);
            });
            builder.ConfigureAppConfiguration(c => c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { nameof(UsePathBaseNet7Workaround), UsePathBaseNet7Workaround.ToString() },
                { nameof(UsePathBase), UsePathBase.ToString() }
            }));
            //builder.UseWebRoot(Path.Combine(AppContext.BaseDirectory, "Content"));
        }
    }

    /// <summary>
    /// This provider allows us to capture log messages and show them for each xUnit test
    /// </summary>
    internal sealed class TestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, TestOutputLogger> _loggers = new();

        
        public ITestOutputHelper Output { get; }

        public TestOutputLoggerProvider(ITestOutputHelper output)
        {
            Output = output;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, _ => new TestOutputLogger(this));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    internal sealed class TestOutputLogger : ILogger
    {
        private readonly TestOutputLoggerProvider _provider;

        public TestOutputLogger(TestOutputLoggerProvider provider)
        {
            _provider = provider;
        }

#if NET6_0
        public IDisposable BeginScope<TState>(TState state)
#else
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
#endif
        {
            return new Scope();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var formattetMessage = formatter(state, exception);

            _provider.Output?.WriteLine(formattetMessage);
            if (exception?.StackTrace != null)
            {
                _provider.Output?.WriteLine(exception.Message);
                _provider.Output?.WriteLine(exception.StackTrace);
            }
        }

        private sealed class Scope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }

    /// <summary>
    /// Information about a single log entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// The category of the log entry
        /// </summary>
        public string Category { get; set; } = default!;
        /// <summary>
        /// The level of the log entry
        /// </summary>
        public LogLevel LogLevel { get; set; }
        /// <summary>
        /// The message of the log entry
        /// </summary>
        public string Message { get; set; } = default!;
        /// <summary>
        /// Any associated exception with the log entry
        /// </summary>
        public Exception? Exception { get; set; }
        /// <summary>
        /// Any captured values from logging scope
        /// </summary>
        public Dictionary<string, object>? Values { get; init; }
    }
}
