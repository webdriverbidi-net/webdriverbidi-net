namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using PinchHitter;
using TestUtilities;
using Xunit.Sdk;

public class TransportTests
{
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
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);
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
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250), TestContext.Current.CancellationToken);
        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.True(hasResult);
        Assert.NotNull(actualResult);

        Assert.False(actualResult.IsError);
        Assert.IsType<TestCommandResult>(actualResult);

        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.NotNull(convertedResult);

        Assert.Equal("response value", convertedResult.Value);
        Assert.Single(convertedResult.AdditionalData);
        Assert.Equal("extraDataValue", convertedResult.AdditionalData["extraDataName"]);
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
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250), TestContext.Current.CancellationToken);
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
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250), TestContext.Current.CancellationToken);
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
        object? receivedData = null;
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
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);
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
    public async Task TestConcurrentDisconnectCallsAreThreadSafe()
    {
        // This test verifies the double-checked locking fix for the race condition
        // where multiple threads could pass the fast-path IsConnected check simultaneously
        // and then all proceed to execute disconnect logic.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            // Add a small delay to increase the likelihood of threads overlapping
            DisconnectDelay = TimeSpan.FromMilliseconds(50)
        };

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Launch multiple concurrent disconnect calls
        const int concurrentCalls = 5;
        Task[] disconnectTasks = new Task[concurrentCalls];

        for (int i = 0; i < concurrentCalls; i++)
        {
            disconnectTasks[i] = Task.Run(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);
        }

        // Wait for all disconnect calls to complete
        await Task.WhenAll(disconnectTasks);

        // Verify that:
        // 1. All disconnect tasks completed successfully (no exceptions)
        Assert.All(disconnectTasks, item => Assert.True(item.IsCompletedSuccessfully));

        // 2. The connection's StopAsync was only called once (not multiple times)
        // This verifies that the double-checked locking prevented redundant disconnect logic
        Assert.Equal(1, connection.StopCallCount);
    }

    [Fact]
    public async Task TestDisconnectSecondCallHitsDoubleCheckAndReturnsEarly()
    {
        // This test specifically exercises the double-check branch where a thread
        // acquires the semaphore but finds IsConnected = false
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            // Add a delay to simulate long disconnect operation
            DisconnectDelay = TimeSpan.FromMilliseconds(100)
        };

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);

        // Start first disconnect (will take 100ms)
        Task firstDisconnect = Task.Run(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        // Give first disconnect time to start and set IsConnected = false
        await Task.Delay(TimeSpan.FromMilliseconds(20), TestContext.Current.CancellationToken);

        // Start second disconnect while first is still running
        // This should:
        // 1. Pass the fast-path check (IsConnected was true when checked)
        // 2. Wait for semaphore
        // 3. Acquire semaphore after first disconnect completes
        // 4. Hit the double-check and find IsConnected = false
        // 5. Return early without executing disconnect logic
        Task secondDisconnect = Task.Run(async () => await transport.DisconnectAsync(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken);

        // Wait for both to complete
        await Task.WhenAll(firstDisconnect, secondDisconnect);

        // Verify both completed successfully
        Assert.True(firstDisconnect.IsCompletedSuccessfully);
        Assert.True(secondDisconnect.IsCompletedSuccessfully);

        // Verify disconnect logic only executed once (the double-check prevented the second execution)
        Assert.Equal(1, connection.StopCallCount);
    }

    [Fact]
    public async Task TestDisconnectWithMultipleConcurrentCallsOperatesCorrectly()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);

        await transport.ConnectAsync("ws://localhost", TestContext.Current.CancellationToken);
        transport.EnableConnectLockConcurrencyTesting();

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
    public async Task TestExceptionInTransportEventReceivedCanTerminate()
    {
        string receivedName = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
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
                taskCompletionSource.TrySetResult();
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
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250), TestContext.Current.CancellationToken);
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
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);

        await transport.DisposeAsync();
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
        transport.EnableConnectLockConcurrencyTesting();

        Task connectTask = transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        Task registerTask = transport.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
        await connectTask;

        Assert.Contains("Cannot register a type info resolver after the transport is connected", (await Assert.ThrowsAnyAsync<InvalidOperationException>(async () => await registerTask)).Message);
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
        // Covers the inner "if (!this.IsConnected) return" branch (line 609): OnConnectionErrorAsync
        // passes the fast-path, blocks on the lock, then by the time it acquires the lock
        // DisconnectAsync has already set IsConnected = false.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.EnableConnectLockConcurrencyTesting();

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
        // Covers the inner "if (!this.IsConnected) return" branch in OnConnectionRemotelyDisconnectedAsync:
        // the fast-path passes (IsConnected == true), but by the time the lock is acquired
        // DisconnectAsync has already set IsConnected = false.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost", TestContext.Current.CancellationToken);
        transport.EnableConnectLockConcurrencyTesting();

        Task disconnectTask = transport.DisconnectAsync(TestContext.Current.CancellationToken);
        await connection.RaiseRemoteDisconnectedEventAsync();
        await disconnectTask;

        TestCommandParameters commandParameters = new("module.command");
        Assert.Contains("Transport must be connected", (await Assert.ThrowsAnyAsync<WebDriverBiDiConnectionException>(async () => await transport.SendCommandAsync(commandParameters, TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestExceptionInErrorEventHandlerIsIgnoredByDefault()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
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
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
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
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
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
        await transport.DisconnectAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestExceptionInUnknownMessageHandlerCanCollect()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
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
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnUnknownMessageReceived.AddObserver(e =>
        {
            taskCompletionSource.TrySetResult();
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
}
