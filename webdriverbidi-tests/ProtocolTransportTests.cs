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
        JToken commandParameters = JToken.Parse(@"{ ""parameterName"": ""parameterValue"" }");

        Dictionary<string, object?> expectedCommandParameters = new Dictionary<string, object?>()
        {
            { "parameterName", "parameterValue" }
        };
        Dictionary<string, object?> expected = new Dictionary<string, object?>()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters }
        };

        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.Zero, connection);
        await transport.Connect("ws://localhost:5555");
        long commandId = await transport.SendCommand(commandName, commandParameters);

        var dataValue = JObject.Parse(connection.DataSent ?? "").ToParsedDictionary();       
        Assert.That(dataValue, Is.EquivalentTo(expected));
    }

    [Test]
    public async Task TestTransportCanWaitForCommandComplete()
    {
        string commandName = "module.command";
        JToken commandParameters = JToken.Parse(@"{ ""parameterName"": ""parameterValue"" }");
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");
        long commandId = await transport.SendCommand(commandName, commandParameters);
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
        JToken commandParameters = JToken.Parse(@"{ ""parameterName"": ""parameterValue"" }");
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(10), connection);
        await transport.Connect("ws://localhost:5555");
        long commandId = await transport.SendCommand(commandName, commandParameters);
        Assert.That(() => transport.WaitForCommandComplete(1, TimeSpan.FromMilliseconds(50)), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("Timed out"));
    }

    [Test]
    public void TestTransportWaitForCommandCompleteWillErrorForInvalidId()
    {
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(10), connection);
        Assert.That(() => transport.WaitForCommandComplete(1, TimeSpan.FromMilliseconds(50)), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("Unknown command id"));
    }

    [Test]
    public async Task TestTransportCanGetResponse()
    {
        string commandName = "module.command";
        JToken commandParameters = JToken.Parse(@"{ ""parameterName"": ""parameterValue"" }");
        Dictionary<string, object?> expected = new Dictionary<string, object?>()
        {
            { "value", "response value" }
        };
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");
        long commandId = await transport.SendCommand(commandName, commandParameters);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""result"": { ""value"": ""response value"" } }");
        });
        transport.WaitForCommandComplete(1, TimeSpan.FromSeconds(250));
        var actualResult = transport.GetCommandResponse(1);
        Assert.That(actualResult.IsError, Is.False);
        Assert.That(actualResult.Result.ToParsedDictionary(), Is.EquivalentTo(expected));
    }

    [Test]
    public async Task TestTransportCanGetErrorResponse()
    {
        string commandName = "module.command";
        JToken commandParameters = JToken.Parse(@"{ ""parameterName"": ""parameterValue"" }");
        Dictionary<string, object?> expected = new Dictionary<string, object?>()
        {
            { "id", 1 },
            { "error", "unknown command" },
            { "message", "This is a test error message" }
        };
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(100), connection);
        await transport.Connect("ws://localhost:5555");
        long commandId = await transport.SendCommand(commandName, commandParameters);
        _ = Task.Run(() => 
        {
            Task.Delay(TimeSpan.FromMilliseconds(50));
            connection.RaiseDataReceivedEvent(@"{ ""id"": 1, ""error"": ""unknown command"", ""message"": ""This is a test error message"" }");
        });
        transport.WaitForCommandComplete(1, TimeSpan.FromSeconds(250));
        var actualResult = transport.GetCommandResponse(1);
        Assert.That(actualResult.IsError, Is.True);
        Assert.That(actualResult.Result.ToParsedDictionary(), Is.EquivalentTo(expected));
    }

    [Test]
    public void TestTransportGetResponseWillErrorForInvalidId()
    {
        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(100), connection);
        Assert.That(() => transport.GetCommandResponse(1), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("Could not remove command"));
    }

    [Test]
    public void TestTransportEventReceived()
    {
        string receivedName = string.Empty;
        JToken? receivedData = null;
        ManualResetEvent syncEvent = new ManualResetEvent(false);
        Dictionary<string, object?> expected = new Dictionary<string, object?>()
        {
            { "paramName", "paramValue" }
        };

        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(100), connection);
        transport.EventReceived += (object? sender, ProtocolEventReceivedEventArgs e) => {
            receivedName = e.EventName;
            receivedData = e.EventData;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(@"{ ""method"": ""protocol.event"", ""params"": { ""paramName"": ""paramValue"" } }");
        syncEvent.WaitOne();
        Assert.That(receivedName, Is.EqualTo("protocol.event"));
        Assert.That(receivedData!.ToParsedDictionary(), Is.Not.Null.And.EquivalentTo(expected));
    }

    [Test]
    public void TestTransportErrorEventReceived()
    {
        JToken? receivedData = null;
        ManualResetEvent syncEvent = new ManualResetEvent(false);
        Dictionary<string, object?> expected = new Dictionary<string, object?>()
        {
            { "id", null },
            { "error", "unknown error" },
            { "message", "This is a test error message" }
        };

        TestConnection connection = new TestConnection();
        ProtocolTransport transport = new ProtocolTransport(TimeSpan.FromMilliseconds(100), connection);
        transport.ErrorEventReceived += (object? sender, ProtocolErrorReceivedEventArgs e) => {
            receivedData = e.ErrorData;
            syncEvent.Set();
        };
        connection.RaiseDataReceivedEvent(@"{ ""id"": null, ""error"": ""unknown error"", ""message"": ""This is a test error message"" }");
        syncEvent.WaitOne();
        Assert.That(receivedData!.ToParsedDictionary(), Is.Not.Null.And.EquivalentTo(expected));
    }
}