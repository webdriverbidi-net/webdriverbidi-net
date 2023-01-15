namespace WebDriverBidi;

using TestUtilities;
using WebDriverBidi.BrowsingContext;
using WebDriverBidi.Log;
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
        connection.DataSendComplete += delegate(object? sender, EventArgs e)
        {
            syncEvent.Set();
        };

        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(500), connection);
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
        connection.DataSendComplete += delegate(object? sender, EventArgs e)
        {
            syncEvent.Set();
        };

        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);

        string commandName = "module.command";
        TestCommand command = new(commandName);
        Assert.That(() => {
            var task = Task.Run(() => driver.ExecuteCommand<TestCommandResult>(command));
            syncEvent.WaitOne(TimeSpan.FromMilliseconds(100));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
            task.Wait(TimeSpan.FromSeconds(3));
        }, Throws.InstanceOf<AggregateException>().With.InnerException.TypeOf<WebDriverBidiException>().With.Message.Contains("'unknown command' error executing command module.command: This is a test error message"));
    }

    [Test]
    public async Task CanExecuteReceiveErrorWithoutCommand()
    {
        ErrorResponse? response = null;
        ManualResetEvent syncEvent = new(false);
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.UnexpectedErrorReceived += delegate(object? sender, ProtocolErrorReceivedEventArgs e)
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
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.RegisterEvent(eventName, typeof(TestEventArgs));
        driver.EventReceived += delegate(object? sender, ProtocolEventReceivedEventArgs e)
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
    public async Task TestUnregisteredEventRaisesUnknownMessageEvent()
    {
        string receivedMessage = string.Empty;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.UnknownMessageReceived += delegate(object? sender, ProtocolUnknownMessageReceivedEventArgs e)
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
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(500), connection);
        await transport.Connect("ws://localhost:5555");
        Driver driver = new(transport);
        driver.UnknownMessageReceived += delegate(object? sender, ProtocolUnknownMessageReceivedEventArgs e)
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
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(500), connection);
        // await transport.Connect("ws://localhost:5555");
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
        List<LogMessageEventArgs> logs = new();
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        TestDriver driver = new(transport);
        driver.LogMessage += (sender, e) =>
        {
            logs.Add(e);
        };
        connection.EmitLogMessage("test log message", WebDriverBidiLogLevel.Warn);
        Assert.That(logs, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(logs[0].Message, Is.EqualTo("test log message"));
            Assert.That(logs[0].Level, Is.EqualTo(WebDriverBidiLogLevel.Warn));
        });
    }
}