namespace WebDriverBiDi.Docs.Code.Advanced;

using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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

        var serviceProvider = services.BuildServiceProvider();

        // Structured properties will be captured by Serilog
        await using var driver = new BiDiDriver();
        await driver.StartAsync("ws://localhost:9222");

        // Serilog output includes structured properties:
        // [12:34:56 INF] CommandCompleted {"EventId":7,"EventName":"CommandCompleted","commandId":"1","method":"session.status","elapsedMilliseconds":42}
#endregion
    }
}