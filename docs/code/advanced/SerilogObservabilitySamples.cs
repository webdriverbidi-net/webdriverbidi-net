namespace WebDriverBiDi.Docs.Code.Advanced;

using System.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

public static class SerilogObservabilitySamples
{

    /// <summary>
    /// Serilog structured logging with AddWebDriverBiDi.
    /// </summary>
    public static async Task LoggingWithSerilog()
    {
        #region SerilogStructuredLogging
        // Configure Serilog with structured logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // the bridge logs EventLevel.Verbose events at LogLevel.Debug
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}")
            .Enrich.FromLogContext()
            .CreateLogger();

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
            builder.AddWebDriverBiDi(EventLevel.Verbose); // Capture all events including verbose
        });

        await using var serviceProvider = services.BuildServiceProvider();

        // The bridge starts listening when the logging pipeline is built, which happens the first time
        // ILoggerFactory (or an ILogger) is resolved. A generic or web host does this at startup.
        _ = serviceProvider.GetRequiredService<ILoggerFactory>();

        // Structured properties will be captured by Serilog
        await using var driver = new BiDiDriver();
        await driver.StartAsync("ws://localhost:9515/session/YOUR-SESSION-ID");

        // Serilog renders the message from the event's message template. The properties that template uses
        // (commandId, method, elapsedMilliseconds) are captured on the log event, so {Properties} does not
        // repeat them:
        // [12:34:56 INF] Command 1 (session.status) completed in 42ms {"EventId": {"Id": 7, "Name": "CommandCompleted"}, "EventName": "CommandCompleted", "EventSource": "WebDriverBiDi", "SourceContext": "WebDriverBiDi.Logging.WebDriverBiDiEventSourceLogger"}
        #endregion
    }
}
