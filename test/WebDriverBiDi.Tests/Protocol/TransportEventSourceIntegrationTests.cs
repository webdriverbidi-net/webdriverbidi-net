namespace WebDriverBiDi.Protocol;

using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using TestUtilities;

/// <summary>
/// Integration tests to verify EventSource instrumentation in Transport class.
/// These tests ensure the Transport emits appropriate events during its lifecycle.
/// </summary>
[Collection("EventSourceTests")]
public class TransportEventSourceIntegrationTests
{
    [Fact]
    public async Task TestTransportEmitsConnectionOpeningAndOpenedEvents()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("ConnectionOpening", "ConnectionOpened", "TransportStarted");
        Assert.Equal(3, events.Count);

        Assert.Equal("ConnectionOpening", events[0].EventName);
        ReadOnlyCollection<object?>? payload0 = events[0].Payload;
        Assert.NotNull(payload0);
        Assert.Equal("ws://localhost:9222", payload0[1]);

        Assert.Equal("ConnectionOpened", events[1].EventName);
        ReadOnlyCollection<object?>? payload1 = events[1].Payload;
        Assert.NotNull(payload1);
        Assert.Equal("ws://localhost:9222", payload1[1]);

        Assert.Equal("TransportStarted", events[2].EventName);

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsConnectionClosingAndClosedEvents()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("ConnectionClosing", "ConnectionClosed", "TransportStopped");
        Assert.Equal(3, events.Count);

        Assert.Equal("ConnectionClosing", events[0].EventName);
        ReadOnlyCollection<object?>? payload0 = events[0].Payload;
        Assert.NotNull(payload0);
        Assert.Equal("Normal shutdown", payload0[1]);

        Assert.Equal("ConnectionClosed", events[1].EventName);

