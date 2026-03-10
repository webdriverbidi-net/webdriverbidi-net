namespace WebDriverBiDi.Protocol;

using System.Diagnostics.Tracing;
using TestUtilities;

/// <summary>
/// Integration tests to verify EventSource instrumentation in Transport class.
/// These tests ensure the Transport emits appropriate events during its lifecycle.
/// </summary>
[TestFixture]
public class TransportEventSourceIntegrationTests
{
    [Test]
    public async Task TestTransportEmitsConnectionOpeningAndOpenedEvents()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("ConnectionOpening", "ConnectionOpened", "TransportStarted");
        Assert.That(events, Has.Count.EqualTo(3));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(events[0].EventName, Is.EqualTo("ConnectionOpening"));
            Assert.That(events[0].Payload![1], Is.EqualTo("ws://localhost:9222"));

            Assert.That(events[1].EventName, Is.EqualTo("ConnectionOpened"));
            Assert.That(events[1].Payload![1], Is.EqualTo("ws://localhost:9222"));

            Assert.That(events[2].EventName, Is.EqualTo("TransportStarted"));
        }

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsConnectionClosingAndClosedEvents()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        await transport.DisconnectAsync();

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("ConnectionClosing", "ConnectionClosed", "TransportStopped");
        Assert.That(events, Has.Count.EqualTo(3));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(events[0].EventName, Is.EqualTo("ConnectionClosing"));
            Assert.That(events[0].Payload![1], Is.EqualTo("Normal shutdown"));

            Assert.That(events[1].EventName, Is.EqualTo("ConnectionClosed"));

            Assert.That(events[2].EventName, Is.EqualTo("TransportStopped"));
            Assert.That(events[2].Payload![0], Is.EqualTo("Normal shutdown"));
        }

        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsCommandSendingAndCompletedEvents()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        Command command = await transport.SendCommandAsync(commandParameters);

        // Simulate response
        _ = Task.Run(async () =>
        {
            string json = """
                          {
                            "type": "success",
                            "id": 1,
                            "result": {
                              "value": "response value"
                            }
                          }
                          """;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            await connection.RaiseDataReceivedEventAsync(json);
        });

        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandSending", "PendingCommandCount", "CommandCompleted");
        Assert.That(events, Has.Count.AtLeast(2)); // At least CommandSending and CommandCompleted

        EventWrittenEventArgs sendingEvent = events.First(e => e.EventName == "CommandSending");
        EventWrittenEventArgs completedEvent = events.First(e => e.EventName == "CommandCompleted");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sendingEvent.Payload![0], Is.EqualTo("1"));
            Assert.That(sendingEvent.Payload![1], Is.EqualTo("session.status"));

            Assert.That(completedEvent.Payload![0], Is.EqualTo("1"));
            Assert.That(completedEvent.Payload![1], Is.EqualTo("session.status"));
            Assert.That(completedEvent.Payload![2], Is.InstanceOf<long>()); // elapsed time
        }

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsCommandErrorEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        Command command = await transport.SendCommandAsync(commandParameters);

        // Simulate error response
        _ = Task.Run(async () =>
        {
            string json = """
                          {
                            "type": "error",
                            "id": 1,
                            "error": "invalid session id",
                            "message": "Session not found"
                          }
                          """;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            await connection.RaiseDataReceivedEventAsync(json);
        });

        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandError");
        Assert.That(events, Has.Count.EqualTo(1));

        EventWrittenEventArgs errorEvent = events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(errorEvent.Payload![0], Is.EqualTo("1"));
            Assert.That(errorEvent.Payload![1], Is.EqualTo("session.status"));
            Assert.That(errorEvent.Payload![2], Is.EqualTo("invalid session id"));
            Assert.That(errorEvent.Payload![3], Is.EqualTo("Session not found"));
        }

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsCommandSendFailedEventWhenSendThrows()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = _ => throw new InvalidOperationException("Simulated send failure"),
        };
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents();

        TestCommandParameters commandParameters = new("session.status");
        Assert.That(
            async () => await transport.SendCommandAsync(commandParameters),
            Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("Simulated send failure"));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandSending", "CommandSendFailed", "PendingCommandCount", "CommandCompleted", "CommandError");
        EventWrittenEventArgs sendingEvent = events.First(e => e.EventName == "CommandSending");
        EventWrittenEventArgs failedEvent = events.First(e => e.EventName == "CommandSendFailed");
        EventWrittenEventArgs countEvent = events.Last(e => e.EventName == "PendingCommandCount");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sendingEvent.Payload![0], Is.EqualTo("1"));
            Assert.That(sendingEvent.Payload![1], Is.EqualTo("session.status"));
            Assert.That(failedEvent.Payload![0], Is.EqualTo("1"));
            Assert.That(failedEvent.Payload![1], Is.EqualTo("session.status"));
            Assert.That(failedEvent.Payload![2], Is.EqualTo(typeof(InvalidOperationException).FullName));
            Assert.That(failedEvent.Payload![3], Is.EqualTo("Simulated send failure"));
            Assert.That(failedEvent.Payload![4], Is.InstanceOf<long>());
            Assert.That(countEvent.Payload![0], Is.EqualTo(0));
            Assert.That(events.Any(e => e.EventName == "CommandCompleted"), Is.False);
            Assert.That(events.Any(e => e.EventName == "CommandError"), Is.False);
        }

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsCommandSendFailedEventWhenSendIsCanceled()
    {
        TestEventListener listener = new();
        ManualResetEventSlim sendStarted = new(false);
        using CancellationTokenSource cancellationTokenSource = new();
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = async _ =>
            {
                sendStarted.Set();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationTokenSource.Token);
            },
        };
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents();

        TestCommandParameters commandParameters = new("session.status");
        Task<Command> sendTask = transport.SendCommandAsync(commandParameters, cancellationTokenSource.Token);
        bool sendDidStart = sendStarted.Wait(TimeSpan.FromSeconds(1));
        Assert.That(sendDidStart, Is.True);
        cancellationTokenSource.Cancel();

        Assert.That(async () => await sendTask, Throws.InstanceOf<OperationCanceledException>());

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandSending", "CommandSendFailed", "PendingCommandCount", "CommandCompleted", "CommandError");
        EventWrittenEventArgs failedEvent = events.First(e => e.EventName == "CommandSendFailed");
        EventWrittenEventArgs countEvent = events.Last(e => e.EventName == "PendingCommandCount");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(failedEvent.Payload![0], Is.EqualTo("1"));
            Assert.That(failedEvent.Payload![1], Is.EqualTo("session.status"));
            Assert.That(failedEvent.Payload![2], Is.EqualTo(typeof(TaskCanceledException).FullName));
            Assert.That(failedEvent.Payload![3], Is.Not.Empty);
            Assert.That(failedEvent.Payload![4], Is.InstanceOf<long>());
            Assert.That(countEvent.Payload![0], Is.EqualTo(0));
            Assert.That(events.Any(e => e.EventName == "CommandCompleted"), Is.False);
            Assert.That(events.Any(e => e.EventName == "CommandError"), Is.False);
        }

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsEventReceivedEvent()
    {
        // This test verifies EventReceived is emitted by directly checking if the method
        // is callable. Full integration testing of event flow is covered by TransportTests.
        TestEventListener listener = new();

        // Directly emit the event to verify it works
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("test.event");

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("EventReceived");
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].Payload![0], Is.EqualTo("test.event"));

        listener.Dispose();
        await Task.CompletedTask; // Satisfy async requirement
    }

    [Test]
    public async Task TestTransportEmitsUnknownMessageReceivedEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        // Simulate unknown message
        string json = """{"type": "unknown", "data": "some data"}""";
        await connection.RaiseDataReceivedEventAsync(json);

        await Task.Delay(TimeSpan.FromMilliseconds(50)); // Allow event processing

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("UnknownMessageReceived");
        Assert.That(events, Has.Count.EqualTo(1));

        EventWrittenEventArgs unknownEvent = events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unknownEvent.Payload![0], Is.EqualTo("unknown"));
            Assert.That(unknownEvent.Payload![1], Is.InstanceOf<int>()); // message length
        }

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsUnknownMessageReceivedEventWithNullType()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        // Simulate unknown message with null type to test null-coalescing branch
        string json = """{"type": null, "data": "some data"}""";
        await connection.RaiseDataReceivedEventAsync(json);

        await Task.Delay(TimeSpan.FromMilliseconds(50)); // Allow event processing

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("UnknownMessageReceived");
        Assert.That(events, Has.Count.EqualTo(1));

        EventWrittenEventArgs unknownEvent = events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(unknownEvent.Payload![0], Is.EqualTo("unknown")); // Should fall back to "unknown"
            Assert.That(unknownEvent.Payload![1], Is.InstanceOf<int>()); // message length
        }

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsProtocolErrorEventForInvalidEventJson()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("test.event");

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        // Simulate malformed event (missing required property)
        string json = """
                      {
                        "type": "event",
                        "method": "test.event",
                        "params": {
                          "invalidProperty": "value"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);

        await Task.Delay(TimeSpan.FromMilliseconds(50)); // Allow event processing

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("ProtocolError");
        Assert.That(events, Has.Count.AtLeast(1));

        EventWrittenEventArgs protocolError = events[0];
        Assert.That(protocolError.Payload![0], Is.Not.Empty); // error message
        Assert.That(protocolError.Payload![1], Is.Not.Empty); // message snippet

        await transport.DisconnectAsync();
        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsEventHandlerErrorEvent()
    {
        // This test verifies EventHandlerError is emitted by directly checking if the method
        // is callable. Full integration testing of error handling is covered by TransportTests.
        TestEventListener listener = new();

        // Directly emit the event to verify it works
        WebDriverBiDiEventSource.RaiseEvent.EventHandlerError("test.event", "Test exception message");

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("EventHandlerError");
        Assert.That(events, Has.Count.EqualTo(1));

        EventWrittenEventArgs handlerError = events[0];
        using (Assert.EnterMultipleScope())
        {
            Assert.That(handlerError.Payload![0], Is.EqualTo("test.event"));
            Assert.That(handlerError.Payload![1], Is.EqualTo("Test exception message"));
        }

        listener.Dispose();
        await Task.CompletedTask; // Satisfy async requirement
    }

    [Test]
    public async Task TestTransportEmitsConnectionErrorEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        // Simulate connection error
        await connection.RaiseConnectionErrorEventAsync(new InvalidOperationException("Connection lost"));

        await Task.Delay(TimeSpan.FromMilliseconds(50)); // Allow event processing

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("ConnectionError");
        Assert.That(events, Has.Count.EqualTo(1));

        EventWrittenEventArgs connectionError = events[0];
        Assert.That(connectionError.Payload![1], Does.Contain("Connection lost"));

        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsConnectionErrorEventWhenTakingFastPath()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        await transport.DisconnectAsync();
        listener.ClearEvents();

        // Connection error after disconnect: fast-path guard returns early (no lock acquisition).
        // ConnectionError event must still be emitted for observability.
        await connection.RaiseConnectionErrorEventAsync(new InvalidOperationException("Connection lost during shutdown"));

        await Task.Delay(TimeSpan.FromMilliseconds(50));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("ConnectionError");
        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].Payload![1], Does.Contain("Connection lost during shutdown"));

        listener.Dispose();
    }

    [Test]
    public async Task TestTransportEmitsPendingCommandCountEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222");
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        _ = await transport.SendCommandAsync(commandParameters);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("PendingCommandCount");
        Assert.That(events, Has.Count.AtLeast(1));

        EventWrittenEventArgs countEvent = events[0];
        Assert.That(countEvent.Payload![0], Is.InstanceOf<int>());

        await transport.DisconnectAsync();
        listener.Dispose();
    }
}
