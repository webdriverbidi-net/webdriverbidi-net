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
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);

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

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsConnectionClosingAndClosedEvents()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

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

    }

    [Fact]
    public async Task TestReconnectResetsTerminationReasonFromPriorTerminate()
    {
        TaskCompletionSource captured = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };

        // Session 1: a terminal unknown-message error sets the termination reason to a non-default
        // value; the next command surfaces the error and forces the terminate teardown.
        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync("""{"someProperty":"someValue"}""");
        await captured.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));

        // The terminate teardown reported the non-default reason.
        EventWrittenEventArgs terminateStopped = Assert.Single(listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "TransportStopped"));
        Assert.NotNull(terminateStopped.Payload);
        Assert.Equal("Unknown message from connection", terminateStopped.Payload[0]);
        listener.ClearEvents();

        // Session 2: reconnecting resets the termination reason to its default, so a normal shutdown
        // does not report the stale terminate reason from session 1.
        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        EventWrittenEventArgs stopped = Assert.Single(listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "TransportStopped"));
        Assert.NotNull(stopped.Payload);
        Assert.Equal("Normal shutdown", stopped.Payload[0]);

    }

    [Fact]
    public async Task TestTransportEmitsCommandSendingAndCompletedEvents()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        // Simulate response
        Task responseTask = Task.Run(
            async () =>
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
                await connection.RaiseDataReceivedEventAsync(json);
            },
            TestContext.Current.CancellationToken);

        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Awaited so that a fault inside the Task is reported as itself rather
        // than as whichever assertion below fails first.
        await responseTask;

        // CommandCompleted is raised on the reader task; block for it (the signalled listener wakes as
        // soon as it fires) so the snapshot below deterministically contains it and the preceding
        // CommandSending rather than racing the reader task.
        listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "CommandCompleted");
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

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsCommandErrorEvent()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        // Simulate error response
        Task responseTask = Task.Run(
            async () =>
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

        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Awaited so that a fault inside the Task is reported as itself rather than as whichever assertion below fails first.
        await responseTask;

        // CommandError is raised on the reader task; block for it (the signalled listener wakes as soon
        // as it fires) rather than snapshotting immediately and racing.
        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "CommandError");
        Assert.Single(events);

        EventWrittenEventArgs errorEvent = events[0];
        ReadOnlyCollection<object?>? errorPayload = errorEvent.Payload;
        Assert.NotNull(errorPayload);

        Assert.Equal("1", errorPayload[0]);
        Assert.Equal("session.status", errorPayload[1]);
        Assert.Equal("InvalidSessionId", errorPayload[2]);
        Assert.Equal("invalid session id", errorPayload[3]);
        Assert.Equal("Session not found", errorPayload[4]);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsCommandSendFailedEventWhenSendThrows()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = _ => throw new InvalidOperationException("Simulated send failure"),
        };
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents();

        TestCommandParameters commandParameters = new("session.status");
        Assert.Contains("Simulated send failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);

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

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsCommandSendFailedEventWhenSendIsCanceled()
    {
        using TestEventListener listener = new();
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenSource cancellationTokenSource = new();
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = async _ =>
            {
                taskCompletionSource.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationTokenSource.Token);
            },
        };
        Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents();

        TestCommandParameters commandParameters = new("session.status");
        Task<Command> sendTask = transport.SendCommandAsync(commandParameters, cancellationTokenSource.Token);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
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

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsEventReceivedEvent()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");

        // The transport writes EventReceived before it dispatches the event to observers, so once
        // the observer has run the event is already in the listener: no timed wait is needed.
        TaskCompletionSource observerInvoked = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnEventReceived.AddObserver(e =>
        {
            observerInvoked.TrySetResult();
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);
        await observerInvoked.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("EventReceived");
        EventWrittenEventArgs eventReceived = Assert.Single(events);

        ReadOnlyCollection<object?>? payload0 = eventReceived.Payload;
        Assert.NotNull(payload0);
        Assert.Equal("protocol.event", payload0[0]);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsUnknownMessageReceivedEvent()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        // Simulate unknown message
        string json = """{"type": "unknown", "data": "some data"}""";
        await connection.RaiseDataReceivedEventAsync(json);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "UnknownMessageReceived");
        Assert.Single(events);

        EventWrittenEventArgs unknownEvent = events[0];
        ReadOnlyCollection<object?>? unknownPayload = unknownEvent.Payload;
        Assert.NotNull(unknownPayload);

        Assert.Equal("unknown", unknownPayload[0]);
        Assert.IsType<int>(unknownPayload[1]); // message length

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsUnknownMessageReceivedEventWithNullType()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        // Simulate unknown message with null type to test null-coalescing branch
        string json = """{"type": null, "data": "some data"}""";
        await connection.RaiseDataReceivedEventAsync(json);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "UnknownMessageReceived");
        Assert.Single(events);

        EventWrittenEventArgs unknownEvent = events[0];
        ReadOnlyCollection<object?>? unknownPayload = unknownEvent.Payload;
        Assert.NotNull(unknownPayload);

        Assert.Equal("unknown", unknownPayload[0]); // Should fall back to "unknown"
        Assert.IsType<int>(unknownPayload[1]); // message length

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsProtocolErrorEventForInvalidEventJson()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("test.event");

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
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

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "ProtocolError");
        Assert.True(events.Count >= 1);

        EventWrittenEventArgs protocolError = events[0];
        ReadOnlyCollection<object?>? protocolPayload = protocolError.Payload;
        Assert.NotNull(protocolPayload);
        Assert.NotEmpty((string)protocolPayload[0]!); // error message
        Assert.NotEmpty((string)protocolPayload[1]!); // message snippet

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsEventHandlerErrorEvent()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Test exception message");
        });

        // The transport writes EventHandlerError before it raises OnEventHandlerErrorOccurred, so
        // once that observer has run the event is already in the listener: no timed wait is needed.
        TaskCompletionSource errorReported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            errorReported.TrySetResult();
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await connection.RaiseDataReceivedEventAsync(json);
        await errorReported.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("EventHandlerError");
        EventWrittenEventArgs handlerError = Assert.Single(events);
        ReadOnlyCollection<object?>? handlerPayload = handlerError.Payload;
        Assert.NotNull(handlerPayload);

        Assert.Equal("protocol.event", handlerPayload[0]);
        Assert.Equal("Test exception message", handlerPayload[1]);

        // Collect mode surfaces the collected handler failure when the transport disconnects.
        AggregateException exception = await Assert.ThrowsAsync<AggregateException>(
            async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
    }

    [Fact]
    public async Task TestTransportEmitsConnectionErrorEvent()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        // Simulate connection error
        await connection.RaiseConnectionErrorEventAsync(new InvalidOperationException("Connection lost"));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "ConnectionError");
        Assert.Single(events);

        EventWrittenEventArgs connectionError = events[0];
        ReadOnlyCollection<object?>? connectionPayload = connectionError.Payload;
        Assert.NotNull(connectionPayload);
        Assert.Contains("Connection lost", (string)connectionPayload[1]!);

    }

    [Fact]
    public async Task TestTransportEmitsConnectionErrorEventWhenTakingFastPath()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        listener.ClearEvents();

        // Connection error after disconnect: fast-path guard returns early (no lock acquisition).
        // ConnectionError event must still be emitted for observability.
        await connection.RaiseConnectionErrorEventAsync(new InvalidOperationException("Connection lost during shutdown"));

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName(TimeSpan.FromSeconds(5), "ConnectionError");
        Assert.Single(events);

        ReadOnlyCollection<object?>? errorPayload = events[0].Payload;
        Assert.NotNull(errorPayload);
        Assert.Contains("Connection lost during shutdown", (string)errorPayload[1]!);

    }

    [Fact]
    public async Task TestTransportEmitsPendingCommandCountEvent()
    {
        using TestEventListener listener = new();
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        listener.ClearEvents(); // Clear connection events

        TestCommandParameters commandParameters = new("session.status");
        _ = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("PendingCommandCount");
        Assert.True(events.Count >= 1);

        EventWrittenEventArgs countEvent = events[0];
        ReadOnlyCollection<object?>? countPayload = countEvent.Payload;
        Assert.NotNull(countPayload);
        Assert.IsType<int>(countPayload[0]);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportEmitsCanceledCommandResponseDiscardedEvent()
    {
        using TestEventListener listener = new();
        TaskCompletionSource discardedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.Contains("Discarding late response"))
            {
                discardedTaskCompletionSource.TrySetResult();
            }
        });

        await transport.ConnectAsync("ws://localhost:9222", TestContext.Current.CancellationToken);
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        transport.CancelCommand(command, CommandCancellationReason.TimedOut);
        listener.ClearEvents();

        await connection.RaiseDataReceivedEventAsync($$$"""{"type":"success","id":{{{command.CommandId}}},"result":{"parameterName":"parameterValue"}}""");
        await discardedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        List<EventWrittenEventArgs> events = listener.GetEventsForEventName("CanceledCommandResponseDiscarded");
        EventWrittenEventArgs discardedEvent = Assert.Single(events);
        Assert.Equal(EventLevel.Informational, discardedEvent.Level);
        ReadOnlyCollection<object?>? payload = discardedEvent.Payload;
        Assert.NotNull(payload);
        Assert.Equal(command.CommandId.ToString(), payload[0]);
        Assert.Equal("module.command", payload[1]);
        Assert.Equal("TimedOut", payload[2]);
        Assert.True((long)payload[3]! >= 0);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestMessageStatisticsExcludeMessagesProcessedByPreviousSessionReader()
    {
        // A reconnect that gives up waiting for a stuck handler leaves the previous session's reader
        // still draining the previous session's queue. The messages it processes after the reconnect
        // belong to the previous session, so the new session's statistics do not count them.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource firstHandlerBlockedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstHandlerTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource staleEventsProcessedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int eventCount = 0;

        using TestEventListener listener = new();
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            int currentCount = Interlocked.Increment(ref eventCount);
            if (currentCount == 1)
            {
                firstHandlerBlockedTaskCompletionSource.TrySetResult();
                releaseFirstHandlerTaskCompletionSource.Task.GetAwaiter().GetResult();
            }
            else if (currentCount == 3)
            {
                staleEventsProcessedTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });

        string eventJson = """{ "type": "event", "method": "protocol.event", "params": { "paramName": "paramValue" } }""";
        await transport.ConnectAsync("ws://localhost", cancellationToken);

        // The reader blocks in the handler for the first event, leaving two more unread on the first
        // session's queue, and the reconnect gives up waiting for it.
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await firstHandlerBlockedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await connection.RaiseRemoteDisconnectedEventAsync();

        Task reconnectTask = transport.ConnectAsync("ws://localhost", cancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(reconnectTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), cancellationToken);
        await reconnectTask;

        // The new session sends one command and receives its response.
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationToken);
        await connection.RaiseDataReceivedEventAsync($$"""{ "type": "success", "id": {{command.CommandId}}, "result": { "value": "response value" } }""");
        Assert.True(await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), cancellationToken));

        // Releasing the handler lets the previous session's reader process all three events.
        releaseFirstHandlerTaskCompletionSource.SetResult();
        await staleEventsProcessedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        listener.ClearEvents();
        await transport.DisconnectAsync(cancellationToken);

        EventWrittenEventArgs statisticsEvent = Assert.Single(listener.GetEventsForEventName("MessageStatistics"));
        ReadOnlyCollection<object?>? payload = statisticsEvent.Payload;
        Assert.NotNull(payload);
        Assert.Equal(1L, payload[0]);
        Assert.Equal(1L, payload[1]);
        Assert.Equal(0L, payload[2]);
        Assert.Equal(0L, payload[3]);
    }
}
