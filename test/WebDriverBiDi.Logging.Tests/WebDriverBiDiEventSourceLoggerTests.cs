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
    public void Constructor_WhenLoggerIsNull_LeavesNoListenerAttached()
    {
        // The base EventListener constructor subscribes before any derived body runs, and EventListener
        // keeps every listener in a static list, so a constructor that threw after that point would leave
        // an orphan the caller never received and can never dispose, holding the source enabled at
        // LogAlways for the life of the process. A full collection does not reclaim it, because that
        // static list is a strong reference, so the only observable is the source's own state.
        WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("conn-1", "session-1", 1, "warm-up", 1);
        Assert.False(
            WebDriverBiDiEventSource.RaiseEvent.IsEnabled(EventLevel.Verbose, EventKeywords.None),
            "Another EventListener already has the WebDriverBiDi EventSource enabled at Verbose, so this test cannot measure what the throwing constructor left behind.");

        Assert.Throws<ArgumentNullException>(() => new WebDriverBiDiEventSourceLogger(null!));

        Assert.False(
            WebDriverBiDiEventSource.RaiseEvent.IsEnabled(EventLevel.Verbose, EventKeywords.None),
            "A constructor that threw left an EventListener attached to the WebDriverBiDi EventSource, which nothing can now dispose.");
    }

    [Fact]
    public void OnEventWritten_ForwardsInformationalEventToLogger()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "session-1", "ws://localhost:9222");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.Equal(LogLevel.Information, entry.LogLevel);
        Assert.Equal(1, entry.EventId.Id);
        Assert.Equal("ConnectionOpening", entry.EventId.Name);
        Assert.Equal("[conn-123/session-1] Opening connection to ws://localhost:9222", entry.Message);
    }

    [Fact]
    public void OnEventWritten_ForwardsVerboseEventAsDebugLevel()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-1", "session-1", 1, "session.status");
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
            WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("conn-1", "session-1", 1, "session.status", 5000);
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
            WebDriverBiDiEventSource.RaiseEvent.ConnectionError("conn-123", "session-1", "Socket closed");
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
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-456", "session-1", "ws://example.com");
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
    public void OnEventWritten_FormatsMessageFromEventSourceMessageTemplate()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.TransportStopped("conn-1", "session-1", "Normal shutdown");
        }

        // The template is "Transport stopped: {0}".
        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "TransportStopped");
        Assert.Equal("[conn-1/session-1] Transport stopped: Normal shutdown", entry.Message);
    }

    [Fact]
    public void OnEventWritten_AddsNamedMessageTemplateToState()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-456", "session-1", "ws://example.com");
        }

        // The EventSource template "Opening connection {0} to {1}" is carried with its holes named after
        // the payload properties, so a template-aware provider can bind each hole to a state property.
        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.Equal("[{connectionId}/{sessionId}] Opening connection to {url}", state["{OriginalFormat}"]);
        Assert.Equal("{OriginalFormat}", state.Keys.Last());
    }

    [Fact]
    public void OnEventWritten_FormatsNumericPayloadValuesInMessage()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandCompleted("conn-1", "session-1", 7, "session.status", 42);
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandCompleted");
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.Equal("[{connectionId}/{sessionId}] Command {commandId} ({method}) completed in {elapsedMilliseconds}ms", state["{OriginalFormat}"]);
        Assert.Equal("[conn-1/session-1] Command 7 (session.status) completed in 42ms", entry.Message);
    }

    [Fact]
    public void OnEventWritten_DoesNotSubstitutePayloadValuesThatLookLikeTemplateHoles()
    {
        // A payload value can carry text from the remote end. It is copied into the message as it is,
        // even when it contains the name of another hole in braces.
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionError("conn-1", "session-1", "unexpected {connectionId}");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionError");
        Assert.Equal("[conn-1/session-1] Connection error: unexpected {connectionId}", entry.Message);
    }

    [Fact]
    public void OnEventWritten_ReusesNamedMessageTemplateForRepeatedEvent()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-1", "session-1");
            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-2", "session-1");
        }

        TestLogger.LogEntry[] entries = fakeLogger.Entries.Where(e => e.EventId.Name == "ConnectionClosed").ToArray();
        Assert.Equal(2, entries.Length);
        Assert.Equal("[conn-1/session-1] Connection closed", entries[0].Message);
        Assert.Equal("[conn-2/session-1] Connection closed", entries[1].Message);

        // The second event is given the template rewritten for the first, rather than a new rewrite.
        object? firstTemplate = ((Dictionary<string, object?>)entries[0].State!)["{OriginalFormat}"];
        object? secondTemplate = ((Dictionary<string, object?>)entries[1].State!)["{OriginalFormat}"];
        Assert.Equal("[{connectionId}/{sessionId}] Connection closed", firstTemplate);
        Assert.Same(firstTemplate, secondTemplate);
    }

    [Fact]
    public void OnEventWritten_RespectsMinimumLevel_WhenSetToWarning()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Warning))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-1", "session-1", "ws://test");
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-1", "session-1", 1, "test");
            WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("conn-1", "session-1", 1, "test", 5000);
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandTimeout");
        Assert.Equal("CommandTimeout", entry.EventId.Name);

        // Events below the configured minimum are dropped, regardless of whether the EventSource
        // already existed when the listener was constructed.
        Assert.DoesNotContain(fakeLogger.Entries, e => e.EventId.Name == "ConnectionOpening");
        Assert.DoesNotContain(fakeLogger.Entries, e => e.EventId.Name == "CommandSending");
    }

    [Fact]
    public void OnEventWritten_DropsEventAboveMinimumLevel_WhenDeliveredBeforeConstructorSubscribes()
    {
        // Regression test for the base-constructor ordering window: OnEventSourceCreated (run during
        // the base EventListener constructor) enables an already-existing WebDriverBiDi EventSource at
        // the default EventLevel.LogAlways, because this instance's minimum level is not assigned until
        // the constructor body runs. The constructor re-subscribes at the configured level, but an event
        // written by another thread inside that window is still delivered at the default level, so
        // OnEventWritten must enforce the configured minimum itself. Capture a real Verbose event args
        // and invoke OnEventWritten directly on a Warning-level listener to reproduce that delivery:
        // the normal path cannot, because the constructor has by then lowered the source's own level.
        EventWrittenEventArgs? capturedArgs = null;
        using (WebDriverBiDiArgsCapturingListener capture = new(args => capturedArgs ??= args))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-1", "session-1", 1, "session.status");
        }

        Assert.NotNull(capturedArgs);
        Assert.Equal(EventLevel.Verbose, capturedArgs!.Level);

        TestLogger fakeLogger = new();
        using WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Warning);

        MethodInfo onEventWritten = typeof(WebDriverBiDiEventSourceLogger).GetMethod(
            "OnEventWritten",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        onEventWritten.Invoke(eventSourceLogger, new object[] { capturedArgs! });

        Assert.Empty(fakeLogger.Entries);
    }

    [Fact]
    public void Constructor_LowersEventSourceLevel_WhenEventSourceAlreadyExists()
    {
        // Touch the source first so it is guaranteed to pre-exist, which is the ordering that
        // previously left it enabled at LogAlways: OnEventSourceCreated runs from the base
        // EventListener constructor, before the configured level has been assigned.
        WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("conn-1", "session-1", 1, "warm-up", 1);

        // IsEnabled answers for the source across every EventListener in the process, not for this one
        // alone, so it measures what this listener subscribed at only while nothing else has the source
        // enabled above Warning. Establishing that first means a process where something else is
        // listening fails here, saying so, rather than at the assertion below, where the same reading
        // would look like the constructor having failed to lower the level.
        Assert.False(
            WebDriverBiDiEventSource.RaiseEvent.IsEnabled(EventLevel.Verbose, EventKeywords.None),
            "Another EventListener already has the WebDriverBiDi EventSource enabled at Verbose, so this test cannot measure the level its own listener subscribes at.");

        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Warning))
        {
            // The listener discards sub-Warning events either way; what matters here is that the source
            // itself no longer reports them as enabled, so they are never formatted or dispatched at all.
            Assert.False(
                WebDriverBiDiEventSource.RaiseEvent.IsEnabled(EventLevel.Verbose, EventKeywords.None),
                "The constructor left the EventSource enabled at Verbose for a listener configured at Warning, so the re-subscription that lowers the level did not take effect.");
            Assert.True(
                WebDriverBiDiEventSource.RaiseEvent.IsEnabled(EventLevel.Warning, EventKeywords.None),
                "The constructor left the EventSource disabled at the level the listener was configured for.");
        }
    }

    [Fact]
    public void OnEventWritten_RespectsMinimumLevel_WhenSetToInformational()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Informational))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-1", "session-1", 1, "test");
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-1", "session-1", "ws://test");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionOpening");
        Assert.Equal("ConnectionOpening", entry.EventId.Name);

        // The verbose CommandSending event is below the Informational minimum and is dropped.
        Assert.DoesNotContain(fakeLogger.Entries, e => e.EventId.Name == "CommandSending");
    }

    [Fact]
    public void OnEventWritten_CapturesAllLevels_WhenMinimumLevelIsLogAlways()
    {
        // EventLevel.LogAlways means "no level filtering": every event, including verbose ones, is
        // captured.
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.LogAlways))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-1", "session-1", 1, "test");
        }

        Assert.Contains(fakeLogger.Entries, e => e.EventId.Name == "CommandSending");
    }

    [Fact]
    public void OnEventWritten_HandlesEventWithMultiplePayloadProperties()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.CommandError("conn-1", "session-1", 1, "session.status", ErrorCode.InvalidSessionId, "invalid session id", "Session not found");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "CommandError");
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.Equal("1", state["commandId"]);
        Assert.Equal("session.status", state["method"]);
        Assert.Equal("invalid session id", state["errorType"]);
        Assert.Equal("Session not found", state["errorMessage"]);
    }

    [Fact]
    public void OnEventWritten_WhenEventCarriesNoPayload_ForwardsEventWithoutPayloadProperties()
    {
        // EventSource.Write raises a self-describing (TraceLogging) event rather than a
        // manifest event, which allows us to test the case where the Payload and PayloadNames
        // properties are both null. Note carefully that we must use the real singleton even
        // source here, as a second EventSource sharing the same name will fail to emit
        // on Windows.
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.Write("PayloadlessEvent");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "PayloadlessEvent");
        Assert.Equal("PayloadlessEvent", entry.EventId.Name);

        // A self-describing event declares no message template, so the message is composed from its name.
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.False(state.ContainsKey("{OriginalFormat}"));
        Assert.Equal("PayloadlessEvent", entry.Message);
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
    public void OnEventWritten_WhenTargetLoggerCategoryDisabled_DropsEventWithoutBuildingState()
    {
        // The listener subscribes to the EventSource at EventLevel.Verbose, but the target ILogger
        // may still have this category filtered off. OnEventWritten must consult IsEnabled and drop
        // the event before building any per-event state, so a disabled logger records nothing.
        TestLogger fakeLogger = new()
        {
            Enabled = false
        };
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "session-1", "ws://localhost:9222");
        }

        Assert.Empty(fakeLogger.Entries);
    }

    [Fact]
    public void OnEventWritten_WhenLoggerFieldNotYetAssigned_DropsEventWithoutThrowing()
    {
        // Regression test for the base-constructor ordering window: OnEventSourceCreated (run during
        // the base EventListener constructor) can enable an already-existing WebDriverBiDi EventSource
        // before this instance's constructor body assigns the logger field, so an event delivered in
        // that window must be dropped rather than dereferencing a null field. Capture a real
        // WebDriverBiDi event args, clear the logger field to simulate that window, then invoke
        // OnEventWritten directly: without the guard the null dereference surfaces as a
        // TargetInvocationException; with it, the call returns and nothing is logged.
        EventWrittenEventArgs? capturedArgs = null;
        using (WebDriverBiDiArgsCapturingListener capture = new(args => capturedArgs ??= args))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "session-1", "ws://localhost:9222");
        }

        Assert.NotNull(capturedArgs);

        TestLogger fakeLogger = new();
        using WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose);

        FieldInfo loggerField = typeof(WebDriverBiDiEventSourceLogger).GetField(
            "logger",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        loggerField.SetValue(eventSourceLogger, null);

        MethodInfo onEventWritten = typeof(WebDriverBiDiEventSourceLogger).GetMethod(
            "OnEventWritten",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        onEventWritten.Invoke(eventSourceLogger, new object[] { capturedArgs! });

        Assert.Empty(fakeLogger.Entries);
    }

    [Fact]
    public void OnEventWritten_HandlesEventWithNullPayload()
    {
        TestLogger fakeLogger = new();
        using (WebDriverBiDiEventSourceLogger eventSourceLogger = new(fakeLogger, EventLevel.Verbose))
        {
            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-123", "session-1");
        }

        TestLogger.LogEntry entry = GetLastEntryForEvent(fakeLogger, "ConnectionClosed");
        Assert.IsType<Dictionary<string, object?>>(entry.State);
        Dictionary<string, object?> state = (Dictionary<string, object?>)entry.State!;
        Assert.Equal(4, state["EventId"]);
        Assert.Equal("ConnectionClosed", state["EventName"]);
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

    [Fact]
    public void FormatMessage_CopiesTemplateHoleWithoutMatchingStateEntryLiterally()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventName"] = "TestEvent",
            ["present"] = "value",
            ["{OriginalFormat}"] = "Missing {missing}, present {present}",
        };

        string result = InvokeFormatMessage(state, null);
        Assert.Equal("Missing {missing}, present value", result);
    }

    [Fact]
    public void FormatMessage_CopiesUnterminatedTemplateBraceLiterally()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventName"] = "TestEvent",
            ["present"] = "value",
            ["{OriginalFormat}"] = "Present {present}, then {present",
        };

        string result = InvokeFormatMessage(state, null);
        Assert.Equal("Present value, then {present", result);
    }

    [Fact]
    public void FormatMessage_RendersNullTemplateValueAsNullMarker()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventName"] = "TestEvent",
            ["present"] = null,
            ["{OriginalFormat}"] = "Value {present}",
        };

        string result = InvokeFormatMessage(state, null);
        Assert.Equal("Value (null)", result);
    }

    [Fact]
    public void FormatMessage_ComposesFromEventNameWhenOriginalFormatIsNotString()
    {
        Dictionary<string, object?> state = new()
        {
            ["EventName"] = "TestEvent",
            ["Key1"] = "value1",
            ["{OriginalFormat}"] = 123,
        };

        string result = InvokeFormatMessage(state, null);
        Assert.StartsWith("TestEvent, Key1=value1", result);
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

    private sealed class WebDriverBiDiArgsCapturingListener : EventListener
    {
        private readonly Action<EventWrittenEventArgs> onEvent;

        public WebDriverBiDiArgsCapturingListener(Action<EventWrittenEventArgs> onEvent)
        {
            this.onEvent = onEvent;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "WebDriverBiDi")
            {
                this.EnableEvents(eventSource, EventLevel.Verbose);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource.Name == "WebDriverBiDi")
            {
                this.onEvent(eventData);
            }
        }
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
