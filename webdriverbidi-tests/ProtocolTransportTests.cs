namespace WebDriverBidi;

using Newtonsoft.Json.Linq;
using TestUtilities;

[TestFixture]
public class ProtocolTransportTests
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
        ProtocolTransport transport = new(TimeSpan.Zero, connection);
        await transport.Connect("ws://localhost:5555");

        TestCommand command = new(commandName);
        _ = await transport.SendCommand(command);

        var dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();       
        Assert.That(dataValue, Is.EquivalentTo(expected));
    }

    [Test]
    public async Task TestTransportCanWaitForCommandComplete()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");

        TestCommand command = new(commandName);
        long commandId = await transport.SendCommand(command);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""result"": { ""value"": ""response value"" } }");
        });
        transport.WaitForCommandComplete(1, TimeSpan.FromSeconds(250));
    }

    [Test]
    public async Task TestTransportWaitForCommandCompleteWillTimeout()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(10), connection);
        await transport.Connect("ws://localhost:5555");

        TestCommand command = new(commandName);
        long commandId = await transport.SendCommand(command);
        Assert.That(() => transport.WaitForCommandComplete(1, TimeSpan.FromMilliseconds(50)), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("Timed out"));
    }

    [Test]
    public void TestTransportWaitForCommandCompleteWillErrorForInvalidId()
    {
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(10), connection);
        Assert.That(() => transport.WaitForCommandComplete(1, TimeSpan.FromMilliseconds(50)), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("Unknown command id"));
    }

    [Test]
    public async Task TestTransportCanGetResponse()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");

        TestCommand command = new(commandName);
        long commandId = await transport.SendCommand(command);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""result"": { ""value"": ""response value"" } }");
        });
        transport.WaitForCommandComplete(1, TimeSpan.FromSeconds(250));
        var actualResult = transport.GetCommandResponse(1);
        Assert.Multiple(() =>
        {
            Assert.That(actualResult.IsError, Is.False);
            Assert.That(actualResult, Is.TypeOf<TestCommandResult>());
        });
        var convertedResult = actualResult as TestCommandResult;
        Assert.That(convertedResult!.Value, Is.EqualTo("response value"));
    }

    [Test]
    public async Task TestTransportCanGetErrorResponse()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");

        TestCommand command = new(commandName);
        long commandId = await transport.SendCommand(command);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
        });
        transport.WaitForCommandComplete(1, TimeSpan.FromSeconds(250));
        var actualResult = transport.GetCommandResponse(1);
        Assert.Multiple(() =>
        {
            Assert.That(actualResult.IsError, Is.True);
            Assert.That(actualResult, Is.InstanceOf<ErrorResponse>());
        });
        var convertedResponse = actualResult as ErrorResponse;
        Assert.Multiple(() =>
        {
            Assert.That(convertedResponse!.ErrorType, Is.EqualTo("unknown command"));
            Assert.That(convertedResponse!.ErrorMessage, Is.EqualTo("This is a test error message"));
            Assert.That(convertedResponse.StackTrace, Is.Null);
        });
    }

    [Test]
    public void TestTransportGetResponseWillErrorForInvalidId()
    {
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        Assert.That(() => transport.GetCommandResponse(1), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("Could not remove command"));
    }

    [Test]
    public async Task TestTransportGetResponseWithThrownException()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");

        TestCommand command = new(commandName);
        long commandId = await transport.SendCommand(command);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""invalid"": ""unknown command"", ""message"": ""This is a test error message"" }");
        });
        transport.WaitForCommandComplete(1, TimeSpan.FromSeconds(250));
        Assert.That(() => transport.GetCommandResponse(1), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Response contained neither result nor error"));
   }

    [Test]
    public async Task TestTransportGetResponseWithNullResultAndThrownException()
    {
        string commandName = "module.command";
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");

        TestCommand command = new(commandName);
        long commandId = await transport.SendCommand(command);
        Assert.That(() => transport.GetCommandResponse(1), Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Result and thrown exception for command with id 1 are both null"));
   }

    [Test]
    public void TestTransportEventReceived()
    {
        string receivedName = string.Empty;
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        transport.RegisterEventArgsType("protocol.event", typeof(TestEventArgs));
        transport.EventReceived += (object? sender, ProtocolEventReceivedEventArgs e) => {
            receivedName = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(@"{ ""method"": ""protocol.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        syncEvent.WaitOne();
        Assert.Multiple(() =>
        {
            Assert.That(receivedName, Is.EqualTo("protocol.event"));
            Assert.That(receivedData, Is.TypeOf<TestEventArgs>());
        });
        var convertedData = receivedData as TestEventArgs;
        Assert.That(convertedData!.ParamName, Is.EqualTo("paramValue"));
    }

    [Test]
    public void TestTransportErrorEventReceived()
    {
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        transport.ErrorEventReceived += (object? sender, ProtocolErrorReceivedEventArgs e) => {
            receivedData = e.ErrorData;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(@"{ ""id"": null, ""error"": ""unknown error"", ""message"": ""This is a test error message"" }");
        syncEvent.WaitOne();

        Assert.That(receivedData, Is.TypeOf<ErrorResponse>());
        var convertedData = receivedData as ErrorResponse;
        Assert.Multiple(() =>
        {
            Assert.That(convertedData!.ErrorType, Is.EqualTo("unknown error"));
            Assert.That(convertedData.ErrorMessage, Is.EqualTo("This is a test error message"));
        });
    }

    [Test]
    public void TestTransportErrorEventReceivedWithNullValues()
    {
        object? receivedData = null;
        ManualResetEvent syncEvent = new(false);

        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        transport.UnknownMessageReceived += (object? sender, ProtocolUnknownMessageReceivedEventArgs e) =>
        {
            receivedData = e.Message;
            syncEvent.Set();
        };
        transport.ErrorEventReceived += (object? sender, ProtocolErrorReceivedEventArgs e) => {
        };
        connection.RaiseDataReceivedEvent(@"{ ""method"": null }");
        syncEvent.WaitOne();
    }

    [Test]
    public void TestTransportLogsCommands()
    {
        List<LogMessageEventArgs> logs = new();
        TestConnection connection = new();
        ProtocolTransport transport = new(TimeSpan.FromMilliseconds(100), connection);
        transport.LogMessage += (sender, e) =>
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