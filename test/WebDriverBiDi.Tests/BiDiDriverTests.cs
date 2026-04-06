namespace WebDriverBiDi;

using System.Globalization;
using System.Text.Json.Serialization.Metadata;
using NUnit.Framework.Internal;
using PinchHitter;
using TestUtilities;
using WebDriverBiDi.Bluetooth;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
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

[TestFixture]
public class BiDiDriverTests
{
    [Test]
    public async Task TestCanDetermineIsStarted()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        Assert.That(driver.IsStarted, Is.False);
        await driver.StartAsync("ws:localhost");
        Assert.That(driver.IsStarted, Is.True);
        await driver.StopAsync();
        Assert.That(driver.IsStarted, Is.False);
    }

    [Test]
    public async Task CanExecuteCommand()
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
        await driver.StartAsync("ws://localhost:5555");

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command);
        Assert.That(result.Value, Is.EqualTo("command result value"));
    }

    [Test]
    public async Task CanExecuteCommandWithError()
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
        await driver.StartAsync("ws://localhost:5555");

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        WebDriverBiDiCommandException? caughtException = Assert.ThrowsAsync<WebDriverBiDiCommandException>(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command));
        Assert.That(caughtException, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(caughtException!.Message, Does.Contain("'unknown command' error executing command module.command: This is a test error message"));
            Assert.That(caughtException.ErrorCode, Is.EqualTo(ErrorCode.UnknownCommand));
            Assert.That(caughtException.ProtocolErrorType, Is.EqualTo("unknown command"));
            Assert.That(caughtException.ProtocolErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(caughtException.RemoteStackTrace, Is.EqualTo("remote stack trace"));
            Assert.That(caughtException.ErrorDetails, Is.Not.Null);
            Assert.That(caughtException.ErrorDetails.ErrorType, Is.EqualTo("unknown command"));
            Assert.That(caughtException.ErrorDetails.ErrorCode, Is.EqualTo(ErrorCode.UnknownCommand));
            Assert.That(caughtException.ErrorDetails.ErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(caughtException.ErrorDetails.StackTrace, Is.EqualTo("remote stack trace"));
        }
    }

    [Test]
    public async Task CanExecuteCommandThatReturnsThrownExceptionThrows()
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
        await driver.StartAsync("ws://localhost:5555");

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command), Throws.InstanceOf<WebDriverBiDiSerializationException>().With.Message.Contains("Response did not contain properly formed JSON for response type"));
    }

    [Test]
    public async Task CanExecuteReceiveErrorWithoutCommand()
    {
        ErrorResult? response = null;
        ManualResetEvent syncEvent = new(false);
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnexpectedErrorReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            response = e.ErrorData;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

        string errorJson = """
                           {
                             "type": "error",
                             "id": null,
                             "error": "unknown command",
                             "message": "This is a test error message"
                           }
                           """;
        await connection.RaiseDataReceivedEventAsync(errorJson);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));

        Assert.That(response, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(response!.ErrorCode, Is.EqualTo(ErrorCode.UnknownCommand));
            Assert.That(response!.ErrorType, Is.EqualTo("unknown command"));
            Assert.That(response.ErrorMessage, Is.EqualTo("This is a test error message"));
        }
    }

    [Test]
    public async Task CanReceiveKnownEvent()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        driver.OnEventReceived.AddObserver((e) =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

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
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(receivedEvent, Is.EqualTo(eventName));
            Assert.That(receivedData, Is.Not.Null);
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        }
        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public void TestRegisteringDuplicateEventThrows()
    {
        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask);
        Assert.That(() => driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask), Throws.InstanceOf<ArgumentException>().With.Message.StartsWith("An event named 'module.event' has already been registered."));
    }

    [Test]
    public async Task TestRegisteringEventAfterStartingDriverThrows()
    {
        string eventName = "module.event";
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(() => driver.RegisterEvent<TestEventArgs>(eventName, (e) => Task.CompletedTask), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public async Task TestDriverWillProcessPendingMessagesOnStop()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

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
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

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
        await driver.StopAsync();
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(receivedEvent, Is.EqualTo(eventName));
            Assert.That(receivedData, Is.Not.Null);
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        }
        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public async Task TestUnregisteredEventRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver((e) =>
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

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
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

    [Test]
    public async Task TestUnconformingDataRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver((e) =>
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

        string serialized = """
                            {
                              "someProperty": "someValue",
                              "params": {
                                "thisMessage": "matches no protocol message"
                              }
                            }
                            """;
        await connection.RaiseDataReceivedEventAsync(serialized);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

    [Test]
    public async Task TestNotificationOfEventHandlerError()
    {
        EventObserverErrorInfo? errorInfo = null;
        ManualResetEvent syncEvent = new(false);
        ManualResetEvent handlerReturnedEvent = new(false);

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
                await Task.Delay(50);
                syncEvent.Set();
            }
        }, ObservableEventHandlerOptions.RunHandlerAsynchronously, "Test observer");

        driver.OnEventHandlerErrorOccurred.AddObserver(e =>
        {
            errorInfo = e.ErrorInfo with { };
            handlerReturnedEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

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
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        bool handlerReturned = handlerReturnedEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(handlerReturned, Is.True);
            Assert.That(errorInfo, Is.Not.Null);
            Assert.That(errorInfo!.ObserverId, Is.EqualTo(browsingContextObserver.Id));
            Assert.That(errorInfo.Exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(errorInfo.ObservableEventName, Is.EqualTo("browsingContext.contextCreated"));
            Assert.That(errorInfo.ObserverDescription, Is.EqualTo("Test observer"));
        }
    }

    [Test]
    public async Task TestModuleAvailability()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        try
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(driver.Bluetooth, Is.InstanceOf<BluetoothModule>());
                Assert.That(driver.Browser, Is.InstanceOf<BrowserModule>());
                Assert.That(driver.BrowsingContext, Is.InstanceOf<BrowsingContextModule>());
                Assert.That(driver.Emulation, Is.InstanceOf<EmulationModule>());
                Assert.That(driver.Input, Is.InstanceOf<InputModule>());
                Assert.That(driver.Log, Is.InstanceOf<LogModule>());
                Assert.That(driver.Network, Is.InstanceOf<NetworkModule>());
                Assert.That(driver.Permissions, Is.InstanceOf<PermissionsModule>());
                Assert.That(driver.Script, Is.InstanceOf<ScriptModule>());
                Assert.That(driver.Session, Is.InstanceOf<SessionModule>());
                Assert.That(driver.Speculation, Is.InstanceOf<SpeculationModule>());
                Assert.That(driver.Storage, Is.InstanceOf<StorageModule>());
                Assert.That(driver.UserAgentClientHints, Is.InstanceOf<UserAgentClientHintsModule>());
                Assert.That(driver.WebExtension, Is.InstanceOf<WebExtensionModule>());
            }
        }
        finally
        {
            await driver.StopAsync();
        }
    }

    [Test]
    public void TestCanRegisterModule()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.That(driver.GetModule<TestProtocolModule>("protocol"), Is.InstanceOf<TestProtocolModule>());
    }

    [Test]
    public void TestRegisteringModuleWithDuplicateNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver, 0, false));
        Assert.That(() => driver.RegisterModule(new TestProtocolModule(driver)), Throws.InstanceOf<ArgumentException>().With.Message.StartsWith("A module with the name 'protocol' has already been registered"));
    }

    [Test]
    public async Task TestRegisteringModuleAfterStartingDriverThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(() => driver.RegisterModule(new TestProtocolModule(driver, 0, false)), Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void TestGettingInvalidModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.GetModule<TestProtocolModule>("protocol"), Throws.InstanceOf<ArgumentException>().With.Message.Contains("Module 'protocol' is not registered with this driver"));
    }

    [Test]
    public void TestGettingInvalidModuleTypeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.That(() => driver.GetModule<SessionModule>("protocol"), Throws.InstanceOf<InvalidCastException>().With.Message.EqualTo("Module 'protocol' is registered with this driver, but the module object is not of type WebDriverBiDi.Session.SessionModule"));
    }

    [Test]
    public async Task TestReceivingNullValueFromSendingCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws:localhost");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("is unexpectedly null"));
    }

    [Test]
    public async Task TestCanceledCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ShouldCancelCommand = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws:localhost");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("was canceled before a result was received"));
    }

    [Test]
    public async Task TestUncompletedCommandThrows()
    {
        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnUncompletedCommand = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws:localhost");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("is unexpectedly null"));
    }

    [Test]
    public async Task TestExecutingCommandWillThrowWhenTimeout()
    {
        BiDiDriver driver = new(TimeSpan.Zero, new Transport(new TestWebSocketConnection()));
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiTimeoutException>().With.Message.Contains("Timed out executing command test.command"));
    }

    [Test]
    public async Task TestTimedOutCommandIgnoresLateResponse()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1), transport);
        await driver.StartAsync("ws://localhost:5555");

        Assert.That(
            async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")),
            Throws.InstanceOf<WebDriverBiDiTimeoutException>());

        string lateResponse = """{"type":"success","id":1,"result":{"parameterName":"parameterValue"}}""";
        Assert.That(async () => await connection.RaiseDataReceivedEventAsync(lateResponse), Throws.Nothing);
    }

    [Test]
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
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Could not convert error response from transport for SendCommandAndWait to ErrorResult"));
    }

    [Test]
    public async Task TestReceivingInvalidResultTypeFromSendingCommandThrows()
    {
        TestCommandResultInvalid result = new()
        {
            Value = "invalid",
        };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.EqualTo("invalid"));
            Assert.That(result with { }, Is.EqualTo(result));
        }

        TestTransport transport = new(new TestWebSocketConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Could not convert response from transport for SendCommandAndWait to WebDriverBiDi.TestUtilities.TestCommandResult"));
    }

    [Test]
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
        await driver.StartAsync("ws:localhost");
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        Assert.That(logs, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(logs[0].Message, Is.EqualTo("test log message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Warn));
            Assert.That(logs[0].Timestamp, Is.GreaterThanOrEqualTo(testStart));
            Assert.That(logs[0].ComponentName, Is.EqualTo("TestWebSocketConnection"));
        }
    }

    [Test]
    public async Task TestDriverCanUseDefaultTransport()
    {
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(ClientConnectionEventArgs e) { connectionSyncEvent.Set(); }
        static void handler(ServerDataReceivedEventArgs e) { }
        Server server = new();
        ServerEventObserver<ServerDataReceivedEventArgs> dataReceivedObserver = server.OnDataReceived.AddObserver(handler);
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();

        BiDiDriver driver = new();
        await driver.StartAsync($"ws://localhost:{server.Port}");
        bool connectionEventRaised = connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));
        await driver.StopAsync();

        await server.StopAsync();
        dataReceivedObserver.Unobserve();
        Assert.That(connectionEventRaised, Is.True);
    }

    [Test]
    public async Task TestMalformedEventResponseLogsError()
    {
        string connectionId = string.Empty;
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionSyncEvent.Set();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();
        BiDiDriver driver = new(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));
            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver((e) =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logSyncEvent.Set();
                }
            });

            ManualResetEvent unknownMessageSyncEvent = new(false);
            string unknownMessage = string.Empty;
            driver.OnUnknownMessageReceived.AddObserver((e) =>
            {
                unknownMessage = e.Message;
                unknownMessageSyncEvent.Set();
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
            bool eventsRaised = WaitHandle.WaitAll(new WaitHandle[] { logSyncEvent, unknownMessageSyncEvent }, TimeSpan.FromSeconds(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(eventsRaised, Is.True);
                Assert.That(driverLog, Has.Count.EqualTo(1));
                Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing event JSON"));
                Assert.That(unknownMessage, Is.Not.Empty);
            }
        }
        finally
        {
            await driver.StopAsync();
            await server.StopAsync();
        }
    }

    [Test]
    public async Task TestMalformedNonCommandErrorResponseLogsError()
    {
        string connectionId = string.Empty;
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionSyncEvent.Set();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();
        BiDiDriver driver = new();

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

            driver.BrowsingContext.OnLoad.AddObserver((e) =>
            {
            });

            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver((e) =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logSyncEvent.Set();
                }
            });

            ManualResetEvent unknownMessageSyncEvent = new(false);
            string unknownMessage = string.Empty;
            driver.OnUnknownMessageReceived.AddObserver((e) =>
            {
                unknownMessage = e.Message;
                unknownMessageSyncEvent.Set();
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
            bool eventsRaised = WaitHandle.WaitAll(new WaitHandle[] { logSyncEvent, unknownMessageSyncEvent }, TimeSpan.FromSeconds(1));
            Assert.That(eventsRaised, Is.True);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(driverLog, Has.Count.EqualTo(1));
                Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing error JSON"));
                Assert.That(unknownMessage, Is.Not.Empty);
            }
        }
        finally
        {
            await driver.StopAsync();
            await server.StopAsync();
        }
    }

    [Test]
    public async Task TestMalformedIncomingMessageLogsError()
    {
        string connectionId = string.Empty;
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionSyncEvent.Set();
        }

        Server server = new();
        server.OnClientConnected.AddObserver(connectionHandler);
        await server.StartAsync();
        BiDiDriver driver = new();

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = [];
            driver.OnLogMessage.AddObserver((e) =>
            {
                if (e.Level >= WebDriverBiDiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logSyncEvent.Set();
                }
            });

            ManualResetEvent unknownMessageSyncEvent = new(false);
            string unknownMessage = string.Empty;
            driver.OnUnknownMessageReceived.AddObserver((e) =>
            {
                unknownMessage = e.Message;
                unknownMessageSyncEvent.Set();
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
            bool eventsRaised = WaitHandle.WaitAll([logSyncEvent, unknownMessageSyncEvent], TimeSpan.FromSeconds(1));
            Assert.That(eventsRaised, Is.True);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(driverLog, Has.Count.EqualTo(1));
                Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing JSON message"));
                Assert.That(unknownMessage, Is.Not.Empty);
            }
        }
        finally
        {
            await driver.StopAsync();
            await server.StopAsync();
        }
    }

    [Test]
    public async Task TestCanModifyEventHandlerExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(driver.EventHandlerExceptionBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
        driver.EventHandlerExceptionBehavior = TransportErrorBehavior.Collect;
        Assert.That(transport.EventHandlerExceptionBehavior, Is.EqualTo(TransportErrorBehavior.Collect));
    }

    [Test]
    public async Task TestCanModifyUnexpectedErrorExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(driver.UnexpectedErrorBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
        driver.UnexpectedErrorBehavior = TransportErrorBehavior.Collect;
        Assert.That(transport.UnexpectedErrorBehavior, Is.EqualTo(TransportErrorBehavior.Collect));
    }

    [Test]
    public async Task TestCanModifyProtocolErrorExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(driver.ProtocolErrorBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
        driver.ProtocolErrorBehavior = TransportErrorBehavior.Collect;
        Assert.That(transport.ProtocolErrorBehavior, Is.EqualTo(TransportErrorBehavior.Collect));
    }

    [Test]
    public async Task TestCanModifyUnknownMessageExceptionBehavior()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(driver.UnknownMessageBehavior, Is.EqualTo(TransportErrorBehavior.Ignore));
        driver.UnknownMessageBehavior = TransportErrorBehavior.Collect;
        Assert.That(transport.UnknownMessageBehavior, Is.EqualTo(TransportErrorBehavior.Collect));
    }

    [Test]
    public async Task CanExecuteParallelCommands()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            Task.Run(async () =>
            {
                DateTime start = DateTime.Now;
                if (e.SentCommandName!.Contains("delay"))
                {
                    Task.Delay(250).Wait();
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
        await driver.StartAsync("ws://localhost:5555");

        string delayCommandName = "module.delayCommand";
        TestCommandParameters delayCommand = new(delayCommandName);

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);

        Task<TestCommandResult>[] parallelTasks = new[]
        {
            driver.ExecuteCommandAsync<TestCommandResult>(delayCommand),
            driver.ExecuteCommandAsync<TestCommandResult>(command),
        };

        int indexOfFirstFinishedTask = Task.WaitAny(parallelTasks);
        bool allTasksCompleted = Task.WaitAll(parallelTasks, TimeSpan.FromSeconds(1));
        Assert.That(allTasksCompleted, Is.True);
        Assert.That(indexOfFirstFinishedTask, Is.EqualTo(1));
        Assert.That(parallelTasks[0].Result.Value, Is.EqualTo($"command result value for {delayCommandName}"));
        Assert.That(parallelTasks[0].Result.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(240));
        Assert.That(parallelTasks[1].Result.Value, Is.EqualTo($"command result value for {commandName}"));
    }

    [Test]
    public async Task TestCanDisposeStartedDriver()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        Assert.That(async () => await driver.StartAsync("ws://localhost:5555"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestCanDisposeDriverWithoutStarting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.DisposeAsync();
        Assert.That(async () => await driver.StartAsync("ws://localhost:5555"), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestDoubleDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        Assert.That(async () => await driver.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestStopThenDisposeDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.StopAsync();
        Assert.That(async () => await driver.DisposeAsync(), Throws.Nothing);
    }

    [Test]
    public async Task TestExecuteCommandAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestExecuteCommandWithTimeoutAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command"), TimeSpan.FromMilliseconds(100)), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
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
            await driver.StartAsync("ws://localhost:5555");
            TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("module.command"));
            commandValue = result.Value!;
        }

        Assert.That(commandValue, Is.EqualTo("command result value"));
    }

    [Test]
    public async Task TestRegisteringModuleAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        Assert.That(() => driver.RegisterModule(new TestProtocolModule(driver)), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestGettingModuleAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        TestProtocolModule module = new(driver);
        driver.RegisterModule(module);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        Assert.That(() => driver.GetModule<TestProtocolModule>(module.ModuleName), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestRegisteringEventAfterDisposeThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        Assert.That(() => driver.RegisterEvent("protocol.event", eventInvoker), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestDisposeDisposesTransport()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(transport.IsDisposed, Is.False);
        await driver.DisposeAsync();
        Assert.That(transport.IsDisposed, Is.True);
    }

    [Test]
    public async Task TestDisposeDisposesTransportWithoutStarting()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(transport.IsDisposed, Is.False);
        await driver.DisposeAsync();
        Assert.That(transport.IsDisposed, Is.True);
    }

    [Test]
    public async Task TestDisposeLogsExceptionFromStopAsync()
    {
        List<LogMessageEventArgs> logs = [];
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        driver.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
        });
        transport.ThrowOnDisconnect = true;
        await driver.DisposeAsync();
        Assert.That(logs, Has.Some.Matches<LogMessageEventArgs>(
            log => log.Message.Contains("Unexpected exception during disposal")
                   && log.Message.Contains("Simulated disconnect failure")
                   && log.Level == WebDriverBiDiLogLevel.Warn
                   && log.ComponentName == BiDiDriver.LoggerComponentName));
    }

    [Test]
    public void TestRegisterTypeInfoResolverBeforeStarting()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.Nothing);
    }

    [Test]
    public async Task TestRegisterTypeInfoResolverAfterStartingThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(() => driver.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.InstanceOf<InvalidOperationException>().With.Message.Contains("Cannot register a type info resolver after the transport is connected"));
    }

    [Test]
    public async Task TestRegisterTypeInfoResolverAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.DisposeAsync();
        Assert.That(() => driver.RegisterTypeInfoResolver(new DefaultJsonTypeInfoResolver()), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task CanExecuteCommandWithUntypedCommandParameters()
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
        await driver.StartAsync("ws://localhost:5555");

        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command);
        Assert.That(result.Value, Is.EqualTo("command result value"));
    }

    [Test]
    public async Task CanExecuteCommandWithUntypedCommandParametersAndTimeout()
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
        await driver.StartAsync("ws://localhost:5555");

        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromMilliseconds(1500));
        Assert.That(result.Value, Is.EqualTo("command result value"));
    }

    [Test]
    public async Task TestExecuteCommandWithUntypedParametersAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        CommandParameters command = new TestCommandParameters("test.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public async Task TestExecuteCommandWithUntypedParametersAndTimeoutAfterDisposeThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        await driver.DisposeAsync();
        CommandParameters command = new TestCommandParameters("test.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromMilliseconds(100)), Throws.InstanceOf<ObjectDisposedException>());
    }

    [Test]
    public void TestStartAsyncThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Assert.That(async () => await driver.StartAsync("ws://localhost:5555", cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestExecuteCommandThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParams = new("module.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(commandParams, cancellationToken: cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestExecuteCommandWithTimeoutThrowsWhenCancellationTokenIsCanceled()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        using CancellationTokenSource cts = new();
        cts.Cancel();

        TestCommandParameters commandParams = new("module.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(commandParams, TimeSpan.FromSeconds(5), cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestExecuteCommandCancelsCommandOnCancellation()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        await driver.StartAsync("ws://localhost:5555");

        using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(50));
        CommandParameters command = new TestCommandParameters("module.command");

        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(30), cts.Token), Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task TestExecuteCommandAsyncWithNullCommandParametersThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task TestExecuteCommandAsyncWithNegativeTimeoutThrows()
    {
        TestWebSocketConnection connection = new();
        TestTransport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromSeconds(30), transport);
        CommandParameters command = new TestCommandParameters("module.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(-5)), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public async Task TestExecuteCommandAsyncWithZeroTimeoutDoesNotThrowArgumentOutOfRangeException()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            // force a delay in responding to ensure that the timeout is actually being applied
            await Task.Delay(TimeSpan.FromMilliseconds(50));
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
        await driver.StartAsync("ws://localhost:5555");
        CommandParameters command = new TestCommandParameters("module.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.Zero), Throws.InstanceOf<WebDriverBiDiTimeoutException>());
    }

    [Test]
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
        await driver.StartAsync("ws://localhost:5555");
        CommandParameters command = new TestCommandParameters("module.command");
        TestCommandResult result = await driver.ExecuteCommandAsync<TestCommandResult>(command, TimeSpan.FromSeconds(5));
        Assert.That(result.Value, Is.EqualTo("command result value"));
    }

    [Test]
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
        await driver.StartAsync("ws://localhost:5555");
        CommandParameters command = new TestCommandParameters("module.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, Timeout.InfiniteTimeSpan), Throws.Nothing);
    }

    [Test]
    public async Task TestExecuteCommandAsyncWithNullTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestWebSocketConnectionDataSentEventArgs e) =>
        {
            // force a delay in responding to ensure that the timeout is actually being applied
            await Task.Delay(TimeSpan.FromMilliseconds(50));
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
        await driver.StartAsync("ws://localhost:5555");
        CommandParameters command = new TestCommandParameters("module.command");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command, null), Throws.Nothing);
    }

    [Test]
    public async Task TestRegisterEventWithNullEventNameThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.RegisterEvent(null!, eventInvoker), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public async Task TestRegisterEventWithEmptyEventNameThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = (eventData) => Task.CompletedTask;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.RegisterEvent(string.Empty, eventInvoker), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public async Task TestRegisterEventWithNullEventInvokerThrows()
    {
        Func<EventInfo<TestEventArgs>, Task> eventInvoker = null!;
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.RegisterEvent("protocol.event", eventInvoker), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task TestRegisterModuleWithNullModuleThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.RegisterModule(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task TestGetModuleWithNullModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.GetModule<TestProtocolModule>(null!), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public async Task TestGetModuleWithEmptyModuleNameThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.GetModule<TestProtocolModule>(string.Empty), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public async Task TestRegisterTypeInfoResolverWithNullResolverThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.RegisterTypeInfoResolver(null!), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void TestCreatingWithNegativeDefaultCommandTimeoutThrows()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.That(() => new BiDiDriver(TimeSpan.FromSeconds(-1), transport), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void TestCreatingWithZeroDefaultCommandTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.That(() => new BiDiDriver(TimeSpan.Zero, transport), Throws.Nothing);
    }

    [Test]
    public void TestCreatingWithInfiniteDefaultCommandTimeoutDoesNotThrow()
    {
        TestWebSocketConnection connection = new();
        Transport transport = new(connection);
        Assert.That(() => new BiDiDriver(Timeout.InfiniteTimeSpan, transport), Throws.Nothing);
    }
}
