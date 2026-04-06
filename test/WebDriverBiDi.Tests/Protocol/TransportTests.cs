namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using PinchHitter;
using TestUtilities;

[TestFixture]
public class TransportTests
{
    [Test]
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
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.That(dataValue, Is.EquivalentTo(expected));
    }

    [Test]
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
        await transport.ConnectAsync("ws:localhost");

        TestComplexCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.That(dataValue, Is.EquivalentTo(expected));
    }

    [Test]
    public async Task TestTransportCanGetResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
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
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            await connection.RaiseDataReceivedEventAsync(json);
        });
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));
        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.That(hasResult, Is.True);
        Assert.That(actualResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actualResult.IsError, Is.False);
            Assert.That(actualResult, Is.TypeOf<TestCommandResult>());
        }
        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.That(convertedResult, Is.Not.Null);
        Assert.That(convertedResult.Value, Is.EqualTo("response value"));
    }

    [Test]
    public async Task TestTransportCanGetResponseWithAdditionalData()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        _ = Task.Run(async () =>
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
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            await connection.RaiseDataReceivedEventAsync(json);
        });
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250));
        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.That(hasResult, Is.True);
        Assert.That(actualResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actualResult.IsError, Is.False);
            Assert.That(actualResult, Is.TypeOf<TestCommandResult>());
        }
        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.That(convertedResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(convertedResult.Value, Is.EqualTo("response value"));
            Assert.That(convertedResult.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(convertedResult.AdditionalData["extraDataName"], Is.EqualTo("extraDataValue"));
        }
    }

    [Test]
    public async Task TestTransportCanGetErrorResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        _ = Task.Run(async () =>
        {
            string json = """
                          {
                            "type": "error",
                            "id": 1,
                            "error": "unknown command",
                            "message": "This is a test error message"
                          }
                          """;
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            await connection.RaiseDataReceivedEventAsync(json);
        });
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250));
        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.That(hasResult, Is.True);
        Assert.That(actualResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actualResult.IsError, Is.True);
            Assert.That(actualResult, Is.InstanceOf<ErrorResult>());
        }
        ErrorResult? convertedResponse = actualResult as ErrorResult;
        Assert.That(convertedResponse, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(convertedResponse.ErrorType, Is.EqualTo("unknown command"));
            Assert.That(convertedResponse.ErrorCode, Is.EqualTo(ErrorCode.UnknownCommand));
            Assert.That(convertedResponse.ErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(convertedResponse.StackTrace, Is.Null);
        }
    }

    [Test]
    public async Task TestTransportGetResponseWithThrownException()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        _ = Task.Run(async () =>
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
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            await connection.RaiseDataReceivedEventAsync(json);
        });
        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(250));
        Assert.That(command.ThrownException, Is.InstanceOf<WebDriverBiDiSerializationException>().With.Message.Contains("Response did not contain properly formed JSON for response type"));
    }

    [Test]
    public void TestTransportCannotSendCommandWithoutConnection()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected to a remote end to execute commands."));
    }

    [Test]
    public async Task TestTransportLeavesCommandResultAndThrownExceptionNullWithoutResponse()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(hasResult, Is.False);
            Assert.That(commandResult, Is.Null);
            Assert.That(command.ThrownException, Is.Null);
        }
    }

    [Test]
    public async Task TestSendCommandExceptionRollsBackPendingCommandState()
    {
        TestWebSocketConnection connection = new()
        {
            SendWebSocketDataOverride = _ => throw new InvalidOperationException("Simulated send failure"),
        };
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("Simulated send failure"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(transport.TestPendingCommandCount, Is.Zero);
        }
    }

    [Test]
    public async Task TestSendCommandCancellationRollsBackPendingCommandState()
    {
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
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new("module.command");
        Task<Command> sendTask = transport.SendCommandAsync(commandParameters, cancellationTokenSource.Token);
        sendStarted.Wait(TimeSpan.FromSeconds(1));
        cancellationTokenSource.Cancel();

        Assert.That(async () => await sendTask, Throws.InstanceOf<OperationCanceledException>());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(transport.TestPendingCommandCount, Is.Zero);
        }
    }

    [Test]
    public async Task TestTransportEventReceived()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver((EventReceivedEventArgs e) =>
        {
            receivedName = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(receivedName, Is.EqualTo("protocol.event"));
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        }
        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData, Is.Not.Null);
        Assert.That(convertedData.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public async Task TestTransportErrorEventReceived()
    {
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnErrorEventReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            receivedData = e.ErrorData;
            syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));

        Assert.That(receivedData, Is.TypeOf<ErrorResult>());
        ErrorResult? convertedData = receivedData as ErrorResult;
        Assert.That(convertedData, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(convertedData.ErrorType, Is.EqualTo("unknown error"));
            Assert.That(convertedData.ErrorCode, Is.EqualTo(ErrorCode.UnknownError));
            Assert.That(convertedData.ErrorMessage, Is.EqualTo("This is a test error message"));
        }
    }

    [Test]
    public async Task TestTransportErrorEventReceivedWithNullValues()
    {
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            receivedData = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        transport.OnErrorEventReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            return Task.CompletedTask;
        });
        string json = """
                      {
                        "type": "event",
                        "method": null
                      }
                      """;
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public async Task TestTransportLogsCommands()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        Assert.That(logs, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs[0].Message, Is.EqualTo("test log message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Warn));
        }
    }

    [Test]
    public async Task TestTransportLogsSuccessfulCommandResponses()
    {
        ManualResetEventSlim syncEvent = new(false);
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        TaskCompletionSource logCompletionSource = new();
        transport.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
            syncEvent.Set();
            return Task.CompletedTask;
        });
        string commandName = "module.command";
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
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
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            await connection.RaiseDataReceivedEventAsync(json);
        });
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));
        bool hasResult = command.TryGetResult(out CommandResult? actualResult);
        Assert.That(hasResult, Is.True);
        Assert.That(actualResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actualResult.IsError, Is.False);
            Assert.That(actualResult, Is.TypeOf<TestCommandResult>());
        }
        TestCommandResult? convertedResult = actualResult as TestCommandResult;
        Assert.That(convertedResult, Is.Not.Null);
        Assert.That(convertedResult.Value, Is.EqualTo("response value"));
        bool logEventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        Assert.That(logEventRaised, Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Command response message processed"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Trace));
        }
    }

    [Test]
    public async Task TestTransportLogsMalformedJsonMessages()
    {
        ManualResetEventSlim syncEvent = new(false);
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.ComponentName == Transport.LoggerComponentName)
            {
                logs.Add(e);
                syncEvent.Set();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync("{ { }");
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing JSON message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        }
    }

    [Test]
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
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
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
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
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
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
    public async Task TestTransportRaisesUnknownMessageEventForSuccessMessageWitInvalidIdDataType()
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
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
    public async Task TestTransportRaisesUnknownMessageEventForSuccessMessageWitInvalidIdValue()
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
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
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
        CountdownEvent signaler = new(2);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                signaler.Signal();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing error JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        }
    }

    [Test]
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
        CountdownEvent signaler = new(2);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                signaler.Signal();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing error JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        }
    }

    [Test]
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
        CountdownEvent signaler = new(2);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                signaler.Signal();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing error JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        }
    }

    [Test]
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
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
    public async Task TestTransportRaisesUnknownMessageEventForEventMessageWithMissingParams()
    {
        string json = """
                      {
                        "type": "event",
                        "method": "protocol.event"
                      }
                      """;
        string loggedEvent = string.Empty;
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
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
        ManualResetEventSlim syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            syncEvent.Set();
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = syncEvent.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
        }
    }

    [Test]
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
        CountdownEvent signaler = new(2);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                signaler.Signal();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Unexpected error parsing event JSON"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        }
    }

    [Test]
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
        CountdownEvent signaler = new(2);
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.RegisterInvalidEventMessageType("protocol.event", typeof(object));
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            loggedEvent = e.Message;
            signaler.Signal();
            return Task.CompletedTask;
        });
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.Level > WebDriverBiDiLogLevel.Trace)
            {
                logs.Add(e);
                signaler.Signal();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool eventRaised = signaler.Wait(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventRaised, Is.True);
            Assert.That(loggedEvent, Is.EqualTo(json));
            Assert.That(logs, Has.Count.EqualTo(1));
            Assert.That(logs[0].Message, Contains.Substring("Deserialization of event message returned null"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Error));
        }
    }

    [Test]
    public async Task TestTransportCanUseDefaultConnection()
    {
        ManualResetEvent connectionSyncEvent = new(false);
        static void dataReceivedHandler(ServerDataReceivedEventArgs e) { }
        void connectionHandler(ClientConnectionEventArgs e) { connectionSyncEvent.Set(); }
        Server server = new();
        ServerEventObserver<ServerDataReceivedEventArgs> dataReceivedObserver = server.OnDataReceived.AddObserver(dataReceivedHandler);
        ServerEventObserver<ClientConnectionEventArgs> connectedObserver = server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();

        Transport transport = new();
        await transport.ConnectAsync($"ws://localhost:{server.Port}");
        bool connectionEventRaised = connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

        await server.StopAsync();
        dataReceivedObserver.Unobserve();
        connectedObserver.Unobserve(); ;
        Assert.That(connectionEventRaised, Is.True);
    }

    [Test]
    public async Task TestCannotConnectWhenAlreadyConnected()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync($"ws://localhost:1234");
        Assert.That(async () => await transport.ConnectAsync($"ws://localhost:5678"), Throws.InstanceOf<WebDriverBiDiException>().With.Message.StartsWith($"The transport is already connected to ws://localhost:1234"));
    }

    [Test]
    public async Task TestConcurrentConnectAsyncCallsAreSerialized()
    {
        TaskCompletionSource startBarrier = new();
        TestWebSocketConnection connection = new()
        {
            StartBarrier = startBarrier
        };
        Transport transport = new(connection);

        Task firstConnect = transport.ConnectAsync("ws://localhost:1234");
        Assert.That(firstConnect.IsCompleted, Is.False);

        Task secondConnect = transport.ConnectAsync("ws://localhost:5678");

        startBarrier.SetResult();
        await firstConnect;

        Assert.That(async () => await secondConnect, Throws.InstanceOf<WebDriverBiDiException>().With.Message.StartsWith("The transport is already connected"));
    }

    [Test]
    public void TestDisconnectWhenNotConnectedDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.That(async () => await transport.DisconnectAsync(), Throws.Nothing);
    }

    [Test]
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

        await transport.ConnectAsync("ws://localhost");

        // Launch multiple concurrent disconnect calls
        const int concurrentCalls = 5;
        Task[] disconnectTasks = new Task[concurrentCalls];

        for (int i = 0; i < concurrentCalls; i++)
        {
            disconnectTasks[i] = Task.Run(async () => await transport.DisconnectAsync());
        }

        // Wait for all disconnect calls to complete
        await Task.WhenAll(disconnectTasks);

        // Verify that:
        // 1. All disconnect tasks completed successfully (no exceptions)
        Assert.That(disconnectTasks, Has.All.Property("IsCompletedSuccessfully").True);

        // 2. The connection's StopAsync was only called once (not multiple times)
        // This verifies that the double-checked locking prevented redundant disconnect logic
        Assert.That(connection.StopCallCount, Is.EqualTo(1), "Connection.StopAsync should only be called once despite concurrent disconnect calls");
    }

    [Test]
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

        await transport.ConnectAsync("ws://localhost");

        // Start first disconnect (will take 100ms)
        Task firstDisconnect = Task.Run(async () => await transport.DisconnectAsync());

        // Give first disconnect time to start and set IsConnected = false
        await Task.Delay(TimeSpan.FromMilliseconds(20));

        // Start second disconnect while first is still running
        // This should:
        // 1. Pass the fast-path check (IsConnected was true when checked)
        // 2. Wait for semaphore
        // 3. Acquire semaphore after first disconnect completes
        // 4. Hit the double-check and find IsConnected = false
        // 5. Return early without executing disconnect logic
        Task secondDisconnect = Task.Run(async () => await transport.DisconnectAsync());

        // Wait for both to complete
        await Task.WhenAll(firstDisconnect, secondDisconnect);

        // Verify both completed successfully
        Assert.That(firstDisconnect.IsCompletedSuccessfully, Is.True);
        Assert.That(secondDisconnect.IsCompletedSuccessfully, Is.True);

        // Verify disconnect logic only executed once (the double-check prevented the second execution)
        Assert.That(connection.StopCallCount, Is.EqualTo(1), "Connection.StopAsync should only be called once due to double-check");
    }

    [Test]
    public async Task TestDisconnectWithMultipleConcurrentCallsOperatesCorrectly()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);

        await transport.ConnectAsync("ws://localhost");
        transport.EnableConnectLockConcurrencyTesting();

        Task task1 = transport.DisconnectAsync();
        Task task2 = transport.DisconnectAsync();
        await Task.WhenAll(task1, task2);

        Assert.That(connection.StopCallCount, Is.EqualTo(1));
        Assert.That(transport.ConcurrentConnectLockAcquisitions, Is.EqualTo(2));
    }

    [Test]
    public async Task TestDisconnectMultipleTimesAfterAlreadyDisconnectedHitsFastPath()
    {
        // This test verifies that calling disconnect on an already-disconnected transport
        // returns immediately via the fast-path check without acquiring the semaphore
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);

        await transport.ConnectAsync("ws://localhost");

        // First disconnect - this will set IsConnected = false
        await transport.DisconnectAsync();

        // Verify the first disconnect executed fully
        Assert.That(connection.StopCallCount, Is.EqualTo(1));

        // Second disconnect - should hit the fast-path check and return immediately
        await transport.DisconnectAsync();

        // The fast-path check prevents the disconnect logic from executing
        Assert.That(connection.StopCallCount, Is.EqualTo(1), "Connection.StopAsync should only be called once");

        // Third call for good measure
        await transport.DisconnectAsync();
        Assert.That(connection.StopCallCount, Is.EqualTo(1), "Connection.StopAsync should still only be called once after third disconnect");
    }

    [Test]
    public async Task TestTransportDisconnectWithPendingIncomingMessagesWillProcess()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            MessageProcessingDelay = TimeSpan.FromMilliseconds(100)
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver((EventReceivedEventArgs e) =>
        {
            receivedName = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        await transport.DisconnectAsync();
        bool eventRaised = syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(eventRaised, Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(receivedName, Is.EqualTo("protocol.event"));
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        }
        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData, Is.Not.Null);
        Assert.That(convertedData.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
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
        await transport.ConnectAsync("ws://example.com:1234");

        TestCommandParameters command = new(commandName);
        _ = await transport.SendCommandAsync(command);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.That(dataValue, Is.EquivalentTo(expected));
        await transport.DisconnectAsync();

        await transport.ConnectAsync("ws://example.com:5678");
        _ = await transport.SendCommandAsync(command);

        dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.That(dataValue, Is.EquivalentTo(expected));
        await transport.DisconnectAsync();
    }

    [Test]
    public async Task TestExceptionInTransportEventReceivedCanCollect()
    {
        string receivedName = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver((EventReceivedEventArgs e) =>
        {
            syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(async () => await transport.DisconnectAsync(), Throws.InstanceOf<AggregateException>().With.InnerException.InstanceOf<WebDriverBiDiException>().And.Message.Contains("Normal shutdown").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("This is an unexpected exception"));
    }

    [Test]
    public async Task TestExceptionInTransportEventReceivedCanCollectMultiple()
    {
        string receivedName = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver((EventReceivedEventArgs e) =>
        {
            try
            {
                throw new WebDriverBiDiException("This is an unexpected exception");
            }
            finally
            {
                syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        syncEvent.Reset();
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(async () => await transport.DisconnectAsync(), Throws.InstanceOf<AggregateException>().With.Message.Contains("Normal shutdown").And.Property("InnerExceptions").Count.EqualTo(2).And.Property("InnerExceptions").All.InstanceOf<WebDriverBiDiException>().And.Message.Contains("This is an unexpected exception"));
    }

    [Test]
    public async Task TestExceptionInTransportEventReceivedCanTerminate()
    {
        string receivedName = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver((EventReceivedEventArgs e) =>
        {
            try
            {
                throw new WebDriverBiDiException("This is an unexpected exception");
            }
            finally
            {
                syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("protocol.event").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("This is an unexpected exception"));
    }

    [Test]
    public async Task TestAsyncExceptionInTransportEventReceivedCanCollect()
    {
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(async (EventReceivedEventArgs e) =>
        {
            try
            {
                await Task.Delay(50).ConfigureAwait(false);
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Collect);
        Assert.That(errorPropagated, Is.True, "The transport should collect the late async aggregate handler failure before shutdown.");

        Assert.That(async () => await transport.DisconnectAsync(), Throws.InstanceOf<AggregateException>().With.InnerException.InstanceOf<WebDriverBiDiException>().And.Message.Contains("Normal shutdown").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("This is an async unexpected exception"));
    }

    [Test]
    public async Task TestAsyncExceptionInTransportEventReceivedCanTerminate()
    {
        TaskCompletionSource<bool> handlerCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver(async (EventReceivedEventArgs e) =>
        {
            try
            {
                await Task.Delay(50).ConfigureAwait(false);
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));
        bool errorPropagated = await transport.WaitForCollectedEventHandlerExceptionAsync(TimeSpan.FromSeconds(1), TransportErrorBehavior.Terminate);
        Assert.That(errorPropagated, Is.True, "The transport should collect the late async aggregate handler failure before shutdown.");

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("protocol.event").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("This is an async unexpected exception"));
    }

    [Test]
    public async Task TestCapturedExceptionsCanBeReset()
    {
        string receivedName = string.Empty;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver((EventReceivedEventArgs e) =>
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        await transport.DisconnectAsync();

        await transport.ConnectAsync("ws:localhost");
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
        _ = await transport.SendCommandAsync(commandParameters);

        Dictionary<string, object?> dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();
        Assert.That(dataValue, Is.EquivalentTo(expected));
    }

    [Test]
    public async Task TestTransportTracksCommandId()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        Assert.That(transport.LastTestCommandId, Is.Zero);

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
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
            await Task.Delay(TimeSpan.FromMilliseconds(50));
            await connection.RaiseDataReceivedEventAsync(json);
        });
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));
        Assert.That(transport.LastTestCommandId, Is.EqualTo(1));
    }

    [Test]
    public void TestTransportSubclassesCanAccessConnection()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        Assert.That(transport.GetConnection(), Is.EqualTo(connection));
    }

    [Test]
    public void TestTransportShutdownTimeoutDefaultValue()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.That(transport.ShutdownTimeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void TestTransportShutdownTimeoutCanBeSet()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(1)
        };
        Assert.That(transport.ShutdownTimeout, Is.EqualTo(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public async Task TestTransportDisconnectTimesOutWithHangingEventHandler()
    {
        ManualResetEvent handlerStarted = new(false);
        List<LogMessageEventArgs> logs = [];

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromMilliseconds(250),
        };
        transport.RegisterEventMessage<TestEventArgs>("protocol.event");
        transport.OnEventReceived.AddObserver((EventReceivedEventArgs e) =>
        {
            handlerStarted.Set();
            return new TaskCompletionSource<bool>().Task;
        });
        transport.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        bool handlerDidStart = handlerStarted.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(handlerDidStart, Is.True);

        await transport.DisconnectAsync();

        Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Timed out waiting for message processing to complete during shutdown")
                   && log.Level == WebDriverBiDiLogLevel.Warn));
    }

    [Test]
    public async Task TestTransportDisconnectCompletesWithinShutdownTimeout()
    {
        List<LogMessageEventArgs> logs = [];

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            ShutdownTimeout = TimeSpan.FromSeconds(5),
        };
        transport.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });

        await transport.ConnectAsync("ws:localhost");
        await transport.DisconnectAsync();

        Assert.That(logs, Has.None.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Timed out waiting for message processing to complete during shutdown")));
    }

    [Test]
    public async Task TestCanDisposeWithoutConnecting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeAfterConnectAndDisconnect()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        await transport.DisconnectAsync();
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeWhileConnected()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        await transport.DisposeAsync();
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeDefaultTransport()
    {
        Transport transport = new();
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeDisposesOldPendingCommandsAfterReconnect()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        await transport.DisconnectAsync();

        await transport.ConnectAsync("ws:localhost");
        await transport.DisconnectAsync();

        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeSuppressesDisconnectException()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        transport.ThrowOnDisconnect = true;
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeLogsExceptionFromDisconnect()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");
        transport.ThrowOnDisconnect = true;
        await transport.DisposeAsync();
        Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated disconnect failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == Transport.LoggerComponentName));
    }

    [Test]
    public async Task TestMessageProcessingLoopContinuesAfterUnhandledException()
    {
        string commandName = "module.command";
        ManualResetEventSlim syncEvent = new(false);
        List<LogMessageEventArgs> logs = [];

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");

        transport.DeserializeThrowCount = 1;

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);

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
        _ = Task.Run(async () =>
        {
            await connection.RaiseDataReceivedEventAsync(responseJson);
        });

        await command.WaitForCompletionAsync(TimeSpan.FromSeconds(1));

        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        Assert.That(hasResult, Is.True);
        Assert.That(commandResult, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(commandResult.IsError, Is.False);
            Assert.That(commandResult, Is.TypeOf<TestCommandResult>());
            Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
                log => log.Message.Contains("Unexpected error in message processing loop")
                       && log.Message.Contains("Simulated deserialization failure")
                       && log.Level == WebDriverBiDiLogLevel.Error));
        }
    }

    [Test]
    public async Task TestMessageProcessingLoopExceptionCapturedAsUnhandledError()
    {
        ManualResetEventSlim syncEvent = new(false);

        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            ProtocolErrorBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.Level == WebDriverBiDiLogLevel.Error)
            {
                syncEvent.Set();
            }

            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");

        transport.DeserializeThrowCount = 1;
        await connection.RaiseDataReceivedEventAsync("this message will cause the exception");
        syncEvent.Wait(TimeSpan.FromSeconds(1));

        Assert.That(
            async () => await transport.DisconnectAsync(),
            Throws.InstanceOf<AggregateException>()
                .With.Message.Contains("Normal shutdown")
                .And.InnerException.InstanceOf<InvalidOperationException>()
                .And.InnerException.Message.Contains("Simulated deserialization failure"));
    }

    [Test]
    public async Task TestCancelCommandRemovesFromPendingAndCancelsCommand()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"));
        Assert.That(command.IsCanceled, Is.False);

        transport.CancelCommand(command);
        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(command.IsCanceled, Is.True);
            Assert.That(hasResult, Is.False);
            Assert.That(commandResult, Is.Null);
        }
    }

    [Test]
    public async Task TestCancelCommandIsIdempotent()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"));
        transport.CancelCommand(command);
        Assert.That(() => transport.CancelCommand(command), Throws.Nothing);
    }

    [Test]
    public async Task TestCancelCommandPreventsLateResponseFromSettingResult()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"));
        transport.CancelCommand(command);

        string responseJson = $$$"""{"type":"success","id":{{{command.CommandId}}},"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(responseJson);
        await Task.Delay(50);

        bool hasResult = command.TryGetResult(out CommandResult? commandResult);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(command.IsCanceled, Is.True);
            Assert.That(hasResult, Is.False);
            Assert.That(commandResult, Is.Null);
        }
    }

    [Test]
    public void TestRegisterTypeInfoResolverBeforeConnecting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.That(async () => await transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.Nothing);
    }

    [Test]
    public async Task TestRegisterTypeInfoResolverMultipleTimesBeforeConnecting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver());
        Assert.That(async () => await transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.Nothing);
    }

    [Test]
    public async Task TestRegisterTypeInfoResolverAfterConnectingThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        Assert.That(async () => await transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("Cannot register a type info resolver after the transport is connected"));
    }

    [Test]
    public async Task TestRegisterTypeInfoDuringConnectIsSynchronized()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        transport.EnableConnectLockConcurrencyTesting();

        Task connectTask = transport.ConnectAsync("ws:localhost");
        Task registerTask = transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver());
        await connectTask;

        Assert.That(async () => await registerTask, Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("Cannot register a type info resolver after the transport is connected"));
    }

    [Test]
    public async Task TestConnectionErrorFailsPendingCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);

        Exception simulatedError = new("WebSocket connection dropped");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));

        Assert.That(command.ThrownException, Is.InstanceOf<WebDriverBiDiConnectionException>());
        Assert.That(command.ThrownException!.Message, Does.Contain("Unexpected connection error"));
        Assert.That(command.ThrownException!.InnerException, Is.SameAs(simulatedError));
    }

    [Test]
    public async Task TestConnectionErrorPreventsNewCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Exception simulatedError = new("WebSocket connection dropped");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);

        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestConnectionErrorLogsMessage()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");

        Exception simulatedError = new("WebSocket connection dropped");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);

        Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Connection error; pending commands failed")
                   && log.Message.Contains("WebSocket connection dropped")
                   && log.Level == WebDriverBiDiLogLevel.Error));
    }

    [Test]
    public async Task TestConnectionErrorWhenNotConnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        // Never call ConnectAsync - IsConnected remains false
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost"));

        // Should not throw; early return path taken. Verify transport rejects commands.
        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestConnectionErrorWhenAlreadyDisconnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        await transport.DisconnectAsync();

        // IsConnected is now false; raise error (e.g., receive loop dying during shutdown)
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost"));

        // Should not throw; early return path taken. Verify still disconnected.
        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestConnectionErrorWhenDisconnectRacesHitsInnerReturnBranch()
    {
        // Covers the inner "if (!this.IsConnected) return" branch (line 609): OnConnectionErrorAsync
        // passes the fast-path, blocks on the lock, then by the time it acquires the lock
        // DisconnectAsync has already set IsConnected = false.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        transport.EnableConnectLockConcurrencyTesting();

        Task disconnectTask = transport.DisconnectAsync();
        await connection.RaiseConnectionErrorEventAsync(new Exception("Connection lost during race"));
        await disconnectTask;

        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestConnectionErrorFailsMultiplePendingCommands()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command1 = await transport.SendCommandAsync(new TestCommandParameters("module.command1"));
        Command command2 = await transport.SendCommandAsync(new TestCommandParameters("module.command2"));

        Exception simulatedError = new("connection lost");
        await connection.RaiseConnectionErrorEventAsync(simulatedError);

        await command1.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));
        await command2.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(command1.ThrownException, Is.InstanceOf<WebDriverBiDiConnectionException>());
            Assert.That(command2.ThrownException, Is.InstanceOf<WebDriverBiDiConnectionException>());
        }
    }

    [Test]
    public async Task TestRemoteDisconnectFailsPendingCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);

        await connection.RaiseRemoteDisconnectedEventAsync();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));

        Assert.That(command.ThrownException, Is.InstanceOf<WebDriverBiDiConnectionException>());
        Assert.That(command.ThrownException!.Message, Does.Contain("Remote end closed the connection"));
    }

    [Test]
    public async Task TestRemoteDisconnectPreventsNewCommands()
    {
        string commandName = "module.command";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestRemoteDisconnectLogsMessage()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnLogMessage.AddObserver((LogMessageEventArgs e) =>
        {
            logs.Add(e);
            return Task.CompletedTask;
        });
        await transport.ConnectAsync("ws:localhost");

        await connection.RaiseRemoteDisconnectedEventAsync();

        Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Remote end closed connection")
                   && log.Level == WebDriverBiDiLogLevel.Warn));
    }

    [Test]
    public async Task TestRemoteDisconnectWhenNotConnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestRemoteDisconnectWhenAlreadyDisconnectedDoesNothing()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        await transport.DisconnectAsync();

        await connection.RaiseRemoteDisconnectedEventAsync();

        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestRemoteDisconnectFailsMultiplePendingCommands()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command1 = await transport.SendCommandAsync(new TestCommandParameters("module.command1"));
        Command command2 = await transport.SendCommandAsync(new TestCommandParameters("module.command2"));

        await connection.RaiseRemoteDisconnectedEventAsync();

        await command1.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));
        await command2.WaitForCompletionAsync(TimeSpan.FromMilliseconds(250));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(command1.ThrownException, Is.InstanceOf<WebDriverBiDiConnectionException>());
            Assert.That(command2.ThrownException, Is.InstanceOf<WebDriverBiDiConnectionException>());
        }
    }

    [Test]
    public async Task TestRemoteDisconnectWhenDisconnectRacesHitsInnerReturnBranch()
    {
        // Covers the inner "if (!this.IsConnected) return" branch in OnConnectionRemotelyDisconnectedAsync:
        // the fast-path passes (IsConnected == true), but by the time the lock is acquired
        // DisconnectAsync has already set IsConnected = false.
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        transport.EnableConnectLockConcurrencyTesting();

        Task disconnectTask = transport.DisconnectAsync();
        await connection.RaiseRemoteDisconnectedEventAsync();
        await disconnectTask;

        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected"));
    }

    [Test]
    public async Task TestExceptionInErrorEventHandlerIsIgnoredByDefault()
    {
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnErrorEventReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        await transport.DisconnectAsync();
    }

    [Test]
    public async Task TestExceptionInErrorEventHandlerCanCollect()
    {
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnErrorEventReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(async () => await transport.DisconnectAsync(), Throws.InstanceOf<AggregateException>().With.InnerException.InstanceOf<WebDriverBiDiException>().And.With.Message.Contains("Normal shutdown").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("Error handler exception"));
    }

    [Test]
    public async Task TestExceptionInErrorEventHandlerCanTerminate()
    {
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnErrorEventReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            syncEvent.Set();
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
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("error event").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("Error handler exception"));
    }

    [Test]
    public async Task TestExceptionInUnknownMessageHandlerIsIgnoredByDefault()
    {
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            syncEvent.Set();
            throw new WebDriverBiDiException("Unknown message handler exception");
        });
        string json = """
                      {
                        "type": "unknown"
                      }
                      """;
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        await transport.DisconnectAsync();
    }

    [Test]
    public async Task TestExceptionInUnknownMessageHandlerCanCollect()
    {
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Collect,
        };
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            syncEvent.Set();
            throw new WebDriverBiDiException("Unknown message handler exception");
        });
        string json = """
                      {
                        "type": "unknown"
                      }
                      """;
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));
        Assert.That(async () => await transport.DisconnectAsync(), Throws.InstanceOf<AggregateException>().With.InnerException.InstanceOf<WebDriverBiDiException>().And.With.Message.Contains("Normal shutdown").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("Unknown message handler exception"));
    }

    [Test]
    public async Task TestExceptionInUnknownMessageHandlerCanTerminate()
    {
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection)
        {
            EventHandlerExceptionBehavior = TransportErrorBehavior.Terminate,
        };
        transport.OnUnknownMessageReceived.AddObserver((UnknownMessageReceivedEventArgs e) =>
        {
            syncEvent.Set();
            throw new WebDriverBiDiException("Unknown message handler exception");
        });
        string json = """
                      {
                        "type": "unknown"
                      }
                      """;
        await transport.ConnectAsync("ws:localhost");
        await connection.RaiseDataReceivedEventAsync(json);
        syncEvent.WaitOne(TimeSpan.FromSeconds(1));

        string commandName = "module.command";
        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("unknown message event").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("Unknown message handler exception"));
    }

    [Test]
    public void TestConnectAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.That(async () => await transport.ConnectAsync("ws://localhost", cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestSendCommandAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost");
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParameters = new("module.command");
        Assert.That(async () => await transport.SendCommandAsync(commandParameters, cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }
}
