using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace Diffused.Tests.Helpers
{
    public static class TestLoggingExtensions
    {
        public static ILogger SetupLogging(this ITestOutputHelper output, LogEventLevel logEventLevel = LogEventLevel.Verbose)
        {
            return Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logEventLevel)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.XunitTestOutput(output)
                .CreateLogger();
        }
    }
}