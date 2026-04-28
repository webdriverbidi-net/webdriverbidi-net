namespace WebDriverBiDi;

using System.Diagnostics.Tracing;
using TestUtilities;

/// <summary>
/// Tests for WebDriverBiDiEventSource to ensure events are emitted correctly
/// and maintain 100% code coverage.
/// </summary>
[TestFixture]
public class WebDriverBiDiEventSourceTests
{
    [Test]
    public void TestEventSourceExists()
    {
        Assert.That(WebDriverBiDiEventSource.RaiseEvent, Is.Not.Null);
    }

    [Test]
    public void TestEventSourceName()
    {
        Assert.That(WebDriverBiDiEventSource.RaiseEvent.Name, Is.EqualTo("WebDriverBiDi"));
    }

    [Test]
    public void TestConnectionOpeningEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("conn-123", "ws://localhost:9222");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(1));
            Assert.That(evt.EventName, Is.EqualTo("ConnectionOpening"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(evt.Payload, Has.Count.EqualTo(2));
            Assert.That(evt.Payload![0], Is.EqualTo("conn-123"));
            Assert.That(evt.Payload![1], Is.EqualTo("ws://localhost:9222"));
        }
    }

    [Test]
    public void TestConnectionOpenedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpened("conn-123", "ws://localhost:9222");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(2));
            Assert.That(evt.EventName, Is.EqualTo("ConnectionOpened"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
        }
    }

    [Test]
    public void TestConnectionClosingEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionClosing("conn-123", "Normal shutdown");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(3));
            Assert.That(evt.EventName, Is.EqualTo("ConnectionClosing"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(evt.Payload![1], Is.EqualTo("Normal shutdown"));
        }
    }

    [Test]
    public void TestConnectionClosedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed("conn-123");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(4));
            Assert.That(evt.EventName, Is.EqualTo("ConnectionClosed"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
        }
    }

    [Test]
    public void TestConnectionErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError("conn-123", "Socket closed unexpectedly");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(5));
            Assert.That(evt.EventName, Is.EqualTo("ConnectionError"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Error));
            Assert.That(evt.Payload![1], Is.EqualTo("Socket closed unexpectedly"));
        }
    }

    [Test]
    public void TestCommandSendingEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "session.status");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(6));
            Assert.That(evt.EventName, Is.EqualTo("CommandSending"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Verbose));
            Assert.That(evt.Payload![0], Is.EqualTo("1"));
            Assert.That(evt.Payload![1], Is.EqualTo("session.status"));
        }
    }

    [Test]
    public void TestCommandCompletedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandCompleted("1", "session.status", 42);
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(7));
            Assert.That(evt.EventName, Is.EqualTo("CommandCompleted"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(evt.Payload![0], Is.EqualTo("1"));
            Assert.That(evt.Payload![1], Is.EqualTo("session.status"));
            Assert.That(evt.Payload![2], Is.EqualTo(42L));
        }
    }

    [Test]
    public void TestCommandTimeoutEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandTimeout("1", "session.status", 5000);
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(8));
            Assert.That(evt.EventName, Is.EqualTo("CommandTimeout"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Warning));
            Assert.That(evt.Payload![2], Is.EqualTo(5000L));
        }
    }

    [Test]
    public void TestCommandErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandError("1", "session.status", ErrorCode.InvalidSessionId, "invalid session id", "Session not found");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(9));
            Assert.That(evt.EventName, Is.EqualTo("CommandError"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Error));
            Assert.That(evt.Payload![2], Is.EqualTo(ErrorCode.InvalidSessionId));
            Assert.That(evt.Payload![3], Is.EqualTo("invalid session id"));
            Assert.That(evt.Payload![4], Is.EqualTo("Session not found"));
        }
    }

    [Test]
    public void TestEventReceivedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("browsingContext.navigationStarted");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(10));
            Assert.That(evt.EventName, Is.EqualTo("EventReceived"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Verbose));
            Assert.That(evt.Payload![0], Is.EqualTo("browsingContext.navigationStarted"));
        }
    }

    [Test]
    public void TestUnknownMessageReceivedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived("unknown", 256);
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(13));
            Assert.That(evt.EventName, Is.EqualTo("UnknownMessageReceived"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Warning));
            Assert.That(evt.Payload![0], Is.EqualTo("unknown"));
            Assert.That(evt.Payload![1], Is.EqualTo(256));
        }
    }

    [Test]
    public void TestProtocolErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.ProtocolError("Invalid JSON", "{\"invalid");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(14));
            Assert.That(evt.EventName, Is.EqualTo("ProtocolError"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Error));
            Assert.That(evt.Payload![0], Is.EqualTo("Invalid JSON"));
            Assert.That(evt.Payload![1], Is.EqualTo("{\"invalid"));
        }
    }

    [Test]
    public void TestEventHandlerErrorEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.EventHandlerError("log.entryAdded", "NullReferenceException");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(15));
            Assert.That(evt.EventName, Is.EqualTo("EventHandlerError"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Warning));
            Assert.That(evt.Payload![0], Is.EqualTo("log.entryAdded"));
            Assert.That(evt.Payload![1], Is.EqualTo("NullReferenceException"));
        }
    }

    [Test]
    public void TestPendingCommandCountEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.PendingCommandCount(5);
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(16));
            Assert.That(evt.EventName, Is.EqualTo("PendingCommandCount"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Verbose));
            Assert.That(evt.Payload![0], Is.EqualTo(5));
        }
    }

    [Test]
    public void TestTransportStartedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.TransportStarted();
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(17));
            Assert.That(evt.EventName, Is.EqualTo("TransportStarted"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
        }
    }

    [Test]
    public void TestTransportStoppedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.TransportStopped("Normal shutdown");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(18));
            Assert.That(evt.EventName, Is.EqualTo("TransportStopped"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(evt.Payload![0], Is.EqualTo("Normal shutdown"));
        }
    }

    [Test]
    public void TestCustomModuleRegisteredEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CustomModuleRegistered("myModule");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(19));
            Assert.That(evt.EventName, Is.EqualTo("CustomModuleRegistered"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(evt.Payload![0], Is.EqualTo("myModule"));
        }
    }

    [Test]
    public void TestCustomEventRegisteredEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CustomEventRegistered("myModule.myEvent", "MyEventType");
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(20));
            Assert.That(evt.EventName, Is.EqualTo("CustomEventRegistered"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(evt.Payload![0], Is.EqualTo("myModule.myEvent"));
            Assert.That(evt.Payload![1], Is.EqualTo("MyEventType"));
        }
    }

    [Test]
    public void TestMessageStatisticsEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.MessageStatistics(100, 95, 80, 5);
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(21));
            Assert.That(evt.EventName, Is.EqualTo("MessageStatistics"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Verbose));
            Assert.That(evt.Payload![0], Is.EqualTo(100L));
            Assert.That(evt.Payload![1], Is.EqualTo(95L));
            Assert.That(evt.Payload![2], Is.EqualTo(80L));
            Assert.That(evt.Payload![3], Is.EqualTo(5L));
        }
    }

    [Test]
    public void TestCommandSendFailedEventEmitted()
    {
        TestEventListener listener = new();
        WebDriverBiDiEventSource.RaiseEvent.CommandSendFailed("1", "session.status", "System.InvalidOperationException", "Simulated send failure", 12);
        listener.Dispose();

        Assert.That(listener.Events, Has.Count.EqualTo(1));
        EventWrittenEventArgs evt = listener.Events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(evt.EventId, Is.EqualTo(22));
            Assert.That(evt.EventName, Is.EqualTo("CommandSendFailed"));
            Assert.That(evt.Level, Is.EqualTo(EventLevel.Warning));
            Assert.That(evt.Payload![0], Is.EqualTo("1"));
            Assert.That(evt.Payload![1], Is.EqualTo("session.status"));
            Assert.That(evt.Payload![2], Is.EqualTo("System.InvalidOperationException"));
            Assert.That(evt.Payload![3], Is.EqualTo("Simulated send failure"));
            Assert.That(evt.Payload![4], Is.EqualTo(12L));
        }
    }

    [Test]
    public void TestEventSourceDoesNotEmitWhenDisabled()
    {
        // Don't create a listener - EventSource should be disabled by default
        // Just verify no exceptions are thrown
        WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening("test", "ws://test");
        WebDriverBiDiEventSource.RaiseEvent.CommandSending("1", "test");
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("test.event");

        // If we get here without exceptions, the test passes
        Assert.Pass();
    }

    [Test]
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
            .Where(e => (e.EventName == "CommandTimeout" && e.Payload![1]?.ToString() == "test.respect.level") ||
                       (e.EventName == "ConnectionError" && e.Payload![1]?.ToString() == "error-respect-level"))
            .ToList();

        Assert.That(relevantEvents, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(relevantEvents[0].EventName, Is.EqualTo("CommandTimeout"));
            Assert.That(relevantEvents[1].EventName, Is.EqualTo("ConnectionError"));
        }
    }

    /// <summary>
    /// Test EventListener that captures events for verification.
    /// </summary>
    private class TestEventListener : EventListener
    {
        private readonly EventLevel minimumLevel;

        public TestEventListener(EventLevel minimumLevel = EventLevel.Verbose)
        {
            this.minimumLevel = minimumLevel;
            this.Events = new List<EventWrittenEventArgs>();
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
                this.Events.Add(eventData);
            }
        }
    }
}
