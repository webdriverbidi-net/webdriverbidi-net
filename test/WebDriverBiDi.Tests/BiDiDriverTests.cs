namespace WebDriverBiDi;

using TestUtilities;
using PinchHitter;
using WebDriverBiDi.Browser;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Input;
using WebDriverBiDi.Log;
using WebDriverBiDi.Network;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;
using WebDriverBiDi.Storage;
using WebDriverBiDi.Permissions;

[TestFixture]
public class BiDiDriverTests
{
    [Test]
    public async Task CanExecuteCommand()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestConnectionDataSentEventArgs e) =>
        {
            await connection.RaiseDataReceivedEventAsync(@"{ ""type"": ""success"", ""id"": 1, ""result"": { ""value"": ""command result value"" } }");
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
        TestConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestConnectionDataSentEventArgs e) =>
        {
            await connection.RaiseDataReceivedEventAsync(@"{ ""type"": ""error"", ""id"": 1, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555");

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("'unknown command' error executing command module.command: This is a test error message"));
    }

    [Test]
    public async Task CanExecuteCommandThatReturnsThrownExceptionThrows()
    {
        TestConnection connection = new();
        connection.DataSendComplete += async (object? sender, TestConnectionDataSentEventArgs e) =>
        {
            await connection.RaiseDataReceivedEventAsync(@"{ ""type"": ""success"", ""id"": 1,  ""noResult"": { ""invalid"": ""unknown command"", ""message"": ""This is a test error message"" } }");
        };

        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);
        await driver.StartAsync("ws://localhost:5555");

        string commandName = "module.command";
        TestCommandParameters command = new(commandName);
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(command), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Response did not contain properly formed JSON for response type"));
    }

    [Test]
    public async Task CanExecuteReceiveErrorWithoutCommand()
    {
        ErrorResult? response = null;
        ManualResetEvent syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnexpectedErrorReceived.AddObserver((ErrorReceivedEventArgs e) =>
        {
            response = e.ErrorData;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

        await connection.RaiseDataReceivedEventAsync(@"{ ""type"": ""error"", ""id"": null, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));

        Assert.That(response, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response!.ErrorType, Is.EqualTo("unknown command"));
            Assert.That(response.ErrorMessage, Is.EqualTo("This is a test error message"));
        });
    }

    [Test]
    public async Task CanReceiveKnownEvent()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        string eventName = "module.event";
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName);
        driver.OnEventReceived.AddObserver((e) =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

        await connection.RaiseDataReceivedEventAsync(@"{ ""type"": ""event"", ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(receivedEvent, Is.EqualTo(eventName));
            Assert.That(receivedData, Is.Not.Null);
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        });
        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public async Task TestDriverWillProcessPendingMessagesOnStop()
    {
        string receivedEvent = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        string eventName = "module.event";
        TestConnection connection = new();
        TestTransport transport = new(connection)
        {
            MessageProcessingDelay = TimeSpan.FromMilliseconds(100)
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterEvent<TestEventArgs>(eventName);
        driver.OnEventReceived.AddObserver((e) =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

        await connection.RaiseDataReceivedEventAsync(@"{ ""type"": ""event"", ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        await driver.StopAsync();
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.Multiple(() =>
        {
            Assert.That(receivedEvent, Is.EqualTo(eventName));
            Assert.That(receivedData, Is.Not.Null);
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        });
        TestEventArgs? convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public async Task TestUnregisteredEventRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver((e) =>
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

        string serialized = @"{ ""type"": ""event"", ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }";
        await connection.RaiseDataReceivedEventAsync(serialized);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

    [Test]
    public async Task TestUnconformingDataRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.OnUnknownMessageReceived.AddObserver((e) =>
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        });
        await driver.StartAsync("ws://localhost:5555");

        string serialized = @"{ ""someProperty"": ""someValue"", ""params"": { ""thisMessage"": ""matches no protocol message"" } }";
        await connection.RaiseDataReceivedEventAsync(serialized);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

    [Test]
    public async Task TestModuleAvailability()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        await driver.StartAsync("ws://localhost:5555");
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(driver.Browser, Is.InstanceOf<BrowserModule>());
                Assert.That(driver.BrowsingContext, Is.InstanceOf<BrowsingContextModule>());
                Assert.That(driver.Input, Is.InstanceOf<InputModule>());
                Assert.That(driver.Log, Is.InstanceOf<LogModule>());
                Assert.That(driver.Network, Is.InstanceOf<NetworkModule>());
                Assert.That(driver.Permissions, Is.InstanceOf<PermissionsModule>());
                Assert.That(driver.Script, Is.InstanceOf<ScriptModule>());
                Assert.That(driver.Session, Is.InstanceOf<SessionModule>());
                Assert.That(driver.Storage, Is.InstanceOf<StorageModule>());
            });
        }
        finally
        {
            await driver.StopAsync();
        }
    }

    [Test]
    public async Task TestDriverCanEmitLogMessagesFromProtocol()
    {
        DateTime testStart = DateTime.UtcNow;
        List<LogMessageEventArgs> logs = new();
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(100), transport);
        driver.OnLogMessage.AddObserver((e) =>
        {
            logs.Add(e);
        });
        await driver.StartAsync("ws:localhost");
        await connection.RaiseLogMessageEventAsync("test log message", WebDriverBiDiLogLevel.Warn);
        Assert.That(logs, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(logs[0].Message, Is.EqualTo("test log message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBiDiLogLevel.Warn));
            Assert.That(logs[0].Timestamp, Is.GreaterThanOrEqualTo(testStart));
            Assert.That(logs[0].ComponentName, Is.EqualTo("TestConnection"));
        });
    }

    [Test]
    public void TestCanRegisterModule()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.That(driver.GetModule<TestProtocolModule>("protocol"), Is.InstanceOf<TestProtocolModule>());
    }

    [Test]
    public void TestGettingInvalidModuleNameThrows()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        Assert.That(() => driver.GetModule<TestProtocolModule>("protocol"), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Module 'protocol' is not registered with this driver"));
    }

    [Test]
    public void TestGettingInvalidModuleTypeThrows()
    {
        TestConnection connection = new();
        Transport transport = new(connection);
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(500), transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.That(() => driver.GetModule<SessionModule>("protocol"), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Module 'protocol' is registered with this driver, but the module object is not of type WebDriverBiDi.Session.SessionModule"));
    }

    [Test]
    public async Task TestReceivingNullValueFromSendingCommandThrows()
    {
        TestTransport transport = new(new TestConnection())
        {
            ReturnCustomValue = true
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws:localhost");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Result and thrown exception for command test.command with id 0 are both null"));
    }

    [Test]
    public async Task TestExecutingCommandWillThrowWhenTimeout()
    {
        BiDiDriver driver = new(TimeSpan.Zero, new Transport(new TestConnection()));
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("Timed out executing command test.command"));
    }

    [Test]
    public async Task TestReceivingInvalidErrorValueFromSendingCommandThrows()
    {
        TestCommandResult result = new();
        result.SetIsErrorValue(true);
        TestTransport transport = new(new TestConnection())
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
        TestCommandResultInvalid result = new();
        TestTransport transport = new(new TestConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(250), transport);
        await driver.StartAsync("ws://localhost:5555");
        Assert.That(async () => await driver.ExecuteCommandAsync<TestCommandResult>(new TestCommandParameters("test.command")), Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("Could not convert response from transport for SendCommandAndWait to WebDriverBiDi.TestUtilities.TestCommandResult"));
    }

    [Test]
    public async Task TestDriverCanUseDefaultTransport()
    {
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(object? sender, ClientConnectionEventArgs e) { connectionSyncEvent.Set(); }
        static void handler(object? sender, ServerDataReceivedEventArgs e) { }
        Server server = new();
        server.DataReceived += handler;
        server.ClientConnected += connectionHandler;
        server.Start();

        BiDiDriver driver = new();
        await driver.StartAsync($"ws://localhost:{server.Port}");
        bool connectionEventRaised = connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));
        await driver.StopAsync();

        server.Stop();
        server.DataReceived -= handler;
        Assert.That(connectionEventRaised, Is.True);
    }

    [Test]
    public async Task TestMalformedEventResponseLogsError()
    {
        string connectionId = string.Empty;
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(object? sender, ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionSyncEvent.Set();
        }

        Server server = new();
        server.ClientConnected += connectionHandler;
        server.Start();
        BiDiDriver driver = new(TimeSpan.FromSeconds(30));

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));
            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = new();
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
            await server.SendDataAsync(connectionId, @"{ ""type"": ""event"", ""method"": ""browsingContext.load"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }");
            bool eventsRaised = WaitHandle.WaitAll(new WaitHandle[] { logSyncEvent, unknownMessageSyncEvent }, TimeSpan.FromSeconds(1));
            Assert.Multiple(() =>
            {
                Assert.That(eventsRaised, Is.True);
                Assert.That(driverLog, Has.Count.EqualTo(1));
                Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing event JSON"));
                Assert.That(unknownMessage, Is.Not.Empty);
            });
        }
        finally
        {
            await driver.StopAsync();
            server.Stop();
        }
    }

    [Test]
    public async Task TestMalformedNonCommandErrorResponseLogsError()
    {
        string connectionId = string.Empty;
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(object? sender, ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionSyncEvent.Set();
        }

        Server server = new();
        server.ClientConnected += connectionHandler;
        server.Start();
        BiDiDriver driver = new();

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

            driver.BrowsingContext.OnLoad.AddObserver((e) =>
            {
            });

            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = new();
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
            await server.SendDataAsync(connectionId, @"{ ""type"": ""error"", ""id"": null, ""error"": { ""code"": ""unknown error"" }, ""message"": ""This is a test error message"" }");
            bool eventsRaised = WaitHandle.WaitAll(new WaitHandle[] { logSyncEvent, unknownMessageSyncEvent }, TimeSpan.FromSeconds(1));
            Assert.That(eventsRaised, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(driverLog, Has.Count.EqualTo(1));
                Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing error JSON"));
                Assert.That(unknownMessage, Is.Not.Empty);
            });
        }
        finally
        {
            await driver.StopAsync();
            server.Stop();
        }
    }

    [Test]
    public async Task TestMalformedIncomingMessageLogsError()
    {
        string connectionId = string.Empty;
        ManualResetEvent connectionSyncEvent = new(false);
        void connectionHandler(object? sender, ClientConnectionEventArgs e)
        {
            connectionId = e.ConnectionId;
            connectionSyncEvent.Set();
        }

        Server server = new();
        server.ClientConnected += connectionHandler;
        server.Start();
        BiDiDriver driver = new();

        try
        {
            await driver.StartAsync($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = new();
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
            await server.SendDataAsync(connectionId, @"{ ""type"": ""error"", ""id"": null, { ""errorMessage"" }, ""message"": ""This is a test error message"" }");
            bool eventsRaised = WaitHandle.WaitAll(new WaitHandle[] { logSyncEvent, unknownMessageSyncEvent }, TimeSpan.FromSeconds(1));
            Assert.That(eventsRaised, Is.True);
            Assert.Multiple(() =>
            {
                Assert.That(driverLog, Has.Count.EqualTo(1));
                Assert.That(driverLog[0], Contains.Substring("Unexpected error parsing JSON message"));
                Assert.That(unknownMessage, Is.Not.Empty);
            });
        }
        finally
        {
            await driver.StopAsync();
            server.Stop();
        }
    }

    [Test]
    public async Task CanExecuteParallelCommands()
    {
        TestConnection connection = new();
        connection.DataSendComplete += (object? sender, TestConnectionDataSentEventArgs e) =>
        {
            Task.Run(async () =>
            {
                DateTime start = DateTime.Now;
                if (e.SentCommandName!.Contains("delay"))
                {
                    Task.Delay(250).Wait();
                }

                TimeSpan elapsed = DateTime.Now - start;
                await connection.RaiseDataReceivedEventAsync(@$"{{ ""type"": ""success"", ""id"": {e.SentCommandId}, ""result"": {{ ""value"": ""command result value for {e.SentCommandName}"", ""elapsed"": {elapsed.TotalMilliseconds} }} }}");
            });
       };

        Transport transport = new(connection);
        await transport.ConnectAsync("ws://localhost:5555");
        BiDiDriver driver = new(TimeSpan.FromMilliseconds(1500), transport);

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
}
