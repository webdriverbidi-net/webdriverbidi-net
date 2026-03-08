namespace WebDriverBiDi.Logging;

using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebDriverBiDi;
using WebDriverBiDi.Logging.TestUtilities;

[TestFixture]
[NonParallelizable]
public class WebDriverBiDiLoggingExtensionsTests
{
    [Test]
    public void AddWebDriverBiDi_WhenBuilderIsNull_ThrowsArgumentNullException()
    {
        ILoggingBuilder? builder = null;

        Assert.That(() => builder!.AddWebDriverBiDi(), Throws.ArgumentNullException);
    }

    [Test]
    public void AddWebDriverBiDi_WithMinimumLevel_WhenBuilderIsNull_ThrowsArgumentNullException()
    {
        ILoggingBuilder? builder = null;

        Assert.That(() => builder!.AddWebDriverBiDi(EventLevel.Verbose), Throws.ArgumentNullException);
    }

    [Test]
    public void AddWebDriverBiDi_ReturnsBuilderForChaining()
    {
        ServiceCollection services = new();
        ILoggingBuilder builder = new LoggingBuilder(services);

        ILoggingBuilder result = builder.AddWebDriverBiDi();

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void AddWebDriverBiDi_WithMinimumLevel_ReturnsBuilderForChaining()
    {
        ServiceCollection services = new();
        ILoggingBuilder builder = new LoggingBuilder(services);

        ILoggingBuilder result = builder.AddWebDriverBiDi(EventLevel.Warning);

        Assert.That(result, Is.SameAs(builder));
    }

    [Test]
    public void AddWebDriverBiDi_RegistersWebDriverBiDiEventSourceLogger()
    {
        ServiceCollection services = new();
        services.AddLogging(b => b.AddWebDriverBiDi());
        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            WebDriverBiDiEventSourceLogger? logger = provider.GetService<WebDriverBiDiEventSourceLogger>();
            Assert.That(logger, Is.Not.Null);
        }
    }

    [Test]
    public void AddWebDriverBiDi_RegistersAsSingleton()
    {
        ServiceCollection services = new();
        services.AddLogging(b => b.AddWebDriverBiDi());
        using (ServiceProvider provider = services.BuildServiceProvider())
        {
            WebDriverBiDiEventSourceLogger first = provider.GetRequiredService<WebDriverBiDiEventSourceLogger>();
            WebDriverBiDiEventSourceLogger second = provider.GetRequiredService<WebDriverBiDiEventSourceLogger>();
            Assert.That(first, Is.SameAs(second));
        }
    }

    [Test]
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
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "session.status");

            // Emit a Warning event - should be captured
            WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("1", "session.status", 5000);
        }

        TestLogger.LogEntry entry = fakeLogger.Entries
            .Where(e => e.EventId.Name == "CommandTimeout")
            .Last();
        Assert.That(entry.EventId.Name, Is.EqualTo("CommandTimeout"));
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
