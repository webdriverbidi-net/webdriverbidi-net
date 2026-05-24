namespace WebDriverBiDi;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using PinchHitter;
using TestUtilities;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.DigitalCredentials;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Permissions;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Speculation;
using WebDriverBiDi.Storage;
using WebDriverBiDi.UserAgentClientHints;
using WebDriverBiDi.WebExtension;

[Collection("EventSourceTests")]
public class BiDiDriverTests
{
    [Fact]
    public async Task TestCanDetermineIsStarted()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        Assert.False(driver.IsStarted);
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        Assert.True(driver.IsStarted);
        await driver.StopAsync(TestContext.Current.CancellationToken);
        Assert.False(driver.IsStarted);
    }

    [Fact]
    public async Task TestCanExecuteCommand()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestCanExecuteCommandWithError()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string errorJson = """
                               {
                                 "type": "error",
                                 "id": 1,
                                 "error": "unknown command", 
                                 "message": "This is a test error message",
                                 "stacktrace": "remote stack trace"
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(errorJson);
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        WebDriverBiDiCommandException? caughtException = await Assert.ThrowsAsync<WebDriverBiDiCommandException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken));
        Assert.NotNull(caughtException);

        Assert.Contains("'unknown command' error executing command module.command: This is a test error message", caughtException.Message);
        Assert.Equal(ErrorCode.UnknownCommand, caughtException.ErrorCode);
        Assert.Equal("unknown command", caughtException.ProtocolErrorType);
        Assert.Equal("This is a test error message", caughtException.ProtocolErrorMessage);
        Assert.Equal("remote stack trace", caughtException.RemoteStackTrace);
        Assert.NotNull(caughtException.ErrorDetails);
        Assert.Equal("unknown command", caughtException.ErrorDetails.ErrorType);
        Assert.Equal(ErrorCode.UnknownCommand, caughtException.ErrorDetails.ErrorCode);
        Assert.Equal("This is a test error message", caughtException.ErrorDetails.ErrorMessage);
        Assert.Equal("remote stack trace", caughtException.ErrorDetails.StackTrace);
    }

    [Fact]
    public async Task TestCanExecuteCommandThatReturnsThrownExceptionThrows()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string exceptionJson = """
                                   {
                                     "type": "success",
                                     "id": 1, 
                                     "noResult": {
                                       "invalid": "unknown command",
                                       "message": "This is a test error message"
                                     }
                                   }
                                   """;
            await connection.RaiseDataReceivedEventAsync(exceptionJson);
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        Assert.Contains("Response did not contain properly formed JSON for response type", (await Assert.ThrowsAnyAsync<WebDriverBiDiSerializationException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestCanExecuteReceiveErrorWithoutCommand()
    {
        ErrorResult? response = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnexpectedErrorReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            response = e.ErrorData;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string errorJson = """
                           {
                             "type": "error",
                             "id": null,
                             "error": "unknown command",
                             "message": "This is a test error message"
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(errorJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(response);

        Assert.Equal(ErrorCode.UnknownCommand, response.ErrorCode);
        Assert.Equal("unknown command", response.ErrorType);
        Assert.Equal("This is a test error message", response.ErrorMessage);
    }

    [Fact]
    public async Task TestCanReceiveKnownEvent()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        driver.OnEventReceived.AddObserver((e) =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string eventJson = """
                           {
                             "type": "event",
                             "method": "module.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(eventName, receivedEvent);
        Assert.NotNull(receivedData);
        Assert.IsType<TestEventArgs>(receivedData);

        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.NotNull(convertedData);
        Assert.Equal("paramValue", convertedData.ParamName);
    }

    [Fact]
    public async Task TestRegisteringDuplicateEventThrows()
    {
        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask));
        Assert.StartsWith("An event named 'module.event' has already been registered.", exception.Message);
    }

    [Fact]
    public async Task TestRegisteringEventAfterStartingDriverThrows()
    {
        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask));
    }

    [Fact]
    public async Task TestDriverWillProcessPendingMessagesOnStop()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection)
        {
            MessageProcessingDelay = TimeSpan.FromMilliseconds(100)
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        driver.OnEventReceived.AddObserver((e) =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string eventJson = """
                           {
                             "type": "event",
                             "method": "module.event",
                             "params": {
                               "paramName": "paramValue"
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await driver.StopAsync(TestContext.Current.CancellationToken);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(eventName, receivedEvent);
        Assert.NotNull(receivedData);
        Assert.IsType<TestEventArgs>(receivedData);

        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.NotNull(convertedData);
        Assert.Equal("paramValue", convertedData.ParamName);
    }

    [Fact]
    public async Task TestUnregisteredEventRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver((e) =>
        {
            receivedMessage = e.Message;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string serialized = """
                            {
                              "type": "event",
                              "method": "module.event",
                              "params": {
                                "paramName": "paramValue"
                              }
                            }
                            """;
        await connection.RaiseDataReceivedEventAsync(serialized);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(serialized, receivedMessage);
    }

    [Fact]
    public async Task TestUnconformingDataRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver((e) =>
        {
            receivedMessage = e.Message;
            taskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string serialized = """
                            {
                              "someProperty": "someValue",
                              "params": {
                                "thisMessage": "matches no protocol message"
                              }
                            }
                            """;
        await connection.RaiseDataReceivedEventAsync(serialized);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(serialized, receivedMessage);
    }

    [Fact]
    public async Task TestNotificationOfEventHandlerError()
    {
        EventObserverErrorInfo? errorInfo = null;
        TaskCompletionSource handlerFaultedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource errorReportedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        EventObserver<BrowsingContextEventArgs> browsingContextObserver = driver.BrowsingContext.OnContextCreated.AddObserver(async e =>
        {
            try
            {
                throw new InvalidOperationException("This is a test exception from an event handler");
            }
            finally
            {
                await Task.Yield();
                handlerFaultedTaskCompletionSource.TrySetResult();
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously, "Test observer");

        driver.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            errorInfo = e.ErrorInfo with { };
            errorReportedTaskCompletionSource.TrySetResult();
        });
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string eventJson = """
                           {
                             "type": "event",
                             "method": "browsingContext.contextCreated",
                             "params": {
                               "context": "myContextId",
                               "clientWindow": "myClientWindowId",
                               "url": "http://example.com",
                               "originalOpener": "openerContext",
                               "userContext": "myUserContextId",
                               "children": []
                             }
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(eventJson);
        await handlerFaultedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await errorReportedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.NotNull(errorInfo);
        Assert.Equal(browsingContextObserver.Id, errorInfo.ObserverId);
        Assert.IsType<InvalidOperationException>(errorInfo.Exception);
        Assert.Equal("browsingContext.contextCreated", errorInfo.ObservableEventName);
        Assert.Equal("Test observer", errorInfo.ObserverDescription);
    }

    [Fact]
    public async Task TestModuleAvailability()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        try
        {

            Assert.IsType<BluetoothModule>(driver.Bluetooth);
            Assert.IsType<BrowserModule>(driver.Browser);
            Assert.IsType<BrowsingContextModule>(driver.BrowsingContext);
            Assert.IsType<DigitalCredentialsModule>(driver.DigitalCredentials);
            Assert.IsType<EmulationModule>(driver.Emulation);
            Assert.IsType<InputModule>(driver.Input);
            Assert.IsType<LogModule>(driver.Log);
            Assert.IsType<NetworkModule>(driver.Network);
            Assert.IsType<PermissionsModule>(driver.Permissions);
            Assert.IsType<ScriptModule>(driver.Script);
            Assert.IsType<SessionModule>(driver.Session);
            Assert.IsType<SpeculationModule>(driver.Speculation);
            Assert.IsType<StorageModule>(driver.Storage);
            Assert.IsType<UserAgentClientHintsModule>(driver.UserAgentClientHints);
            Assert.IsType<WebExtensionModule>(driver.WebExtension);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public async Task TestCanRegisterModule()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.IsType<TestProtocolModule>(driver.GetModule<TestProtocolModule>("protocol"));
    }

    [Fact]
    public async Task TestRegisteringModuleWithDuplicateNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver, 0, false));
        Assert.StartsWith("A module with the name 'protocol' has already been registered", Assert.ThrowsAny<ArgumentException>(() => driver.RegisterModule(new TestProtocolModule(driver))).Message);
    }

    [Fact]
    public async Task TestRegisteringModuleAfterStartingDriverThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.ThrowsAny<InvalidOperationException>(() => driver.RegisterModule(new TestProtocolModule(driver, 0, false)));
    }

    [Fact]
    public async Task TestGettingInvalidModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Contains("Module 'protocol' is not registered with this driver", Assert.ThrowsAny<ArgumentException>(() => driver.GetModule<TestProtocolModule>("protocol")).Message);
    }

    [Fact]
    public async Task TestGettingInvalidModuleTypeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.Equal("Module 'protocol' is registered with this driver, but the module object is not of type WebDriverBiDi.Session.SessionModule", Assert.ThrowsAny<InvalidCastException>(() => driver.GetModule<SessionModule>("protocol")).Message);
    }

    [Fact]
    public async Task TestReceivingNullValueFromSendingCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        WebDriverBiDiException exception = await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("is unexpectedly null", exception.Message);
    }

    [Fact]
    public async Task TestCanceledCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ShouldCancelCommand = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        Assert.Contains("was canceled before a result was received", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestUncompletedCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnUncompletedCommand = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        Assert.Contains("is unexpectedly null", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestExecutingCommandWillThrowWhenTimeout()
    {
        BiDiDriver driver = new(TimeSpan.Zero, new Transport(new TestWebSocketConnection()));
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Contains("Timed out executing command test.command", (await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestTimedOutCommandIgnoresLateResponse()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken));

        string lateResponse = """{"type":"success","id":1,"result":{"parameterName":"parameterValue"}}""";
        await connection.RaiseDataReceivedEventAsync(lateResponse);
    }

    [Fact]
    public async Task TestReceivingInvalidErrorValueFromSendingCommandThrows()
    {
        TestCommandResult result = new();
        result.SetIsErrorValue(true);
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal("Could not convert error response from transport for SendCommandAndWait to ErrorResult", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestReceivingInvalidResultTypeFromSendingCommandThrows()
    {
        TestCommandResultInvalid result = new()
        {
            Value = "invalid",
        };

        Assert.Equal("invalid", result.Value);
        Assert.Equal(result, result with { });

        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.Equal("Could not convert response from transport for SendCommandAndWait to WebDriverBiDi.TestUtilities.TestCommandResult", (await Assert.ThrowsAnyAsync<WebDriverBiDiException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken))).Message);
    }

    [Fact]
    public async Task TestDriverCanEmitLogMessagesFromProtocol()
    {
        DateTime testStart = DateTime.UtcNow;
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(100), transport);
        driver.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
        });
        await driver.StartAsync("ws:localhost", TestContext.Current.CancellationToken);
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        Assert.Single(logs);

        Assert.Equal("test log message", logs[0].Message);
        Assert.Equal(WebDriverBiDiLogLevel.Warn, logs[0].Level);
        Assert.True(logs[0].Timestamp >= testStart);
        Assert.Equal("TestWebSocketConnection", logs[0].ComponentName);
    }

    [Fact]
    public async Task TestDriverCanUseDefaultTransport()
    {
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void connectionHandler(ClientConnectionEventArgs e) { connectionTaskCompletionSource.TrySetResult(); }
        static void handler(ServerDataReceivedEventArgs e) { }
        Server server = new();
        ServerEventObserver<ServerDataReceivedEventArgs> dataReceivedObserver = server.OnDataReceived.AddObserver(handler);
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();

        BiDiDriver driver = new();
        await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
        await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        await driver.StopAsync(TestContext.Current.CancellationToken);

        await server.StopAsync();
        dataReceivedObserver.Unobserve();
    }

    [Fact]
    public async Task TestMalformedEventResponseLogsError()
    {
        string connectionId = string.Empty;
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void connectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionTaskCompletionSource.TrySetResult();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();
        BiDiDriver driver = new(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
            await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver((e) =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logTaskCompletionSource.TrySetResult();
                }
            });

            TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            string unknownMessage = string.Empty;
            driver.OnUnknownMessageReceived.AddObserver((e) =>
            {
                unknownMessage = e.Message;
                unknownMessageTaskCompletionSource.TrySetResult();
            });

            // This payload omits the required "timestamp" field, which should cause an exception
            // in parsing.
            string eventJson = """
                               {
                                 "type": "event",
                                 "method": "browsingContext.load",
                                 "params": {
                                   "context": "myContext",
                                   "url": "https://example.com",
                                   "navigation": "myNavigationId"
                                 }
                               }
                               """;
            await server.SendWebSocketDataAsync(connectionId, eventJson);
            await Task.WhenAll(
                logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
                unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

            Assert.Single(driverLog);
            Assert.Contains("Unexpected error parsing event JSON", driverLog[0]);
            Assert.NotEmpty(unknownMessage);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task TestMalformedNonCommandErrorResponseLogsError()
    {
        string connectionId = string.Empty;
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void connectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionTaskCompletionSource.TrySetResult();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();
        BiDiDriver driver = new();

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
            await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            driver.BrowsingContext.OnLoad.AddObserver((e) =>
            {
            });

            TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver((e) =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logTaskCompletionSource.TrySetResult();
                }
            });

            TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            string unknownMessage = string.Empty;
            driver.OnUnknownMessageReceived.AddObserver((e) =>
            {
                unknownMessage = e.Message;
                unknownMessageTaskCompletionSource.TrySetResult();
            });

            // This payload uses an object for the error field, which should cause an exception
            // in parsing.
            string json = """
                          {
                            "type": "error",
                            "id": null,
                            "error": {
                              "code": "unknown error"
                            },
                            "message": "This is a test error message"
                          }
                          """;
            await server.SendWebSocketDataAsync(connectionId, json);
            await Task.WhenAll(
                logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
                unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

            Assert.Single(driverLog);
            Assert.Contains("Unexpected error parsing error JSON", driverLog[0]);
            Assert.NotEmpty(unknownMessage);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task TestMalformedIncomingMessageLogsError()
    {
        string connectionId = string.Empty;
        TaskCompletionSource connectionTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        void connectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionTaskCompletionSource.TrySetResult();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();
        BiDiDriver driver = new();

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}", TestContext.Current.CancellationToken);
            await connectionTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

            TaskCompletionSource logTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver((e) =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logTaskCompletionSource.TrySetResult();
                }
            });

            TaskCompletionSource unknownMessageTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
            string unknownMessage = string.Empty;
            driver.OnUnknownMessageReceived.AddObserver((e) =>
            {
                unknownMessage = e.Message;
                unknownMessageTaskCompletionSource.TrySetResult();
            });

            // This payload uses unparsable JSON, which should cause an exception
            // in parsing.
            string unparsableJson = """
                               {
                                 "type": "error",
                                 "id": null,
                                 { "errorMessage" },
                                 "message": "This is a test error message"
                               }
                               """;
            await server.SendWebSocketDataAsync(connectionId, unparsableJson);
            await Task.WhenAll(
                logTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken),
                unknownMessageTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

            Assert.Single(driverLog);
            Assert.Contains("Unexpected error parsing JSON message", driverLog[0]);
            Assert.NotEmpty(unknownMessage);
        }
        finally
        {
            await driver.StopAsync(TestContext.Current.CancellationToken);
            await server.StopAsync();
        }
    }

    [Fact]
    public async Task TestCanModifyEventHandlerExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.EventHandlerExceptionBehavior);
        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.EventHandlerExceptionBehavior);
    }

    [Fact]
    public async Task TestCanModifyUnexpectedErrorExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.UnexpectedErrorBehavior);
        driver.UnexpectedErrorBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.UnexpectedErrorBehavior);
    }

    [Fact]
    public async Task TestCanModifyProtocolErrorExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.ProtocolErrorBehavior);
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.ProtocolErrorBehavior);
    }

    [Fact]
    public async Task TestCanModifyUnknownMessageExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.Equal(TransportErrorBehavior.Ignore, driver.UnknownMessageBehavior);
        driver.UnknownMessageBehavior = TransportErrorBehavior.Collect;
        Assert.Equal(TransportErrorBehavior.Collect, transport.UnknownMessageBehavior);
    }

    [Fact]
    public async Task TestCanExecuteParallelCommands()
    {
        // delayCommandInFlight: set by the delay command's handler as soon as it starts,
        // before waiting on the gate. The fast command's handler waits on this first,
        // proving both commands were in-flight simultaneously before either responded.
        // delayResponseGate: held until after Task.WaitAny confirms the fast command
        // finished first, then released so the delay command can respond.
        TaskCompletionSource delayCommandInFlightTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource delayResponseGateTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            Task.Run(async () =>
            {
                DateTime start = DateTime.Now;
                if (e.SentCommandName is not null && e.SentCommandName.Contains("delay"))
                {
                    delayCommandInFlightTaskCompletionSource.TrySetResult();
                    await delayResponseGateTaskCompletionSource.Task;
                }
                else
                {
                    await delayCommandInFlightTaskCompletionSource.Task;
                }

                TimeSpan elapsed = DateTime.Now - start;
                string eventJson = $$"""
                                   {
                                     "type": "success",
                                     "id": {{e.SentCommandId}},
                                     "result": {
                                       "value": "command result value for {{e.SentCommandName}}",
                                       "elapsed": {{elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)}}
                                     }
                                   }
                                   """;
                await connection.RaiseDataReceivedEventAsync(eventJson);
            });
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string delayCommandName = "module.delayCommand";
        TestCommandParameters delayCommand = new(delayCommandName);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);

        Task<TestCommandResult>[] parallelTasks =
        [
            driver.ExecuteCommandAsync(delayCommand, cancellationToken: TestContext.Current.CancellationToken),
            driver.ExecuteCommandAsync(command, cancellationToken: TestContext.Current.CancellationToken),
        ];

        Task<TestCommandResult> firstFinished = await Task.WhenAny(parallelTasks).WaitAsync(TestContext.Current.CancellationToken);
        int indexOfFirstFinishedTask = Array.IndexOf(parallelTasks, firstFinished);
        delayResponseGateTaskCompletionSource.TrySetResult();
        TestCommandResult[] results = await Task.WhenAll(parallelTasks).WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal(1, indexOfFirstFinishedTask);
        Assert.Equal($"command result value for {delayCommandName}", results[0].Value);
        Assert.Equal($"command result value for {commandName}", results[1].Value);
        Assert.True(results[0].ElapsedMilliseconds >= results[1].ElapsedMilliseconds);
    }

    [Fact]
    public async Task TestCanDisposeStartedDriver()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanDisposeDriverWithoutStarting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await driver.DisposeAsync();
    }

    [Fact]
    public async Task TestStopThenDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.StopAsync(TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
    }

    [Fact]
    public async Task TestExecuteCommandAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandWithTimeoutAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanUseAwaitUsingPattern()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        string commandValue = string.Empty;
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };

        await using (BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport))
        {
            await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
            TestCommandResult result = await driver.ExecuteCommandAsync(new TestCommandParameters("module.command"), cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(result.Value);
            commandValue = result.Value;
        }

        Assert.Equal("command result value", commandValue);
    }

    [Fact]
    public async Task TestRegisteringModuleAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        Assert.ThrowsAny<ObjectDisposedException>(() => driver.RegisterModule(new TestProtocolModule(driver)));
    }

    [Fact]
    public async Task TestGettingModuleAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        driver.RegisterModule(module);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        Assert.ThrowsAny<ObjectDisposedException>(() => driver.GetModule<TestProtocolModule>(module.ModuleName));
    }

    [Fact]
    public async Task TestRegisteringEventAfterDisposeThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        Assert.ThrowsAny<ObjectDisposedException>(() => driver.RegisterEvent("protocol.event", eventInvoker));
    }

    [Fact]
    public async Task TestDisposeDisposesTransport()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        Assert.False(transport.IsDisposed);
        await driver.DisposeAsync();
        Assert.True(transport.IsDisposed);
    }

    [Fact]
    public async Task TestDisposeDisposesTransportWithoutStarting()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.False(transport.IsDisposed);
        await driver.DisposeAsync();
        Assert.True(transport.IsDisposed);
    }

    [Fact]
    public async Task TestDisposeLogsExceptionFromStopAsync()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        driver.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
        });
        transport.ThrowOnDisconnect = true;
        await driver.DisposeAsync();
        Assert.Contains(logs,
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated disconnect failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == BiDiDriver.LoggerComponentName);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverBeforeStarting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverAfterStartingThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        InvalidOperationException exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => driver.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken));
        Assert.Contains("Cannot register a type info resolver after the transport is connected", exception.Message);
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.DisposeAsync();
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(() => driver.RegisterTypeInfoResolverAsync(new DefaultJsonTypeInfoResolver(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCanExecuteCommandWithUntypedCommandParameters()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestCanExecuteCommandWithUntypedCommandParametersAndTimeout()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromMilliseconds(1500), TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestExecuteCommandWithUntypedParametersAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        CommandParameters command = new TestCommandParameters("test.command");
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandWithUntypedParametersAndTimeoutAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        await driver.DisposeAsync();
        CommandParameters command = new TestCommandParameters("test.command");
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromMilliseconds(100), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestStartAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await driver.StartAsync("ws://localhost:5555", cts.Token));
    }

    [Fact]
    public async Task TestExecuteCommandThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParams = new("module.command");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(commandParams, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task TestExecuteCommandWithTimeoutThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParams = new("module.command");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(commandParams, TimeSpan.FromSeconds(5), cts.Token));
    }

    [Fact]
    public async Task TestExecuteCommandCancelsCommandOnCancellation()
    {
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += (_, _) => taskCompletionSource.TrySetResult();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        using CancellationTokenSource cts = new();
        CommandParameters command = new TestCommandParameters("module.command");

        Task<TestCommandResult> executeTask = driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(30), cts.Token);
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await executeTask);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithNullCommandParametersThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await Assert.ThrowsAnyAsync<ArgumentNullException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(null!, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithNegativeTimeoutThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        CommandParameters command = new TestCommandParameters("module.command");
        await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(-5), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithZeroTimeoutDoesNotThrowArgumentOutOfRangeException()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            // force a delay in responding to ensure that the timeout is actually being applied
            await Task.Yield();
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        await Assert.ThrowsAnyAsync<WebDriverBiDiTimeoutException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.Zero, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithExplicitPositiveTimeoutSucceeds()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal("command result value", result.Value);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithInfiniteTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        await driver.ExecuteCommandAsync<TestCommandResult>(command, Timeout.InfiniteTimeSpan, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestExecuteCommandAsyncWithNullTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            // force a delay in responding to ensure that the timeout is actually being applied
            await Task.Yield();
            string eventJson = """
                               {
                                 "type": "success",
                                 "id": 1,
                                 "result": {
                                   "value": "command result value"
                                 }
                               }
                               """;
            await connection.RaiseDataReceivedEventAsync(eventJson);
        };
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);
        CommandParameters command = new TestCommandParameters("module.command");
        await driver.ExecuteCommandAsync<TestCommandResult>(command, null, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestRegisterEventWithNullEventNameThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.RegisterEvent(null!, eventInvoker));
    }

    [Fact]
    public async Task TestRegisterEventWithEmptyEventNameThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.RegisterEvent(string.Empty, eventInvoker));
    }

    [Fact]
    public async Task TestRegisterEventWithNullEventInvokerThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = null!;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentNullException>(() => driver.RegisterEvent("protocol.event", eventInvoker!));
    }

    [Fact]
    public async Task TestRegisterModuleWithNullModuleThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentNullException>(() => driver.RegisterModule(null!));
    }

    [Fact]
    public async Task TestGetModuleWithNullModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.GetModule<TestProtocolModule>(null!));
    }

    [Fact]
    public async Task TestGetModuleWithEmptyModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.ThrowsAny<ArgumentException>(() => driver.GetModule<TestProtocolModule>(string.Empty));
    }

    [Fact]
    public async Task TestRegisterTypeInfoResolverWithNullResolverThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await Assert.ThrowsAnyAsync<ArgumentNullException>(() => driver.RegisterTypeInfoResolverAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TestCreatingWithNegativeDefaultCommandTimeoutThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new BiDiDriver(TimeSpan.FromSeconds(-1), transport));
    }

    [Fact]
    public async Task TestCreatingWithZeroDefaultCommandTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        new BiDiDriver(TimeSpan.Zero, transport);
    }

    [Fact]
    public async Task TestCreatingWithInfiniteDefaultCommandTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        new BiDiDriver(Timeout.InfiniteTimeSpan, transport);
    }

    [Fact]
    public async Task TestCreatingWithNullTransportThrows()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => _ = new BiDiDriver(TimeSpan.FromSeconds(1), null!));
    }

    [Fact]
    public async Task TestConcurrentExecuteCommandAsyncRoutesResponsesByCommandId()
    {
        // This stress test exercises the ID-correlation path in BiDiDriver.ExecuteCommandAsync
        // under concurrent callers. It is fully deterministic — no Task.Delay, no wall-clock
        // polling — because the transport's send path serializes through its internal
        // connection semaphore, and the test uses a CountdownEvent to know precisely when all
        // sends have completed before delivering responses in reverse order.
        //
        // The correlation claim being tested: each caller's ExecuteCommandAsync returns a
        // result that matches the specific command that caller sent, even when many callers
        // race to send and responses arrive out of send order.
        const int concurrentCallerCount = 100;

        using CountdownEvent allSendsCompleted = new(concurrentCallerCount);
        Dictionary<long, string> commandIdToSenderValue = [];
        object captureLock = new();

        TestWebSocketConnection connection = new();
        connection.DataSendComplete += (sender, e) =>
        {
            // Because sends serialize under the transport's connection semaphore, this
            // handler runs exclusively per send. It is safe to read connection.DataSent
            // here — no other sender can overwrite it until we release the semaphore by
            // returning. We still take an explicit lock so the Dictionary mutation is
            // safe against any future changes to the send path.
            JsonDocument document = JsonDocument.Parse(connection.DataSent ??= string.Empty);
            string? senderValue = document.RootElement.GetProperty("params").GetProperty("parameterName").GetString();
            lock (captureLock)
            {
                commandIdToSenderValue[e.SentCommandId] = senderValue ??= string.Empty;
            }

            allSendsCompleted.Signal();
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(Timeout.InfiniteTimeSpan, transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        // Fire N tasks concurrently. Each sender tags its command with a unique
        // parameterName so we can prove end-to-end that the response it receives was
        // produced from its own send.
        Task<TestCommandResult>[] senderTasks = new Task<TestCommandResult>[concurrentCallerCount];
        string[] expectedValues = new string[concurrentCallerCount];
        for (int i = 0; i < concurrentCallerCount; i++)
        {
            int capturedIndex = i;
            string senderValue = $"sender-{capturedIndex}";
            expectedValues[capturedIndex] = senderValue;
            senderTasks[capturedIndex] = Task.Run(async () => await driver.ExecuteCommandAsync(new TestCommandParameters("module.command", senderValue), cancellationToken: TestContext.Current.CancellationToken));
        }

        // Block deterministically until every send has completed. After this Wait()
        // returns, every command is in the pending collection (it is added before the
        // send begins) and every caller is awaiting WaitForCompletionAsync. The 30-second
        // bound is a safety net to prevent a CI hang if something goes wrong; under normal
        // operation Wait returns as soon as all signals arrive. If Wait returns false, the
        // remaining assertions in the test are not meaningful, so fail fast here.
        if (!allSendsCompleted.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken))
        {
            throw new Xunit.Sdk.XunitException("all sends should complete before the safety timeout");
        }

        Assert.Equal(concurrentCallerCount, transport.PendingCommandCount);

        // Deliver responses in reverse send order to exercise out-of-order delivery.
        // Each response carries the sender's own parameterName as its "value" field, so
        // the per-caller assertion below is a strong correlation check: if the transport
        // ever routed a response to the wrong caller, the value mismatch would surface.
        List<long> commandIdsInSendOrder;
        lock (captureLock)
        {
            commandIdsInSendOrder = [.. commandIdToSenderValue.Keys];
            commandIdsInSendOrder.Sort();
        }

        for (int i = commandIdsInSendOrder.Count - 1; i >= 0; i--)
        {
            long commandId = commandIdsInSendOrder[i];
            string senderValue = commandIdToSenderValue[commandId];
            string responseJson = $$$"""{"type":"success","id":{{{commandId}}},"result":{"value":"{{{senderValue}}}"}}""";
            await connection.RaiseDataReceivedEventAsync(responseJson);
        }

        // Task.WhenAll completes only after every caller has received its response; this
        // is the final deterministic synchronization point. The assertions then check
        // that each caller i received a result whose Value equals its own sender tag.
        TestCommandResult[] results = await Task.WhenAll(senderTasks);
        Assert.Equal(0, transport.PendingCommandCount);

        for (int i = 0; i < concurrentCallerCount; i++)
        {
            Assert.Equal(expectedValues[i], results[i].Value);
        }

        await driver.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestMidCommandRemoteDisconnectFaultsExecuteCommandWithConnectionException()
    {
        // Verify that a remote disconnect while a command is pending causes
        // BiDiDriver.ExecuteCommandAsync to fault with WebDriverBiDiConnectionException
        // promptly, rather than hanging until the command timeout expires. This is the
        // driver-level passthrough of the transport behavior covered by
        // TransportTests.TestRemoteDisconnectFailsPendingCommands.
        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            taskCompletionSource.TrySetResult();
        };

        Transport transport = new(connection);
        // Large default timeout: if the disconnect path were broken and the
        // command sat in the pending collection, the test would hang until this
        // elapsed. The assertion timeout below is much shorter, so a broken
        // path fails fast rather than timing out the whole suite.
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        TestCommandParameters command = new("module.command");
        Task<TestCommandResult> executeTask = Task.Run(() => driver.ExecuteCommandAsync<TestCommandResult>(command, cancellationToken: TestContext.Current.CancellationToken));

        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        await connection.RaiseRemoteDisconnectedEventAsync();

        WebDriverBiDiConnectionException? caught = await Assert.ThrowsAsync<WebDriverBiDiConnectionException>(async () =>
        {
            Task completed = await Task.WhenAny(executeTask, Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken));
            Assert.Same(executeTask, completed);
            await executeTask;
        });
        Assert.Contains("Remote end closed the connection", caught.Message);
    }

    [Fact]
    public async Task TestRacedCapturedTaskFaultIsReportedViaEventHandlerError()
    {
        // A handler task that races into the capture buffer after WaitForAsync collected
        // its Nth task (but before TryComplete closes the writer) has
        // ShouldReportAsyncFault = false on its original continuation. The drain in
        // WaitForAsync must attach a new reporting continuation so the fault surfaces
        // via OnEventHandlerErrorOccurred instead of being silently swallowed.
        EventObserverErrorInfo? errorInfo = null;
        TaskCompletionSource handlerStartedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource allowFaultTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource errorReportedTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);

        driver.BrowsingContext.OnContextCreated.AddObserver(
            async (BrowsingContextEventArgs e) =>
            {
                if (e.BrowsingContextId == "racedContextId")
                {
                    handlerStartedTaskCompletionSource.TrySetResult();
                    await allowFaultTaskCompletionSource.Task;
                    throw new InvalidOperationException("raced task fault");
                }
            },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        driver.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            errorInfo = e.ErrorInfo with { };
            errorReportedTaskCompletionSource.TrySetResult();
        });

        await driver.StartAsync("ws://localhost:5555", TestContext.Current.CancellationToken);

        string collectedEventJson = """
            {
              "type": "event",
              "method": "browsingContext.contextCreated",
              "params": {
                "context": "collectedContextId",
                "clientWindow": "myClientWindowId",
                "url": "http://example.com",
                "originalOpener": null,
                "userContext": "default",
                "children": []
              }
            }
            """;

        string racedEventJson = """
            {
              "type": "event",
              "method": "browsingContext.contextCreated",
              "params": {
                "context": "racedContextId",
                "clientWindow": "myClientWindowId",
                "url": "http://example.com",
                "originalOpener": null,
                "userContext": "default",
                "children": []
              }
            }
            """;

        EventObserver<BrowsingContextEventArgs> observer = driver.BrowsingContext.OnContextCreated.AddObserver(
            _ => { },
            ObservableEventHandlerOptions.RunHandlerAsynchronously);

        observer.StartCapturingTasks();

        // Deliver the event that WaitForAsync(1) will collect.
        await connection.RaiseDataReceivedEventAsync(collectedEventJson);
        // Deliver the raced event — its handler starts and blocks.
        await connection.RaiseDataReceivedEventAsync(racedEventJson);
        await handlerStartedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // WaitForAsync collects 1 task, auto-closes, drains the raced task.
        Task[] tasks = await observer.WaitForCapturedTasksAsync(1, TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        _ = Assert.Single(tasks);

        // Let the raced handler fault.
        allowFaultTaskCompletionSource.TrySetResult();
        await errorReportedTaskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.NotNull(errorInfo);
        InvalidOperationException ex = Assert.IsType<InvalidOperationException>(errorInfo.Exception);
        Assert.Equal("raced task fault", ex.Message);
        Assert.Equal("browsingContext.contextCreated", errorInfo.ObservableEventName);
    }
}
