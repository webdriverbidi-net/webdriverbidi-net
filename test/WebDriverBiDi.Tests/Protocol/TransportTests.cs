namespace WebDriverBiDi.Protocol;

using System.Text.Json.Serialization.Metadata;
using Newtonsoft.Json.Linq;
using TestUtilities;
using PinchHitter;

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

        TestConnection connection = new();
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

        TestConnection connection = new();
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
        TestConnection connection = new();
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
        CommandResult? actualResult = command.Result;
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
        TestConnection connection = new();
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
        CommandResult? actualResult = command.Result;
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
        TestConnection connection = new();
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
        CommandResult? actualResult = command.Result;
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
            Assert.That(convertedResponse.ErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(convertedResponse.StackTrace, Is.Null);
        }
    }

    [Test]
    public async Task TestTransportGetResponseWithThrownException()
    {
        string commandName = "module.command";
        TestConnection connection = new();
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
        TestConnection connection = new();
        Transport transport = new(connection);

        TestCommandParameters commandParameters = new(commandName);
        Assert.That(async () => await transport.SendCommandAsync(commandParameters), Throws.InstanceOf<WebDriverBiDiConnectionException>().With.Message.Contains("Transport must be connected to a remote end to execute commands."));
    }

    [Test]
    public async Task TestTransportLeavesCommandResultAndThrownExceptionNullWithoutResponse()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        TestCommandParameters commandParameters = new(commandName);
        Command command = await transport.SendCommandAsync(commandParameters);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(command.Result, Is.Null);
            Assert.That(command.ThrownException, Is.Null);
        }
    }

    [Test]
    public async Task TestTransportEventReceived()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
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

        TestConnection connection = new();
        Transport transport = new(connection);
        transport.OnErrorEventReceived.AddObserver((ErrorReceivedEventArgs e) => {
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
            Assert.That(convertedData.ErrorMessage, Is.EqualTo("This is a test error message"));
        }
    }

    [Test]
    public async Task TestTransportErrorEventReceivedWithNullValues()
    {
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        CommandResult? actualResult = command.Result;
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
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.OnLogMessage.AddObserver((e) =>
        {
            if (e.ComponentName == "Transport")
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        server.Start();

        Transport transport = new();
        await transport.ConnectAsync($"ws://localhost:{server.Port}");
        bool connectionEventRaised = connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

        server.Stop();
        dataReceivedObserver.Unobserve();
        connectedObserver.Unobserve(); ;
        Assert.That(connectionEventRaised, Is.True);
    }

    [Test]
    public async Task TestCannotConnectWhenAlreadyConnected()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync($"ws://localhost:1234");
        Assert.That(async () => await transport.ConnectAsync($"ws://localhost:5678"), Throws.InstanceOf<WebDriverBiDiException>().With.Message.StartsWith($"The transport is already connected to ws://localhost:1234"));
    }

    [Test]
    public async Task TestConcurrentConnectAsyncCallsAreSerialized()
    {
        TaskCompletionSource startBarrier = new();
        TestConnection connection = new()
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
        TestConnection connection = new();
        Transport transport = new(connection);
        Assert.That(async () => await transport.DisconnectAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestTransportDisconnectWithPendingIncomingMessagesWillProcess()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
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

        TestConnection connection = new();
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

        TestConnection connection = new();
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
        Assert.That(async () => await transport.DisconnectAsync(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Normal shutdown").And.InnerException.InstanceOf<WebDriverBiDiException>().And.InnerException.Message.Contains("This is an unexpected exception"));
    }

    [Test]
    public async Task TestExceptionInTransportEventReceivedCanCollectMultiple()
    {
        string receivedName = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
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

        TestConnection connection = new();
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
    public async Task TestCapturedExceptionsCanBeReset()
    {
        string receivedName = string.Empty;
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
        TestTransport transport = new(connection);
        Assert.That(transport.GetConnection(), Is.EqualTo(connection));
    }

    [Test]
    public void TestTransportShutdownTimeoutDefaultValue()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        Assert.That(transport.ShutdownTimeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void TestTransportShutdownTimeoutCanBeSet()
    {
        TestConnection connection = new();
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

        TestConnection connection = new();
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

        TestConnection connection = new();
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
        TestConnection connection = new();
        Transport transport = new(connection);
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeAfterConnectAndDisconnect()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        await transport.DisconnectAsync();
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestCanDisposeWhileConnected()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestConnection connection = new();
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
        TestConnection connection = new();
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
        TestConnection connection = new();
        TestTransport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        transport.ThrowOnDisconnect = true;
        Assert.That(async () => await transport.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestDisposeLogsExceptionFromDisconnect()
    {
        List<LogMessageEventArgs> logs = [];
        TestConnection connection = new();
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
                   && log.ComponentName == "Transport"));
    }

    [Test]
    public async Task TestMessageProcessingLoopContinuesAfterUnhandledException()
    {
        string commandName = "module.command";
        ManualResetEventSlim syncEvent = new(false);
        List<LogMessageEventArgs> logs = [];

        TestConnection connection = new();
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

        Assert.That(command.Result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(command.Result.IsError, Is.False);
            Assert.That(command.Result, Is.TypeOf<TestCommandResult>());
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

        TestConnection connection = new();
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
            Throws.InstanceOf<WebDriverBiDiException>()
                .With.Message.Contains("Normal shutdown")
                .And.InnerException.InstanceOf<InvalidOperationException>()
                .And.InnerException.Message.Contains("Simulated deserialization failure"));
    }

    [Test]
    public async Task TestCancelCommandRemovesFromPendingAndCancelsCommand()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"));
        Assert.That(command.IsCanceled, Is.False);

        transport.CancelCommand(command);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(command.IsCanceled, Is.True);
            Assert.That(command.Result, Is.Null);
        }
    }

    [Test]
    public async Task TestCancelCommandIsIdempotent()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"));
        transport.CancelCommand(command);
        Assert.That(() => transport.CancelCommand(command), Throws.Nothing);
    }

    [Test]
    public async Task TestCancelCommandPreventsLateResponseFromSettingResult()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");

        Command command = await transport.SendCommandAsync(new TestCommandParameters("module.command"));
        transport.CancelCommand(command);

        string responseJson = $$$"""{"type":"success","id":{{{command.CommandId}}},"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(responseJson);
        await Task.Delay(50);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(command.IsCanceled, Is.True);
            Assert.That(command.Result, Is.Null);
        }
    }

    [Test]
    public void TestRegisterTypeInfoResolverBeforeConnecting()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        Assert.That(() => transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.Nothing);
    }

    [Test]
    public void TestRegisterTypeInfoResolverMultipleTimesBeforeConnecting()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver());
        Assert.That(() => transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.Nothing);
    }

    [Test]
    public async Task TestRegisterTypeInfoResolverAfterConnectingThrows()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        await transport.ConnectAsync("ws:localhost");
        Assert.That(() => transport.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Cannot register a type info resolver after the transport is connected"));
    }
}
