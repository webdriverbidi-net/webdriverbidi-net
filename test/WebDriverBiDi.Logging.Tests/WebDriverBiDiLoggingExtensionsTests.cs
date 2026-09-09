namespace WebDriverBiDi.Logging;

using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebDriverBiDi;
using WebDriverBiDi.Logging.TestUtilities;

[Collection("NonParallel")]
public class WebDriverBiDiLoggingExtensionsTests
{
    [Fact]
    public void AddWebDriverBiDi_WhenBuilderIsNull_ThrowsArgumentNullException()
    {
        ILoggingBuilder? builder = null;

        Assert.Throws<ArgumentNullException>(() => builder!.AddWebDriverBiDi());
    }

    [Fact]
    public void AddWebDriverBiDi_WithMinimumLevel_WhenBuilderIsNull_ThrowsArgumentNullException()
    {
        ILoggingBuilder? builder = null;

        Assert.Throws<ArgumentNullException>(() => builder!.AddWebDriverBiDi(EventLevel.Verbose));
    }

    [Fact]
    public void AddWebDriverBiDi_ReturnsBuilderForChaining()
    {
        ServiceCollection services = new();
        ILoggingBuilder builder = new LoggingBuilder(services);

        ILoggingBuilder result = builder.AddWebDriverBiDi();

        Assert.Same(builder, result);
    }

    [Fact]
    public void AddWebDriverBiDi_WithMinimumLevel_ReturnsBuilderForChaining()
    {
        ServiceCollection services = new();
        ILoggingBuilder builder = new LoggingBuilder(services);

        ILoggingBuilder result = builder.AddWebDriverBiDi(EventLevel.Warning);

        Assert.Same(builder, result);
    }

    [Fact]
    public void AddWebDriverBiDi_ActivatesListenerWhenLoggingPipelineIsBuilt()
    {
        // Regression guard for the bridge doing nothing: building the logging pipeline must activate
        // the listener (so it subscribes to the EventSource) without the application resolving
        // WebDriverBiDiEventSourceLogger itself.
        ServiceCollection services = new();
        TestLogger fakeLogger = new();
        services.AddSingleton<ILogger<WebDriverBiDiEventSourceLogger>>(fakeLogger);
        services.AddLogging(b => b.AddWebDriverBiDi(EventLevel.Verbose));
        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            // Build the logging pipeline and create a category logger (which constructs registered
            // ILoggerProviders) without ever resolving WebDriverBiDiEventSourceLogger.
            _ = provider.GetRequiredService<ILoggerFactory>().CreateLogger("activation-test");

            WebDriverBiDiEventSource.RaiseEvent.TransportStarted();
        }

        Assert.Contains(fakeLogger.Entries, e => e.EventId.Name == "TransportStarted");
    }

    [Fact]
    public void AddWebDriverBiDi_AfterProviderIsDisposed_StopsForwardingEvents()
    {
        // The listener is a container-owned singleton, so disposing the provider disposes it, and
        // EventListener.Dispose unsubscribes it from the EventSource. An event raised after that must
        // reach nothing: a listener left subscribed would go on writing through an ILogger the
        // application has already torn down. The sibling tests all raise their events inside the
        // using block, so nothing else covers the state after disposal.
        ServiceCollection services = new();
        TestLogger fakeLogger = new();
        services.AddSingleton<ILogger<WebDriverBiDiEventSourceLogger>>(fakeLogger);
        services.AddLogging(b => b.AddWebDriverBiDi(EventLevel.Verbose));

        ServiceProvider provider = services.BuildServiceProvider();
        _ = provider.GetRequiredService<ILoggerFactory>().CreateLogger("disposal-test");

        // Established first so the assertion below means "stopped forwarding" rather than
        // "never started forwarding", which would pass even if the bridge were inert.
        WebDriverBiDiEventSource.RaiseEvent.TransportStarted();
        Assert.Contains(fakeLogger.Entries, e => e.EventId.Name == "TransportStarted");

        provider.Dispose();
        fakeLogger.Clear();

        WebDriverBiDiEventSource.RaiseEvent.TransportStarted();

        Assert.Empty(fakeLogger.Entries);
    }

    [Fact]
    public void AddWebDriverBiDi_RegistersWebDriverBiDiEventSourceLogger()
    {
        ServiceCollection services = new();
        services.AddLogging(b => b.AddWebDriverBiDi());
        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            WebDriverBiDiEventSourceLogger? logger = provider.GetService<WebDriverBiDiEventSourceLogger>();
            Assert.NotNull(logger);
        }
    }

    [Fact]
    public void AddWebDriverBiDi_RegistersAsSingleton()
    {
        ServiceCollection services = new();
        services.AddLogging(b => b.AddWebDriverBiDi());
        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            WebDriverBiDiEventSourceLogger first = provider.GetRequiredService<WebDriverBiDiEventSourceLogger>();
            WebDriverBiDiEventSourceLogger second = provider.GetRequiredService<WebDriverBiDiEventSourceLogger>();
            Assert.Same(second, first);
        }
    }

    [Fact]
    public void AddWebDriverBiDi_WithMinimumLevel_RegistersLoggerWithCorrectLevel()
    {
        ServiceCollection services = new();
        TestLogger fakeLogger = new();
        services.AddSingleton<ILogger<WebDriverBiDiEventSourceLogger>>(fakeLogger);
        services.AddLogging(b => b.AddWebDriverBiDi(EventLevel.Warning));
        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            // Resolve the logger to trigger creation - it will subscribe to EventSource
            WebDriverBiDiEventSourceLogger eventSourceLogger = provider.GetRequiredService<WebDriverBiDiEventSourceLogger>();

            // Emit a Verbose event - should NOT be captured (below Warning)
            WebDriverBiDiEventSource.RaiseEvent.CommandSending(1, "session.status");

            // Emit a Warning event - should be captured
            WebDriverBiDiEventSource.RaiseEvent.CommandTimeout(1, "session.status", 5000);
        }

        // The Warning event was forwarded and the Verbose one was not. 
        Assert.Contains(fakeLogger.Entries, e => e.EventId.Name == "CommandTimeout");
        Assert.DoesNotContain(fakeLogger.Entries, e => e.EventId.Name == "CommandSending");
    }

    /// <summary>
    /// Minimal ILoggingBuilder implementation for testing.
    /// </summary>
    private sealed class LoggingBuilder : ILoggingBuilder
    {
        public LoggingBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
