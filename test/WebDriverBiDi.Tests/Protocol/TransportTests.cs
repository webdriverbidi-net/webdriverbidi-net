namespace WebDriverBiDi.Protocol;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using PinchHitter;
using TestUtilities;
using WebDriverBiDi.Script;
using Xunit.Sdk;

public class TransportTests
{
    // Mirrors the transport's internal MaxJsonDepth constant.
    private const int MaxJsonDepth = 512;

    // The 5-second bound below is a deadlock detector, not a timing assumption;
    // a correct implementation completes effectively immediately.
    private static readonly TimeSpan DeadlockDetectionTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task TestTransportCanSendCommand()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" }
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters }
        };

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command, TestContext.Current.CancellationToken);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
    }

    [Fact]
    public async Task TestTransportCanSendCommandWithComplexParameters()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
            { "complex", new object?[] { "stringValue", 1, 2.3d, true, null } }
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters }
        };

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestComplexCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command, TestContext.Current.CancellationToken);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
    }

    [Fact]
    public async Task TestTransportCanGetResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
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

        // Awaited here so that a fault inside the Task is reported as itself rather
        // than as whichever assertion below fails first.
        await responseTask;

        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.True(hasResult);
        Assert.NotNull(actualResult);

        Assert.False(actualResult.IsError);
        Assert.IsType<TestCommandResult>(actualResult);

        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.NotNull(convertedResult);
        Assert.Equal("response value", convertedResult.Value);
    }

    [Fact]
    public async Task TestTransportCanGetResponseWithAdditionalData()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        Task responseTask = Task.Run(
            async () =>
            {
                string json = """
                            {
                              "type": "success",
                              "id": 1,
                              "result": {
                                "value": "response value" 
                              },
                              "extraDataName": "extraDataValue"
                            }
                            """;
                await connection.RaiseDataReceivedEventAsync(json);
            },
            TestContext.Current.CancellationToken);
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Awaited here so that a fault inside the Task is reported as itself rather
        // than as whichever assertion below fails first.
        await responseTask;

        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.True(hasResult);
        Assert.NotNull(actualResult);

        Assert.False(actualResult.IsError);
        Assert.IsType<TestCommandResult>(actualResult);

        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.NotNull(convertedResult);

        // "extraDataName" sits on the envelope, so it is a response property, not payload data.
        Assert.Empty(convertedResult.AdditionalData);

        Assert.Equal("response value", convertedResult.Value);
        Assert.Single(convertedResult.AdditionalResponseProperties);
        Assert.Equal("extraDataValue", convertedResult.AdditionalResponseProperties["extraDataName"]);
    }

    [Fact]
    public async Task TestTransportCanGetErrorResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        Task responseTask = Task.Run(
            async () =>
            {
                string json = """
                            {
                              "type": "error",
                              "id": 1,
                              "error": "unknown command",
                              "message": "This is a test error message"
                            }
                            """;
                await connection.RaiseDataReceivedEventAsync(json);
            },
            TestContext.Current.CancellationToken);
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Awaited here so that a fault inside the Task is reported as itself rather
        // than as whichever assertion below fails first.
        await responseTask;

        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.True(hasResult);
        Assert.NotNull(actualResult);

        Assert.True(actualResult.IsError);
        Assert.IsType<ErrorResult>(actualResult);

        ErrorResult? convertedResponse = actualResult as ErrorResult;
        Assert.NotNull(convertedResponse);

        Assert.Equal("unknown command", convertedResponse.ErrorType);
        Assert.Equal(ErrorCode.UnknownCommand, convertedResponse.ErrorCode);
        Assert.Equal("This is a test error message", convertedResponse.ErrorMessage);
        Assert.Null(convertedResponse.StackTrace);
    }

    [Fact]
    public async Task TestTransportCompletesCommandOnMalformedErrorResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        Task responseTask = Task.Run(
            async () =>
            {
                // The required "message" field is absent, so the typed error deserialization throws.
                string json = """
                            {
                              "type": "error",
                              "id": 1,
                              "error": "unknown command"
                            }
                            """;
                await connection.RaiseDataReceivedEventAsync(json);
            },
            TestContext.Current.CancellationToken);
        bool commandCompleted = await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(commandCompleted);

        // Awaited here, where the wait above has already proved the producer delivered, so that a
        // fault inside it is reported as itself rather than as whichever assertion below fails first.
        await responseTask;

        Assert.IsType<WebDriverBiDiSerializationException>(command.ThrownException);
        Assert.Contains("Error response for command 1 contained incorrect JSON for protocol error", command.ThrownException.Message);
    }

    [Fact]
    public async Task TestTransportGetResponseWithThrownException()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        Task responseTask = Task.Run(
            async () =>
            {
                string json = """
                            {
                              "type": "success",
                              "id": 1, 
                              "noResult": {
                                "invalid": "unknown command",
                                "message": "This is a test error message"
                              }
                            }
                            """;
                await connection.RaiseDataReceivedEventAsync(json);
            },
            TestContext.Current.CancellationToken);
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Awaited here, where the wait above has already proved the producer delivered, so that a
        // fault inside it is reported as itself rather than as whichever assertion below fails first.
        await responseTask;

        Assert.IsType<WebDriverBiDiSerializationException>(command.ThrownException);
        Assert.Contains("Response did not contain properly formed JSON for response type", command.ThrownException.Message);
    }

    [Fact]
    public async Task TestSendingWithNullParametersThrows()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await transport.SendCommandAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestTransportCannotSendCommandWithoutConnection()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Assert.Contains("Transport must be connected to a remote end to execute commands.", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestTransportLeavesCommandResultAndThrownExceptionNullWithoutResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);

        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.Null(command.ThrownException);
    }

    [Fact]
    public async Task TestElapsedMillisecondsRunsWhileTheCommandIsInFlight()
    {
        // The remote end never answers, so the command stays pending and its timing stays running.
        // A running command reports the interval live rather than the frozen value a completed one has.
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new("module.command");
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        Assert.False(command.TryGetResult(out _));
        long firstReading = command.ElapsedMilliseconds;
        Assert.True(firstReading >= 0);

        // A second reading of a still-running command never goes backwards. This asserts monotonicity
        // rather than growth, so it does not depend on how long the test takes to reach this line.
        Assert.True(command.ElapsedMilliseconds >= firstReading);
    }

    [Fact]
    public async Task TestSendCommandExceptionRollsBackPendingCommandState()
    {
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = _ => throw new InvalidOperationException("Simulated send failure"),
        };
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Simulated send failure", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);

        Assert.Equal(0, transport.TestPendingCommandCount);
    }

    [Fact]
    public async Task TestSendCommandCancellationRollsBackPendingCommandState()
    {
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
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new("module.command");
        Task<Command> sendTask = transport.SendCommandAsync(commandParameters, cancellationTokenSource.Token);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await sendTask);

        Assert.Equal(0, transport.TestPendingCommandCount);
    }

    [Fact]
    public async Task TestTransportEventReceived()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            receivedName = e.EventName;
            receivedData = e.EventData;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal("protocol.event", receivedName);
        Assert.IsType<TestEventArgs>(receivedData);

        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.NotNull(convertedData);
        Assert.Equal("paramValue", convertedData.ParamName);
    }

    [Fact]
    public async Task TestTransportErrorEventReceived()
    {
        object? receivedData = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnexpectedErrorReceived.AddObserver(e =>
        {
            receivedData = e.ErrorData;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.IsType<ErrorResult>(receivedData);
        ErrorResult? convertedData = receivedData as ErrorResult;
        Assert.NotNull(convertedData);

        Assert.Equal("unknown error", convertedData.ErrorType);
        Assert.Equal(ErrorCode.UnknownError, convertedData.ErrorCode);
        Assert.Equal("This is a test error message", convertedData.ErrorMessage);
    }

    [Fact]
    public async Task TestTransportErrorEventReceivedWithNullValues()
    {
        string? receivedData = null;
        bool errorEventReceived = false;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            receivedData = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        transport.OnUnexpectedErrorReceived.AddObserver(e =>
        {
            errorEventReceived = true;
            return Task.CompletedTask;
        });
        string json = """
                      {
                        "type": "event",
                        "method": null
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // An event with a null method is not an error response; it is reported verbatim
        // as an unknown message and never reaches the error observers.
        Assert.NotNull(receivedData);
        Assert.Contains("\"method\": null", receivedData);
        Assert.False(errorEventReceived);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestTransportLogsCommands()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        Assert.Single(logs);

        Assert.Equal("test log message", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Warn, logs[0].Level);
    }

    [Fact]
    public async Task TestTransportLogsSuccessfulCommandResponses()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
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

        // Awaited here so that a fault inside the Task is reported as itself rather
        // than as whichever assertion below fails first.
        await responseTask;

        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.True(hasResult);
        Assert.NotNull(actualResult);

        Assert.False(actualResult.IsError);
        Assert.IsType<TestCommandResult>(actualResult);

        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.NotNull(convertedResult);
        Assert.Equal("response value", convertedResult.Value);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The connection delivers the response the way a real receive loop does, so at Trace it logs the received
        // message before the transport reads it. The transport's own command messages bracket that entry.
        Assert.Equal(3, logs.Count);
        Assert.Contains("Sending command data for command", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Debug, logs[0].Level);
        Assert.Equal(Transport.LoggerComponentName, logs[0].ComponentName);
        Assert.StartsWith("RECV <<< ", logs[1].Message, StringComparison.Ordinal);
        Assert.Equal(WebDriverBiDiLogLevel.Trace, logs[1].Level);
        Assert.Equal(Connection.LoggerComponentName, logs[1].ComponentName);
        Assert.Contains("Received result for command", logs[2].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Debug, logs[2].Level);
        Assert.Equal(Transport.LoggerComponentName, logs[2].ComponentName);
    }

    [Fact]
    public async Task TestSynchronousLogObserverCanReenterTransportWithoutDeadlock()
    {
        // The "Sending command data" notification is emitted before the connection lock is acquired,
        // so a synchronous log observer that re-enters the transport (here, by sending another command)
        // can acquire the same non-reentrant lock and complete. Were the notification emitted while the
        // lock was held, the re-entrant send would block on the lock the notifying call already owns and
        // deadlock, and the bounded wait below would time out.
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        int reentrantSendCount = 0;
        Command? reentrantCommand = null;
        transport.OnLogMessage.AddObserver(async e =>
        {
            // Re-enter exactly once: each send emits its own "Sending command data" notification, which
            // would otherwise fire this observer again without bound.
            if (e.Message.Contains("Sending command data") && Interlocked.Increment(ref reentrantSendCount) == 1)
            {
                reentrantCommand = await transport.SendCommandAsync(new TestCommandParameters("module.reentrant"), TestContext.Current.CancellationToken);
            }
        });

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(command);
        Assert.NotNull(reentrantCommand);
    }

    [Fact]
    public async Task TestTransportLogsMalformedJsonMessages()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.ComponentName == Transport.LoggerComponentName)
            {
                logs.Add(e);
                taskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.RaiseDataReceivedEventAsync("{ { }");
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing JSON message", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventWithMissingMessageType()
    {
        string json = """
                      {
                        "id": 1,
                        "result": {
                          "value": "response value"
                        }
                      }
                      """;
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventWithInvalidMessageTypeValue()
    {
        string json = """
                      {
                        "type": "invalid",
                        "id": 1,
                        "result": {
                          "value": "response value"
                        }
                      }
                      """;
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForSuccessMessageWithMissingId()
    {
        string json = """
                      {
                        "type": "success",
                        "result": {
                          "value": "response value"
                        }
                      }
                      """;
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForSuccessMessageWithInvalidIdDataType()
    {
        string json = """
                      {
                        "type": "success",
                        "id": true,
                        "result": {
                          "value": "response value"
                        }
                      }
                      """;
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForSuccessMessageWithInvalidIdValue()
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
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportReportsProtocolErrorForErrorMessageWithMissingId()
    {
        string json = """
                      {
                        "type": "error",
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        bool unknownMessageEventRaised = false;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageEventRaised = true;
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                logTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.RaiseDataReceivedEventAsync(json);
        await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing error JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);

        // With both categories set to Collect, exactly one collected exception proves the
        // malformed-but-recognized message was captured once, as a protocol error, and
        // was not additionally reported as an unknown message.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Single(exception.InnerExceptions);
        Assert.False(unknownMessageEventRaised);
    }

    [Fact]
    public async Task TestTransportReportsProtocolErrorForErrorMessageWithMissingErrorProperty()
    {
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "message": "This is a test error message"
                      }
                      """;
        bool unknownMessageEventRaised = false;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageEventRaised = true;
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                logTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.RaiseDataReceivedEventAsync(json);
        await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing error JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);

        // With both categories set to Collect, exactly one collected exception proves the
        // malformed-but-recognized message was captured once, as a protocol error, and
        // was not additionally reported as an unknown message.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Single(exception.InnerExceptions);
        Assert.False(unknownMessageEventRaised);
    }

    [Fact]
    public async Task TestTransportReportsProtocolErrorForErrorMessageWithMissingMessageProperty()
    {
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error"
                      }
                      """;
        bool unknownMessageEventRaised = false;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageEventRaised = true;
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                logTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.RaiseDataReceivedEventAsync(json);
        await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing error JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);

        // With both categories set to Collect, exactly one collected exception proves the
        // malformed-but-recognized message was captured once, as a protocol error, and
        // was not additionally reported as an unknown message.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Single(exception.InnerExceptions);
        Assert.False(unknownMessageEventRaised);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForEventMessageWithMissingMethod()
    {
        string json = """
                      {
                        "type": "event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForEventMessageWithMissingParams()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event"
                      }
                      """;
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForEventMessageWithUnregisteredEventMethod()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        string loggedEvent = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportReportsProtocolErrorForEventMessageWithMismatchingEventParameters()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "invalidParamName": "paramValue"
                        }
                      }
                      """;
        bool unknownMessageEventRaised = false;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageEventRaised = true;
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                logTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.RaiseDataReceivedEventAsync(json);
        await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing event JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);

        // With both categories set to Collect, exactly one collected exception proves the
        // malformed payload of a registered event was captured once, as a protocol error,
        // and was not additionally reported as an unknown message.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Single(exception.InnerExceptions);
        Assert.False(unknownMessageEventRaised);
    }

    [Fact]
    public async Task TestTransportReportsProtocolErrorForEventMessageWithNullParams()
    {
        // A registered event whose 'params' is JSON null must be reported as a protocol
        // error (surfaced as an error-level log and a capture governed by
        // ProtocolErrorBehavior) — not as an unknown message, and not silently handed to
        // the event dispatch pipeline where it would be misattributed to a user event
        // handler.
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": null
                      }
                      """;
        bool unknownMessageEventRaised = false;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageEventRaised = true;
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                logTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.RaiseDataReceivedEventAsync(json);
        await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing event JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);

        // With both categories set to Collect, exactly one collected exception proves the
        // malformed payload of a registered event was captured once, as a protocol error,
        // and was not additionally reported as an unknown message.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Single(exception.InnerExceptions);
        Assert.False(unknownMessageEventRaised);
    }

    [Fact]
    public async Task TestTransportReportsProtocolErrorForEventMessageDeserializingToNonEventMessageType()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        bool unknownMessageEventRaised = false;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterInvalidEventMessageType("protocol.event", typeof(object));

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageEventRaised = true;
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                logTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await connection.RaiseDataReceivedEventAsync(json);
        await logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Single(logs);
        Assert.Contains("Deserialization of event message returned null", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);

        // With both categories set to Collect, exactly one collected exception proves the
        // malformed payload of a registered event was captured once, as a protocol error,
        // and was not additionally reported as an unknown message.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Single(exception.InnerExceptions);
        Assert.False(unknownMessageEventRaised);
    }

    [Fact]
    public async Task TestTransportCanUseDefaultConnection()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        static void dataReceivedHandler(ServerDataReceivedEventArgs e) { }
        void connectionHandler(ClientConnectionEventArgs e) { taskCompletionSource.TrySetResult(); }
        await using Server server = new();
        ServerEventObserver<ServerDataReceivedEventArgs> dataReceivedObserver = server.OnDataReceived.AddObserver(dataReceivedHandler);
        ServerEventObserver<ClientConnectionEventArgs> connectedObserver = server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();

        await using Transport transport = new();
        await transport.ConnectAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await server.StopAsync();
        dataReceivedObserver.Unobserve();
        connectedObserver.Unobserve();
    }

    [Fact]
    public async Task TestCannotConnectWhenAlreadyConnected()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync($"ws://localhost:1234", TestContext.Current.CancellationToken);
        Assert.StartsWith($"The transport is already connected to ws://localhost:1234", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.ConnectAsync($"ws://localhost:5678", TestContext.Current.CancellationToken))).Message);
    }

    /// <summary>
    /// A connection the caller opened before handing it to the transport is adopted rather than opened
    /// again, so a session can be started over it. The connection string must name the remote end the
    /// connection is already open to, because adopting it opens nothing and so cannot honour any other
    /// value.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestConnectAdoptsAnAlreadyOpenConnection()
    {
        TestWebSocketConnection connection = new();

        // Open the connection first, as a caller supplying an already-open connection would. The
        // bypassed start records the connection string without opening a socket; the override is what
        // makes the connection report itself open to the transport afterwards.
        await connection.StartAsync("ws://localhost:1234", TestContext.Current.CancellationToken);
        connection.IsConnectionOpenOverride = () => true;

        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost:1234", TestContext.Current.CancellationToken);

        Assert.Equal(TransportState.Connected, transport.State);

        // The connection was adopted, not reopened: it is still open to the value it was opened with.
        Assert.Equal("ws://localhost:1234", connection.ConnectionString);
    }

    /// <summary>
    /// The counterpart of the adoption case: a connection string naming a different remote end than the
    /// open connection is rejected rather than silently discarded, and the transport is left
    /// disconnected so a corrected attempt may be made.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestConnectToAnAlreadyOpenConnectionWithADifferentConnectionStringThrows()
    {
        TestWebSocketConnection connection = new();
        await connection.StartAsync("ws://localhost:1234", TestContext.Current.CancellationToken);
        connection.IsConnectionOpenOverride = () => true;

        await using Transport transport = new(connection);

        ArgumentException exception = await Assert.ThrowsAnyAsync<ArgumentException>(
            async () => await transport.ConnectAsync("ws://localhost:5678", TestContext.Current.CancellationToken));
        Assert.Equal("connectionString", exception.ParamName);
        Assert.Contains("already open to 'ws://localhost:1234'", exception.Message);

        // The failed attempt rolls back, so the transport is idle and the open connection is untouched.
        Assert.Equal(TransportState.Disconnected, transport.State);
        Assert.Equal("ws://localhost:1234", connection.ConnectionString);
    }

    /// <summary>
    /// A custom connection whose receive loop ends by reporting an error stays open until it is stopped, but
    /// nothing reads from it any more. Before a connection's activity accounted for its receive loop, the next
    /// connect adopted such a connection as already open, and no command on the new session was ever answered.
    /// The connect must start the connection again instead.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestConnectAfterConnectionErrorStartsAnOpenCustomConnectionInsteadOfAdoptingIt()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;
        TestReportingConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("custom://remote", testCancellationToken);

        // Added after the transport's own observer, which the transport's constructor added, so it is
        // notified once the transport has finished tearing the session down.
        TaskCompletionSource connectionErrorHandled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.OnConnectionError.AddObserver(e =>
        {
            connectionErrorHandled.TrySetResult();
        });

        connection.EndReceiveLoopWithConnectionError(new IOException("Simulated read failure"));
        await connectionErrorHandled.Task.WaitAsync(TimeSpan.FromSeconds(5), testCancellationToken);
        Assert.Equal(TransportState.Disconnected, transport.State);
        Assert.True(connection.IsOpen);
        Assert.False(connection.IsActive);

        await transport.DisconnectAsync(testCancellationToken);
        await transport.ConnectAsync("custom://remote", testCancellationToken);

        Assert.Equal(TransportState.Connected, transport.State);
        Assert.Equal(2, connection.StartConnectionCallCount);
        Assert.True(connection.IsActive);
    }

    [Fact]
    public async Task TestConcurrentConnectAsyncCallsAreSerialized()
    {
        TaskCompletionSource startBarrier = new();
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier
        };
        Transport transport = new(connection);

        Task firstConnect = transport.ConnectAsync("ws://localhost:1234", TestContext.Current.CancellationToken);
        Assert.False(firstConnect.IsCompleted);

        // While the first connect is held open inside the connection's StartAsync, the transport
        // reports the in-flight Connecting state rather than Disconnected or Connected.
        Assert.Equal(TransportState.Connecting, transport.State);

        Task secondConnect = transport.ConnectAsync("ws://localhost:5678", TestContext.Current.CancellationToken);

        startBarrier.SetResult();
        await firstConnect;

        Assert.StartsWith("The transport is already connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await secondConnect)).Message);
    }

    [Fact]
    public async Task TestStateReflectsConnectionLifecycle()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        Assert.Equal(TransportState.Disconnected, transport.State);

        await transport.ConnectAsync("ws://localhost:1234", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, transport.State);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Disconnected, transport.State);
    }

    [Fact]
    public async Task TestFailedConnectAsyncRollsBackStateToDisconnected()
    {
        // A connect attempt that fails after the Connecting state has been published must roll the
        // transport back to Disconnected, so a later ConnectAsync is permitted and no observer is left
        // seeing a stuck Connecting state. Driving the failure through the real WebSocket connect seam
        // exercises the body of ConnectAsync after Connecting is published.
        TestWebSocketConnection connection = new()
        {
            BypassStart = false,
            ConnectWebSocketOverride = (uri, cancellationToken) => throw new WebDriverBiDiException("Simulated connect failure"),
        };
        await using Transport transport = new(connection);

        await Assert.ThrowsAnyAsync<Exception>(async () => await transport.ConnectAsync("ws://localhost:1234", TestContext.Current.CancellationToken));

        Assert.Equal(TransportState.Disconnected, transport.State);

        // The transport is idle again, so a subsequent connect attempt is accepted rather than being
        // rejected as already connected.
        connection.ConnectWebSocketOverride = null;
        connection.BypassStart = true;
        await transport.ConnectAsync("ws://localhost:5678", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, transport.State);
    }

    [Fact]
    public void TestShutdownTimeoutRejectsNegativeValue()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.Throws<ArgumentOutOfRangeException>(() => transport.ShutdownTimeout = TimeSpan.FromMilliseconds(-5));
    }

    [Fact]
    public void TestShutdownTimeoutAllowsInfiniteTimeSpan()
    {
        // Shutdown waits use Task.Delay, which treats Timeout.InfiniteTimeSpan as "no timeout", so an
        // infinite value is a valid "wait indefinitely for message processing to drain" setting.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ShutdownTimeout = Timeout.InfiniteTimeSpan,
        };
        Assert.Equal(Timeout.InfiniteTimeSpan, transport.ShutdownTimeout);
    }

    [Fact]
    public void TestConnectionLockTimeoutDefaultValue()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.Equal(TimeSpan.FromSeconds(60), transport.ConnectionLockTimeout);
    }

    [Fact]
    public void TestConnectionLockTimeoutCanBeSet()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ConnectionLockTimeout = TimeSpan.FromSeconds(5),
        };
        Assert.Equal(TimeSpan.FromSeconds(5), transport.ConnectionLockTimeout);
    }

    [Fact]
    public void TestConnectionLockTimeoutRejectsNegativeValue()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.Throws<ArgumentOutOfRangeException>(() => transport.ConnectionLockTimeout = TimeSpan.FromMilliseconds(-5));
    }

    [Fact]
    public void TestConnectionLockTimeoutAllowsInfiniteTimeSpan()
    {
        // An infinite value restores the unbounded wait the transport used before the bound existed,
        // for a consumer who would rather have a hang than a bounded failure.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ConnectionLockTimeout = Timeout.InfiniteTimeSpan,
        };
        Assert.Equal(Timeout.InfiniteTimeSpan, transport.ConnectionLockTimeout);
    }

    [Fact]
    public async Task TestReentrantCommandFromSynchronousTraceLogObserverTimesOutInsteadOfDeadlocking()
    {
        // The transport holds the connection lock across Connection.SendDataAsync, which raises the
        // connection's own Trace-level "SEND >>>" message before it takes the connection's send
        // semaphore. A synchronous observer of that message that sends a command therefore asks for a
        // lock its own caller holds. Before ConnectionLockTimeout existed the two waited on each other
        // forever, and no command timeout applied, because the deadlock happens inside SendCommandAsync
        // before the command's completion is ever awaited. The nested send must now fail with a
        // WebDriverBiDiTimeoutException, and the send it interrupted must go on to complete.
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
        {
            ConnectionLockTimeout = TimeSpan.FromSeconds(5),
        };
        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        // Route the send through the real Connection.SendDataAsync so that it logs the traffic message,
        // while keeping the connection off an actual socket.
        connection.BypassStart = false;
        connection.IsConnectionOpenOverride = () => true;
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;

        Exception? nestedSendException = null;
        int nestedSendAttempts = 0;
        transport.OnLogMessage.AddObserver(async (e) =>
        {
            if (!e.Message.StartsWith("SEND >>>", StringComparison.Ordinal) ||
                Interlocked.Increment(ref nestedSendAttempts) != 1)
            {
                return;
            }

            try
            {
                await transport.SendCommandAsync(new TestCommandParameters("module.nestedCommand"));
            }
            catch (Exception ex)
            {
                nestedSendException = ex;
            }
        });

        Task<Command> outerSendTask = transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);

        // The nested send's lock wait is the only timer armed here, and it is elapsed on the virtual
        // clock as soon as it is armed, so the test neither waits nor depends on wall-clock timing.
        await timeProvider.AdvanceUntilCompletedAsync(outerSendTask, transport.ConnectionLockTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        Command outerCommand = await outerSendTask;

        Assert.Equal(1, nestedSendAttempts);
        Assert.NotNull(nestedSendException);
        WebDriverBiDiTimeoutException timeoutException = Assert.IsType<WebDriverBiDiTimeoutException>(nestedSendException);
        Assert.Contains("waiting for exclusive access to the connection", timeoutException.Message);
        Assert.Equal("module.command", outerCommand.CommandName);
    }

    [Fact]
    public async Task TestConnectionLockAcquisitionPropagatesCallerCancellation()
    {
        // A caller who cancels while waiting for the lock gets its own cancellation, not the bound's
        // timeout exception. The two are distinguished by the exception filter on the catch, so this
        // covers the case where the filter declines to convert.
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider);
        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        TaskCompletionSource lockHeldTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseLockTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.AfterAcquireLockAsyncCallback = async () =>
        {
            lockHeldTaskCompletionSource.TrySetResult();
            await releaseLockTaskCompletionSource.Task;
        };

        Task<Command> lockHolderTask = transport.SendCommandAsync(new TestCommandParameters("module.lockHolder"), TestContext.Current.CancellationToken);
        await lockHeldTaskCompletionSource.Task.WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);

        using CancellationTokenSource cancellationTokenSource = new();
        int observedTimerCount = timeProvider.TimerCount;
        Task<Command> contenderTask = transport.SendCommandAsync(new TestCommandParameters("module.contender"), cancellationTokenSource.Token);

        // The contender arms its bound immediately before waiting on the lock, so waiting for the timer
        // establishes that it is contending rather than racing ahead of it.
        await timeProvider.WaitForTimerCreatedAsync(observedTimerCount).WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await contenderTask);

        releaseLockTaskCompletionSource.TrySetResult();
        await lockHolderTask;
    }

    [Fact]
    public async Task TestConnectionLostWhileConnectingFailsTheAttempt()
    {
        // The connection's receive loop is live before Connection.StartAsync returns, so the remote end
        // can close while the transport is still Connecting. The loss cannot be handled where it is
        // reported, because there is no session to tear down yet, so it is recorded and fails the
        // attempt. Publishing Connected instead would wedge the transport: the receive loop has already
        // exited, so nothing further would notice, and IsStarted would report true over a dead
        // connection until the caller happened to stop it.
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource startReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
            StartBarrierReached = startReached,
        };
        TestTransport transport = new(connection);

        Task connectTask = transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await startReached.Task.WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connecting, transport.State);

        await connection.RaiseRemoteDisconnectedEventAsync();
        startBarrier.TrySetResult();

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () => await connectTask);
        Assert.Contains("lost while the session was being established", exception.Message);
        WebDriverBiDiConnectionException reportedLoss = Assert.IsType<WebDriverBiDiConnectionException>(exception.InnerException);
        Assert.Contains("Remote end closed the connection", reportedLoss.Message);

        // The attempt rolled back, so the transport is left ready for another one rather than stuck
        // part-way through a session that never started.
        Assert.Equal(TransportState.Disconnected, transport.State);
        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, transport.State);
    }

    [Fact]
    public async Task TestConnectionErrorWhileConnectingFailsTheAttempt()
    {
        // A connection error reported during the same window is recorded through the same path as a
        // remote close, and names itself as the cause of the failed attempt.
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource startReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
            StartBarrierReached = startReached,
        };
        TestTransport transport = new(connection);

        Task connectTask = transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await startReached.Task.WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);

        await connection.RaiseConnectionErrorEventAsync(new InvalidOperationException("Simulated receive failure"));
        startBarrier.TrySetResult();

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () => await connectTask);
        WebDriverBiDiConnectionException reportedLoss = Assert.IsType<WebDriverBiDiConnectionException>(exception.InnerException);
        Assert.Contains("Simulated receive failure", reportedLoss.Message);
        Assert.Equal(TransportState.Disconnected, transport.State);
    }

    [Fact]
    public async Task TestConnectionLostWhileConnectingDrainsBufferedMessages()
    {
        // A message delivered before the loss sits in a queue whose reader is never started, holding a
        // pooled buffer that only its disposal returns. The failing attempt drains and disposes it, and
        // leaves no phantom depth behind for IncomingQueueDepth to report.
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource startReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
            StartBarrierReached = startReached,
        };
        TestTransport transport = new(connection);

        Task connectTask = transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await startReached.Task.WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);
        Assert.Equal(1, transport.IncomingQueueDepth);

        await connection.RaiseRemoteDisconnectedEventAsync();
        startBarrier.TrySetResult();

        await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () => await connectTask);
        Assert.True(owner.IsDisposed, "The buffered message's pooled buffer was not returned by the failed connect attempt.");
        Assert.Equal(0, transport.IncomingQueueDepth);
    }

    [Fact]
    public async Task TestConnectDiscardsMessagesReceivedBeforeConnecting()
    {
        // An adopted connection can deliver before the first connect. Those messages belong to no session,
        // so the connect must discard them, return their pooled buffers, leave no phantom depth, and say
        // how many were lost rather than losing them silently.
        List<LogMessageEventArgs> logs = [];
        bool eventReceived = false;
        bool unknownMessageReceived = false;
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logs)
            {
                logs.Add(e);
            }
        });
        transport.OnEventReceived.AddObserver(e =>
        {
            eventReceived = true;
            return Task.CompletedTask;
        });
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageReceived = true;
            return Task.CompletedTask;
        });

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);
        Assert.Equal(1, transport.IncomingQueueDepth);
        Assert.False(owner.IsDisposed);

        // The discard happens before the new session's reader starts, so the assertions below are settled.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Assert.True(owner.IsDisposed, "The pre-connect message's pooled buffer was not returned by the connect.");
        Assert.Equal(0, transport.IncomingQueueDepth);
        Assert.False(eventReceived, "A message received before the transport connected was dispatched into the new session.");
        Assert.False(unknownMessageReceived);
        lock (logs)
        {
            Assert.Contains(logs, log => log.Message.Contains("Discarded 1 message(s) that arrived before the transport connected") && log.Level == WebDriverBiDiLogLevel.Warn);
        }

        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestDisposeDiscardsMessagesReceivedBeforeConnecting()
    {
        // Nothing ever reads a never-connected transport's queue, so only disposal returns its buffers.
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logs)
            {
                logs.Add(e);
            }
        });

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);
        Assert.Equal(1, transport.IncomingQueueDepth);

        await transport.DisposeAsync();

        Assert.True(owner.IsDisposed, "The pre-connect message's pooled buffer was not returned by disposal.");
        Assert.Equal(0, transport.IncomingQueueDepth);
        lock (logs)
        {
            Assert.Contains(logs, log => log.Message.Contains("Discarded 1 message(s) that arrived before the transport connected and were still buffered at disposal") && log.Level == WebDriverBiDiLogLevel.Warn);
        }
    }

    [Fact]
    public async Task TestReconnectDiscardsNothingAndReportsNoLoss()
    {
        // The connect path's drain must not accuse an ordinary reconnect of losing messages: the previous
        // queue has a reader, so it is skipped entirely and nothing is reported.
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logs)
            {
                logs.Add(e);
            }
        });

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync("""{"type":"event","method":"protocol.event","params":{}}""");
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Assert.Equal(0, transport.IncomingQueueDepth);
        lock (logs)
        {
            Assert.DoesNotContain(logs, log => log.Message.Contains("Discarded"));
        }

        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestConnectionLostAfterConnectionStartReturnsFailsTheAttempt()
    {
        // The loss is raised from the "connection opened" log message, which Connection.StartAsync
        // emits after StartConnectionAsync has returned and the receive task has started — the latest
        // point reachable while the transport is still Connecting. The sibling tests park inside
        // StartConnectionAsync, so their loss is recorded much earlier.
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);

        bool lossRaised = false;
        using EventObserver<LogMessageEventArgs> connectionLogObserver = connection.OnLogMessage.AddObserver(async e =>
        {
            if (!lossRaised && e.Message == "WebSocket connection opened")
            {
                lossRaised = true;
                await connection.RaiseRemoteDisconnectedEventAsync();
            }
        });

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () => await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken));
        Assert.Contains("lost while the session was being established", exception.Message);
        WebDriverBiDiConnectionException reportedLoss = Assert.IsType<WebDriverBiDiConnectionException>(exception.InnerException);
        Assert.Contains("Remote end closed the connection", reportedLoss.Message);

        Assert.True(lossRaised, "The connection never emitted the log message the loss was raised from.");
        Assert.Equal(TransportState.Disconnected, transport.State);

        // The record is consumed by the failing attempt rather than left behind, so the next attempt
        // is judged only by what happens to its own connection.
        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, transport.State);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionLossReportedAfterConnectedIsPublishedTearsDownTheSession()
    {
        // The counterpart: once Connected is published the handler must stop recording and tear the
        // session down. The pending command is the proof it did — only FailAllPendingCommands faults it.
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, transport.State);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        Assert.Equal(TransportState.Disconnected, transport.State);
        Assert.NotNull(command.ThrownException);
        Assert.Contains("Remote end closed the connection", command.ThrownException.Message);

        // Nothing was recorded against a connect attempt, so a later attempt is unaffected by it.
        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, transport.State);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionLockAcquisitionThrowsWhenTokenAlreadyCanceled()
    {
        // The lock is free here, so this covers the guard that keeps the uncontended fast path from
        // taking the lock for a caller that has already given up.
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationTokenSource.Token));
    }

    [Fact]
    public async Task TestDisconnectWhenNotConnectedDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestDisconnectWithMultipleConcurrentCallsOperatesCorrectly()
    {
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        _ = transport.EnableConnectLockConcurrencyTesting();

        Task task1 = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Task task2 = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await Task.WhenAll(task1, task2);

        Assert.Equal(1, connection.StopCallCount);
        Assert.Equal(2, transport.ConcurrentConnectLockAcquisitions);
    }

    [Fact]
    public async Task TestDisconnectMultipleTimesAfterAlreadyDisconnectedHitsFastPath()
    {
        // This test verifies that calling disconnect on an already-disconnected transport
        // returns immediately via the fast-path check without acquiring the semaphore
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // First disconnect - this sets State to Disconnected
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        // Verify the first disconnect executed fully
        Assert.Equal(1, connection.StopCallCount);

        // Second disconnect - should hit the fast-path check and return immediately
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        // The fast-path check prevents the disconnect logic from executing
        Assert.Equal(1, connection.StopCallCount);

        // Third call for good measure
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Assert.Equal(1, connection.StopCallCount);
    }

    [Fact]
    public async Task TestTransportDisconnectWithPendingIncomingMessagesWillProcess()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TaskCompletionSource processingReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            MessageProcessingStarted = () => processingReached.TrySetResult(),
            MessageProcessingGate = () => gate.Task,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            receivedName = e.EventName;
            receivedData = e.EventData;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);

        // The event is held inside the processing loop when the disconnect begins, so the disconnect
        // must process it before completing; releasing the gate afterwards lets that happen without
        // a timed delay.
        await processingReached.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        gate.TrySetResult();
        await disconnectTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal("protocol.event", receivedName);
        Assert.IsType<TestEventArgs>(receivedData);

        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.NotNull(convertedData);
        Assert.Equal("paramValue", convertedData.ParamName);
    }

    [Fact]
    public async Task TestTransportCanReuseConnectionToDifferentUrl()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" }
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters }
        };

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://example.com:1234", TestContext.Current.CancellationToken);

        TestCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command, TestContext.Current.CancellationToken);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        // Command IDs continue across the reconnect rather than restarting.
        await transport.ConnectAsync("ws://example.com:5678", TestContext.Current.CancellationToken);
        _ = await transport.SendCommandAsync(command, TestContext.Current.CancellationToken);

        expected["id"] = 2;
        dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestExceptionInTransportEventReceivedCanCollect()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
            throw new WebDriverBiDiException("This is an unexpected exception");
        });
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("This is an unexpected exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestExceptionInTransportEventReceivedCanCollectMultiple()
    {
        TaskCompletionSource firstEventTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondEventTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int callCount = 0;

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            try
            {
                throw new WebDriverBiDiException("This is an unexpected exception");
            }
            finally
            {
                if (Interlocked.Increment(ref callCount) == 1)
                {
                    firstEventTaskCompletionSource.TrySetResult();
                }
                else
                {
                    secondEventTaskCompletionSource.TrySetResult();
                }
            }
        });
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await firstEventTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await secondEventTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.All(exception.InnerExceptions, e => Assert.IsType<WebDriverBiDiException>(e));
        Assert.All(exception.InnerExceptions, e => Assert.Contains("This is an unexpected exception", e.Message));
    }

    [Fact]
    public async Task TestAsyncFaultingObserverOfEventHandlerErrorOccurredDoesNotCauseFeedbackLoop()
    {
        // A failure in an observer of OnEventHandlerErrorOccurred is captured without
        // re-raising OnEventHandlerErrorOccurred; re-raising would re-invoke the same
        // failing observer again in an unbounded feedback loop of error events.
        int errorObserverInvocationCount = 0;
        int capturedErrorCount = 0;
        TaskCompletionSource secondCaptureTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () =>
            {
                if (Interlocked.Increment(ref capturedErrorCount) == 2)
                {
                    secondCaptureTaskCompletionSource.TrySetResult();
                }
            },
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(
            async e =>
            {
                await Task.Yield();
                throw new WebDriverBiDiException("original handler failure");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        transport.OnEventHandlerErrorOccurred.AddObserver(
            async e =>
            {
                Interlocked.Increment(ref errorObserverInvocationCount);
                await Task.Yield();
                throw new WebDriverBiDiException("error observer failure");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);

        // The original handler failure is captured after the error event is raised; the
        // error observer's own asynchronous fault is captured without re-raising.
        await secondCaptureTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(1, Volatile.Read(ref errorObserverInvocationCount));
        Assert.Equal(2, Volatile.Read(ref capturedErrorCount));

        // A feedback loop would keep re-invoking the error observer and capturing further errors,
        // every one of which would surface as an extra inner exception on disconnect; the exact
        // count below is the deterministic negative check.

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("original handler failure"));
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("error observer failure"));
    }

    [Fact]
    public async Task TestSyncFaultingObserverOfEventHandlerErrorOccurredDoesNotDerailErrorCapture()
    {
        // A synchronously throwing observer of OnEventHandlerErrorOccurred must not
        // prevent the original handler failure from being captured, must have its own
        // failure captured as an event-handler error (rather than escaping to the
        // message loop, where it would be classified as a protocol error), and must
        // not cause a feedback loop of error events.
        int errorObserverInvocationCount = 0;
        int capturedErrorCount = 0;
        TaskCompletionSource secondCaptureTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () =>
            {
                if (Interlocked.Increment(ref capturedErrorCount) == 2)
                {
                    secondCaptureTaskCompletionSource.TrySetResult();
                }
            },
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e => throw new WebDriverBiDiException("original handler failure"));
        transport.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            Interlocked.Increment(ref errorObserverInvocationCount);
            throw new WebDriverBiDiException("error observer failure");
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);

        // Both the original handler failure and the error observer's own failure are
        // captured, the latter without re-raising the error event.
        await secondCaptureTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(1, Volatile.Read(ref errorObserverInvocationCount));
        Assert.Equal(2, Volatile.Read(ref capturedErrorCount));

        // A feedback loop would keep re-invoking the error observer and capturing further errors,
        // every one of which would surface as an extra inner exception on disconnect; the exact
        // count below is the deterministic negative check.

        // Only EventHandlerExceptionBehavior is set to Collect here, so both inner
        // exceptions surfacing on disconnect proves both failures were captured under
        // the event-handler category rather than any other.
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("original handler failure"));
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("error observer failure"));
    }

    [Fact]
    public async Task TestRacedDrainAttributesLateFaultToConcreteEventName()
    {
        // A fault surfacing through a continuation attached by the raced-task drain in
        // WaitForCapturedTasksAsync is attributed to the concrete protocol event that
        // produced the invocation, the same as a fault reported through the
        // notification-time continuation — not to the generic observable's name.
        TaskCompletionSource bothCapturedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<EventObserverErrorInfo> reportedErrorTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource handlerGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int observedEventCount = 0;
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventHandlerErrorOccurred.AddObserver(e => reportedErrorTaskCompletionSource.TrySetResult(e.ErrorInfo));
        EventObserver<EventReceivedEventArgs> observer = transport.OnEventReceived.AddObserver(
            async e =>
            {
                await handlerGate.Task;
                throw new WebDriverBiDiException("late handler failure");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        // Observers are notified in addition order, so when this second observer has seen
        // both events, the first observer's tasks for both events have been captured.
        transport.OnEventReceived.AddObserver(e =>
        {
            if (Interlocked.Increment(ref observedEventCount) == 2)
            {
                bothCapturedTaskCompletionSource.TrySetResult();
            }
        });
        observer.StartCapturingTasks();

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await connection.RaiseDataReceivedEventAsync(json);
        await bothCapturedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The wait consumes one buffered task and auto-closes; the drain attaches a
        // reporting continuation to the surplus one.
        Task[] capturedTasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        _ = Assert.Single(capturedTasks);

        // Release the gate so both handler tasks fault late. The caller-owned task's
        // fault stays with the caller; the surplus task's fault is reported through the
        // drain-attached continuation, attributed to the concrete protocol event name.
        handlerGate.TrySetResult();
        EventObserverErrorInfo errorInfo = await reportedErrorTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("protocol.event", errorInfo.ObservableEventName);
        Assert.Contains("late handler failure", errorInfo.Exception.Message);

        observer.Dispose();
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestDisposeDuringInFlightConnectSerializesWithConnectAttempt()
    {
        // Disposing a transport whose connect attempt is still in flight must not tear the
        // semaphore and connection out from under the attempt. Disposal waits for the
        // attempt to complete, then tears down the connected transport normally.
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
        };
        TestTransport transport = new(connection);

        // ConnectAsync runs synchronously (holding the connection lock) until it awaits the
        // connection's start barrier, so when the call returns a pending task the transport
        // is deterministically mid-connect.
        Task connectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connecting, transport.State);

        // Disposal must be blocked waiting for the in-flight attempt, not proceeding.
        ValueTask disposeTask = transport.DisposeAsync();
        Assert.False(disposeTask.IsCompleted);

        startBarrier.SetResult();
        await connectTask;
        await disposeTask;

        Assert.Equal(TransportState.Disconnected, transport.State);
        Assert.True(transport.IsDisposed);
    }

    [Fact]
    public async Task TestDisposeDuringStuckConnectTimesOutAndProceeds()
    {
        // When the in-flight connect attempt never completes, disposal must not deadlock:
        // after ShutdownTimeout it logs a warning and proceeds with the teardown, exactly
        // as disposal behaved before the serialization was added.
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
        };
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logs)
            {
                logs.Add(e);
            }
        });

        Task connectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connecting, transport.State);

        // The shutdown timeout is elapsed on the virtual clock as soon as disposal arms it.
        Task disposeTask = transport.DisposeAsync().AsTask();
        await timeProvider.AdvanceUntilCompletedAsync(disposeTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await disposeTask;

        Assert.True(transport.IsDisposed);
        lock (logs)
        {
            Assert.Contains(logs, log => log.Message.Contains("Timed out waiting for an in-flight connect attempt to complete during disposal") && log.Level == WebDriverBiDiLogLevel.Warn);
        }

        // Release the stuck attempt; it now completes against a disposed transport and
        // faults (its finally releases the disposed connection lock), which is the same
        // degradation the pre-serialization disposal produced for this pathological case.
        startBarrier.SetResult();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await connectTask);
    }

    [Fact]
    public async Task TestDisposeDuringStuckConnectProceedsWhenConnectionLockTimeoutIsZero()
    {
        // Disposal waits for an in-flight connect attempt through the connection lock, whose wait
        // is bounded by ConnectionLockTimeout as well as by ShutdownTimeout, and reports the former
        // bound as a WebDriverBiDiTimeoutException rather than a cancellation. With a zero lock
        // timeout the wait ends at once on that path, and disposal must still proceed: log the same
        // warning, tear the transport down, and dispose the connection and the semaphore.
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
        };
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
            ConnectionLockTimeout = TimeSpan.Zero,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logs)
            {
                logs.Add(e);
            }
        });

        Task connectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connecting, transport.State);

        // A zero lock timeout is armed as an already-elapsed timer, so the disposal completes without
        // the shutdown timeout ever being reached on the virtual clock.
        Task disposeTask = transport.DisposeAsync().AsTask();
        await timeProvider.AdvanceUntilCompletedAsync(disposeTask, TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await disposeTask;

        Assert.True(transport.IsDisposed);
        Assert.True(connection.Disposed);
        lock (logs)
        {
            Assert.Contains(logs, log => log.Message.Contains("Timed out waiting for an in-flight connect attempt to complete during disposal") && log.Level == WebDriverBiDiLogLevel.Warn);
        }

        // The stuck attempt completes against a disposed transport and faults, as in the
        // shutdown-timeout case above.
        startBarrier.SetResult();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await connectTask);
    }

    [Fact]
    public async Task TestDisposeDuringStuckConnectProceedsWhenConnectionLockTimeoutIsShorterThanShutdownTimeout()
    {
        // The same path as the zero-timeout case, but with a lock timeout that is finite and shorter
        // than the shutdown timeout: the lock timeout elapses first on the virtual clock, ends the
        // wait as a WebDriverBiDiTimeoutException, and disposal proceeds on that path.
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier,
        };
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
            ConnectionLockTimeout = TimeSpan.FromSeconds(5),
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logs)
            {
                logs.Add(e);
            }
        });

        Task connectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connecting, transport.State);

        // Advance by the lock timeout only, which is well short of the shutdown timeout, so the wait
        // can only have ended on the lock-timeout path.
        Task disposeTask = transport.DisposeAsync().AsTask();
        await timeProvider.AdvanceUntilCompletedAsync(disposeTask, transport.ConnectionLockTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await disposeTask;

        Assert.True(transport.IsDisposed);
        Assert.True(connection.Disposed);
        lock (logs)
        {
            Assert.Contains(logs, log => log.Message.Contains("Timed out waiting for an in-flight connect attempt to complete during disposal") && log.Level == WebDriverBiDiLogLevel.Warn);
        }

        startBarrier.SetResult();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await connectTask);
    }

    [Fact]
    public async Task TestExceptionInTransportEventReceivedCanTerminate()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e => throw new WebDriverBiDiException("This is an unexpected exception"));
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken));
        Assert.Contains("protocol.event", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("This is an unexpected exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestAsyncExceptionInTransportEventReceivedCanCollect()
    {
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(async e =>
        {
            try
            {
                await Task.Yield();
                throw new WebDriverBiDiException("This is an async unexpected exception");
            }
            finally
            {
                handlerCompleted.TrySetResult(true);
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Collect);
        Assert.True(errorPropagated);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("This is an async unexpected exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestAsyncExceptionInTransportEventReceivedCanTerminate()
    {
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(async e =>
        {
            try
            {
                await Task.Yield();
                throw new WebDriverBiDiException("This is an async unexpected exception");
            }
            finally
            {
                handlerCompleted.TrySetResult(true);
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously);
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Terminate);
        Assert.True(errorPropagated);

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken));
        Assert.Contains("protocol.event", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("This is an async unexpected exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestCapturedExceptionsCanBeReset()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            return Task.FromException(new WebDriverBiDiException("This is an unexpected exception"));
        });
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" }
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters }
        };
        TestCommandParameters commandParameters = new(commandName);
        _ = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
    }

    [Fact]
    public async Task TestTransportTracksCommandId()
    {
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Equal(0, transport.LastTestCommandId);

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
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

        // Awaited here so that a fault inside the Task is reported as itself rather
        // than as whichever assertion below fails first.
        await responseTask;

        Assert.Equal(1, transport.LastTestCommandId);
    }

    [Fact]
    public async Task TestTransportSubclassesCanAccessConnection()
    {
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        Assert.Equal(connection, transport.GetConnection());
    }

    [Fact]
    public async Task TestTransportShutdownTimeoutDefaultValue()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        Assert.Equal(TimeSpan.FromSeconds(10), transport.ShutdownTimeout);
    }

    [Fact]
    public async Task TestTransportShutdownTimeoutCanBeSet()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1)
        };
        Assert.Equal(TimeSpan.FromSeconds(1), transport.ShutdownTimeout);
    }

    [Fact]
    public async Task TestTransportIncomingQueueDepthReflectsPendingMessages()
    {
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource handlerMayCompleteTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        // Pre-connect: documented to return 0 rather than throw.
        Assert.Equal(0, transport.IncomingQueueDepth);

        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            handlerStartedTaskCompletionSource.TrySetResult();
            return handlerMayCompleteTaskCompletionSource.Task;
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Raise the first message and wait until the reader has pulled it and begun
        // running the handler. At this point the reader is blocked inside the handler
        // and will not consume additional queued messages until the gate is released.
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Enqueue two more messages while the reader is stalled. Writer.WriteAsync
        // completes synchronously for an unbounded channel, so these are observable
        // in the queue immediately.
        await connection.RaiseDataReceivedEventAsync(json);
        await connection.RaiseDataReceivedEventAsync(json);

        Assert.Equal(2, transport.IncomingQueueDepth);

        // Release the handler gate so the reader can drain.
        handlerMayCompleteTaskCompletionSource.TrySetResult();

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        // DisconnectAsync awaits Reader.Completion before returning, so the queue
        // has drained by this point and IncomingQueueDepth must be 0. This also
        // exercises the documented post-disconnect read-without-throw contract.
        Assert.Equal(0, transport.IncomingQueueDepth);
    }

    [Fact]
    public async Task TestTransportPendingCommandCountIsZeroBeforeConnect()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        // Documented behavior: reads before ConnectAsync return zero rather than throw.
        Assert.Equal(0, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestTransportPendingCommandCountReflectsSentCommands()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Assert.Equal(0, transport.PendingCommandCount);

        Command firstCommand = await transport.SendCommandAsync(new TestCommandParameters("module.first"), TestContext.Current.CancellationToken);
        Assert.Equal(1, transport.PendingCommandCount);

        Command secondCommand = await transport.SendCommandAsync(new TestCommandParameters("module.second"), TestContext.Current.CancellationToken);
        Assert.Equal(2, transport.PendingCommandCount);

        // Complete the first command by delivering its matching success response.
        string firstResponseJson = $$$"""{"type":"success","id":{{{firstCommand.CommandId}}},"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(firstResponseJson);
        await firstCommand.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(1, transport.PendingCommandCount);

        // Complete the second command similarly.
        string secondResponseJson = $$$"""{"type":"success","id":{{{secondCommand.CommandId}}},"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(secondResponseJson);
        await secondCommand.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(0, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestDataReceivedAfterDisconnectIsDisposedAndNotQueued()
    {
        // DisconnectAsync completes the incoming message channel's writer. A connection whose
        // receive loop outlives StopAsync (PipeConnection abandons the loop after its
        // ShutdownTimeout) can still deliver data afterwards. That data must not throw, must not
        // be counted in the queue depth, and its pooled buffer must be returned via disposal.
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0, transport.IncomingQueueDepth);

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"module.event","params":{}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);

        Assert.True(owner.IsDisposed);
        Assert.Equal(0, transport.IncomingQueueDepth);
    }

    [Fact]
    public async Task TestTransportPendingCommandCountIsZeroAfterDisconnect()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Send a command and do not deliver a response, so it sits in the pending collection.
        _ = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.Equal(1, transport.PendingCommandCount);

        // DisconnectAsync closes and clears the pending command collection; the property
        // must reflect the cleared state and must not throw post-disconnect.
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Assert.Equal(0, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestMessageProcessingTaskFaultIsCapturedAsUnhandledError()
    {
        // This test exercises the fault continuation attached to
        // messageQueueProcessingTask in Transport.ConnectAsync. Under normal operation
        // the outer await in ReadIncomingMessagesAsync never faults — the per-message
        // try/catch inside that method handles everything else. The continuation is
        // defence-in-depth; this test simulates an unrecoverable outer-loop fault by
        // having TestTransport.ReadIncomingMessagesAsync return an already-faulted task.
        //
        // The fault propagation is asynchronous: Task.Run(() => ...) schedules the
        // lambda on the thread pool, so the returned messageQueueProcessingTask
        // transitions to Faulted on a pool thread after ConnectAsync returns. The
        // fault-capture continuation runs at that moment. We use the existing polling
        // helper to wait deterministically (bounded by a safety timeout) for the fault
        // to appear in the UnhandledErrors collection.
        InvalidOperationException injectedFault = new("simulated outer-loop fault");
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [injectedFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        bool faultCaptured = await transport.WaitForCollectedEventHandlerExceptionAsync(
            TimeSpan.FromSeconds(5),
            TransportErrorBehavior.Collect);
        if (!faultCaptured)
        {
            throw new XunitException("the fault-capture continuation should record the injected fault before the safety timeout");
        }

        // Under Collect mode, DisconnectAsync surfaces the captured fault as an
        // AggregateException whose single inner exception wraps the injected fault.
        AggregateException? caught = await Assert.ThrowsAsync<AggregateException>(
            async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));

        Assert.NotNull(caught);
        Assert.Single(caught.InnerExceptions);
        Assert.Same(injectedFault, caught.InnerExceptions[0]);
    }

    [Fact]
    public async Task TestMessageProcessingTaskFaultWithMultipleInnerExceptionsIsCapturedAsAggregate()
    {
        // Companion to TestMessageProcessingTaskFaultIsCapturedAsUnhandledError.
        // Covers the Count != 1 branch in Transport.LogMessageProcessingFault,
        // where the faulted processing task carries more than one inner
        // exception. In that branch the continuation forwards the whole
        // AggregateException rather than unwrapping to a single inner.
        InvalidOperationException firstFault = new("first simulated outer-loop fault");
        ArgumentException secondFault = new("second simulated outer-loop fault");
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [firstFault, secondFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        bool faultCaptured = await transport.WaitForCollectedEventHandlerExceptionAsync(
            TimeSpan.FromSeconds(5),
            TransportErrorBehavior.Collect);
        if (!faultCaptured)
        {
            throw new XunitException("the fault-capture continuation should record the injected faults before the safety timeout");
        }

        // Under Collect mode, DisconnectAsync surfaces the captured fault as an
        // outer AggregateException. Because the captured fault was already an
        // AggregateException with multiple inner exceptions, the library
        // forwarded it whole — so the outer aggregate has a single inner that
        // is itself an AggregateException containing both injected faults.
        AggregateException? caught = await Assert.ThrowsAsync<AggregateException>(
            async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.NotNull(caught);
        Assert.Single(caught.InnerExceptions);
        AggregateException? forwardedAggregate = caught.InnerExceptions[0] as AggregateException;
        Assert.NotNull(forwardedAggregate);

        Assert.Equal(2, forwardedAggregate.InnerExceptions.Count);
        Assert.Contains(firstFault, forwardedAggregate.InnerExceptions);
        Assert.Contains(secondFault, forwardedAggregate.InnerExceptions);
    }

    [Fact]
    public async Task TestTransportDisconnectTimesOutWithHangingEventHandler()
    {
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            handlerStartedTaskCompletionSource.TrySetResult();
            return new TaskCompletionSource<bool>().Task;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The shutdown timeout is elapsed on the virtual clock as soon as the disconnect arms it.
        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(disconnectTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await disconnectTask;

        Assert.Contains(logs,
            log => log.Message.Contains("Timed out waiting for message processing to complete during shutdown")
                   && log.Level == WebDriverBiDiLogLevel.Warn);
    }

    [Fact]
    public async Task TestTransportDisconnectTimesOutWithHangingEventHandlerAndQueuedMessages()
    {
        // Regression test for the shutdown liveness defect where DisconnectAsync
        // waited unbounded on the incoming message queue draining. A handler that
        // never completes suspends the reader task while it is processing the first
        // message, so a second message written to the queue is never read. The
        // queue-drain wait must time out (within the shared ShutdownTimeout budget)
        // rather than hanging forever; the message-processing wait then short-circuits
        // on the already-elapsed timeout and logs its own warning.
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            handlerStartedTaskCompletionSource.TrySetResult();
            return new TaskCompletionSource<bool>().Task;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // With the reader task suspended in the hanging handler, this second
        // message is guaranteed to remain unread in the incoming message queue,
        // so the queue can never drain during shutdown.
        await connection.RaiseDataReceivedEventAsync(json);

        // The shutdown timeout is elapsed on the virtual clock as soon as the disconnect arms it.
        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(disconnectTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await disconnectTask;

        Assert.Contains(logs,
            log => log.Message.Contains("Timed out waiting for message writer to complete during shutdown")
                   && log.Level == WebDriverBiDiLogLevel.Warn);
        Assert.Contains(logs,
            log => log.Message.Contains("Timed out waiting for message processing to complete during shutdown")
                   && log.Level == WebDriverBiDiLogLevel.Warn);
    }

    [Fact]
    public async Task TestTransportDisconnectCompletesWithinShutdownTimeout()
    {
        List<LogMessageEventArgs> logs = [];

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        Assert.DoesNotContain(logs,
            log => log.Message.Contains("Timed out waiting for message processing to complete during shutdown"));
    }

    [Fact]
    public async Task TestCanDisposeWithoutConnecting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestCanDisposeAfterConnectAndDisconnect()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestCanDisposeWhileConnected()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestDoubleDisposeDoesNotThrowWhenTheFirstDisposalLeftAConnectAttemptInFlight()
    {
        // A connect attempt that does not complete within ShutdownTimeout leaves the transport in the
        // Connecting state after disposal, because disposal gives up waiting for it and proceeds. The
        // teardown that ran disposed the semaphore that guards the connection, so a second disposal
        // that repeated the teardown would wait on a disposed semaphore and throw
        // ObjectDisposedException out of a method that IAsyncDisposable requires to ignore repeated
        // calls. Disposal is therefore performed once and once only.
        TaskCompletionSource startBarrier = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource startReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new(timeProvider)
        {
            StartBarrier = startBarrier,
            StartBarrierReached = startReached,
        };
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1),
        };

        // Hold the connect inside the connection, so that the transport is provably Connecting rather
        // than merely likely to be.
        Task connectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await startReached.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connecting, transport.State);

        // The first disposal waits for the attempt, bounded by ShutdownTimeout on the virtual clock,
        // then proceeds without it.
        Task firstDisposeTask = transport.DisposeAsync().AsTask();
        await timeProvider.AdvanceUntilCompletedAsync(firstDisposeTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await firstDisposeTask;
        Assert.Equal(TransportState.Connecting, transport.State);

        // The second disposal must do nothing at all, rather than repeat a teardown whose resources
        // are already gone.
        await transport.DisposeAsync();

        startBarrier.TrySetResult();
        await Assert.ThrowsAnyAsync<Exception>(async () => await connectTask);
    }

    [Fact]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.DisposeAsync();
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestCanDisposeDefaultTransport()
    {
        Transport transport = new();
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestDisposeDisposesOldPendingCommandsAfterReconnect()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Command oldCommand = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.Equal(1, transport.PendingCommandCount);

        // Disconnecting clears the pending collection, canceling the command it still held.
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Assert.True(oldCommand.IsCanceled);
        Assert.Equal(0, transport.PendingCommandCount);

        // A reconnected transport starts with an empty collection and accepts new commands.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Command newCommand = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.False(newCommand.IsCanceled);
        Assert.Equal(1, transport.PendingCommandCount);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Assert.True(newCommand.IsCanceled);
        Assert.Equal(0, transport.PendingCommandCount);

        // Once disposed, the transport accepts no further commands.
        await transport.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestDisposeSuppressesDisconnectException()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.ThrowOnDisconnect = true;
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestDisposeLogsExceptionFromDisconnect()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.ThrowOnDisconnect = true;
        await transport.DisposeAsync();
        Assert.Contains(logs,
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated disconnect failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == Transport.LoggerComponentName);
    }

    [Fact]
    public async Task TestConnectingAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestSendingCommandAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestMessageProcessingLoopContinuesAfterUnhandledException()
    {
        string commandName = "module.command";
        List<LogMessageEventArgs> logs = [];

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        transport.DeserializeThrowCount = 1;

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        await connection.RaiseDataReceivedEventAsync("this message will cause the exception");

        string responseJson = """
                              {
                                "type": "success",
                                "id": 1,
                                "result": {
                                  "value": "response value"
                                }
                              }
                              """;
        Task responseTask = Task.Run(async () => await connection.RaiseDataReceivedEventAsync(responseJson), TestContext.Current.CancellationToken);

        bool commandCompleted = await command.WaitForCompletionAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);
        Assert.True(commandCompleted);

        // Awaited here so that a fault inside the Task is reported as itself rather
        // than as whichever assertion below fails first.
        await responseTask;

        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.True(hasResult);
        Assert.NotNull(commandResult);

        Assert.False(commandResult.IsError);
        Assert.IsType<TestCommandResult>(commandResult);
        Assert.Contains(logs,
            log => log.Message.Contains("Unexpected error in message processing loop")
                   && log.Message.Contains("Simulated deserialization failure")
                   && log.Level == WebDriverBiDiLogLevel.Error);
    }

    [Fact]
    public async Task TestMessageProcessingLoopExceptionCapturedAsUnhandledError()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level == WebDriverBiDiLogLevel.Error)
            {
                taskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        transport.DeserializeThrowCount = 1;
        await connection.RaiseDataReceivedEventAsync("this message will cause the exception");
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Contains("Simulated deserialization failure", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestCancelCommandRemovesFromPendingAndCancelsCommand()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.False(command.IsCanceled);

        transport.CancelCommand(command);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);

        Assert.True(command.IsCanceled);
        Assert.False(hasResult);
        Assert.Null(commandResult);
    }

    [Fact]
    public async Task TestCancelCommandIsIdempotent()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        transport.CancelCommand(command);
        transport.CancelCommand(command);
    }

    [Fact]
    public async Task TestCancelCommandPreventsLateResponseFromSettingResult()
    {
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
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationToken: TestContext.Current.CancellationToken);
        transport.CancelCommand(command);

        string responseJson = $$$"""{"type":"success","id":{{{command.CommandId}}},"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(responseJson);
        await discardedTaskCompletionSource.Task.WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);

        bool hasResult = command.TryGetResult(out CommandResult? commandResult);

        Assert.True(command.IsCanceled);
        Assert.False(hasResult);
        Assert.Null(commandResult);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverBeforeConnecting()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverMultipleTimesBeforeConnecting()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverAfterConnectingThrows()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Contains("Cannot register a type info resolver after the transport is connected", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRegisterTypeInfoDuringConnectIsSynchronized()
    {
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        Task firstCallerReadyTask = transport.EnableConnectLockConcurrencyTesting();

        // Start ConnectAsync first; wait until it has entered the lock callback before
        // starting RegisterTypeInfoResolverAsync. This guarantees ConnectAsync acquires
        // the semaphore first and sets State before RegisterTypeInfoResolverAsync
        // reads it, making the test deterministic regardless of thread scheduling.
        Task connectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await firstCallerReadyTask;
        Task registerTask = transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
        await connectTask;

        Assert.Contains("Cannot register a type info resolver after the transport is connected", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await registerTask)).Message);
    }

    [Fact]
    public async Task TestRegisterNullTypeInfoThrows()
    {
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await transport.RegisterTypeInfoResolverAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestRegisterTypeInfoOnDisposedTranportThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.DisposeAsync();
        ObjectDisposedException exception = await Assert.ThrowsAsync<ObjectDisposedException>(async () => await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken));

        // Asserting the exception type alone would not test the guard, so let's
        // also assert the type of the object disposed.
        Assert.Equal(transport.GetType().FullName, exception.ObjectName);
    }

    [Fact]
    public async Task TestConstructionWithConnectionHavingExistingDataReceivedObserverThrows()
    {
        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(e => { });
        Assert.Throws<ArgumentException>(() => new Transport(connection));
    }

    [Fact]
    public async Task TestConstructionWithNullConnectionThrows()
    {
        WebSocketConnection connection = new();
        connection.OnDataReceived.AddObserver(e => { });
        Assert.Throws<ArgumentNullException>(() => new Transport(null!));
    }

    [Fact]
    public async Task TestConnectionErrorFailsPendingCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        Exception simulatedError = new("WebSocket connection dropped");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);

        Assert.IsType<WebDriverBiDiConnectionException>(command.ThrownException);
        Assert.Contains("Unexpected connection error", command.ThrownException.Message);
        Assert.Same(simulatedError, command.ThrownException.InnerException);
    }

    [Fact]
    public async Task TestConnectionErrorPreventsNewCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Exception simulatedError = new("WebSocket connection dropped");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);

        TestCommandParameters commandParameters = new(commandName);
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionErrorLogsMessage()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Exception simulatedError = new("WebSocket connection dropped");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);

        Assert.Contains(logs,
            log => log.Message.Contains("Connection error; pending commands failed")
                   && log.Message.Contains("WebSocket connection dropped")
                   && log.Level == WebDriverBiDiLogLevel.Error);
    }

    [Fact]
    public async Task TestConnectionErrorWhenNotConnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        // Never call ConnectAsync - State remains Disconnected
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost"));

        // Should not throw; early return path taken. Verify transport rejects commands.
        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionErrorWhenAlreadyDisconnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        // State is now Disconnected; raise error (e.g., receive loop dying during shutdown)
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost"));

        // Should not throw; early return path taken. Verify still disconnected.
        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionErrorWhenDisconnectRacesHitsDisconnectOwnershipBranch()
    {
        // Covers the disconnect-ownership signal branch of HandleConnectionDisconnectionAsync:
        // OnConnectionErrorAsync passes the fast-path, then observes DisconnectAsync's ownership
        // signal completing before it acquires the lock, and returns without tearing down (handing
        // the lock back through its completion continuation). The inner "if (this.State !=
        // TransportState.Connected) return" branch is covered separately by
        // TestConcurrentConnectionLossEventsHitInnerReturnBranch.
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        _ = transport.EnableConnectLockConcurrencyTesting();

        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost during race"));
        await disconnectTask;

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);

        // The handler's outstanding wait was granted once the disconnect released the lock, and the handler hands it
        // straight back. The command above fails on the transport's state before it would take the lock, so it cannot
        // show that. An operation that always takes the lock, with no bound on the wait, completes only if the lock
        // was handed back; the outer bound only turns a lock that never comes back into a failure rather than a hang.
        transport.ConnectionLockTimeout = Timeout.InfiniteTimeSpan;
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken).WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestConnectionErrorFailsMultiplePendingCommands()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command1 = await transport.SendCommandAsync(new TestCommandParameters("module.command1"), TestContext.Current.CancellationToken);
        Command command2 = await transport.SendCommandAsync(new TestCommandParameters("module.command2"), TestContext.Current.CancellationToken);

        Exception simulatedError = new("connection lost");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);

        await command1.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);
        await command2.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);

        Assert.IsType<WebDriverBiDiConnectionException>(command1.ThrownException);
        Assert.IsType<WebDriverBiDiConnectionException>(command2.ThrownException);
    }

    [Fact]
    public async Task TestEachFailedPendingCommandGetsItsOwnException()
    {
        // BiDiDriver.ExecuteCommandAsync rethrows a command's exception with ExceptionDispatchInfo,
        // which appends to that object's stack trace. Sharing one instance across commands would let
        // concurrent callers overwrite each other's diagnostics.
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command1 = await transport.SendCommandAsync(new TestCommandParameters("module.command1"), TestContext.Current.CancellationToken);
        Command command2 = await transport.SendCommandAsync(new TestCommandParameters("module.command2"), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        Assert.NotNull(command1.ThrownException);
        Assert.NotNull(command2.ThrownException);
        Assert.Contains("Remote end closed the connection", command1.ThrownException.Message);
        Assert.Contains("Remote end closed the connection", command2.ThrownException.Message);
        Assert.NotSame(command1.ThrownException, command2.ThrownException);
    }

    [Fact]
    public async Task TestRemoteDisconnectFailsPendingCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);

        Assert.IsType<WebDriverBiDiConnectionException>(command.ThrownException);
        Assert.Contains("Remote end closed the connection", command.ThrownException.Message);
    }

    [Fact]
    public async Task TestRemoteDisconnectPreventsNewCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new(commandName);
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRemoteDisconnectLogsMessage()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        Assert.Contains(logs,
            log => log.Message.Contains("Remote end closed connection")
                   && log.Level == WebDriverBiDiLogLevel.Warn);
    }

    [Fact]
    public async Task TestRemoteDisconnectWhenNotConnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRemoteDisconnectWhenAlreadyDisconnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRemoteDisconnectFailsMultiplePendingCommands()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command1 = await transport.SendCommandAsync(new TestCommandParameters("module.command1"), TestContext.Current.CancellationToken);
        Command command2 = await transport.SendCommandAsync(new TestCommandParameters("module.command2"), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        await command1.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);
        await command2.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);

        Assert.IsType<WebDriverBiDiConnectionException>(command1.ThrownException);
        Assert.IsType<WebDriverBiDiConnectionException>(command2.ThrownException);
    }

    [Fact]
    public async Task TestRemoteDisconnectWhenDisconnectRacesHitsDisconnectOwnershipBranch()
    {
        // Covers the disconnect-ownership signal branch of HandleConnectionDisconnectionAsync: the
        // remote-disconnect handler passes the fast-path (State == Connected), then observes
        // DisconnectAsync's ownership signal completing before it acquires the lock, and returns
        // without tearing down (handing the lock back through its completion continuation). The
        // inner "if (this.State != TransportState.Connected) return" branch is covered separately by
        // TestConcurrentConnectionLossEventsHitInnerReturnBranch.
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        _ = transport.EnableConnectLockConcurrencyTesting();

        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await connection.RaiseRemoteDisconnectedEventAsync();
        await disconnectTask;

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);

        // The handler's outstanding wait was granted once the disconnect released the lock, and the handler hands it
        // straight back. The command above fails on the transport's state before it would take the lock, so it cannot
        // show that. An operation that always takes the lock, with no bound on the wait, completes only if the lock
        // was handed back; the outer bound only turns a lock that never comes back into a failure rather than a hang.
        transport.ConnectionLockTimeout = Timeout.InfiniteTimeSpan;
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken).WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCollectedExceptionsAreSurfacedOnDisconnectAfterRemoteDisconnect()
    {
        // Regression test: Collect-mode errors captured during the session must still be
        // surfaced when the connection is later torn down by a remote disconnect rather
        // than an explicit StopAsync/DisconnectAsync call. HandleConnectionDisconnectionAsync
        // marks the transport disconnected without running the normal teardown, so a
        // subsequent DisconnectAsync call previously hit the fast-path guard and returned
        // silently, losing the collected errors.
        InvalidOperationException injectedFault = new("simulated outer-loop fault");
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [injectedFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        bool faultCaptured = await transport.WaitForCollectedEventHandlerExceptionAsync(
            TimeSpan.FromSeconds(5),
            TransportErrorBehavior.Collect);
        if (!faultCaptured)
        {
            throw new XunitException("the fault-capture continuation should record the injected fault before the safety timeout");
        }

        // The remote end closes the connection before the caller ever calls DisconnectAsync.
        // This marks the transport disconnected via HandleConnectionDisconnectionAsync,
        // bypassing the normal teardown path that (before this fix) was the only place
        // collected exceptions were thrown.
        await connection.RaiseRemoteDisconnectedEventAsync();

        // A subsequent call to DisconnectAsync (as BiDiDriver.StopAsync would make) must
        // still surface the collected exception via the fast-path guard.
        AggregateException? caught = await Assert.ThrowsAsync<AggregateException>(
            async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));

        Assert.NotNull(caught);
        Assert.Single(caught.InnerExceptions);
        Assert.Same(injectedFault, caught.InnerExceptions[0]);
    }

    [Fact]
    public async Task TestReconnectingAfterRemoteDisconnectWithoutDisconnectingDiscardsCollectedExceptions()
    {
        // Specifies the documented contract of ConnectAsync: after a remote disconnect the transport
        // is already disconnected, so a caller may reconnect without calling DisconnectAsync first,
        // and doing so starts a new session that clears the Collect-mode errors of the old one.
        // Only DisconnectAsync throws collected errors; a DisconnectAsync after the reconnect sees
        // the new session's (empty) collection.
        InvalidOperationException injectedFault = new("simulated outer-loop fault");
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [injectedFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        bool faultCaptured = await transport.WaitForCollectedEventHandlerExceptionAsync(
            TimeSpan.FromSeconds(5),
            TransportErrorBehavior.Collect);
        if (!faultCaptured)
        {
            throw new XunitException("the fault-capture continuation should record the injected fault before the safety timeout");
        }

        await connection.RaiseRemoteDisconnectedEventAsync();
        Assert.Equal(TransportState.Disconnected, transport.State);

        // The new session's read loop must not fault, or the disconnect below would throw the new
        // session's own collected error rather than prove the old one was discarded.
        transport.ReadLoopOuterFault = null;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Assert.Equal(TransportState.Connected, transport.State);

        // Had the old session's fault survived the reconnect, this would throw the AggregateException.
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestCollectedExceptionsAreSurfacedOnlyOnceAcrossRepeatedDisconnectCalls()
    {
        // Companion to TestCollectedExceptionsAreSurfacedOnDisconnectAfterRemoteDisconnect:
        // once collected exceptions have been thrown from one DisconnectAsync call, a
        // second call (e.g., a caller invoking StopAsync twice) must not re-throw the
        // same stale exceptions, since UnhandledErrorCollection.TryGetExceptions does not
        // remove entries from the collection.
        InvalidOperationException injectedFault = new("simulated outer-loop fault");
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [injectedFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        bool faultCaptured = await transport.WaitForCollectedEventHandlerExceptionAsync(
            TimeSpan.FromSeconds(5),
            TransportErrorBehavior.Collect);
        if (!faultCaptured)
        {
            throw new XunitException("the fault-capture continuation should record the injected fault before the safety timeout");
        }

        await connection.RaiseRemoteDisconnectedEventAsync();

        await Assert.ThrowsAsync<AggregateException>(
            async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));

        // The second call finds the transport already disconnected and takes the same
        // fast-path guard, but must not throw again.
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestExceptionInErrorEventHandlerIsIgnoredByDefault()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        Assert.Equal(TransportErrorBehavior.Ignore, transport.EventHandlerExceptionBehavior);
        transport.OnUnexpectedErrorReceived.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
            throw new WebDriverBiDiException("Error handler exception");
        });
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Ignored means neither effect of the other behaviors: the transport is not
        // terminated (a command is still accepted) and nothing is collected (disconnect
        // does not throw).
        await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Exception? disconnectException = await Record.ExceptionAsync(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Null(disconnectException);
    }

    [Fact]
    public async Task TestExceptionInErrorEventHandlerCanCollect()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnUnexpectedErrorReceived.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
            throw new WebDriverBiDiException("Error handler exception");
        });
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("Error handler exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestExceptionInErrorEventHandlerCanTerminate()
    {
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnUnexpectedErrorReceived.AddObserver(e =>
            throw new WebDriverBiDiException("Error handler exception"));
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Terminate);

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken));
        Assert.Contains("error event", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("Error handler exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestExceptionInUnknownMessageHandlerIsIgnoredByDefault()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };
        Assert.Equal(TransportErrorBehavior.Ignore, transport.EventHandlerExceptionBehavior);
        Assert.Equal(TransportErrorBehavior.Ignore, transport.UnknownMessageBehavior);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Unknown message handler exception");
        });
        string json = """
                      {
                        "type": "unknown"
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Ignored means neither effect of the other behaviors: the transport is not
        // terminated (a command is still accepted) and nothing is collected (disconnect
        // does not throw).
        await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Exception? disconnectException = await Record.ExceptionAsync(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Null(disconnectException);
    }

    [Fact]
    public async Task TestExceptionInUnknownMessageHandlerCanCollect()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Unknown message handler exception");
        });
        string json = """
                      {
                        "type": "unknown"
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("Unknown message handler exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestExceptionInUnknownMessageHandlerCanTerminate()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Unknown message handler exception");
        });
        string json = """
                      {
                        "type": "unknown"
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken));
        Assert.Contains("unknown message event", exception.Message);
        Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("Unknown message handler exception", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestDisconnectCompletesTeardownWhenConnectionStopThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.False(command.IsCanceled);

        // Stopping the connection fails partway through the teardown sequence.
        connection.ThrowOnStop = true;
        connection.BypassStop = false;
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Simulated stop failure", exception.Message);

        // The teardown steps that make the transport reusable must still have run: the pending
        // command is canceled rather than left to wait out its timeout, and the message queue
        // writer is completed so the message processing task can finish.
        Assert.True(command.IsCanceled);

        List<LogMessageEventArgs> reconnectLogs = [];
        transport.OnLogMessage.AddObserver(e =>
        {
            reconnectLogs.Add(e);
            return Task.CompletedTask;
        });

        connection.ThrowOnStop = false;
        connection.BypassStop = true;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Had the writer been left uncompleted, the message processing task of the previous
        // connection could never finish, and this reconnect would have waited out the whole
        // shutdown timeout before logging that it gave up waiting.
        Assert.DoesNotContain(reconnectLogs, log => log.Message.Contains("Timed out waiting for message processing of the previous connection"));

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestExceptionInLogMessageHandlerIsIgnoredByDefault()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;
        Assert.Equal(TransportErrorBehavior.Ignore, transport.EventHandlerExceptionBehavior);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Log message handler exception");
        });

        // The command emits a log message before sending its data. A throwing log observer must
        // not fail the command that happened to emit the log message.
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.NotNull(command);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Ignored means neither effect of the other behaviors: the transport is not terminated
        // and nothing is collected, so disconnect does not throw.
        Exception? disconnectException = await Record.ExceptionAsync(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Null(disconnectException);
    }

    [Fact]
    public async Task TestExceptionInLogMessageHandlerCanCollect()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Log message handler exception");
        });
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.Contains(exception.InnerExceptions, innerException => innerException is WebDriverBiDiException && innerException.Message.Contains("Log message handler exception"));
    }

    [Fact]
    public async Task TestExceptionInLogMessageHandlerCanTerminate()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Log message handler exception");
        });
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new("module.command");
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken));
        Assert.Contains("transport.logMessage", exception.Message);
    }

    [Fact]
    public async Task TestExceptionInLogMessageHandlerIsReportedAsEventHandlerError()
    {
        TaskCompletionSource<EventHandlerErrorOccurredEventArgs> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult(e);
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Log message handler exception");
        });
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);

        EventHandlerErrorOccurredEventArgs eventArgs = await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("transport.logMessage", eventArgs.ErrorInfo.ObservableEventName);
        Assert.Equal("transport log message observer", eventArgs.ErrorInfo.ObserverDescription);
        Assert.IsType<WebDriverBiDiException>(eventArgs.ErrorInfo.Exception);
        Assert.Contains("Log message handler exception", eventArgs.ErrorInfo.Exception.Message);
    }

    [Fact]
    public async Task TestExceptionInLogMessageHandlerDuringMessageProcessingIsNotProtocolError()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();

        // Collecting protocol errors while ignoring event handler exceptions proves the
        // categorization of the failure: were the log observer's exception captured as a
        // protocol error, the disconnect below would throw it.
        await using TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            EventHandlerExceptionBehavior = TransportErrorBehavior.Ignore,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            throw new WebDriverBiDiException("Log message handler exception");
        });

        // Processing a command response emits a log message from inside the message processing
        // loop. The loop must survive the throwing observer and complete the command.
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
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
        bool commandCompleted = await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.True(commandCompleted);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Exception? disconnectException = await Record.ExceptionAsync(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Null(disconnectException);
    }

    [Fact]
    public async Task TestTransportSilentlyDiscardsFilteredMessages()
    {
        bool unknownMessageRaised = false;
        TaskCompletionSource filteredTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource sentinelTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        FilteringTransport transport = new(connection, filteredTaskCompletionSource);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            if (!e.Message.Contains("sentinel"))
            {
                unknownMessageRaised = true;
            }

            sentinelTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // This message should be silently discarded by the transformer.
        await connection.RaiseDataReceivedEventAsync("""{"method":"CDP.someEvent","params":{}}""");

        // Wait for the filtered message to be processed by the transport.
        await filteredTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Send a sentinel unknown message to confirm the transport is still processing normally.
        await connection.RaiseDataReceivedEventAsync("""{"type":"sentinel"}""");
        await sentinelTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.False(unknownMessageRaised);
    }

    [Fact]
    public async Task TestConnectAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await transport.ConnectAsync("ws://localhost", cts.Token));
    }

    [Fact]
    public async Task TestSendCommandAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParameters = new("module.command");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await transport.SendCommandAsync(commandParameters, cts.Token));
    }

    [Fact]
    public async Task TestRemoteDisconnectWhileDisconnectHoldsLockDoesNotDeadlock()
    {
        await RunDeadlockScenarioAsync(connection => connection.SignalRemoteClose());
    }

    [Fact]
    public async Task TestConnectionErrorWhileDisconnectHoldsLockDoesNotDeadlock()
    {
        await RunDeadlockScenarioAsync(connection => connection.SignalConnectionError());
    }

    /// <summary>
    /// The caller's token reaches the socket write through the connection's real send path, so a caller who
    /// gives up on a command cancels the write in progress, and the command is rolled back.
    /// </summary>
    [Fact]
    public async Task TestSendCommandCallerCancellationCancelsWriteInProgress()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;

        // The write is held open until released, and only then observes its token, so the token can be
        // inspected while the send is under way, and a failure cannot leave the write blocked.
        TaskCompletionSource<CancellationToken> writeEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseWrite = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestReceiveLoopWebSocketConnection connection = new()
        {
            WriteHandler = async cancellationToken =>
            {
                writeEntered.TrySetResult(cancellationToken);
                await releaseWrite.Task;
                cancellationToken.ThrowIfCancellationRequested();
            },
        };
        await using TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", testCancellationToken);

        using CancellationTokenSource callerTokenSource = new();
        Task<Command> sendTask = transport.SendCommandAsync(new TestCommandParameters("module.command"), callerTokenSource.Token);
        CancellationToken writeToken = await writeEntered.Task.WaitAsync(DeadlockDetectionTimeout, testCancellationToken);
        try
        {
            Assert.False(writeToken.IsCancellationRequested);
            callerTokenSource.Cancel();
            Assert.True(writeToken.IsCancellationRequested, "Canceling the caller's token did not cancel the write in progress.");
        }
        finally
        {
            releaseWrite.TrySetResult();
        }

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sendTask);
        Assert.Equal(0, transport.TestPendingCommandCount);
    }

    /// <summary>
    /// A connection loss whose wait for the connection lock is abandoned must leave the session
    /// standing rather than tear it down without the lock, and must not release a lock it never
    /// acquired.
    /// </summary>
    /// <remarks>
    /// The lock is held here by a command send rather than by a disconnect, so no ownership signal is
    /// raised and the abandoned wait is the only task that can win the race. An implementation that
    /// reads winning the race as holding the lock releases a lock the send still holds.
    /// </remarks>
    [Fact]
    public async Task TestConnectionLossWithAbandonedLockWaitLeavesSessionStanding()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;

        TestReceiveLoopWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", testCancellationToken);

        // Never wait for the lock, so the handler's wait is abandoned the instant it finds it held.
        transport.ConnectionLockTimeout = TimeSpan.Zero;

        // Raised when the handler gives up, or by the teardown a broken implementation performs
        // instead, which logs at the same level: either way the send below is released, so a failure
        // here is a failed assertion rather than a hang.
        TaskCompletionSource connectionLossHandled = new(TaskCreationOptions.RunContinuationsAsynchronously);
        string? warningMessage = null;
        using EventObserver<LogMessageEventArgs> logObserver = transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Level == WebDriverBiDiLogLevel.Warn)
            {
                warningMessage ??= e.Message;
                connectionLossHandled.TrySetResult();
            }
        });

        // Fires once, while SendCommandAsync holds the lock: dispatch the connection loss there, and
        // hold the send until the handler has dealt with it.
        transport.AfterAcquireLockAsyncCallback = async () =>
        {
            connection.SignalRemoteClose();
            await connectionLossHandled.Task;
        };

        // The receive loop has ended, so the connection refuses the send, and the transport rolls the command
        // back. It is the connection that refuses it: a transport torn down during the abandoned wait would
        // already have refused it for not being connected. An over-release surfaces here instead as a
        // SemaphoreFullException from the send's own release.
        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(() => transport.SendCommandAsync(new TestCommandParameters("module.command"), testCancellationToken));
        Assert.StartsWith("The WebSocket connection is not active", exception.Message);

        Assert.NotNull(warningMessage);
        Assert.Contains("waiting for exclusive access to the connection to handle a connection loss", warningMessage);

        // The session was left standing, with the pending command collection still open.
        Assert.Equal(TransportState.Connected, transport.State);
        Assert.True(transport.TestIsAcceptingCommands, "The pending command collection was closed.");

        await transport.DisposeAsync();
    }

    /// <summary>
    /// A wait for the connection lock that is left outstanding when DisconnectAsync wins the ownership
    /// race, and is then abandoned rather than granted, must not release the lock the disconnect
    /// holds.
    /// </summary>
    /// <remarks>
    /// The companion of the scenario above, on the other side of the race. An implementation that
    /// hands the lock back regardless of how the wait settled either faults the disconnect's own
    /// release or silently leaves the semaphore admitting a second holder; the probe at the end of
    /// this test rules out both.
    /// </remarks>
    [Fact]
    public async Task TestOutstandingLockWaitAbandonedWhileDisconnectOwnsTeardownDoesNotReleaseLock()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;

        TestReceiveLoopWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", testCancellationToken);
        transport.ConnectionLockTimeout = TimeSpan.Zero;

        TaskCompletionSource handlerWaitStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource openHandlerWait = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource handlerWaitAbandoned = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Acquisition 1 is DisconnectAsync's; acquisition 2 is the connection-loss handler's. Holding
        // the handler's wait keeps it outstanding, so the ownership signal wins the race.
        int lockAcquisitionAttempts = 0;
        transport.BeforeAcquireLockCallback = async () =>
        {
            if (Interlocked.Increment(ref lockAcquisitionAttempts) == 2)
            {
                handlerWaitStarted.TrySetResult();
                await openHandlerWait.Task;
            }
        };

        transport.AcquireLockFailedCallback = () => handlerWaitAbandoned.TrySetResult();

        // Fires once, while DisconnectAsync holds the lock: end the receive loop, then hold the
        // disconnect until the handler's wait is outstanding, forcing the interleaving.
        transport.AfterAcquireLockAsyncCallback = async () =>
        {
            connection.SignalRemoteClose();
            await handlerWaitStarted.Task;
        };

        // "Transport disconnected" is logged inside DisconnectAsync's critical section, so the lock is
        // still held here: open the handler's wait so that it finds the lock held and is abandoned,
        // and hold the disconnect until that has happened.
        using EventObserver<LogMessageEventArgs> logObserver = transport.OnLogMessage.AddObserver(async e =>
        {
            if (e.Message == "Transport disconnected")
            {
                openHandlerWait.TrySetResult();
                await handlerWaitAbandoned.Task;
            }
        });

        // An over-release before the disconnect's own release faults it with a SemaphoreFullException,
        // which surfaces here.
        Task disconnectTask = transport.DisconnectAsync(testCancellationToken);
        Task settledTask = await Task.WhenAny(disconnectTask, Task.Delay(DeadlockDetectionTimeout, testCancellationToken));
        if (settledTask != disconnectTask)
        {
            Assert.Fail($"DisconnectAsync did not complete within {DeadlockDetectionTimeout.TotalSeconds} seconds.");
        }

        await disconnectTask;
        Assert.True(handlerWaitAbandoned.Task.IsCompleted, "The connection-loss handler's wait for the lock should have been abandoned.");

        // An over-release after it leaves no exception behind, so probe the invariant directly: with a
        // holder parked in its critical section, a second wait must still be abandoned.
        transport.BeforeAcquireLockCallback = null;
        TaskCompletionSource lockHeld = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseLockHolder = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.AfterAcquireLockAsyncCallback = async () =>
        {
            lockHeld.TrySetResult();
            await releaseLockHolder.Task;
        };

        Task reconnectTask = transport.ConnectAsync("ws://localhost", testCancellationToken);
        await lockHeld.Task;
        await Assert.ThrowsAsync<WebDriverBiDiTimeoutException>(
            async () => await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), testCancellationToken));

        releaseLockHolder.TrySetResult();
        await reconnectTask;
        await transport.DisposeAsync();
    }

    /// <summary>
    /// Guards the per-session reset of the disconnect-ownership state the CT-1 fix introduces: after a
    /// session is stopped and a new one started, a plain remote disconnect on the new session must
    /// still perform its teardown and fail in-flight commands.
    /// </summary>
    /// <remarks>
    /// DisconnectAsync raises a per-session ownership signal when it takes over the teardown, and the
    /// connection-loss handler waits against that signal so it never blocks on the connection lock
    /// behind the disconnect. A normal stop raises the signal; if it is not replaced when the transport
    /// reconnects, a later remote disconnect on the new session would short-circuit its teardown and
    /// silently leave in-flight commands pending forever. The command completes effectively immediately
    /// on success; the timeout is a stall detector for the short-circuit regression.
    /// </remarks>
    [Fact]
    public async Task TestRemoteDisconnectFailsPendingCommandsOnReconnectedSession()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;

        TestReceiveLoopWebSocketConnection connection = new();
        TestTransport transport = new(connection);

        // First session: connect and stop cleanly. The fix raises its disconnect-ownership signal
        // during this stop, so the second session must start from a fresh signal.
        await transport.ConnectAsync("ws://localhost", testCancellationToken);
        await transport.DisconnectAsync(testCancellationToken);

        // Second session: reconnect and send a command that stays pending (the connection double
        // never produces a response).
        await transport.ConnectAsync("ws://localhost", testCancellationToken);
        TestCommandParameters commandParameters = new("module.command");
        Command pendingCommand = await transport.SendCommandAsync(commandParameters, testCancellationToken);

        // A plain remote disconnect on the new session, with no concurrent DisconnectAsync: the lock
        // is free, so the only thing that can stop the handler from failing the command is stale,
        // un-reset disconnect state carried over from the first session.
        connection.SignalRemoteClose();

        bool commandCompleted = await pendingCommand.WaitForCompletionAsync(DeadlockDetectionTimeout, testCancellationToken);
        Assert.True(
            commandCompleted,
            "The remote disconnect on the reconnected session did not fail the pending command; " +
            "per-session disconnect state was not reset on reconnect.");
        Assert.IsType<WebDriverBiDiConnectionException>(pendingCommand.ThrownException);

        await transport.DisposeAsync();
    }

    /// <summary>
    /// Covers the inner <c>if (this.State != TransportState.Connected) return</c> re-check in
    /// <c>HandleConnectionDisconnectionAsync</c> (the path where a connection-loss handler acquires
    /// the connection lock and finds the transport already disconnected).
    /// </summary>
    /// <remarks>
    /// A <see cref="Transport.DisconnectAsync(CancellationToken)"/> racing a loss handler is resolved
    /// through the disconnect-ownership signal, so it no longer reaches this inner re-check (that path
    /// is covered by <c>TestRemoteDisconnectWhenDisconnectRacesHitsDisconnectOwnershipBranch</c> and
    /// <c>TestConnectionErrorWhenDisconnectRacesHitsDisconnectOwnershipBranch</c> elsewhere in this class). The
    /// re-check is now reached only when two connection-loss events race each other: neither raises the
    /// ownership signal, so the second handler waits for the lock, and by the time it acquires it the
    /// first handler has already set the transport disconnected. The two acquisitions are choreographed
    /// deterministically with <see cref="TestTransport.EnableConnectLockConcurrencyTesting"/> so that
    /// both handlers pass their fast-path check before either takes the lock.
    /// </remarks>
    [Fact]
    public async Task TestConcurrentConnectionLossEventsHitInnerReturnBranch()
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", testCancellationToken);

        // Choreograph the two connection-lock acquisitions: the first loss handler enters the lock,
        // and the second is held at its fast-path-passed / pre-lock point until the first has
        // acquired the lock, guaranteeing both saw State == Connected before either tore down.
        Task firstHandlerEnteredLockAcquisition = transport.EnableConnectLockConcurrencyTesting();

        // First loss event: acquires the lock and performs the teardown.
        Task firstLossHandler = connection.RaiseConnectionErrorEventAsync(new Exception("first connection loss"));
        await firstHandlerEnteredLockAcquisition;

        // Second loss event: passes the fast-path while the first still holds the lock, then waits
        // for the lock and, on acquiring it, hits the inner re-check with State already Disconnected.
        Task secondLossHandler = connection.RaiseRemoteDisconnectedEventAsync();

        await Task.WhenAll(firstLossHandler, secondLossHandler);

        // The transport tore down exactly once and further commands fail fast.
        TestCommandParameters commandParameters = new("module.command");
        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(
            async () => await transport.SendCommandAsync(commandParameters, testCancellationToken));
        Assert.Contains("Transport must be connected", exception.Message);

        await transport.DisposeAsync();
    }

    private static async Task RunDeadlockScenarioAsync(Action<TestReceiveLoopWebSocketConnection> endReceiveLoop)
    {
        CancellationToken testCancellationToken = TestContext.Current.CancellationToken;

        TestReceiveLoopWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", testCancellationToken);

        // Signalled by the connection-loss handler when it enters its connection-lock acquisition,
        // i.e., once it has passed its fast-path check (State is still Connected at that point,
        // because DisconnectAsync is parked in the after-acquire callback below and has not yet
        // marked the transport disconnected). This is the moment that makes the deadlock inevitable
        // on an unfixed implementation, and the moment DisconnectAsync must be released to proceed.
        TaskCompletionSource connectionLossHandlerWaitingForLock = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // AcquireConnectionLockAsync is called twice after setup: first by DisconnectAsync, then by
        // the connection-loss handler running on the receive loop. Use the second entry to record
        // that the handler is committed to waiting for the lock.
        int lockAcquisitionAttempts = 0;
        transport.BeforeAcquireLockCallback = () =>
        {
            if (Interlocked.Increment(ref lockAcquisitionAttempts) == 2)
            {
                connectionLossHandlerWaitingForLock.TrySetResult();
            }

            return Task.CompletedTask;
        };

        // Fires once, immediately after DisconnectAsync has acquired the lock and while it still
        // holds it. End the receive loop here (which dispatches the remote-disconnect/error event on
        // that loop), then block DisconnectAsync until the handler is waiting for the lock, so the
        // interleaving is forced rather than raced.
        transport.AfterAcquireLockAsyncCallback = async () =>
        {
            endReceiveLoop(connection);
            await connectionLossHandlerWaitingForLock.Task;
        };

        Task disconnectTask = transport.DisconnectAsync(testCancellationToken);

        Task settledTask = await Task.WhenAny(disconnectTask, Task.Delay(DeadlockDetectionTimeout, testCancellationToken));
        if (settledTask != disconnectTask)
        {
            Assert.Fail(
                $"DisconnectAsync did not complete within {DeadlockDetectionTimeout.TotalSeconds} seconds; " +
                $"the transport deadlocked against the connection-loss handler (receive-loop task status: {connection.DataReceiveTaskStatusDescription}).");
        }

        // Surface any fault from the disconnect itself.
        await disconnectTask;

        // The connection was stopped exactly once (the handler must not have driven a second stop),
        // and its receive loop drained to completion rather than being abandoned.
        Assert.Equal(1, connection.StopCallCount);
        Assert.True(connection.ReceiveLoopCompleted, "The connection's receive loop should have completed once the disconnect finished.");

        // The transport is disconnected, so a follow-up command fails fast with a connection exception.
        Assert.Equal(TransportState.Disconnected, transport.State);
        TestCommandParameters commandParameters = new("module.command");
        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(
            async () => await transport.SendCommandAsync(commandParameters, testCancellationToken));
        Assert.Contains("Transport must be connected", exception.Message);

        // The handler's lock wait was granted once the disconnect released the lock, and the handler must hand it
        // straight back. The command above fails on the transport's state before it would take the lock, so it cannot
        // show that. An operation that always takes the lock, with no bound on the wait, completes only if the lock
        // was handed back; the outer bound only turns a lock that never comes back into a failure rather than a hang.
        transport.ConnectionLockTimeout = Timeout.InfiniteTimeSpan;
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), testCancellationToken).WaitAsync(DeadlockDetectionTimeout, testCancellationToken);

        await transport.DisposeAsync();
    }

    private class FilteringTransport : Transport
    {
        private readonly TaskCompletionSource filteredMessageProcessed;
        private int messageCount;

        public FilteringTransport(TestWebSocketConnection connection, TaskCompletionSource filteredMessageProcessed)
            : base(connection)
        {
            this.filteredMessageProcessed = filteredMessageProcessed;
        }

        protected override IncomingMessage CreateIncomingMessage(System.Buffers.IMemoryOwner<byte> owner, int length)
        {
            // Only the first message is filtered; subsequent messages pass through normally
            // so the sentinel unknown message can still trigger OnUnknownMessageReceived.
            return Interlocked.Increment(ref this.messageCount) == 1
                ? new TestIncomingMessage(owner, length, false, (doc) => null, this.filteredMessageProcessed)
                : new IncomingMessage(owner, length);
        }
    }

    [Fact]
    public async Task TestLateSuccessResponseForCanceledCommandIsDiscarded()
    {
        // A response that arrives after the local end stopped waiting for the command (here,
        // because it timed out) is not an unknown message. Under Terminate behavior it must
        // neither raise OnUnknownMessageReceived nor terminate the transport.
        TaskCompletionSource discardedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool unknownMessageReceived = false;
        LogMessageEventArgs? discardLog = null;
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
        };
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;
        transport.OnUnknownMessageReceived.AddObserver(e => unknownMessageReceived = true);
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.Contains("Discarding late response"))
            {
                discardLog = e;
                discardedTaskCompletionSource.TrySetResult();
            }
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.True(transport.CancelCommand(command, CommandCancellationReason.TimedOut));
        Assert.False(transport.CancelCommand(command, CommandCancellationReason.TimedOut));
        Assert.Equal(0, transport.PendingCommandCount);

        await connection.RaiseDataReceivedEventAsync($$$"""{"type":"success","id":{{{command.CommandId}}},"result":{"parameterName":"parameterValue"}}""");
        await discardedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.False(unknownMessageReceived);
        Assert.True(command.IsCanceled);
        Assert.False(command.TryGetResult(out _));
        Assert.NotNull(discardLog);
        Assert.Equal(WebDriverBiDiLogLevel.Debug, discardLog.Level);
        Assert.Contains("'module.command'", discardLog.Message);
        Assert.Contains($"(command ID: {command.CommandId})", discardLog.Message);
        Assert.Contains("(TimedOut)", discardLog.Message);

        // The transport was not terminated: sending another command succeeds.
        Command nextCommand = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.NotEqual(command.CommandId, nextCommand.CommandId);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestLateErrorResponseForCanceledCommandIsDiscarded()
    {
        TaskCompletionSource discardedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool errorEventReceived = false;
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate,
        };
        // This test asserts on Debug or Trace messages, which the default minimum level excludes.
        transport.LogLevel = WebDriverBiDiLogLevel.Trace;
        transport.OnUnexpectedErrorReceived.AddObserver(e => errorEventReceived = true);
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.Contains("Discarding late response") && e.Message.Contains("(Canceled)"))
            {
                discardedTaskCompletionSource.TrySetResult();
            }
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        transport.CancelCommand(command);

        await connection.RaiseDataReceivedEventAsync($$$"""{"type":"error","id":{{{command.CommandId}}},"error":"unknown error","message":"late error"}""");
        await discardedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.False(errorEventReceived);
        Assert.True(command.IsCanceled);

        // The transport was not terminated: sending another command succeeds.
        await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestNonGenericCommandParametersFallBackToResolvingResponseTypeThroughOptions()
    {
        // A CommandParameters subclass that does not derive from CommandParameters<T> provides no
        // envelope type info of its own, so the transport resolves ResponseType through the options.
        TestWebSocketConnection connection = new();
        connection.OnDataSendComplete.AddObserver(async e =>
        {
            await connection.RaiseDataReceivedEventAsync("""{"type":"success","id":1,"result":{"value":"fallback"}}""");
        });
        await using Transport transport = new(connection);
        await using BiDiDriver driver = new(TimeSpan.FromSeconds(5), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(new NonGenericCommandParameters(), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("fallback", result.Value);
    }

    [Fact]
    public async Task TestUnknownMessageCanCollect()
    {
        TaskCompletionSource captured = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Valid JSON that matches no protocol message shape.
        await connection.RaiseDataReceivedEventAsync("""{"someProperty":"someValue"}""");
        await captured.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Collect mode does not terminate the transport: commands still go through.
        await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        WebDriverBiDiException inner = Assert.IsType<WebDriverBiDiException>(Assert.Single(exception.InnerExceptions));
        Assert.Contains("Received unknown message from protocol connection", inner.Message);
        Assert.Contains("someProperty", inner.Message);
    }

    [Fact]
    public async Task TestUnknownMessageCanCollectMultiple()
    {
        int capturedCount = 0;
        TaskCompletionSource bothCaptured = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () =>
            {
                if (Interlocked.Increment(ref capturedCount) == 2)
                {
                    bothCaptured.TrySetResult();
                }
            },
        };
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await connection.RaiseDataReceivedEventAsync("""{"first":"unknown"}""");
        await connection.RaiseDataReceivedEventAsync("""{"second":"unknown"}""");
        await bothCaptured.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.All(exception.InnerExceptions, e => Assert.IsType<WebDriverBiDiException>(e));
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("\"first\""));
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("\"second\""));
    }

    [Fact]
    public async Task TestUnknownMessageCanTerminate()
    {
        TaskCompletionSource captured = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await connection.RaiseDataReceivedEventAsync("""{"someProperty":"someValue"}""");
        await captured.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Terminate mode surfaces the error on the next command.
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
        Assert.Contains("Unknown message from connection", exception.Message);
        WebDriverBiDiException inner = Assert.IsType<WebDriverBiDiException>(exception.InnerException);
        Assert.Contains("Received unknown message from protocol connection", inner.Message);
    }

    [Fact]
    public async Task TestProtocolErrorInEventMessageCanCollect()
    {
        TaskCompletionSource captured = new(TaskCreationOptions.RunContinuationsAsynchronously);
        bool eventReceived = false;
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e => eventReceived = true);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Structurally an event, but its params cannot be deserialized as TestEventArgs.
        await connection.RaiseDataReceivedEventAsync("""{"type":"event","method":"protocol.event","params":"not an object"}""");
        await captured.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.False(eventReceived);

        // Collect mode does not terminate the transport: commands still go through.
        await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains("Normal shutdown", exception.Message);
        Assert.Single(exception.InnerExceptions);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task TestProtocolErrorInEventMessageCanTerminate()
    {
        TaskCompletionSource captured = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await connection.RaiseDataReceivedEventAsync("""{"type":"event","method":"protocol.event","params":"not an object"}""");
        await captured.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
        Assert.Contains("Invalid JSON in event message", exception.Message);
        Assert.Contains("protocol.event", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task TestErrorResponseForUnknownCommandIdIsReportedWithThatId()
    {
        TaskCompletionSource errorTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnUnexpectedErrorReceived.AddObserver(e => errorTaskCompletionSource.TrySetResult());
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await connection.RaiseDataReceivedEventAsync("""{"type":"error","id":999,"error":"unknown error","message":"no such command"}""");
        await errorTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The transport notifies OnUnexpectedErrorReceived observers before it records the error in
        // its unhandled-error collection, so the observer firing is not sufficient to guarantee
        // the Terminate behavior is armed. Wait for the error to actually be captured.
        bool errorCaptured = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Terminate);
        Assert.True(errorCaptured);

        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
        Assert.Contains("Received error for unknown command ID 999", exception.Message);
        Assert.NotNull(exception.InnerException);
        Assert.Contains("error for unknown command ID 999: no such command", exception.InnerException.Message);
    }

    [Fact]
    public async Task TestLateResponseForForgottenCanceledCommandIsUnknownMessage()
    {
        // With a tracker capacity of one, canceling a second command forgets the first, so a
        // response for the first is indistinguishable from a foreign message and is reported
        // through the unknown-message pipeline, exactly as before tracking existed.
        TaskCompletionSource unknownTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        string? unknownMessage = null;
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        transport.UseCanceledCommandTrackerCapacity(1);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessage = e.Message;
            unknownTaskCompletionSource.TrySetResult();
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Command first = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Command second = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        transport.CancelCommand(first, CommandCancellationReason.TimedOut);
        transport.CancelCommand(second, CommandCancellationReason.TimedOut);

        await connection.RaiseDataReceivedEventAsync($$$"""{"type":"success","id":{{{first.CommandId}}},"result":{"parameterName":"parameterValue"}}""");
        await unknownTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(unknownMessage);
        Assert.Contains($"\"id\":{first.CommandId}", unknownMessage);
        Assert.True(first.IsCanceled);
        Assert.True(second.IsCanceled);
        Assert.False(first.TryGetResult(out _));
        Assert.Equal(0, transport.PendingCommandCount);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestReconnectPreservesCanceledCommandTrackerCapacity()
    {
        // ConnectAsync replaces the pending command collection on every reconnect, because the
        // previous one was closed by the disconnect. The replacement must carry over the tracker
        // capacity a derived transport configured, rather than reverting to the default; a
        // capacity of one is observable because canceling a second command then forgets the
        // first, whose late response is reported as an unknown message rather than discarded.
        TaskCompletionSource unknownTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        string? unknownMessage = null;
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        transport.UseCanceledCommandTrackerCapacity(1);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessage = e.Message;
            unknownTaskCompletionSource.TrySetResult();
        });

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        string initialCollectionId = transport.TestPendingCommandCollectionId;
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Assert.NotEqual(initialCollectionId, transport.TestPendingCommandCollectionId);
        Assert.Equal(1u, transport.TestMaxTrackedCanceledCommands);

        Command first = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Command second = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        transport.CancelCommand(first, CommandCancellationReason.TimedOut);
        transport.CancelCommand(second, CommandCancellationReason.TimedOut);

        await connection.RaiseDataReceivedEventAsync($$$"""{"type":"success","id":{{{first.CommandId}}},"result":{"parameterName":"parameterValue"}}""");
        await unknownTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(unknownMessage);
        Assert.Contains($"\"id\":{first.CommandId}", unknownMessage);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestReconnectAfterRemoteDisconnectWaitsForPreviousMessageProcessing()
    {
        // A remote disconnect completes the incoming message queue but does not wait for the
        // reader task, which may still be executing an event handler. ConnectAsync must wait
        // for that reader to finish before installing the new queue, so that the previous
        // session's reader can never consume the new session's messages and every message is
        // processed exactly once.
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseHandlerTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondEventProcessedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource reconnectWaitingTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int eventCount = 0;

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            LogLevel = WebDriverBiDiLogLevel.Debug,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.StartsWith("Waiting for message processing of the previous session", StringComparison.Ordinal))
            {
                reconnectWaitingTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(async e =>
        {
            int currentCount = Interlocked.Increment(ref eventCount);
            if (currentCount == 1)
            {
                handlerStartedTaskCompletionSource.TrySetResult();
                await releaseHandlerTaskCompletionSource.Task;
            }
            else
            {
                secondEventProcessedTaskCompletionSource.TrySetResult();
            }
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The remote end closes the connection while the handler is still executing.
        await connection.RaiseRemoteDisconnectedEventAsync();

        // Reconnecting must block until the previous reader has exited. The Debug log message is
        // raised only when ConnectAsync has found the previous reader still running and is
        // entering the wait for it, and log observers run before the wait begins, so receiving
        // it proves the reconnect is blocked on the reader rather than pending for some other
        // reason.
        Task reconnectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await reconnectWaitingTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.False(reconnectTask.IsCompleted);

        releaseHandlerTaskCompletionSource.TrySetResult();
        await reconnectTask.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Messages for the new session are processed by the new reader, exactly once.
        await connection.RaiseDataReceivedEventAsync(json);
        await secondEventProcessedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(2, eventCount);
        Assert.Equal(0, transport.IncomingQueueDepth);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestReconnectAfterRemoteDisconnectTimesOutWaitingForHangingHandler()
    {
        // If the previous session's reader never finishes (a handler that hangs), ConnectAsync
        // must not wait forever: it logs a warning after ShutdownTimeout and proceeds, and the
        // new session's messages are still processed by the new reader.
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondEventProcessedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];
        int eventCount = 0;

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            int currentCount = Interlocked.Increment(ref eventCount);
            if (currentCount == 1)
            {
                handlerStartedTaskCompletionSource.TrySetResult();
                return new TaskCompletionSource<bool>().Task;
            }

            secondEventProcessedTaskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        // The shutdown timeout is elapsed on the virtual clock as soon as the reconnect arms it.
        Task reconnectTask = transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(reconnectTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await reconnectTask;

        Assert.Contains(logs,
            log => log.Message.Contains("Timed out waiting for message processing of the previous connection to complete before reconnecting")
                   && log.Level == WebDriverBiDiLogLevel.Warn);

        await connection.RaiseDataReceivedEventAsync(json);
        await secondEventProcessedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(2, eventCount);
        Assert.Equal(0, transport.IncomingQueueDepth);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestReconnectAfterTimedOutHandlerDoesNotCorruptQueueDepth()
    {
        // A reconnect that gives up waiting for a stuck handler leaves the previous connection's
        // reader still draining the previous connection's queue. Those reads must be counted
        // against the queue the messages actually came from, not against the queue the new
        // connection is filling; otherwise the depth reported for the new connection is
        // decremented for messages that were never on it, and goes negative.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource firstHandlerBlockedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstHandlerTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource staleMessageProcessedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int eventCount = 0;

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
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
            else
            {
                staleMessageProcessedTaskCompletionSource.TrySetResult();
            }

            return Task.CompletedTask;
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;

        await transport.ConnectAsync("ws://localhost", cancellationToken);

        // Two messages reach the first connection's queue. The reader takes the first, decrementing
        // that queue's depth, and then blocks in the handler, so the second is left unread on it.
        await connection.RaiseDataReceivedEventAsync(json);
        await firstHandlerBlockedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);

        await connection.RaiseRemoteDisconnectedEventAsync();

        // The reader is still stuck in the handler, so this reconnect times out waiting for it and
        // installs a new queue while the old one still holds an unread message.
        Task reconnectTask = transport.ConnectAsync("ws://localhost", cancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(reconnectTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), cancellationToken);
        await reconnectTask;
        Assert.Equal(0, transport.IncomingQueueDepth);

        // Releasing the handler lets the previous connection's reader drain that unread message.
        releaseFirstHandlerTaskCompletionSource.SetResult();
        await staleMessageProcessedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        // The drain belonged to the previous connection's queue, so the current connection's depth
        // is untouched by it.
        Assert.Equal(0, transport.IncomingQueueDepth);

        await transport.DisconnectAsync(cancellationToken);
    }

    [Fact]
    public async Task TestPreviousSessionReaderDoesNotCompleteNewSessionCommand()
    {
        // A reconnect that gives up waiting for a stuck handler leaves the previous session's reader
        // still draining the previous session's queue. A response on that queue is resolved against
        // the previous session's pending commands, never the new session's, even when it carries the
        // ID of a command the new session has sent.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource firstHandlerBlockedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstHandlerTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<string> unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            firstHandlerBlockedTaskCompletionSource.TrySetResult();
            releaseFirstHandlerTaskCompletionSource.Task.GetAwaiter().GetResult();
            return Task.CompletedTask;
        });
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageTaskCompletionSource.TrySetResult(e.Message);
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost", cancellationToken);

        // The reader blocks in the handler for an event, so the response that follows it is left
        // unread on the first session's queue. It carries ID 1, which the first session never issued
        // and the second session is about to.
        await connection.RaiseDataReceivedEventAsync("""{ "type": "event", "method": "protocol.event", "params": { "paramName": "paramValue" } }""");
        await firstHandlerBlockedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        string staleResponse = """{ "type": "success", "id": 1, "result": { "value": "previous session" } }""";
        await connection.RaiseDataReceivedEventAsync(staleResponse);
        await connection.RaiseRemoteDisconnectedEventAsync();

        Task reconnectTask = transport.ConnectAsync("ws://localhost", cancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(reconnectTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), cancellationToken);
        await reconnectTask;
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationToken);
        Assert.Equal(1, command.CommandId);

        // Releasing the handler lets the previous session's reader process the stale response. It
        // matches nothing in the previous session, so it is reported as an unknown message, and the
        // new session's command is left pending.
        releaseFirstHandlerTaskCompletionSource.SetResult();
        string unknownMessage = await unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        Assert.Equal(staleResponse, unknownMessage);
        Assert.False(command.TryGetResult(out _));
        Assert.Null(command.ThrownException);
        Assert.False(command.IsCanceled);
        Assert.Equal(1, transport.PendingCommandCount);

        await transport.DisconnectAsync(cancellationToken);
    }

    [Fact]
    public async Task TestLateResponseFromPreviousSessionIsNotMatchedToNewSessionCommand()
    {
        // A connection that survives a reconnect, such as a pipe, can deliver a response to a command of
        // the previous session after the new session has started, and the new session's reader reads
        // it. Command IDs are not reused across sessions, so it cannot match a new session's command.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource<string> unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessageTaskCompletionSource.TrySetResult(e.Message);
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost", cancellationToken);
        Command previousSessionCommand = await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationToken);
        await transport.DisconnectAsync(cancellationToken);

        await transport.ConnectAsync("ws://localhost", cancellationToken);
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationToken);
        Assert.NotEqual(previousSessionCommand.CommandId, command.CommandId);

        string lateResponse = $$"""{ "type": "success", "id": {{previousSessionCommand.CommandId}}, "result": { "value": "previous session" } }""";
        await connection.RaiseDataReceivedEventAsync(lateResponse);

        string unknownMessage = await unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        Assert.Equal(lateResponse, unknownMessage);
        Assert.False(command.TryGetResult(out _));
        Assert.Null(command.ThrownException);
        Assert.False(command.IsCanceled);
        Assert.Equal(1, transport.PendingCommandCount);

        await transport.DisconnectAsync(cancellationToken);
    }

    [Fact]
    public async Task TestErrorFromPreviousSessionReaderIsNotCollectedByNewSession()
    {
        // An error the previous session's reader gives rise to after a reconnect belongs to the
        // previous session, which has ended. It is logged rather than collected, so it cannot
        // terminate the new session.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource firstHandlerBlockedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseFirstHandlerTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<string> discardedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
            LogLevel = WebDriverBiDiLogLevel.Warn,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            firstHandlerBlockedTaskCompletionSource.TrySetResult();
            releaseFirstHandlerTaskCompletionSource.Task.GetAwaiter().GetResult();
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.StartsWith("Discarded UnknownMessage error", StringComparison.Ordinal))
            {
                discardedTaskCompletionSource.TrySetResult(e.Message);
            }
        });

        await transport.ConnectAsync("ws://localhost", cancellationToken);

        // The reader blocks in the handler for an event, leaving an unknown message unread on the
        // first session's queue, and the reconnect gives up waiting for it.
        await connection.RaiseDataReceivedEventAsync("""{ "type": "event", "method": "protocol.event", "params": { "paramName": "paramValue" } }""");
        await firstHandlerBlockedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        await connection.RaiseDataReceivedEventAsync("""{ "type": "unknown" }""");
        await connection.RaiseRemoteDisconnectedEventAsync();

        Task reconnectTask = transport.ConnectAsync("ws://localhost", cancellationToken);
        await timeProvider.AdvanceUntilCompletedAsync(reconnectTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), cancellationToken);
        await reconnectTask;

        // Releasing the handler lets the previous session's reader process the unknown message, which
        // would terminate the session under Terminate. It is discarded instead.
        releaseFirstHandlerTaskCompletionSource.SetResult();
        string discardedMessage = await discardedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        Assert.Contains("Received unknown message from protocol connection", discardedMessage);

        // The new session was not terminated: a command is sent normally.
        _ = await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationToken);
        Assert.Equal(1, transport.PendingCommandCount);
        Assert.Equal(TransportState.Connected, transport.State);

        await transport.DisconnectAsync(cancellationToken);
    }

    [Fact]
    public async Task TestAsynchronousHandlerFaultFromPreviousSessionIsNotCollectedByNewSession()
    {
        // A handler run asynchronously can fault long after the reader started it, even after a
        // reconnect. The fault belongs to the session whose reader started the handler, so it is logged
        // rather than collected by the new session.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource releaseHandlerTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<string> discardedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
            LogLevel = WebDriverBiDiLogLevel.Warn,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(
            async e =>
            {
                handlerStartedTaskCompletionSource.TrySetResult();
                await releaseHandlerTaskCompletionSource.Task;
                throw new WebDriverBiDiException("previous session handler failure");
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.StartsWith("Discarded EventHandlerException error", StringComparison.Ordinal))
            {
                discardedTaskCompletionSource.TrySetResult(e.Message);
            }
        });

        await transport.ConnectAsync("ws://localhost", cancellationToken);
        await connection.RaiseDataReceivedEventAsync("""{ "type": "event", "method": "protocol.event", "params": { "paramName": "paramValue" } }""");
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        await transport.DisconnectAsync(cancellationToken);
        await transport.ConnectAsync("ws://localhost", cancellationToken);

        releaseHandlerTaskCompletionSource.SetResult();
        string discardedMessage = await discardedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
        Assert.Contains("previous session handler failure", discardedMessage);

        // Nothing was collected for the new session, so stopping it does not throw.
        await transport.DisconnectAsync(cancellationToken);
    }

    [Fact]
    public async Task TestErrorRaisedForTransportFromAnotherTransportsReaderIsCollected()
    {
        // The session a flow belongs to is tracked per transport. A handler run by one transport's
        // reader that gives rise to an error in a second transport does not carry a session of the
        // second transport, so the error is collected by the second transport's current session.
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        TestWebSocketConnection otherConnection = new();
        await using TestTransport otherTransport = new(otherConnection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        // Only the message raised from the first transport's reader fails, so the connection's own log
        // messages, raised outside any reader, cannot supply the collected error this test looks for.
        Action<LogMessageEventArgs> throwingHandler = e =>
        {
            if (e.Message == "other connection log message")
            {
                throw new WebDriverBiDiException("other transport observer failure");
            }
        };
        otherConnection.OnLogMessage.AddObserver(throwingHandler, ObservableEventHandlerOptions.RunHandlerAsynchronously);
        await otherTransport.ConnectAsync("ws://localhost", cancellationToken);

        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(async e => await otherConnection.RaiseLogMessageEventAsync("other connection log message", WebDriverBiDiLogLevel.Warn));
        await transport.ConnectAsync("ws://localhost", cancellationToken);

        await connection.RaiseDataReceivedEventAsync("""{ "type": "event", "method": "protocol.event", "params": { "paramName": "paramValue" } }""");
        Assert.True(await otherTransport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Collect));

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(
            async () => await otherTransport.DisconnectAsync(cancellationToken));
        Assert.Contains(exception.InnerExceptions, inner => inner.Message.Contains("other transport observer failure"));
        await transport.DisconnectAsync(cancellationToken);
    }

    [Fact]
    public async Task TestDataReceivedAfterRemoteDisconnectIsDisposedAndNotQueued()
    {
        // A remote disconnect completes the incoming message queue, so data delivered by the
        // connection afterwards must be disposed rather than queued, exactly as after an
        // explicit DisconnectAsync.
        bool eventReceived = false;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e =>
        {
            eventReceived = true;
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();
        Assert.Equal(0, transport.IncomingQueueDepth);

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{"paramName":"paramValue"}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);

        Assert.True(owner.IsDisposed);
        Assert.False(eventReceived);
        Assert.Equal(0, transport.IncomingQueueDepth);
    }

    [Fact]
    public async Task TestDisposeWaitsForMessageProcessingAfterConnectionLoss()
    {
        // Connection loss completes the incoming message queue but deliberately does not await the reader,
        // because the handler runs on the connection's receive loop. Disposal must do that waiting, or it
        // tears down resources while observers are still being notified. The gate is never opened, so the
        // wait can only end by timing out - which is exactly what proves disposal waited at all.
        List<string> logMessages = [];
        TaskCompletionSource processingReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestTimeProvider timeProvider = new();
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection, timeProvider)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(10),
            MessageProcessingStarted = () => processingReached.TrySetResult(),
            MessageProcessingGate = () => gate.Task,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logMessages)
            {
                logMessages.Add(e.Message);
            }

            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);

        // The message is now held inside the processing loop, so the reader cannot complete.
        await processingReached.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.RaiseRemoteDisconnectedEventAsync();

        // The shutdown timeout is elapsed on the virtual clock as soon as disposal arms it.
        Task disposeTask = transport.DisposeAsync().AsTask();
        await timeProvider.AdvanceUntilCompletedAsync(disposeTask, transport.ShutdownTimeout + TimeSpan.FromMilliseconds(1), TestContext.Current.CancellationToken);
        await disposeTask;

        lock (logMessages)
        {
            Assert.Contains("Timed out waiting for message processing to complete during disposal", logMessages);
        }

        gate.TrySetResult();
    }

    [Fact]
    public async Task TestDisposeReturnsOnceMessageProcessingCompletesAfterConnectionLoss()
    {
        // The companion to the timeout case: when the in-flight message finishes, disposal stops waiting and
        // completes without logging the timeout warning.
        List<string> logMessages = [];
        TaskCompletionSource processingReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(30),
            MessageProcessingStarted = () => processingReached.TrySetResult(),
            MessageProcessingGate = () => gate.Task,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logMessages)
            {
                logMessages.Add(e.Message);
            }

            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);

        await processingReached.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.RaiseRemoteDisconnectedEventAsync();

        // Release the held message, then dispose: the reader drains and disposal returns without timing out.
        gate.TrySetResult();
        await transport.DisposeAsync();

        lock (logMessages)
        {
            Assert.DoesNotContain("Timed out waiting for message processing to complete during disposal", logMessages);
        }
    }

    [Fact]
    public async Task TestDisposeWaitsIndefinitelyForMessageProcessingWhenShutdownTimeoutIsInfinite()
    {
        // An unbounded ShutdownTimeout cannot be consumed by an earlier wait, so the remaining-budget
        // calculation must hand the message-processing wait an infinite timeout rather than arithmetic on
        // Timeout.InfiniteTimeSpan, which is negative and would make the wait give up at once.
        List<string> logMessages = [];
        TaskCompletionSource processingReached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            ShutdownTimeout = Timeout.InfiniteTimeSpan,
            MessageProcessingStarted = () => processingReached.TrySetResult(),
            MessageProcessingGate = () => gate.Task,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            lock (logMessages)
            {
                logMessages.Add(e.Message);
            }

            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);

        await processingReached.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await connection.RaiseRemoteDisconnectedEventAsync();

        // Disposal must still be waiting on the held message; releasing it is what lets disposal finish.
        ValueTask disposeTask = transport.DisposeAsync();
        gate.TrySetResult();
        await disposeTask.AsTask().WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        lock (logMessages)
        {
            Assert.DoesNotContain("Timed out waiting for message processing to complete during disposal", logMessages);
        }
    }

    [Fact]
    public async Task TestSendCommandWrapsSerializationFailures()
    {
        TestWebSocketConnection connection = new();
        FailingSerializationTransport transport = new(connection, new NotSupportedException("no metadata"));
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        WebDriverBiDiSerializationException exception = await Assert.ThrowsAsync<WebDriverBiDiSerializationException>(
            async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
        Assert.Contains("Could not serialize command 'module.command'", exception.Message);
        Assert.IsType<NotSupportedException>(exception.InnerException);
        Assert.Equal(0, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestSendCommandDoesNotWrapUnrelatedSerializationExceptions()
    {
        // Only the exception types the JSON serializer itself raises are translated; anything
        // else coming out of an overridden SerializeCommand is the override's own problem.
        TestWebSocketConnection connection = new();
        FailingSerializationTransport transport = new(connection, new InvalidOperationException("custom failure"));
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestSendCommandRejectsParametersExtensionDataNamedForASerializedProperty()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        TestCommandParameters parameters = new("module.command");
        parameters.AdditionalData["parameterName"] = "shadowingValue";

        WebDriverBiDiSerializationException exception = await Assert.ThrowsAsync<WebDriverBiDiSerializationException>(
            async () => await transport.SendCommandAsync(parameters, TestContext.Current.CancellationToken));
        Assert.StartsWith("Could not serialize command 'module.command' (command ID: 1): The AdditionalData entry 'parameterName'", exception.Message);
        Assert.IsType<WebDriverBiDiSerializationException>(exception.InnerException);
        Assert.Equal(0, transport.PendingCommandCount);
        Assert.Null(connection.DataSent);
    }

    [Fact]
    public async Task TestSendCommandRejectsNestedExtensionDataNamedForASerializedProperty()
    {
        // The rule the parameters root follows holds for every object inside the parameters that carries its own
        // extension data, so a nested entry is rejected before anything is sent.
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Storage.PartialCookie cookie = new("cookieName", Network.BytesValue.FromString("cookieValue"), "example.com");
        cookie.AdditionalData["domain"] = "other.example.com";

        WebDriverBiDiSerializationException exception = await Assert.ThrowsAsync<WebDriverBiDiSerializationException>(
            async () => await transport.SendCommandAsync(new Storage.SetCookieCommandParameters(cookie), TestContext.Current.CancellationToken));
        Assert.StartsWith("Could not serialize command 'storage.setCookie' (command ID: 1): The AdditionalData entry 'domain'", exception.Message);
        Assert.Contains(typeof(Storage.PartialCookie).FullName!, exception.Message);
        Assert.IsType<WebDriverBiDiSerializationException>(exception.InnerException);
        Assert.Equal(0, transport.PendingCommandCount);
        Assert.Null(connection.DataSent);
    }

    [Fact]
    public async Task TestSendCommandRejectsNestedExtensionDataOfATypeFromARegisteredResolver()
    {
        // Registration rebuilds the serializer state; the rebuilt options must still carry the guard.
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        JsonConverters.GuardedConsumerCommandParameters parameters = new();
        parameters.Nested.AdditionalData["label"] = "shadowingValue";

        WebDriverBiDiSerializationException exception = await Assert.ThrowsAsync<WebDriverBiDiSerializationException>(
            async () => await transport.SendCommandAsync(parameters, TestContext.Current.CancellationToken));
        Assert.StartsWith("Could not serialize command 'custom.guardedCommand' (command ID: 1): The AdditionalData entry 'label'", exception.Message);
        Assert.Null(connection.DataSent);
    }

    [Fact]
    public async Task TestSendCommandRejectsEnvelopeExtensionPropertyNamedForAnEnvelopeProperty()
    {
        TestWebSocketConnection connection = new();
        await using EnvelopeShadowingTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        WebDriverBiDiSerializationException exception = await Assert.ThrowsAsync<WebDriverBiDiSerializationException>(
            async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
        Assert.StartsWith("Could not serialize command 'module.command' (command ID: 1): The AdditionalCommandProperties entry 'method'", exception.Message);
        Assert.Equal(0, transport.PendingCommandCount);
        Assert.Null(connection.DataSent);
    }

    private sealed class EnvelopeShadowingTransport : Transport
    {
        public EnvelopeShadowingTransport(Connection connection)
            : base(connection)
        {
        }

        protected override Command CreateCommand(CommandParameters commandData)
        {
            Command command = base.CreateCommand(commandData);
            command.AdditionalCommandProperties["method"] = "shadowingValue";
            return command;
        }
    }

    private sealed class FailingSerializationTransport : Transport
    {
        private readonly Exception exception;

        public FailingSerializationTransport(Connection connection, Exception exception)
            : base(connection)
        {
            this.exception = exception;
        }

        protected override byte[] SerializeCommand(Command command)
        {
            throw this.exception;
        }
    }

    [Fact]
    public async Task TestCommandSentFromHandlerDuringDisconnectFailsFastWithoutWaitingForShutdownTimeout()
    {
        // DisconnectAsync marks the transport disconnected, then holds the connection lock while
        // it waits (up to ShutdownTimeout) for the message-processing task. A synchronous event
        // handler that sends a command inside that window must fail immediately with a connection
        // exception; if it instead blocked on the lock, the handler (and so the processing task,
        // and so the disconnect) could not finish until the shutdown wait timed out.
        //
        // The transport's clock is virtual and never advanced, so neither the shutdown wait nor the
        // connection lock wait can ever time out. The disconnect can therefore complete only because
        // the handler's send failed at once; a send that blocked on the lock would leave it waiting,
        // and the deadlock detector below would fail the test. Nothing depends on how long it takes.
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource disconnectReachedConnectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Exception? handlerException = null;

        StopSignalingWebSocketConnection connection = new(disconnectReachedConnectionTaskCompletionSource);
        TestTimeProvider timeProvider = new();
        await using TestTransport transport = new(connection, timeProvider);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(async e =>
        {
            handlerStartedTaskCompletionSource.TrySetResult();

            // Connection.StopAsync is called by DisconnectAsync only after it has marked the
            // transport disconnected and while it still holds the connection lock.
            await disconnectReachedConnectionTaskCompletionSource.Task;
            try
            {
                await transport.SendCommandAsync(new TestCommandParameters("module.command"));
            }
            catch (Exception ex)
            {
                handlerException = ex;
            }
        });

        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        try
        {
            // The handler's send fails at once, so the processing task finishes as soon as the handler
            // returns, and the disconnect with it.
            await transport.DisconnectAsync(TestContext.Current.CancellationToken).WaitAsync(DeadlockDetectionTimeout, TestContext.Current.CancellationToken);
        }
        finally
        {
            // Should the send ever block again, the deadlock detector fails the test, but disposing the
            // transport would then wait on the same never-advanced shutdown timeout and hang the run.
            // Elapsing every timeout lets a deadlocked disconnect unwind so the failure is reported; after
            // a disconnect that completed, there is nothing left for the advance to affect.
            timeProvider.Advance(TimeSpan.FromHours(1));
        }

        WebDriverBiDiConnectionException connectionException = Assert.IsType<WebDriverBiDiConnectionException>(handlerException);
        Assert.Contains("Transport must be connected", connectionException.Message);
    }

    private sealed class StopSignalingWebSocketConnection : TestWebSocketConnection
    {
        private readonly TaskCompletionSource stopCalledTaskCompletionSource;

        public StopSignalingWebSocketConnection(TaskCompletionSource stopCalledTaskCompletionSource)
        {
            this.stopCalledTaskCompletionSource = stopCalledTaskCompletionSource;
        }

        protected override Task StopConnectionAsync(CancellationToken cancellationToken)
        {
            this.stopCalledTaskCompletionSource.TrySetResult();
            return base.StopConnectionAsync(cancellationToken);
        }
    }

    [Fact]
    public async Task TestSendCommandFailsWhenDisconnectedBetweenFastPathCheckAndLockAcquisition()
    {
        // The pre-lock connected check in SendCommandAsync is an optimization; the check under
        // the lock is the guarantee. Disconnect the transport after the fast path has passed
        // but before the lock is acquired, and verify the command is still rejected.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        bool disconnectTriggered = false;
        transport.BeforeAcquireLockCallback = async () =>
        {
            // The callback runs for every lock acquisition, including the one made by the
            // DisconnectAsync call below; the flag keeps this a one-shot re-entrancy.
            if (!disconnectTriggered)
            {
                disconnectTriggered = true;
                await transport.DisconnectAsync(TestContext.Current.CancellationToken);
            }
        };

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(
            async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
        Assert.Contains("Transport must be connected", exception.Message);
        Assert.True(disconnectTriggered);
        Assert.Equal(0, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestSendCommandFailsWhenReconnectedBetweenFastPathCheckAndLockAcquisition()
    {
        // A command draws its ID from the command counter of the session that is current when it
        // is created, but that ID is not registered until the connection lock is acquired. If the
        // transport disconnects and reconnects in between, the connected check under the lock
        // passes (the transport really is connected) while the command's ID belongs to the
        // session that has since ended. The command must be rejected rather than registered
        // against the new session's collection.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        string initialCollectionId = transport.TestPendingCommandCollectionId;

        bool reconnectTriggered = false;
        transport.BeforeAcquireLockCallback = async () =>
        {
            // The callback runs for every lock acquisition, including those made by the
            // DisconnectAsync and ConnectAsync calls below; the flag keeps this a one-shot
            // re-entrancy.
            if (!reconnectTriggered)
            {
                reconnectTriggered = true;
                await transport.DisconnectAsync(TestContext.Current.CancellationToken);
                await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
            }
        };

        WebDriverBiDiConnectionException exception = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(
            async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));

        // The message distinguishes the two guards inside the lock: the transport is connected
        // again after the reconnect, so this rejection came from the collection identity check
        // rather than from the connected check immediately above it.
        Assert.Contains("The connection was replaced while the command was being prepared", exception.Message);
        Assert.True(reconnectTriggered);
        Assert.NotEqual(initialCollectionId, transport.TestPendingCommandCollectionId);
        Assert.Equal(0, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestSendCommandAfterRejectedReconnectRaceDoesNotCollideOnCommandId()
    {
        // A command raced by a reconnect is rejected rather than registered with the new session, so
        // the new session holds only its own commands. Command IDs are unique for the life of the
        // transport, so even a stale command that was registered could not occupy an ID the new
        // session issues; the pending count is what shows the stale command was kept out.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        bool reconnectTriggered = false;
        transport.BeforeAcquireLockCallback = async () =>
        {
            if (!reconnectTriggered)
            {
                reconnectTriggered = true;
                await transport.DisconnectAsync(TestContext.Current.CancellationToken);
                await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
            }
        };

        // Deliberately tolerant: the rejection itself is asserted by the sibling test above. Here
        // the raced command is only the setup, so that a regression surfaces on the command it
        // would collide with rather than on this one.
        bool racedCommandRejected = false;
        try
        {
            await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        }
        catch (WebDriverBiDiConnectionException)
        {
            racedCommandRejected = true;
        }

        // The raced command drew ID 1 in the previous session; the reconnect does not reset the
        // counter, so this command is numbered 2.
        transport.BeforeAcquireLockCallback = null;
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);

        Assert.True(racedCommandRejected);
        Assert.Equal(2, command.CommandId);
        Assert.Equal(1, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestAsynchronousObserverFaultOnConnectionEventIsReported()
    {
        // A fault raised after an asynchronously-run handler has already returned cannot propagate
        // to a caller. For the connection's own events it was previously observed and then
        // discarded; the transport now routes it through the same pipeline as a fault in an
        // observer of a transport or module event.
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);

        EventHandlerErrorOccurredEventArgs? reportedError = null;
        TaskCompletionSource errorReported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            reportedError = e;
            errorReported.TrySetResult();
        });

        // Declared explicitly as Action<T> so the handler binds to the overload that queues the
        // whole handler to the thread pool, making the throw a post-return fault of that task.
        Action<LogMessageEventArgs> throwingHandler = e => throw new InvalidOperationException("connection log observer failure");
        EventObserver<LogMessageEventArgs> observer = connection.OnLogMessage.AddObserver(
            throwingHandler,
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Warn);
        await errorReported.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(reportedError);
        Assert.Equal(observer.Id, reportedError.ErrorInfo.ObserverId);
        Assert.Equal(connection.OnLogMessage.EventName, reportedError.ErrorInfo.ObservableEventName);
        Assert.True(reportedError.ErrorInfo.IsAsynchronousHandler);
        Assert.True(reportedError.ErrorInfo.FaultOccurredAfterHandlerReturned);
        InvalidOperationException exception = Assert.IsType<InvalidOperationException>(reportedError.ErrorInfo.Exception);
        Assert.Equal("connection log observer failure", exception.Message);
    }

    [Fact]
    public async Task TestAsynchronousObserverFaultOnConnectionEventIsReportedForObserverAddedBeforeTransport()
    {
        // The reporter is installed by the Transport constructor, which can run after a caller has
        // already added observers to the connection's events. Because the reporter is read when a
        // fault is reported rather than captured when an observer is added, an observer added
        // first is still covered.
        TestWebSocketConnection connection = new();

        Action<LogMessageEventArgs> throwingHandler = e => throw new InvalidOperationException("pre-existing observer failure");
        EventObserver<LogMessageEventArgs> observer = connection.OnLogMessage.AddObserver(
            throwingHandler,
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await using Transport transport = new(connection);

        EventHandlerErrorOccurredEventArgs? reportedError = null;
        TaskCompletionSource errorReported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            reportedError = e;
            errorReported.TrySetResult();
        });

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Warn);
        await errorReported.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(reportedError);
        Assert.Equal(observer.Id, reportedError.ErrorInfo.ObserverId);
        InvalidOperationException exception = Assert.IsType<InvalidOperationException>(reportedError.ErrorInfo.Exception);
        Assert.Equal("pre-existing observer failure", exception.Message);
    }

    [Fact]
    public async Task TestAsynchronousObserverFaultOnConnectionEventIsCollected()
    {
        // Being routed through the unhandled-error pipeline means the fault is governed by
        // EventHandlerExceptionBehavior, exactly as for transport and module events.
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        Action<LogMessageEventArgs> throwingHandler = e => throw new WebDriverBiDiException("collected connection observer failure");
        connection.OnLogMessage.AddObserver(throwingHandler, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Warn);
        Assert.True(await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(5), TransportErrorBehavior.Collect));

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(
            async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Contains(exception.InnerExceptions, inner => inner.Message.Contains("collected connection observer failure"));
    }

    [Fact]
    public async Task TestAsynchronousObserverFaultOnConnectionEventIsDiscardedWhenIgnored()
    {
        // Ignore is the default, so the fault is still observed (no UnobservedTaskException) but
        // is neither collected nor surfaced by the disconnect.
        TestWebSocketConnection connection = new();
        await using TestTransport transport = new(connection);
        Assert.Equal(TransportErrorBehavior.Ignore, transport.EventHandlerExceptionBehavior);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        TaskCompletionSource errorReported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnEventHandlerErrorOccurred.AddObserver(e => errorReported.TrySetResult());

        Action<LogMessageEventArgs> throwingHandler = e => throw new WebDriverBiDiException("ignored connection observer failure");
        connection.OnLogMessage.AddObserver(throwingHandler, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Warn);

        // The event still fires for observability; only the unhandled-error collection is skipped.
        await errorReported.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("array")]
    [InlineData("object")]
    [InlineData("node")]
    [InlineData("shadowRoot")]
    public async Task TestResponseNestedToTheLimitIsRead(string shape)
    {
        int levels = MaximumLevels(shape);
        Command command = await SendEvaluateAndReceiveAsync(CreateNestedRemoteValue(shape, levels, isMalformed: false));

        Assert.True(command.TryGetResult(out CommandResult? result), $"Command failed: {command.ThrownException}");
        EvaluateResultSuccess success = Assert.IsType<EvaluateResultSuccess>(result);
        Assert.Equal(levels, CountNestingLevels(success.Result));
    }

    [Theory]
    [InlineData("array")]
    [InlineData("object")]
    [InlineData("node")]
    [InlineData("shadowRoot")]
    public async Task TestResponseNestedBeyondTheLimitFailsTheCommand(string shape)
    {
        // The response is still recognized as the command's response, so the command fails at once rather
        // than waiting for a response that was discarded as an unknown message.
        Command command = await SendEvaluateAndReceiveAsync(CreateNestedRemoteValue(shape, MaximumLevels(shape) + 1, isMalformed: false));

        WebDriverBiDiSerializationException exception = Assert.IsType<WebDriverBiDiSerializationException>(command.ThrownException);
        JsonException innerException = Assert.IsType<JsonException>(exception.InnerException, exactMatch: false);
        Assert.Contains($"maximum configured depth of {MaxJsonDepth}", innerException.Message);
    }

    [Theory]
    [InlineData("array")]
    [InlineData("object")]
    [InlineData("node")]
    [InlineData("shadowRoot")]
    public async Task TestMalformedResponseNestedToTheLimitReportsTheMalformedValue(string shape)
    {
        Command command = await SendEvaluateAndReceiveAsync(CreateNestedRemoteValue(shape, MaximumLevels(shape), isMalformed: true));

        WebDriverBiDiSerializationException exception = Assert.IsType<WebDriverBiDiSerializationException>(command.ThrownException);
        JsonException innerException = Assert.IsType<JsonException>(exception.InnerException, exactMatch: false);
        Assert.Contains("'bogus'", innerException.Message);
    }

    [Fact]
    public async Task TestCommandParametersNestedWithinTheLimitAreSent()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // The writer starts no object or array at the maximum depth itself, so the deepest value it writes is
        // one level shallower than the deepest the reader accepts. Each nested array uses two levels.
        CallFunctionCommandParameters parameters = new("() => {}", new ContextTarget("context"), true);
        parameters.Arguments.Add(CreateNestedLocalValue((MaxJsonDepth - 6) / 2));
        Command command = await transport.SendCommandAsync(parameters, TestContext.Current.CancellationToken);

        Assert.NotNull(command);
    }

    [Fact]
    public async Task TestCommandParametersNestedBeyondTheLimitFailToSerialize()
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        CallFunctionCommandParameters parameters = new("() => {}", new ContextTarget("context"), true);
        parameters.Arguments.Add(CreateNestedLocalValue(MaxJsonDepth / 2));
        WebDriverBiDiSerializationException exception = await Assert.ThrowsAsync<WebDriverBiDiSerializationException>(() => transport.SendCommandAsync(parameters, TestContext.Current.CancellationToken));

        JsonException innerException = Assert.IsType<JsonException>(exception.InnerException, exactMatch: false);
        Assert.Contains($"maximum allowed depth of {MaxJsonDepth}", innerException.Message);
    }

    private static async Task<Command> SendEvaluateAndReceiveAsync(string remoteValueJson)
    {
        TestWebSocketConnection connection = new();
        await using Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        Command command = await transport.SendCommandAsync(new EvaluateCommandParameters("expression", new ContextTarget("context"), true), TestContext.Current.CancellationToken);

        await connection.RaiseDataReceivedEventAsync(CreateEvaluateResponse(command.CommandId, remoteValueJson));

        Assert.True(await command.WaitForCompletionAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken), "Command did not complete");
        return command;
    }

    private static string CreateEvaluateResponse(long commandId, string remoteValueJson)
    {
        return $$"""{ "type": "success", "id": {{commandId}}, "result": { "type": "success", "realm": "realm", "result": """ + remoteValueJson + " } }";
    }

    /// <summary>
    /// Gets the largest number of levels of the given shape whose evaluate response is nested no more deeply
    /// than the maximum JSON depth.
    /// </summary>
    private static int MaximumLevels(string shape)
    {
        int levels = 1;
        while (MeasureJsonDepth(CreateEvaluateResponse(1, CreateNestedRemoteValue(shape, levels + 1, isMalformed: false))) <= MaxJsonDepth)
        {
            levels++;
        }

        return levels;
    }

    private static int MeasureJsonDepth(string json)
    {
        // None of the strings in the generated JSON contain brackets or braces.
        int depth = 0;
        int maximumDepth = 0;
        foreach (char character in json)
        {
            if (character is '{' or '[')
            {
                maximumDepth = Math.Max(maximumDepth, ++depth);
            }
            else if (character is '}' or ']')
            {
                depth--;
            }
        }

        return maximumDepth;
    }

    private static string CreateNestedRemoteValue(string shape, int levels, bool isMalformed)
    {
        // A level is one remote value containing the next; the innermost value is the last level, and is
        // malformed when requested.
        string node = """{ "type": "node", "sharedId": "id", "value": { "nodeType": 1, "childNodeCount": 0""";
        (string open, string close, string leaf, string malformedLeaf) = shape switch
        {
            "array" => ("""{ "type": "array", "value": [ """, " ] }", """{ "type": "null" }""", """{ "type": "bogus" }"""),
            "object" => ("""{ "type": "object", "value": [ [ "key", """, " ] ] }", """{ "type": "null" }""", """{ "type": "bogus" }"""),
            "node" => (node + """, "children": [ """, " ] } }", node + " } }", node + """, "mode": "bogus" } }"""),
            _ => (node + """, "shadowRoot": """, " } }", node + " } }", node + """, "mode": "bogus" } }"""),
        };

        StringBuilder builder = new();
        for (int i = 1; i < levels; i++)
        {
            builder.Append(open);
        }

        builder.Append(isMalformed ? malformedLeaf : leaf);
        for (int i = 1; i < levels; i++)
        {
            builder.Append(close);
        }

        return builder.ToString();
    }

    private static int CountNestingLevels(RemoteValue value)
    {
        int levels = 1;
        while (true)
        {
            RemoteValue? next = value switch
            {
                CollectionRemoteValue { Value.Count: > 0 } array => array.Value[0],
                KeyValuePairCollectionRemoteValue { Value.Count: > 0 } obj => obj.Value.Values.First(),
                NodeRemoteValue { Value.Children.Count: > 0 } node => node.Value.Children[0],
                NodeRemoteValue { Value.ShadowRoot: not null } node => node.Value.ShadowRoot,
                _ => null,
            };

            if (next is null)
            {
                return levels;
            }

            value = next;
            levels++;
        }
    }

    private static LocalValue CreateNestedLocalValue(int levels)
    {
        LocalValue value = LocalValue.Null;
        for (int i = 0; i < levels; i++)
        {
            value = LocalValue.Array([value]);
        }

        return value;
    }

    private sealed class NonGenericCommandParameters : CommandParameters
    {
        [JsonIgnore]
        public override string MethodName => "module.command";

        [JsonIgnore]
        public override Type ResponseType => typeof(CommandResponseMessage<TestCommandResult>);
    }
}
