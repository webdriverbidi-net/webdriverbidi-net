namespace WebDriverBiDi.Protocol;

using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using PinchHitter;
using TestUtilities;
using Xunit.Sdk;

public class TransportTests
{
    // --- CT-1 (shutdown deadlock) regression tests ---
    // These verify that a remote close or receive-loop fault occurring while DisconnectAsync holds
    // the connection lock does not deadlock: DisconnectAsync awaits the receive loop (through
    // Connection.StopAsync), while the connection-loss handler runs on that loop, so an unfixed
    // implementation forms a cycle. The 5-second bound below is a deadlock detector, not a timing
    // assumption; a correct implementation completes effectively immediately.
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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        _ = Task.Run(
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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        _ = Task.Run(
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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        _ = Task.Run(
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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        _ = Task.Run(
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
        Assert.IsType<WebDriverBiDiSerializationException>(command.ThrownException);
        Assert.Contains("Error response for command 1 contained incorrect JSON for protocol error", command.ThrownException.Message);
    }

    [Fact]
    public async Task TestTransportGetResponseWithThrownException()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        _ = Task.Run(
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
        Assert.IsType<WebDriverBiDiSerializationException>(command.ThrownException);
        Assert.Contains("Response did not contain properly formed JSON for response type", command.ThrownException.Message);
    }

    [Fact]
    public async Task TestTransportCannotSendCommandWithoutConnection()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Assert.Contains("Transport must be connected to a remote end to execute commands.", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestTransportLeavesCommandResultAndThrownExceptionNullWithoutResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);

        Assert.False(hasResult);
        Assert.Null(commandResult);
        Assert.Null(command.ThrownException);
    }

    [Fact]
    public async Task TestSendCommandExceptionRollsBackPendingCommandState()
    {
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = _ => throw new InvalidOperationException("Simulated send failure"),
        };
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnErrorEventReceived.AddObserver(e =>
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            receivedData = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        transport.OnErrorEventReceived.AddObserver(e =>
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        _ = Task.Run(
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
        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.True(hasResult);
        Assert.NotNull(actualResult);

        Assert.False(actualResult.IsError);
        Assert.IsType<TestCommandResult>(actualResult);

        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.NotNull(convertedResult);
        Assert.Equal("response value", convertedResult.Value);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(2, logs.Count);
        Assert.Contains("Sending command data for command", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Debug, logs[0].Level);
        Assert.Contains("Received result for command", logs[1].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Debug, logs[1].Level);
    }

    [Fact]
    public async Task TestTransportLogsMalformedJsonMessages()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForErrorMessageWithMissingId()
    {
        string json = """
                      {
                        "type": "error",
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            unknownMessageTaskCompletionSource.TrySetResult();
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
        await Task.WhenAll(
            unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal(json, loggedEvent);
        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing error JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForErrorMessageWithMissingErrorProperty()
    {
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "message": "This is a test error message"
                      }
                      """;
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            unknownMessageTaskCompletionSource.TrySetResult();
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
        await Task.WhenAll(
            unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal(json, loggedEvent);
        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing error JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForErrorMessageWithMissingMessageProperty()
    {
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error"
                      }
                      """;
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            unknownMessageTaskCompletionSource.TrySetResult();
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
        await Task.WhenAll(
            unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal(json, loggedEvent);
        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing error JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            taskCompletionSource.TrySetResult();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(json, loggedEvent);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForEventMessageWithMismatchingEventParameters()
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
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            unknownMessageTaskCompletionSource.TrySetResult();
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
        await Task.WhenAll(
            unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal(json, loggedEvent);
        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing event JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForEventMessageWithNullParams()
    {
        // A registered event whose 'params' is JSON null must be reported as a protocol error
        // (surfaced here as an unknown message plus an error-level log), not silently handed to the
        // event dispatch pipeline where it would be misattributed to a user event handler.
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event",
                        "params": null
                      }
                      """;
        string loggedEvent = string.Empty;
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            unknownMessageTaskCompletionSource.TrySetResult();
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
        await Task.WhenAll(
            unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal(json, loggedEvent);
        Assert.Single(logs);
        Assert.Contains("Unexpected error parsing event JSON", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);
    }

    [Fact]
    public async Task TestTransportRaisesUnknownMessageEventForEventMessageDeserializingToNonEventMessageType()
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
        List<LogMessageEventArgs> logs = [];
        TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.RegisterInvalidEventMessageType("protocol.event", typeof(object));

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            loggedEvent = e.Message;
            unknownMessageTaskCompletionSource.TrySetResult();
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
        await Task.WhenAll(
            unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
            logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        Assert.Equal(json, loggedEvent);
        Assert.Single(logs);
        Assert.Contains("Deserialization of event message returned null", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Error, logs[0].Level);
    }

    [Fact]
    public async Task TestTransportCanUseDefaultConnection()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        static void dataReceivedHandler(ServerDataReceivedEventArgs e) { }
        void connectionHandler(ClientConnectionEventArgs e) { taskCompletionSource.TrySetResult(); }
        Server server = new();
        ServerEventObserver<ServerDataReceivedEventArgs> dataReceivedObserver = server.OnDataReceived.AddObserver(dataReceivedHandler);
        ServerEventObserver<ClientConnectionEventArgs> connectedObserver = server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();

        Transport transport = new();
        await transport.ConnectAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await server.StopAsync();
        dataReceivedObserver.Unobserve();
        connectedObserver.Unobserve();
    }

    [Fact]
    public async Task TestCannotConnectWhenAlreadyConnected()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync($"ws://localhost:1234", TestContext.Current.CancellationToken);
        Assert.StartsWith($"The transport is already connected to ws://localhost:1234", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await transport.ConnectAsync($"ws://localhost:5678", TestContext.Current.CancellationToken))).Message);
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

        Task secondConnect = transport.ConnectAsync("ws://localhost:5678", TestContext.Current.CancellationToken);

        startBarrier.SetResult();
        await firstConnect;

        Assert.StartsWith("The transport is already connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await secondConnect)).Message);
    }

    [Fact]
    public async Task TestDisconnectWhenNotConnectedDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestDisconnectWithMultipleConcurrentCallsOperatesCorrectly()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);

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
        TestTransport transport = new(connection);

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // First disconnect - this will set IsConnected = false
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

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            MessageProcessingDelay = TimeSpan.FromMilliseconds(100)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://example.com:1234", TestContext.Current.CancellationToken);

        TestCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command, TestContext.Current.CancellationToken);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await transport.ConnectAsync("ws://example.com:5678", TestContext.Current.CancellationToken);
        _ = await transport.SendCommandAsync(command, TestContext.Current.CancellationToken);

        dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.Equivalent(expected, dataValue);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestExceptionInTransportEventReceivedCanCollect()
    {
        string receivedName = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        string receivedName = string.Empty;
        TaskCompletionSource firstEventTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondEventTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int callCount = 0;

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);

        // The original handler failure is captured after the error event is raised; the
        // error observer's own asynchronous fault is captured without re-raising.
        await secondCaptureTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // Negative check: a feedback loop would keep re-invoking the error observer and
        // capturing further errors, so after this delay neither count may have grown.
        await Task.Delay(TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken);
        Assert.Equal(1, Volatile.Read(ref errorObserverInvocationCount));
        Assert.Equal(2, Volatile.Read(ref capturedErrorCount));

        AggregateException exception = await Assert.ThrowsAnyAsync<AggregateException>(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken));
        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("original handler failure"));
        Assert.Contains(exception.InnerExceptions, e => e.Message.Contains("error observer failure"));
    }

    [Fact]
    public async Task TestExceptionInTransportEventReceivedCanTerminate()
    {
        // string receivedName = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Collect);
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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Terminate);
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
        string receivedName = string.Empty;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        Assert.Equal(0, transport.LastTestCommandId);

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken);
        _ = Task.Run(
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
        Assert.Equal(1, transport.LastTestCommandId);
    }

    [Fact]
    public async Task TestTransportSubclassesCanAccessConnection()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        Assert.Equal(connection, transport.GetConnection());
    }

    [Fact]
    public async Task TestTransportShutdownTimeoutDefaultValue()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.Equal(TimeSpan.FromSeconds(10), transport.ShutdownTimeout);
    }

    [Fact]
    public async Task TestTransportShutdownTimeoutCanBeSet()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
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
        Transport transport = new(connection);

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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);

        // Documented behavior: reads before ConnectAsync return zero rather than throw.
        Assert.Equal(0, transport.PendingCommandCount);
    }

    [Fact]
    public async Task TestTransportPendingCommandCountReflectsSentCommands()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [injectedFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [firstFault, secondFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromMilliseconds(250),
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

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

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromMilliseconds(250),
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // With the reader task suspended in the hanging handler, this second
        // message is guaranteed to remain unread in the incoming message queue,
        // so the queue can never drain during shutdown.
        await connection.RaiseDataReceivedEventAsync(json);

        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

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
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestCanDisposeWhileConnected()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await transport.DisposeAsync();
    }

    [Fact]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        Command oldCommand = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        Assert.Equal(1, transport.PendingCommandCount);

        // Disconnecting clears the pending collection, canceling the command it still held.
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
        Assert.True(oldCommand.IsCanceled);
        Assert.Equal(0, transport.PendingCommandCount);

        // A reconnected transport starts with an empty collection and accepts new commands.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        _ = Task.Run(async () => await connection.RaiseDataReceivedEventAsync(responseJson), TestContext.Current.CancellationToken);

        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);
        transport.CancelCommand(command);
        transport.CancelCommand(command);
    }

    [Fact]
    public async Task TestCancelCommandPreventsLateResponseFromSettingResult()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), cancellationToken: TestContext.Current.CancellationToken);
        transport.CancelCommand(command);

        string responseJson = $$$"""{"type":"success","id":{{{command.CommandId}}},"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(responseJson);

        bool hasResult = command.TryGetResult(out CommandResult? commandResult);

        Assert.True(command.IsCanceled);
        Assert.False(hasResult);
        Assert.Null(commandResult);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverBeforeConnecting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverMultipleTimesBeforeConnecting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
        await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverAfterConnectingThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        Assert.Contains("Cannot register a type info resolver after the transport is connected", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRegisterTypeInfoDuringConnectIsSynchronized()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        Task firstCallerReadyTask = transport.EnableConnectLockConcurrencyTesting();

        // Start ConnectAsync first; wait until it has entered the lock callback before
        // starting RegisterTypeInfoResolverAsync. This guarantees ConnectAsync acquires
        // the semaphore first and sets IsConnected before RegisterTypeInfoResolverAsync
        // reads it, making the test deterministic regardless of thread scheduling.
        Task connectTask = transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await firstCallerReadyTask;
        Task registerTask = transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
        await connectTask;

        Assert.Contains("Cannot register a type info resolver after the transport is connected", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await registerTask)).Message);
    }

    [Fact]
    public async Task TestRegisterNullTypeInfoThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);

        // Never call ConnectAsync - IsConnected remains false
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost"));

        // Should not throw; early return path taken. Verify transport rejects commands.
        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionErrorWhenAlreadyDisconnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        // IsConnected is now false; raise error (e.g., receive loop dying during shutdown)
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost"));

        // Should not throw; early return path taken. Verify still disconnected.
        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionErrorWhenDisconnectRacesHitsInnerReturnBranch()
    {
        // Covers the disconnect-ownership signal branch of HandleConnectionDisconnectionAsync:
        // OnConnectionErrorAsync passes the fast-path, then observes DisconnectAsync's ownership
        // signal completing before it acquires the lock, and returns without tearing down (handing
        // the lock back through its completion continuation). The inner "if (!this.IsConnected)
        // return" branch is covered separately by TestConcurrentConnectionLossEventsHitInnerReturnBranch.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        _ = transport.EnableConnectLockConcurrencyTesting();

        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost during race"));
        await disconnectTask;

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestConnectionErrorFailsMultiplePendingCommands()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
    public async Task TestRemoteDisconnectFailsPendingCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new(commandName);
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRemoteDisconnectLogsMessage()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        Assert.Contains(logs,
            log => log.Message.Contains("Remote end closed connection")
                   && log.Level == WebDriverBiDiLogLevel.Warn);
    }

    [Fact]
    public async Task TestRemoteDisconnectWhenNotConnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRemoteDisconnectWhenAlreadyDisconnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestRemoteDisconnectFailsMultiplePendingCommands()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        Command command1 = await transport.SendCommandAsync(new TestCommandParameters("module.command1"), TestContext.Current.CancellationToken);
        Command command2 = await transport.SendCommandAsync(new TestCommandParameters("module.command2"), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        await command1.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);
        await command2.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);

        Assert.IsType<WebDriverBiDiConnectionException>(command1.ThrownException);
        Assert.IsType<WebDriverBiDiConnectionException>(command2.ThrownException);
    }

    [Fact]
    public async Task TestRemoteDisconnectWhenDisconnectRacesHitsInnerReturnBranch()
    {
        // Covers the disconnect-ownership signal branch of HandleConnectionDisconnectionAsync: the
        // remote-disconnect handler passes the fast-path (IsConnected == true), then observes
        // DisconnectAsync's ownership signal completing before it acquires the lock, and returns
        // without tearing down (handing the lock back through its completion continuation). The
        // inner "if (!this.IsConnected) return" branch is covered separately by
        // TestConcurrentConnectionLossEventsHitInnerReturnBranch.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        _ = transport.EnableConnectLockConcurrencyTesting();

        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await connection.RaiseRemoteDisconnectedEventAsync();
        await disconnectTask;

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
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
        TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [injectedFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
    public async Task TestCollectedExceptionsAreSurfacedOnlyOnceAcrossRepeatedDisconnectCalls()
    {
        // Companion to TestCollectedExceptionsAreSurfacedOnDisconnectAfterRemoteDisconnect:
        // once collected exceptions have been thrown from one DisconnectAsync call, a
        // second call (e.g., a caller invoking StopAsync twice) must not re-throw the
        // same stale exceptions, since UnhandledErrorCollection.TryGetExceptions does not
        // remove entries from the collection.
        InvalidOperationException injectedFault = new("simulated outer-loop fault");
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            ReadLoopOuterFault = [injectedFault],
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        Assert.Equal(TransportErrorBehavior.Ignore, transport.EventHandlerExceptionBehavior);
        transport.OnErrorEventReceived.AddObserver(e =>
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnErrorEventReceived.AddObserver(e =>
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnErrorEventReceived.AddObserver(e =>
            throw new WebDriverBiDiException("Error handler exception"));
        string json = """
                      {
                        "type": "error",
                        "id": null,
                        "error": "unknown error",
                        "message": "This is a test error message"
                      }
                      """;
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
        {
            // Keep the reconnect wait short so that a regression fails fast rather than
            // stalling for the default ten-second shutdown timeout.
            ShutdownTimeout = TimeSpan.FromMilliseconds(500),
        };
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
        {
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };
        Assert.Equal(TransportErrorBehavior.Ignore, transport.EventHandlerExceptionBehavior);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection);

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            EventHandlerExceptionBehavior = TransportErrorBehavior.Ignore,
            AfterUnhandledErrorCaptured = () => taskCompletionSource.TrySetResult(),
        };

        // Add the log observer after the connect to prevent capturing connection diagnostic messages.
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await transport.ConnectAsync("ws://localhost", cts.Token));
    }

    [Fact]
    public async Task TestSendCommandAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
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
        await transport.ConnectAsync("ws:localhost", testCancellationToken);
        await transport.DisconnectAsync(testCancellationToken);

        // Second session: reconnect and send a command that stays pending (the connection double
        // never produces a response).
        await transport.ConnectAsync("ws:localhost", testCancellationToken);
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
    /// Covers the inner <c>if (!this.IsConnected) return</c> re-check in
    /// <c>HandleConnectionDisconnectionAsync</c> (the path where a connection-loss handler acquires
    /// the connection lock and finds the transport already disconnected).
    /// </summary>
    /// <remarks>
    /// A <see cref="Transport.DisconnectAsync(CancellationToken)"/> racing a loss handler is resolved
    /// through the disconnect-ownership signal, so it no longer reaches this inner re-check (that path
    /// is covered by <c>TestRemoteDisconnectWhenDisconnectRacesHitsInnerReturnBranch</c> and
    /// <c>TestConnectionErrorWhenDisconnectRacesHitsInnerReturnBranch</c> elsewhere in this class). The
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
        await transport.ConnectAsync("ws:localhost", testCancellationToken);

        // Choreograph the two connection-lock acquisitions: the first loss handler enters the lock,
        // and the second is held at its fast-path-passed / pre-lock point until the first has
        // acquired the lock, guaranteeing both saw IsConnected == true before either tore down.
        Task firstHandlerEnteredLockAcquisition = transport.EnableConnectLockConcurrencyTesting();

        // First loss event: acquires the lock and performs the teardown.
        Task firstLossHandler = connection.RaiseConnectionErrorEventAsync(new Exception("first connection loss"));
        await firstHandlerEnteredLockAcquisition;

        // Second loss event: passes the fast-path while the first still holds the lock, then waits
        // for the lock and, on acquiring it, hits the inner re-check with IsConnected already false.
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
        await transport.ConnectAsync("ws:localhost", testCancellationToken);

        // Signalled by the connection-loss handler when it enters its connection-lock acquisition,
        // i.e., once it has passed its fast-path check (IsConnected is still true at that point,
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

        // The transport is disconnected AND the connection lock was handed back: a follow-up command
        // fails fast with a connection exception instead of blocking on a lock that was never released.
        // (IsConnected is internal to the library, so this is the observable proxy for both facts.)
        TestCommandParameters commandParameters = new("module.command");
        WebDriverBiDiConnectionException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(
            async () => await transport.SendCommandAsync(commandParameters, testCancellationToken));
        Assert.Contains("Transport must be connected", exception.Message);

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
        Transport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnUnknownMessageReceived.AddObserver(e => unknownMessageReceived = true);
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.Contains("Discarding late response"))
            {
                discardLog = e;
                discardedTaskCompletionSource.TrySetResult();
            }
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection)
        {
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnErrorEventReceived.AddObserver(e => errorEventReceived = true);
        transport.OnLogMessage.AddObserver(e =>
        {
            if (e.Message.Contains("Discarding late response") && e.Message.Contains("(Canceled)"))
            {
                discardedTaskCompletionSource.TrySetResult();
            }
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        Transport transport = new(connection);
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
        TestTransport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
        {
            UnknownMessageBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(e => eventReceived = true);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Terminate,
            AfterUnhandledErrorCaptured = () => captured.TrySetResult(),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        TestTransport transport = new(connection)
        {
            UnexpectedErrorBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnErrorEventReceived.AddObserver(e => errorTaskCompletionSource.TrySetResult());
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        await connection.RaiseDataReceivedEventAsync("""{"type":"error","id":999,"error":"unknown error","message":"no such command"}""");
        await errorTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The transport notifies OnErrorEventReceived observers before it records the error in
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
        TestTransport transport = new(connection);
        transport.UseCanceledCommandTrackerCapacity(1);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            unknownMessage = e.Message;
            unknownTaskCompletionSource.TrySetResult();
        });
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        int eventCount = 0;

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The remote end closes the connection while the handler is still executing.
        await connection.RaiseRemoteDisconnectedEventAsync();

        // Reconnecting must block until the previous reader has exited. The fixed delay is a
        // negative check: the reconnect must still be pending after it elapses.
        Task reconnectTask = transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        Task completedTask = await Task.WhenAny(reconnectTask, Task.Delay(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken));
        Assert.NotSame(reconnectTask, completedTask);

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

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromMilliseconds(250),
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();
        Assert.Equal(0, transport.IncomingQueueDepth);

        TrackingMemoryOwner owner = new(Encoding.UTF8.GetBytes("""{"type":"event","method":"protocol.event","params":{"paramName":"paramValue"}}"""));
        await connection.RaiseDataReceivedEventAsync(owner, owner.Length);

        Assert.True(owner.IsDisposed);
        Assert.False(eventReceived);
        Assert.Equal(0, transport.IncomingQueueDepth);
    }

    [Fact]
    public async Task TestSendCommandWrapsSerializationFailures()
    {
        TestWebSocketConnection connection = new();
        FailingSerializationTransport transport = new(connection, new NotSupportedException("no metadata"));
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken));
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
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource disconnectReachedConnectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Exception? handlerException = null;
        List<LogMessageEventArgs> logs = [];

        StopSignalingWebSocketConnection connection = new(disconnectReachedConnectionTaskCompletionSource);
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnLogMessage.AddObserver(e =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // The disconnect must complete well inside ShutdownTimeout: the handler's send fails at once,
        // so the processing task finishes as soon as the handler returns.
        await transport.DisconnectAsync(TestContext.Current.CancellationToken).WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        WebDriverBiDiConnectionException connectionException = Assert.IsType<WebDriverBiDiConnectionException>(handlerException);
        Assert.Contains("Transport must be connected", connectionException.Message);
        Assert.DoesNotContain(logs, log => log.Message.Contains("Timed out waiting for message processing to complete during shutdown"));
    }

    private sealed class StopSignalingWebSocketConnection : TestWebSocketConnection
    {
        private readonly TaskCompletionSource stopCalledTaskCompletionSource;

        public StopSignalingWebSocketConnection(TaskCompletionSource stopCalledTaskCompletionSource)
        {
            this.stopCalledTaskCompletionSource = stopCalledTaskCompletionSource;
        }

        public override Task StopAsync(CancellationToken cancellationToken = default)
        {
            this.stopCalledTaskCompletionSource.TrySetResult();
            return base.StopAsync(cancellationToken);
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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

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
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
                await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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
        // Regression test for the consequence of registering a stale command: ConnectAsync resets
        // the command counter, so a command carried over from the previous session occupies an ID
        // the new session will issue again. The victim is then the later, unrelated command, which
        // fails with "Could not add command with id 1, as id already exists". Rejecting the stale
        // command keeps the new session's first ID free.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        bool reconnectTriggered = false;
        transport.BeforeAcquireLockCallback = async () =>
        {
            if (!reconnectTriggered)
            {
                reconnectTriggered = true;
                await transport.DisconnectAsync(TestContext.Current.CancellationToken);
                await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
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

        // The raced command drew ID 1 from the previous session, and the reconnect reset the
        // counter, so the new session numbers this command 1 as well. Without the identity check
        // the raced command already occupies that ID, and this unrelated command is the one that
        // fails, with "Could not add command with id 1, as id already exists".
        transport.BeforeAcquireLockCallback = null;
        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"), TestContext.Current.CancellationToken);

        Assert.True(racedCommandRejected);
        Assert.Equal(1, command.CommandId);
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
        Transport transport = new(connection);

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

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Debug);
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

        Transport transport = new(connection);

        EventHandlerErrorOccurredEventArgs? reportedError = null;
        TaskCompletionSource errorReported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            reportedError = e;
            errorReported.TrySetResult();
        });

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Debug);
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
        TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        Action<LogMessageEventArgs> throwingHandler = e => throw new WebDriverBiDiException("collected connection observer failure");
        connection.OnLogMessage.AddObserver(throwingHandler, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Debug);
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
        TestTransport transport = new(connection);
        Assert.Equal(TransportErrorBehavior.Ignore, transport.EventHandlerExceptionBehavior);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);

        TaskCompletionSource errorReported = new(TaskCreationOptions.RunContinuationsAsynchronously);
        transport.OnEventHandlerErrorOccurred.AddObserver(e => errorReported.TrySetResult());

        Action<LogMessageEventArgs> throwingHandler = e => throw new WebDriverBiDiException("ignored connection observer failure");
        connection.OnLogMessage.AddObserver(throwingHandler, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        await connection.RaiseLogMessageEventAsync("connection log message", WebDriverBiDiLogLevel.Debug);

        // The event still fires for observability; only the unhandled-error collection is skipped.
        await errorReported.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    private sealed class NonGenericCommandParameters : CommandParameters
    {
        [JsonIgnore]
        public override string MethodName => "module.command";

        [JsonIgnore]
        public override Type ResponseType => typeof(CommandResponseMessage<TestCommandResult>);
    }
}