        Assert.Equal("TransportStopped", events[2].EventName);
        ReadOnlyCollection<object?>? payload2 = events[2].Payload;
        Assert.NotNull(payload2);
        Assert.Equal("Normal shutdown", payload2[0]);

        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsCommandSendingAndCompletedEvents()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        Command command = await transport.SendCommandAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

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
        },
        TestContext.Current.CancellationToken);

        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), cancellationToken: TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandSending", "PendingCommandCount", "CommandCompleted");
        Assert.True(events.Count >= 2); // At least CommandSending and CommandCompleted

        EventWrittenEventArgs sendingEvent = events.First(e => e.EventName == "CommandSending");
        EventWrittenEventArgs completedEvent = events.First(e => e.EventName == "CommandCompleted");

        ReadOnlyCollection<object?>? sendingPayload = sendingEvent.Payload;
        Assert.NotNull(sendingPayload);
        Assert.Equal("1", sendingPayload[0]);
        Assert.Equal("session.status", sendingPayload[1]);

        ReadOnlyCollection<object?>? completedPayload = completedEvent.Payload;
        Assert.NotNull(completedPayload);
        Assert.Equal("1", completedPayload[0]);
        Assert.Equal("session.status", completedPayload[1]);
        Assert.IsType<long>(completedPayload[2]); // elapsed time

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsCommandErrorEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        Command command = await transport.SendCommandAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

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
            await Task.Yield();
            await connection.RaiseDataReceivedEventAsync(json);
        },
        TestContext.Current.CancellationToken);

        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), cancellationToken: TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandError");
        Assert.Single(events);

        EventWrittenEventArgs errorEvent = events[0];
        ReadOnlyCollection<object?>? errorPayload = errorEvent.Payload;
        Assert.NotNull(errorPayload);

        Assert.Equal("1", errorPayload[0]);
        Assert.Equal("session.status", errorPayload[1]);
        Assert.Equal(ErrorCode.InvalidSessionId, errorPayload[2]);
        Assert.Equal("invalid session id", errorPayload[3]);
        Assert.Equal("Session not found", errorPayload[4]);

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsCommandSendFailedEventWhenSendThrows()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = _ => throw new InvalidOperationException("Simulated send failure"),
        };
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents();

        TestCommandParameters commandParameters = new("session.status");
        Assert.Contains("Simulated send failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await transport.SendCommandAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken))).Message);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandSending", "CommandSendFailed", "PendingCommandCount", "CommandCompleted", "CommandError");
        EventWrittenEventArgs sendingEvent = events.First(e => e.EventName == "CommandSending");
        EventWrittenEventArgs failedEvent = events.First(e => e.EventName == "CommandSendFailed");
        EventWrittenEventArgs countEvent = events.Last(e => e.EventName == "PendingCommandCount");

        ReadOnlyCollection<object?>? sendingPayload = sendingEvent.Payload;
        Assert.NotNull(sendingPayload);
        Assert.Equal("1", sendingPayload[0]);
        Assert.Equal("session.status", sendingPayload[1]);

        ReadOnlyCollection<object?>? failedPayload = failedEvent.Payload;
        Assert.NotNull(failedPayload);
        Assert.Equal("1", failedPayload[0]);
        Assert.Equal("session.status", failedPayload[1]);
        Assert.Equal(typeof(InvalidOperationException).FullName, failedPayload[2]);
        Assert.Equal("Simulated send failure", failedPayload[3]);
        Assert.IsType<long>(failedPayload[4]);

        ReadOnlyCollection<object?>? countPayload = countEvent.Payload;
        Assert.NotNull(countPayload);
        Assert.Equal(0, countPayload[0]);

        Assert.DoesNotContain(events, e => e.EventName == "CommandCompleted");
        Assert.DoesNotContain(events, e => e.EventName == "CommandError");

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
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

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents();

        TestCommandParameters commandParameters = new("session.status");
        Task<Command> sendTask = transport.SendCommandAsync(commandParameters, cancellationTokenSource.Token);
        bool sendDidStart = sendStarted.Wait(TimeSpan.FromSeconds(1), cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(sendDidStart);
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await sendTask);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CommandSending", "CommandSendFailed", "PendingCommandCount", "CommandCompleted", "CommandError");
        EventWrittenEventArgs failedEvent = events.First(e => e.EventName == "CommandSendFailed");
        EventWrittenEventArgs countEvent = events.Last(e => e.EventName == "PendingCommandCount");

        ReadOnlyCollection<object?>? failedPayload = failedEvent.Payload;
        Assert.NotNull(failedPayload);
        Assert.Equal("1", failedPayload[0]);
        Assert.Equal("session.status", failedPayload[1]);
        Assert.Equal(typeof(TaskCanceledException).FullName, failedPayload[2]);
        Assert.NotEmpty((string)failedPayload[3]!);
        Assert.IsType<long>(failedPayload[4]);

        ReadOnlyCollection<object?>? countPayload = countEvent.Payload;
        Assert.NotNull(countPayload);
        Assert.Equal(0, countPayload[0]);

        Assert.DoesNotContain(events, e => e.EventName == "CommandCompleted");
        Assert.DoesNotContain(events, e => e.EventName == "CommandError");

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsEventReceivedEvent()
    {
        // This test verifies EventReceived is emitted by directly checking if the method
        // is callable. Full integration testing of event flow is covered by TransportTests.
        TestEventListener listener = new();

        // Directly emit the event to verify it works
        WebDriverBiDiEventSource.RaiseEvent.EventReceived("test.event");

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("EventReceived");
        Assert.Single(events);

        ReadOnlyCollection<object?>? payload0 = events[0].Payload;
        Assert.NotNull(payload0);
        Assert.Equal("test.event", payload0[0]);

        listener.Dispose();
        await Task.CompletedTask; // Satisfy async requirement
    }

    [Fact]
    public async Task TestTransportEmitsUnknownMessageReceivedEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        // Simulate unknown message
        string json = """{"type": "unknown", "data": "some data"}""";
        await connection.RaiseDataReceivedEventAsync(json);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromMilliseconds(50), "UnknownMessageReceived");
        Assert.Single(events);

        EventWrittenEventArgs unknownEvent = events[0];
        ReadOnlyCollection<object?>? unknownPayload = unknownEvent.Payload;
        Assert.NotNull(unknownPayload);

        Assert.Equal("unknown", unknownPayload[0]);
        Assert.IsType<int>(unknownPayload[1]); // message length

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsUnknownMessageReceivedEventWithNullType()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        // Simulate unknown message with null type to test null-coalescing branch
        string json = """{"type": null, "data": "some data"}""";
        await connection.RaiseDataReceivedEventAsync(json);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromMilliseconds(50), "UnknownMessageReceived");
        Assert.Single(events);

        EventWrittenEventArgs unknownEvent = events[0];
        ReadOnlyCollection<object?>? unknownPayload = unknownEvent.Payload;
        Assert.NotNull(unknownPayload);

        Assert.Equal("unknown", unknownPayload[0]); // Should fall back to "unknown"
        Assert.IsType<int>(unknownPayload[1]); // message length

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsProtocolErrorEventForInvalidEventJson()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("test.event");

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
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

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromMilliseconds(50), "ProtocolError");
        Assert.True(events.Count >= 1);

        EventWrittenEventArgs protocolError = events[0];
        ReadOnlyCollection<object?>? protocolPayload = protocolError.Payload;
        Assert.NotNull(protocolPayload);
        Assert.NotEmpty((string)protocolPayload[0]!); // error message
        Assert.NotEmpty((string)protocolPayload[1]!); // message snippet

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsEventHandlerErrorEvent()
    {
        // This test verifies EventHandlerError is emitted by directly checking if the method
        // is callable. Full integration testing of error handling is covered by TransportTests.
        TestEventListener listener = new();

        // Directly emit the event to verify it works
        WebDriverBiDiEventSource.RaiseEvent.EventHandlerError("test.event", "Test exception message");

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("EventHandlerError");
        Assert.Single(events);

        EventWrittenEventArgs handlerError = events[0];
        ReadOnlyCollection<object?>? handlerPayload = handlerError.Payload;
        Assert.NotNull(handlerPayload);

        Assert.Equal("test.event", handlerPayload[0]);
        Assert.Equal("Test exception message", handlerPayload[1]);

        listener.Dispose();
        await Task.CompletedTask; // Satisfy async requirement
    }

    [Fact]
    public async Task TestTransportEmitsConnectionErrorEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        // Simulate connection error
        await connection.RaiseConnectionErrorEventAsync(new InvalidOperationException("Connection lost"));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromMilliseconds(50), "ConnectionError");
        Assert.Single(events);

        EventWrittenEventArgs connectionError = events[0];
        ReadOnlyCollection<object?>? connectionPayload = connectionError.Payload;
        Assert.NotNull(connectionPayload);
        Assert.Contains("Connection lost", (string)connectionPayload[1]!);

        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsConnectionErrorEventWhenTakingFastPath()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents();

        // Connection error after disconnect: fast-path guard returns early (no lock acquisition).
        // ConnectionError event must still be emitted for observability.
        await connection.RaiseConnectionErrorEventAsync(new InvalidOperationException("Connection lost during shutdown"));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromMilliseconds(50), "ConnectionError");
        Assert.Single(events);

        ReadOnlyCollection<object?>? errorPayload = events[0].Payload;
        Assert.NotNull(errorPayload);
        Assert.Contains("Connection lost during shutdown", (string)errorPayload[1]!);

        listener.Dispose();
    }

    [Fact]
    public async Task TestTransportEmitsPendingCommandCountEvent()
    {
        TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", cancellationToken: TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        _ = await transport.SendCommandAsync(commandParameters, cancellationToken: TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("PendingCommandCount");
        Assert.True(events.Count >= 1);

        EventWrittenEventArgs countEvent = events[0];
        ReadOnlyCollection<object?>? countPayload = countEvent.Payload;
        Assert.NotNull(countPayload);
        Assert.IsType<int>(countPayload[0]);

        await transport.DisconnectAsync(cancellationToken: TestContext.Current.CancellationToken);
        listener.Dispose();
    }
}
