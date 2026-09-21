namespace WebDriverBiDi;

using System.Diagnostics.Tracing;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

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
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "session-abc", "ws://localhost:9222");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(1, evt.EventId);
        Assert.Equal("ConnectionOpening", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(3, evt.Payload.Count);
        Assert.Equal("conn-123", evt.Payload[0]);
        Assert.Equal("session-abc", evt.Payload[1]);
        Assert.Equal("ws://localhost:9222", evt.Payload[2]);
    }

    [Fact]
    public void TestConnectionOpenedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpened("conn-123", "session-abc", "ws://localhost:9222");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(2, evt.EventId);
        Assert.Equal("ConnectionOpened", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
    }

    [Fact]
    public void TestConnectionClosingEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionClosing("conn-123", "session-abc", "Normal shutdown");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(3, evt.EventId);
        Assert.Equal("ConnectionClosing", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Normal shutdown", evt.Payload[2]);
    }

    [Fact]
    public void TestConnectionClosedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-123", "session-abc");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(4, evt.EventId);
        Assert.Equal("ConnectionClosed", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
    }

    [Fact]
    public void TestConnectionErrorEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError("conn-123", "session-abc", "Socket closed unexpectedly");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(5, evt.EventId);
        Assert.Equal("ConnectionError", evt.EventName);
        Assert.Equal(EventLevel.Error, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Socket closed unexpectedly", evt.Payload[2]);
    }

    [Fact]
    public void TestCommandSendingEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-123", "session-abc", 1, "session.status");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(6, evt.EventId);
        Assert.Equal("CommandSending", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("1", evt.Payload[2]);
        Assert.Equal("session.status", evt.Payload[3]);
    }

    [Fact]
    public void TestCommandCompletedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandCompleted("conn-123", "session-abc", 1, "session.status", 42);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(7, evt.EventId);
        Assert.Equal("CommandCompleted", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("1", evt.Payload[2]);
        Assert.Equal("session.status", evt.Payload[3]);
        Assert.Equal(42L, evt.Payload[4]);
    }

    [Fact]
    public void TestCommandTimeoutEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("conn-123", "session-abc", 1, "session.status", 5000);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(8, evt.EventId);
        Assert.Equal("CommandTimeout", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(5000L, evt.Payload[4]);
    }

    [Fact]
    public void TestCommandErrorEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandError("conn-123", "session-abc", 1, "session.status", ErrorCode.InvalidSessionId, "invalid session id", "Session not found");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(9, evt.EventId);
        Assert.Equal("CommandError", evt.EventName);
        Assert.Equal(EventLevel.Error, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("InvalidSessionId", evt.Payload[4]);
        Assert.Equal("invalid session id", evt.Payload[5]);
        Assert.Equal("Session not found", evt.Payload[6]);
    }

    [Fact]
    public void TestEventReceivedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("conn-123", "session-abc", "browsingContext.navigationStarted");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(10, evt.EventId);
        Assert.Equal("EventReceived", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("browsingContext.navigationStarted", evt.Payload[2]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived("conn-123", "session-abc", IncomingMessageKind.Unknown, 256);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("unknown", evt.Payload[2]);
        Assert.Equal(256, evt.Payload[3]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmittedWithSuccessCommandResponseMessage()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived("conn-123", "session-abc", IncomingMessageKind.CommandResponse, 256);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("success", evt.Payload[2]);
        Assert.Equal(256, evt.Payload[3]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmittedWithCommandErrorMessage()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived("conn-123", "session-abc", IncomingMessageKind.ErrorResponse, 256);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("error", evt.Payload[2]);
        Assert.Equal(256, evt.Payload[3]);
    }

    [Fact]
    public void TestUnknownMessageReceivedEventEmittedWithEventMessage()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived("conn-123", "session-abc", IncomingMessageKind.Event, 256);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(13, evt.EventId);
        Assert.Equal("UnknownMessageReceived", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("event", evt.Payload[2]);
        Assert.Equal(256, evt.Payload[3]);
    }

    [Fact]
    public void TestProtocolErrorEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ProtocolError("conn-123", "session-abc", "Invalid JSON", "{\"invalid");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(14, evt.EventId);
        Assert.Equal("ProtocolError", evt.EventName);
        Assert.Equal(EventLevel.Error, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Invalid JSON", evt.Payload[2]);
        Assert.Equal("{\"invalid", evt.Payload[3]);
    }

    [Fact]
    public void TestEventHandlerErrorEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.EventHandlerError("conn-123", "session-abc", "log.entryAdded", "NullReferenceException");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(15, evt.EventId);
        Assert.Equal("EventHandlerError", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("log.entryAdded", evt.Payload[2]);
        Assert.Equal("NullReferenceException", evt.Payload[3]);
    }

    [Fact]
    public void TestPendingCommandCountEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.PendingCommandCount("conn-123", "session-abc", 5);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(16, evt.EventId);
        Assert.Equal("PendingCommandCount", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(5, evt.Payload[2]);
    }

    [Fact]
    public void TestTransportStartedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.TransportStarted("conn-123", "session-abc");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(17, evt.EventId);
        Assert.Equal("TransportStarted", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
    }

    [Fact]
    public void TestTransportStoppedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.TransportStopped("conn-123", "session-abc", "Normal shutdown");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(18, evt.EventId);
        Assert.Equal("TransportStopped", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("Normal shutdown", evt.Payload[2]);
    }

    [Fact]
    public void TestCustomModuleRegisteredEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CustomModuleRegistered("conn-123", "session-abc", "myModule");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(19, evt.EventId);
        Assert.Equal("CustomModuleRegistered", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("myModule", evt.Payload[2]);
    }

    [Fact]
    public void TestCustomEventRegisteredEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CustomEventRegistered("conn-123", "session-abc", "myModule.myEvent", "MyEventType");
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(20, evt.EventId);
        Assert.Equal("CustomEventRegistered", evt.EventName);
        Assert.Equal(EventLevel.Informational, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("myModule.myEvent", evt.Payload[2]);
        Assert.Equal("MyEventType", evt.Payload[3]);
    }

    [Fact]
    public void TestMessageStatisticsEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.MessageStatistics("conn-123", "session-abc", 100, 95, 80, 5);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(21, evt.EventId);
        Assert.Equal("MessageStatistics", evt.EventName);
        Assert.Equal(EventLevel.Verbose, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal(100L, evt.Payload[2]);
        Assert.Equal(95L, evt.Payload[3]);
        Assert.Equal(80L, evt.Payload[4]);
        Assert.Equal(5L, evt.Payload[5]);
    }

    [Fact]
    public void TestCommandSendFailedEventEmitted()
    {
        using TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandSendFailed("conn-123", "session-abc", 1, "session.status", "System.InvalidOperationException", "Simulated send failure", 12);
        IReadOnlyList<EventWrittenEventArgs> events = listener.Events;

        EventWrittenEventArgs evt = Assert.Single(events);

        Assert.Equal(22, evt.EventId);
        Assert.Equal("CommandSendFailed", evt.EventName);
        Assert.Equal(EventLevel.Warning, evt.Level);
        Assert.NotNull(evt.Payload);
        Assert.Equal("1", evt.Payload[2]);
        Assert.Equal("session.status", evt.Payload[3]);
        Assert.Equal("System.InvalidOperationException", evt.Payload[4]);
        Assert.Equal("Simulated send failure", evt.Payload[5]);
        Assert.Equal(12L, evt.Payload[6]);
    }

    [Fact]
    public void TestEventSourceDoesNotEmitWhenDisabled()
    {
        // No listener is created here, and every other test in this collection disposes its
        // listener before completing, so the source must be disabled: the guard the library's
        // hot paths rely on to skip payload formatting. Emitting while disabled must also be
        // a no-op rather than an error.
        Assert.False(WebDriverBiDiEventSource.RaiseEvent.IsEnabled());

        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("test", "session", "ws://test");
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-123", "session-abc", 1, "test");
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("conn-123", "session-abc", "test.event");

        Assert.False(WebDriverBiDiEventSource.RaiseEvent.IsEnabled());
    }

    [Fact]
    public void TestEventSourceRespectEventLevel()
    {
        using TestEventListener listener = new(EventLevel.Warning);

        // These should not be captured (below Warning level)
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("test-respect-level", "session-abc", "ws://test-respect"); // Informational
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("conn-respect-level", "session-respect-level", 9999, "test.respect.level"); // Verbose

        // These should be captured (Warning and above)
        WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("conn-respect-level", "session-respect-level", 9999, "test.respect.level", 5000); // Warning
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError("test-respect-level", "session-respect-level", "error-respect-level"); // Error

        // Should have captured at least the Warning and Error events we just emitted
        // Filter to events with our unique identifiers to avoid test interference
        List<EventWrittenEventArgs> relevantEvents = listener.Events
            .Where(e => (e.EventName == "CommandTimeout" && e.Payload?[3]?.ToString() == "test.respect.level") ||
                       (e.EventName == "ConnectionError" && e.Payload?[2]?.ToString() == "error-respect-level"))
            .ToList();

        Assert.Equal(2, relevantEvents.Count);

        Assert.Equal("CommandTimeout", relevantEvents[0].EventName);
        Assert.Equal("ConnectionError", relevantEvents[1].EventName);

        // The assertions above are all about the events that were captured, so on their own they hold
        // whether or not the level was respected: filtering to the two expected events and then
        // counting two cannot detect the other two also arriving. These are the assertions the test's
        // name promises. Each is scoped by the unique payload this test emits, so a concurrently
        // running test raising the same events cannot make them pass or fail.
        Assert.DoesNotContain(
            listener.Events,
            e => e.EventName == "ConnectionOpening" && e.Payload?[0]?.ToString() == "test-respect-level");
        Assert.DoesNotContain(
            listener.Events,
            e => e.EventName == "CommandSending" && e.Payload?[3]?.ToString() == "test.respect.level");
    }

}
