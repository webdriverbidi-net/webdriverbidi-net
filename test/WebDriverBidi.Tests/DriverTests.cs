namespace WebDriverBidi;

using TestUtilities;
using PinchHitter;
using WebDriverBidi.BrowsingContext;
using WebDriverBidi.Input;
using WebDriverBidi.Log;
using WebDriverBidi.Protocol;
using WebDriverBidi.Script;
using WebDriverBidi.Session;

[TestFixture]
public class DriverTests
{
    [Test]
    public async Task CanExecuteCommand()
    {
        ManualResetEvent syncEvent = new(false);
        TestConnection connection = new();
        connection.DataSendComplete += (object? sender, TestConnectionDataSentEventArgs e) =>
        {
            syncEvent.Set();
        };

        Transport transport = new(TimeSpan.FromMilliseconds(1500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);

        string commandName = "module.command";
        TestCommand command = new(commandName);
        var task = Task.Run(() => driver.ExecuteCommand<TestCommandResult>(command));
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));

        connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""result"": { ""value"": ""command result value"" } }");
        task.Wait(TimeSpan.FromSeconds(3));
        Assert.That(task.Result.Value, Is.EqualTo("command result value"));
    }

    [Test]
    public async Task CanExecuteCommandWithError()
    {
        ManualResetEvent syncEvent = new(false);
        TestConnection connection = new();
        connection.DataSendComplete += (object? sender, TestConnectionDataSentEventArgs e) =>
        {
            syncEvent.Set();
        };

        Transport transport = new(TimeSpan.FromMilliseconds(1500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);

        string commandName = "module.command";
        TestCommand command = new(commandName);
        Assert.That(() =>
        {
            var task = Task.Run(() => driver.ExecuteCommand<TestCommandResult>(command));
            syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
            task.Wait(TimeSpan.FromSeconds(3));
        }, Throws.InstanceOf<AggregateException>().With.InnerException.TypeOf<WebDriverBidiException>().With.Message.Contains("'unknown command' error executing command module.command: This is a test error message"));
    }

    [Test]
    public async Task CanExecuteReceiveErrorWithoutCommand()
    {
        ErrorResult? response = null;
        ManualResetEvent syncEvent = new(false);
        TestConnection connection = new();
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.UnexpectedErrorReceived += (object? sender, ErrorReceivedEventArgs e) =>
        {
            response = e.ErrorData;
            syncEvent.Set();
        };

        connection.RaiseDataReceivedEvent(@"{ ""id"": null, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
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
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.RegisterEvent<TestEventArgs>(eventName);
        driver.EventReceived += (sender, e) =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        };

        connection.RaiseDataReceivedEvent(@"{ ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }");
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
        TestTransport transport = new(TimeSpan.FromMilliseconds(500), connection)
        {
            MessageProcessingDelay = TimeSpan.FromMilliseconds(100)
        };
        Driver driver = new(transport);
        await driver.Start("ws://localhost:5555");
        driver.RegisterEvent<TestEventArgs>(eventName);
        driver.EventReceived += (sender, e) =>
        {
            receivedEvent = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        };

        connection.RaiseDataReceivedEvent(@"{ ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        await driver.Stop();
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
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.UnknownMessageReceived += (sender, e) =>
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        };

        string serialized = @"{ ""method"": ""module.event"", ""params"": { ""paramName"": ""paramValue"" } }";
        connection.RaiseDataReceivedEvent(serialized);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

    [Test]
    public async Task TestUnconformingDataRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.UnknownMessageReceived += (sender, e) =>
        {
            receivedMessage = e.Message;
            syncEvent.Set();
        };

        string serialized = @"{ ""someProperty"": ""someValue"", ""params"": { ""thisMessage"": ""matches no protocol message"" } }";
        connection.RaiseDataReceivedEvent(serialized);
        syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.That(receivedMessage, Is.EqualTo(serialized));
    }

    [Test]
    public async Task TestModuleAvailability()
    {
        TestConnection connection = new();
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        Driver driver = new(transport);
        await driver.Start("ws://localhost:5555");
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(driver.BrowsingContext, Is.InstanceOf<BrowsingContextModule>());
                Assert.That(driver.Script, Is.InstanceOf<ScriptModule>());
                Assert.That(driver.Log, Is.InstanceOf<LogModule>());
                Assert.That(driver.Session, Is.InstanceOf<SessionModule>());
                Assert.That(driver.Input, Is.InstanceOf<InputModule>());
            });
        }
        finally
        {
            await driver.Stop();
        }
    }

    [Test]
    public void TestDriverCanEmitLogMessagesFromProtocol()
    {
        DateTime testStart = DateTime.UtcNow;
        List<LogMessageEventArgs> logs = new();
        TestConnection connection = new();
        Transport transport = new(TimeSpan.FromMilliseconds(100), connection);
        Driver driver = new(transport);
        driver.LogMessage += (sender, e) =>
        {
            logs.Add(e);
        };
        connection.RaiseLogMessageEvent("test log message", WebDriverBidiLogLevel.Warn);
        Assert.That(logs, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(logs[0].Message, Is.EqualTo("test log message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBidiLogLevel.Warn));
            Assert.That(logs[0].Timestamp, Is.GreaterThanOrEqualTo(testStart));
            Assert.That(logs[0].ComponentName, Is.EqualTo("TestConnection"));
        });
    }

    [Test]
    public void TestCanRegisterModule()
    {
        TestConnection connection = new();
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        Driver driver = new(transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.That(driver.GetModule<TestProtocolModule>("protocol"), Is.InstanceOf<TestProtocolModule>());
    }

    [Test]
    public void TestGettingInvalidModuleNameThrows()
    {
        TestConnection connection = new();
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        Driver driver = new(transport);
        Assert.That(() => driver.GetModule<TestProtocolModule>("protocol"), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Module 'protocol' is not registered with this driver"));
    }

    [Test]
    public void TestGettingInvalidModuleTypeThrows()
    {
        TestConnection connection = new();
        Transport transport = new(TimeSpan.FromMilliseconds(500), connection);
        Driver driver = new(transport);
        driver.RegisterModule(new TestProtocolModule(driver));
        Assert.That(() => driver.GetModule<SessionModule>("protocol"), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Module 'protocol' is registered with this driver, but the module object is not of type WebDriverBidi.Session.SessionModule"));
    }

    [Test]
    public void TestReceivingNullValueFromSendingCommandThrows()
    {
        TestTransport transport = new(TimeSpan.FromMilliseconds(250), new TestConnection())
        {
            ReturnCustomValue = true
        };
        Driver driver = new(transport);
        Assert.That(async () => await driver.ExecuteCommand<TestCommandResult>(new TestCommand("test.command")), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Received null response from transport for SendCommandAndWait"));
    }

    [Test]
    public void TestReceivingInvalidErrorValueFromSendingCommandThrows()
    {
        TestCommandResult result = new();
        result.SetIsErrorValue(true);
        TestTransport transport = new(TimeSpan.FromMilliseconds(250), new TestConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        Driver driver = new(transport);
        Assert.That(async () => await driver.ExecuteCommand<TestCommandResult>(new TestCommand("test.command")), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Could not convert error response from transport for SendCommandAndWait to ErrorResult"));
    }

    [Test]
    public void TestReceivingInvalidResultTypeFromSendingCommandThrows()
    {
        TestCommandResultInvalid result = new();
        TestTransport transport = new(TimeSpan.FromMilliseconds(250), new TestConnection())
        {
            ReturnCustomValue = true,
            CustomReturnValue = result
        };
        Driver driver = new(transport);
        Assert.That(async () => await driver.ExecuteCommand<TestCommandResult>(new TestCommand("test.command")), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Could not convert response from transport for SendCommandAndWait to WebDriverBidi.TestUtilities.TestCommandResult"));
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

        Driver driver = new();
        await driver.Start($"ws://localhost:{server.Port}");
        bool connectionEventRaised = connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));
        await driver.Stop();

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
        Driver driver = new();

        try
        {
            await driver.Start($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));
            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = new();
            driver.LogMessage += (sender, e) =>
            {
                if (e.Level >= WebDriverBidiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logSyncEvent.Set();
                }
            };

            ManualResetEvent unknownMessageSyncEvent = new(false);
            string unknownMessage = string.Empty;
            driver.UnknownMessageReceived += (sender, e) =>
            {
                unknownMessage = e.Message;
                unknownMessageSyncEvent.Set();
            };

            // This payload omits the required "timestamp" field, which should cause an exception
            // in parsing.
            await server.SendData(connectionId, @"{ ""method"": ""browsingContext.load"", ""params"": { ""context"": ""myContext"", ""url"": ""https://example.com"", ""navigation"": ""myNavigationId"" } }");
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
            await driver.Stop();
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
        Driver driver = new();

        try
        {
            await driver.Start($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

            driver.BrowsingContext.Load += (sender, e) =>
            {
            };

            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = new();
            driver.LogMessage += (sender, e) =>
            {
                if (e.Level >= WebDriverBidiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logSyncEvent.Set();
                }
            };

            ManualResetEvent unknownMessageSyncEvent = new(false);
            string unknownMessage = string.Empty;
            driver.UnknownMessageReceived += (sender, e) =>
            {
                unknownMessage = e.Message;
                unknownMessageSyncEvent.Set();
            };

            // This payload uses an object for the error field, which should cause an exception
            // in parsing.
            await server.SendData(connectionId, @"{ ""id"": null, ""error"": { ""code"": ""unknown error"" }, ""message"": ""This is a test error message"" }");
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
            await driver.Stop();
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
        Driver driver = new();

        try
        {
            await driver.Start($"ws://localhost:{server.Port}");
            connectionSyncEvent.WaitOne(TimeSpan.FromSeconds(1));

            ManualResetEvent logSyncEvent = new(false);
            List<string> driverLog = new();
            driver.LogMessage += (sender, e) =>
            {
                if (e.Level >= WebDriverBidiLogLevel.Error)
                {
                    driverLog.Add(e.Message);
                    logSyncEvent.Set();
                }
            };

            ManualResetEvent unknownMessageSyncEvent = new(false);
            string unknownMessage = string.Empty;
            driver.UnknownMessageReceived += (sender, e) =>
            {
                unknownMessage = e.Message;
                unknownMessageSyncEvent.Set();
            };

            // This payload uses unparsable JSON, which should cause an exception
            // in parsing.
            await server.SendData(connectionId, @"{ ""id"": null, { ""errorMessage"" }, ""message"": ""This is a test error message"" }");
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
            await driver.Stop();
            server.Stop();
        }
    }
}
