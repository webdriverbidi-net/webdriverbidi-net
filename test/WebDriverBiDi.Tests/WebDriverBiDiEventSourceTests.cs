namespace WebDriverBiDi;

using System.Diagnostics.Tracing;
using WebDriverBiDi.Protocol;

/// <summary>
/// Tests for WebDriverBiDiEventSource to ensure events are emitted correctly
/// and maintain 100% code coverage.
/// </summary>
[Collection("EventSourceTests")]
public class WebDriverBiDiEventSourceTests
{
    [Fact]
    public void TestEventSourceExists()
    {
        Assert.NotNull(WebDriverBiDiEventSource.RaiseEvent);
    }

    [Fact]
    public void TestEventSourceName()
    {
        Assert.Equal("WebDriverBiDi", WebDriverBiDiEventSource.RaiseEvent.Name);
    }

    [Fact]
    public void TestConnectionOpeningEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "ws://localhost:9222");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(1, evt.EventId);
        Assert.Equal("ConnectionOpening", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(2, evt.Payload.Count);
        Assert.Equal("conn-123", evt.Payload[0]);
        Assert.Equal("ws://localhost:9222", evt.Payload[1]);
    }

    [Fact]
    public void TestConnectionOpenedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpened("conn-123", "ws://localhost:9222");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(2, evt.EventId);
        Assert.Equal("ConnectionOpened", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
    }

    [Fact]
    public void TestConnectionClosingEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionClosing("conn-123", "Normal shutdown");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(3, evt.EventId);
        Assert.Equal("ConnectionClosing", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Normal shutdown", evt.Payload[1]);
    }

    [Fact]
    public void TestConnectionClosedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-123");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(4, evt.EventId);
        Assert.Equal("ConnectionClosed", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
    }

    [Fact]
    public void TestConnectionErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError("conn-123", "Socket closed unexpectedly");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(5, evt.EventId);
        Assert.Equal("ConnectionError", evt.EventName);
        Assert.Equal(EventLevel.Error, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Socket closed unexpectedly", evt.Payload[1]);
    }

    [Fact]
    public void TestCommandSendingEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "session.status");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(6, evt.EventId);
        Assert.Equal("CommandSending", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("1", evt.Payload[0]);
        Assert.Equal("session.status", evt.Payload[1]);
    }

    [Fact]
    public void TestCommandCompletedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandCompleted("1", "session.status", 42);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(7, evt.EventId);
        Assert.Equal("CommandCompleted", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("1", evt.Payload[0]);
        Assert.Equal("session.status", evt.Payload[1]);
        Assert.Equal(42L, evt.Payload[2]);
    }

    [Fact]
    public void TestCommandTimeoutEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("1", "session.status", 5000);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(8, evt.EventId);
        Assert.Equal("CommandTimeout", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(5000L, evt.Payload[2]);
    }

    [Fact]
    public void TestCommandErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandError("1", "session.status", ErrorCode.InvalidSessionId, "invalid session id", "Session not found");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(9, evt.EventId);
        Assert.Equal("CommandError", evt.EventName);
        Assert.Equal(EventLevel.Error, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(ErrorCode.InvalidSessionId, evt.Payload[2]);
        Assert.Equal("invalid session id", evt.Payload[3]);
        Assert.Equal("Session not found", evt.Payload[4]);
    }

    [Fact]
    public void TestEventReceivedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("browsingContext.navigationStarted");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(10, evt.EventId);
        Assert.Equal("EventReceived", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("browsingContext.navigationStarted", evt.Payload[0]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived(IncomingMessageKind.Unknown, 256);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("unknown", evt.Payload[0]);
        Assert.Equal(256, evt.Payload[1]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmittedWithSuccessCommandResponseMessage()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived(IncomingMessageKind.CommandResponse, 256);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("success", evt.Payload[0]);
        Assert.Equal(256, evt.Payload[1]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmittedWithCommandErrorMessage()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived(IncomingMessageKind.ErrorResponse, 256);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("error", evt.Payload[0]);
        Assert.Equal(256, evt.Payload[1]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmittedWithEventMessage()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived(IncomingMessageKind.Event, 256);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("event", evt.Payload[0]);
        Assert.Equal(256, evt.Payload[1]);
    }

    [Fact]
    public void TestProtocolErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ProtocolError("Invalid JSON", "{\"invalid");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(14, evt.EventId);
        Assert.Equal("ProtocolError", evt.EventName);
        Assert.Equal(EventLevel.Error, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Invalid JSON", evt.Payload[0]);
        Assert.Equal("{\"invalid", evt.Payload[1]);
    }

    [Fact]
    public void TestEventHandlerErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.EventHandlerError("log.entryAdded", "NullReferenceException");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(15, evt.EventId);
        Assert.Equal("EventHandlerError", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("log.entryAdded", evt.Payload[0]);
        Assert.Equal("NullReferenceException", evt.Payload[1]);
    }

    [Fact]
    public void TestPendingCommandCountEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.PendingCommandCount(5);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(16, evt.EventId);
        Assert.Equal("PendingCommandCount", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(5, evt.Payload[0]);
    }

    [Fact]
    public void TestTransportStartedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.TransportStarted();
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(17, evt.EventId);
        Assert.Equal("TransportStarted", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
    }

    [Fact]
    public void TestTransportStoppedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.TransportStopped("Normal shutdown");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(18, evt.EventId);
        Assert.Equal("TransportStopped", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Normal shutdown", evt.Payload[0]);
    }

    [Fact]
    public void TestCustomModuleRegisteredEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CustomModuleRegistered("myModule");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(19, evt.EventId);
        Assert.Equal("CustomModuleRegistered", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("myModule", evt.Payload[0]);
    }

    [Fact]
    public void TestCustomEventRegisteredEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CustomEventRegistered("myModule.myEvent", "MyEventType");
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(20, evt.EventId);
        Assert.Equal("CustomEventRegistered", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("myModule.myEvent", evt.Payload[0]);
        Assert.Equal("MyEventType", evt.Payload[1]);
    }

    [Fact]
    public void TestMessageStatisticsEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.MessageStatistics(100, 95, 80, 5);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(21, evt.EventId);
        Assert.Equal("MessageStatistics", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(100L, evt.Payload[0]);
        Assert.Equal(95L, evt.Payload[1]);
        Assert.Equal(80L, evt.Payload[2]);
        Assert.Equal(5L, evt.Payload[3]);
    }

    [Fact]
    public void TestCommandSendFailedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandSendFailed("1", "session.status", "System.InvalidOperationException", "Simulated send failure", 12);
        listener.Dispose();

        Assert.Single(listener.Events);
        EventWrittenEventArgs evt = listener.Events[0];

        Assert.Equal(22, evt.EventId);
        Assert.Equal("CommandSendFailed", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("1", evt.Payload[0]);
        Assert.Equal("session.status", evt.Payload[1]);
        Assert.Equal("System.InvalidOperationException", evt.Payload[2]);
        Assert.Equal("Simulated send failure", evt.Payload[3]);
        Assert.Equal(12L, evt.Payload[4]);
    }

    [Fact]
    public void TestEventSourceDoesNotEmitWhenDisabled()
    {
        // Don't create a listener - EventSource should be disabled by default
        // Just verify no exceptions are thrown
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("test", "ws://test");
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "test");
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("test.event");

        // If we get here without exceptions, the test passes
    }

    [Fact]
    public void TestEventSourceRespectEventLevel()
    {
        TestEventListener listener = new(EventLevel.Warning);

        // These should not be captured (below Warning level)
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("test-respect-level", "ws://test-respect"); // Informational
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("9999", "test.respect.level"); // Verbose

        // These should be captured (Warning and above)
        WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("9999", "test.respect.level", 5000); // Warning
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError("test-respect-level", "error-respect-level"); // Error

        listener.Dispose();

        // Should have captured at least the Warning and Error events we just emitted
        // Filter to events with our unique identifiers to avoid test interference
        List<EventWrittenEventArgs> relevantEvents = listener.Events
            .Where(e => (e.EventName == "CommandTimeout" && e.Payload?[1]?.ToString() == "test.respect.level") ||
                       (e.EventName == "ConnectionError" && e.Payload?[1]?.ToString() == "error-respect-level"))
            .ToList();

        Assert.Equal(2, relevantEvents.Count);

        Assert.Equal("CommandTimeout", relevantEvents[0].EventName);
        Assert.Equal("ConnectionError", relevantEvents[1].EventName);
    }

    /// <summary>
    /// Test EventListener that captures events for verification.
    /// </summary>
    private class TestEventListener : EventListener
    {
        private readonly object eventLockObject = new();
        private readonly EventLevel minimumLevel;

        public TestEventListener(EventLevel minimumLevel = EventLevel.Verbose)
        {
            this.minimumLevel = minimumLevel;
            this.Events = [];
        }

        public List<EventWrittenEventArgs> Events { get; }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "WebDriverBiDi")
            {
                this.EnableEvents(eventSource, this.minimumLevel);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (eventData.EventSource.Name == "WebDriverBiDi")
            {
                lock (this.eventLockObject)
                {
                    this.Events.Add(eventData);
                }
            }
        }
    }
}
