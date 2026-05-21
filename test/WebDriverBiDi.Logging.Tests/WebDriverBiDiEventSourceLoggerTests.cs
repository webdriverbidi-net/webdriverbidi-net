namespace WebDriverBiDi.Logging;

using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using WebDriverBiDi;
using WebDriverBiDi.Logging.TestUtilities;

[Collection("NonParallel")]
public class WebDriverBiDiEventSourceLoggerTests
{
    private static TestLogger.LogEntry GetLastEntryForEvent(TestLogger logger, string eventName)
    {
        return logger.Entries.Last(e => e.EventId.Name == eventName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebDriverBiDiEventSourceLogger(null!));
    }

    [Fact]
    public void OnEventWritten_ForwardsInformationalEventToLogger()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "ws://localhost:9222");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.Equal(LogLevel.Information, entry.LogLevel);
        Assert.Equal(1, entry.EventId.Id);
        Assert.Equal("ConnectionOpening", entry.EventId.Name);
        Assert.Contains("ConnectionOpening", entry.Message);
        Assert.Contains("conn-123", entry.Message);
        Assert.Contains("ws://localhost:9222", entry.Message);
    }

    [Fact]
    public void OnEventWritten_ForwardsVerboseEventAsDebugLevel()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "session.status");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandSending");
        Assert.Equal(LogLevel.Debug, entry.LogLevel);
        Assert.Equal("CommandSending", entry.EventId.Name);
    }

    [Fact]
    public void OnEventWritten_ForwardsWarningEventAsWarningLevel()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("1", "session.status", 5000);
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandTimeout");
        Assert.Equal(LogLevel.Warning, entry.LogLevel);
        Assert.Equal("CommandTimeout", entry.EventId.Name);
    }

    [Fact]
    public void OnEventWritten_ForwardsErrorEventAsErrorLevel()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionError("conn-123", "Socket closed");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionError");
        Assert.Equal(LogLevel.Error, entry.LogLevel);
        Assert.Equal("ConnectionError", entry.EventId.Name);
    }

    [Fact]
    public void OnEventWritten_IncludesPayloadInState()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-456", "ws://example.com");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.IsType<Dictionary<string, object?>>(entry.State);

        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.Equal(1, state["EventId"]);
        Assert.Equal("ConnectionOpening", state["EventName"]);
        Assert.Equal("WebDriverBiDi", state["EventSource"]);
        Assert.Equal("conn-456", state["connectionId"]);
        Assert.Equal("ws://example.com", state["url"]);
    }

    [Fact]
    public void OnEventWritten_FormatsMessageWithEventNameAndPayload()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.TransportStopped("Normal shutdown");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "TransportStopped");
        Assert.StartsWith("TransportStopped", entry.Message);
        Assert.Contains("Normal shutdown", entry.Message);
    }

    [Fact]
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
        Assert.Equal("CommandTimeout", entry.EventId.Name);
    }

    [Fact]
    public void OnEventWritten_RespectsMinimumLevel_WhenSetToInformational()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Informational))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "test");
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-1", "ws://test");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.Equal("ConnectionOpening", entry.EventId.Name);
    }

    [Fact]
    public void OnEventWritten_HandlesEventWithMultiplePayloadProperties()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandError("1", "session.status", ErrorCode.InvalidSessionId, "invalid session id", "Session not found");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandError");
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.Equal("1", state["commandId"]);
        Assert.Equal("session.status", state["method"]);
        Assert.Equal("invalid session id", state["errorType"]);
        Assert.Equal("Session not found", state["errorMessage"]);
    }

    [Fact]
    public void MapEventLevel_MapsCriticalToLogLevelCritical()
    {
        LogLevel result = InvokeMapEventLevel(EventLevel.Critical);
        Assert.Equal(LogLevel.Critical, result);
    }

    [Fact]
    public void MapEventLevel_MapsLogAlwaysToLogLevelInformation()
    {
        LogLevel result = InvokeMapEventLevel(EventLevel.LogAlways);
        Assert.Equal(LogLevel.Information, result);
    }

    [Fact]
    public void MapEventLevel_MapsUnknownLevelToLogLevelTrace()
    {
        LogLevel result = InvokeMapEventLevel((EventLevel)99);
        Assert.Equal(LogLevel.Trace, result);
    }

    [Fact]
    public void FormatMessage_ReturnsFallbackWhenEventNameMissing()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventId"] = 1,
            ["EventSource"] = "WebDriverBiDi",
        };

        string result = InvokeFormatMessage(state, null);
        Assert.Equal("WebDriverBiDi event", result);
    }

    [Fact]
    public void FormatMessage_ReturnsFallbackWhenEventNameIsNotString()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventId"] = 1,
            ["EventName"] = 123,
            ["EventSource"] = "WebDriverBiDi",
        };

        string result = InvokeFormatMessage(state, null);
        Assert.Equal("WebDriverBiDi event", result);
    }

    [Fact]
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
        Assert.Contains("TestEvent", result);
        Assert.Contains("Key1=value1", result);
        Assert.DoesNotContain("Key2", result);
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

    [Fact]
    public void OnEventWritten_IgnoresEventsFromNonWebDriverBiDiEventSource()
    {
        TestLogger fakeLogger = new();
        using WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose);

        EventWrittenEventArgs? capturedArgs = null;
        using (TestEventListenerForOtherSource listener = new(TestEventSource.Log, args => capturedArgs = args))
        {
            TestEventSource.Log.EmitTestEvent();
        }

        Assert.NotNull(capturedArgs);
        Assert.NotEqual("WebDriverBiDi", capturedArgs!.EventSource.Name);

        MethodInfo onEventWritten = typeof(WebDriverBiDiEventSourceLogger).GetMethod(
            "OnEventWritten",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        onEventWritten.Invoke(eventSourceLogger, new object[] { capturedArgs });

        Assert.Empty(fakeLogger.Entries);
    }

    [Fact]
    public void OnEventWritten_HandlesEventWithNullPayload()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-123");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionClosed");
        Assert.IsType<Dictionary<string, object?>>(entry.State);
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.Equal(4, state["EventId"]);
        Assert.Equal("ConnectionClosed", state["EventName"]);
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
