using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Couchbase.OperationalInsights.Performer.Internal.Logging;

public static class LoggingUtils
{
    private const string LogLevelEnvVarName = "LOG_LEVEL";

    public static ILoggerFactory ConfigureLogging(out LogEventLevel minimumLevel)
    {
        var envLogLevel = Environment.GetEnvironmentVariable(LogLevelEnvVarName);
        minimumLevel = ParseLogLevelOrDefault(envLogLevel, LogEventLevel.Information);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            // .WriteTo.File(
            //     path: "Logs/insights-performer.log",
            //     rollingInterval: RollingInterval.Day,
            //     retainedFileCountLimit: 7,
            //     shared: true)
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        return loggerFactory;
    }

    public static void ShutdownLogging()
    {
        Log.CloseAndFlush();
    }

    public static LogEventLevel ParseLogLevelOrDefault(string? value, LogEventLevel defaultLevel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultLevel;
        }

        // FIT tooling supplies LOG_LEVEL using its own convention (off|error|warn|info|debug|trace),
        // whose names don't all match Serilog's LogEventLevel (e.g. "warn" vs "Warning", "info" vs
        // "Information"). Map those explicitly. Serilog has no "off"; Fatal is the least-verbose level.
        switch (value.Trim().ToLowerInvariant())
        {
            case "off":
                return LogEventLevel.Fatal;
            case "error":
                return LogEventLevel.Error;
            case "warn":
                return LogEventLevel.Warning;
            case "info":
                return LogEventLevel.Information;
            case "debug":
                return LogEventLevel.Debug;
            case "trace":
                return LogEventLevel.Verbose;
        }

        if (Enum.TryParse<LogEventLevel>(value, true, out var serilogLevel))
        {
            return serilogLevel;
        }

        if (Enum.TryParse<LogLevel>(value, true, out var msLevel))
        {
            return msLevel switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => defaultLevel
            };
        }

        return defaultLevel;
    }
}