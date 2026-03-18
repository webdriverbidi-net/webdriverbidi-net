namespace WebDriverBiDi.Logging;

using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using WebDriverBiDi;
using WebDriverBiDi.Logging.TestUtilities;

[TestFixture]
[NonParallelizable]
public class WebDriverBiDiEventSourceLoggerTests
{
    private static TestLogger.LogEntry GetLastEntryForEvent(TestLogger logger, string eventName)
    {
        return logger.Entries.Where(e => e.EventId.Name == eventName).Last();
    }

    [Test]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        Assert.That(() => new WebDriverBiDiEventSourceLogger(null!), Throws.ArgumentNullException);
    }

    [Test]
    public void OnEventWritten_ForwardsInformationalEventToLogger()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "ws://localhost:9222");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.That(entry.LogLevel, Is.EqualTo(LogLevel.Information));
        Assert.That(entry.EventId.Id, Is.EqualTo(1));
        Assert.That(entry.EventId.Name, Is.EqualTo("ConnectionOpening"));
        Assert.That(entry.Message, Does.Contain("ConnectionOpening"));
        Assert.That(entry.Message, Does.Contain("conn-123"));
        Assert.That(entry.Message, Does.Contain("ws://localhost:9222"));
    }

    [Test]
    public void OnEventWritten_ForwardsVerboseEventAsDebugLevel()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "session.status");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandSending");
        Assert.That(entry.LogLevel, Is.EqualTo(LogLevel.Debug));
        Assert.That(entry.EventId.Name, Is.EqualTo("CommandSending"));
    }

    [Test]
    public void OnEventWritten_ForwardsWarningEventAsWarningLevel()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("1", "session.status", 5000);
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandTimeout");
        Assert.That(entry.LogLevel, Is.EqualTo(LogLevel.Warning));
        Assert.That(entry.EventId.Name, Is.EqualTo("CommandTimeout"));
    }

    [Test]
    public void OnEventWritten_ForwardsErrorEventAsErrorLevel()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionError("conn-123", "Socket closed");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionError");
        Assert.That(entry.LogLevel, Is.EqualTo(LogLevel.Error));
        Assert.That(entry.EventId.Name, Is.EqualTo("ConnectionError"));
    }

    [Test]
    public void OnEventWritten_IncludesPayloadInState()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-456", "ws://example.com");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.That(entry.State, Is.InstanceOf<Dictionary<string, object?>>());

        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.That(state["EventId"], Is.EqualTo(1));
        Assert.That(state["EventName"], Is.EqualTo("ConnectionOpening"));
        Assert.That(state["EventSource"], Is.EqualTo("WebDriverBiDi"));
        Assert.That(state["connectionId"], Is.EqualTo("conn-456"));
        Assert.That(state["url"], Is.EqualTo("ws://example.com"));
    }

    [Test]
    public void OnEventWritten_FormatsMessageWithEventNameAndPayload()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.TransportStopped("Normal shutdown");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "TransportStopped");
        Assert.That(entry.Message, Does.StartWith("TransportStopped"));
        Assert.That(entry.Message, Does.Contain("Normal shutdown"));
    }

    [Test]
    public void OnEventWritten_RespectsMinimumLevel_WhenSetToWarning()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Warning))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-1", "ws://test");
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "test");
            WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("1", "test", 5000);
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandTimeout");
        Assert.That(entry.EventId.Name, Is.EqualTo("CommandTimeout"));
    }

    [Test]
    public void OnEventWritten_RespectsMinimumLevel_WhenSetToInformational()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Informational))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "test");
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-1", "ws://test");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.That(entry.EventId.Name, Is.EqualTo("ConnectionOpening"));
    }

    [Test]
    public void OnEventWritten_HandlesEventWithMultiplePayloadProperties()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandError("1", "session.status", ErrorCode.InvalidSessionId, "invalid session id", "Session not found");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandError");
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.That(state["commandId"], Is.EqualTo("1"));
        Assert.That(state["method"], Is.EqualTo("session.status"));
        Assert.That(state["errorType"], Is.EqualTo("invalid session id"));
        Assert.That(state["errorMessage"], Is.EqualTo("Session not found"));
    }

    [Test]
    public void MapEventLevel_MapsCriticalToLogLevelCritical()
    {
        LogLevel result = InvokeMapEventLevel(EventLevel.Critical);
        Assert.That(result, Is.EqualTo(LogLevel.Critical));
    }

    [Test]
    public void MapEventLevel_MapsLogAlwaysToLogLevelInformation()
    {
        LogLevel result = InvokeMapEventLevel(EventLevel.LogAlways);
        Assert.That(result, Is.EqualTo(LogLevel.Information));
    }

    [Test]
    public void MapEventLevel_MapsUnknownLevelToLogLevelTrace()
    {
        LogLevel result = InvokeMapEventLevel((EventLevel)99);
        Assert.That(result, Is.EqualTo(LogLevel.Trace));
    }

    [Test]
    public void FormatMessage_ReturnsFallbackWhenEventNameMissing()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventId"] = 1,
            ["EventSource"] = "WebDriverBiDi",
        };

        string result = InvokeFormatMessage(state, null);
        Assert.That(result, Is.EqualTo("WebDriverBiDi event"));
    }

    [Test]
    public void FormatMessage_ReturnsFallbackWhenEventNameIsNotString()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventId"] = 1,
            ["EventName"] = 123,
            ["EventSource"] = "WebDriverBiDi",
        };

        string result = InvokeFormatMessage(state, null);
        Assert.That(result, Is.EqualTo("WebDriverBiDi event"));
    }

    [Test]
    public void FormatMessage_SkipsNullPayloadValues()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventId"] = 1,
            ["EventName"] = "TestEvent",
            ["EventSource"] = "WebDriverBiDi",
            ["Key1"] = "value1",
            ["Key2"] = null,
        };

        string result = InvokeFormatMessage(state, null);
        Assert.That(result, Does.Contain("TestEvent"));
        Assert.That(result, Does.Contain("Key1=value1"));
        Assert.That(result, Does.Not.Contain("Key2"));
    }

    private static LogLevel InvokeMapEventLevel(EventLevel level)
    {
        MethodInfo method = typeof(WebDriverBiDiEventSourceLogger).GetMethod(
            "MapEventLevel",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        return (LogLevel)method.Invoke(null, new object[] { level })!;
    }

    private static string InvokeFormatMessage(Dictionary<string, object?> state, Exception? exception)
    {
        MethodInfo method = typeof(WebDriverBiDiEventSourceLogger).GetMethod(
            "FormatMessage",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        return (string)method.Invoke(null, new object?[] { state, exception })!;
    }

    [Test]
    public void OnEventWritten_IgnoresEventsFromNonWebDriverBiDiEventSource()
    {
        TestLogger fakeLogger = new();
        using WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose);

        EventWrittenEventArgs? capturedArgs = null;
        using (TestEventListenerForOtherSource listener = new(TestEventSource.Log, args => capturedArgs = args))
        {
            TestEventSource.Log.EmitTestEvent();
        }

        Assert.That(capturedArgs, Is.Not.Null);
        Assert.That(capturedArgs!.EventSource.Name, Is.Not.EqualTo("WebDriverBiDi"));

        MethodInfo onEventWritten = typeof(WebDriverBiDiEventSourceLogger).GetMethod(
            "OnEventWritten",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        onEventWritten.Invoke(eventSourceLogger, new object[] { capturedArgs });

        Assert.That(fakeLogger.Entries, Is.Empty);
    }

    [Test]
    public void OnEventWritten_HandlesEventWithNullPayload()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-123");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionClosed");
        Assert.That(entry.State, Is.InstanceOf<Dictionary<string, object?>>());
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.That(state["EventId"], Is.EqualTo(4));
        Assert.That(state["EventName"], Is.EqualTo("ConnectionClosed"));
    }

    [EventSource(Name = "TestWebDriverBiDiLogger")]
    private sealed class TestEventSource : EventSource
    {
        public static readonly TestEventSource Log = new();

        private TestEventSource()
        {
        }

        [Event(1, Level = EventLevel.Informational)]
        public void TestEvent()
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(1);
            }
        }

        public void EmitTestEvent() => this.TestEvent();
    }


    private sealed class TestEventListenerForOtherSource : EventListener
    {
        private readonly EventSource eventSource;
        private readonly Action<EventWrittenEventArgs> onEvent;

        public TestEventListenerForOtherSource(EventSource eventSource, Action<EventWrittenEventArgs> onEvent)
        {
            this.eventSource = eventSource;
            this.onEvent = onEvent;
            this.EnableEvents(eventSource, EventLevel.Verbose);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource == this.eventSource)
            {
                this.onEvent(eventData);
            }
        }
    }
}
