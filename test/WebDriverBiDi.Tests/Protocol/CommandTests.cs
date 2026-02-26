namespace WebDriverBiDi.Protocol;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class CommandTests
{
    [Test]
    public void TestCanSerializeCommand()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        string json = JsonSerializer.Serialize(command);
        Dictionary<string, object?> dataValue = JObject.Parse(json).ToParsedDictionary();
        Assert.That(dataValue, Is.EquivalentTo(expected));
    }

    [Test]
    public void TestCannotDeserializeCommand()
    {
        string json = """
                      {
                        "id": 1,
                        "method": "module.command",
                        "params": {
                          "paramName": "paramValue"
                        }
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Command>(json), Throws.InstanceOf<NotImplementedException>());
    }

    [Test]
    public void TestCommandResultType()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        Assert.That(command.ResponseType, Is.EqualTo(typeof(CommandResponseMessage<TestCommandResult>)));
    }

    [Test]
    public async Task TestCommandResult()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        Assert.That(command.Result, Is.Null);
        Assert.That(command.ThrownException, Is.Null);
        TestCommandResult result = new();
        command.Result = result;
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));
        Assert.That(command.Result, Is.Not.Null);
        Assert.That(command.Result, Is.InstanceOf<TestCommandResult>());
        Assert.That(command.ThrownException, Is.Null);
        Assert.That(command.Result, Is.EqualTo(result with { }));
    }

    [Test]
    public async Task TestCommandThrownException()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        Assert.That(command.Result, Is.Null);
        Assert.That(command.ThrownException, Is.Null);
        command.ThrownException = new WebDriverBiDiException("test exception");
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));
        Assert.That(command.Result, Is.Null);
        Assert.That(command.ThrownException, Is.Not.Null);
        Assert.That(command.ThrownException, Is.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo("test exception"));
    }

    [Test]
    public async Task TestCommandCancel()
    {
        string commandName = "module.command";
        Dictionary<string, object?> expectedCommandParameters = new()
        {
            { "parameterName", "parameterValue" },
        };
        Dictionary<string, object?> expected = new()
        {
            { "id", 1 },
            { "method", commandName },
            { "params", expectedCommandParameters },
            { "overflowParameterName", "overflowParameterValue" },
        };

        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        commandParams.AdditionalData["overflowParameterName"] = "overflowParameterValue";

        Command command = new(1, commandParams);
        Assert.That(command.Result, Is.Null);
        Assert.That(command.ThrownException, Is.Null);
        Assert.That(command.IsCanceled, Is.False);
        command.Cancel();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));
        Assert.That(command.Result, Is.Null);
        Assert.That(command.ThrownException, Is.Null);
        Assert.That(command.IsCanceled, Is.True);
    }

    [Test]
    public async Task TestSettingNullResultFaultsCommand()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        command.Result = null;
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));

        Assert.That(command.Result, Is.Null);
        Assert.That(command.ThrownException, Is.Not.Null);
        Assert.That(command.ThrownException, Is.InstanceOf<WebDriverBiDiException>());
        Assert.That(command.ThrownException!.Message, Does.Contain("Attempted to set a null result"));
        Assert.That(command.ThrownException!.Message, Does.Contain(commandName));
        Assert.That(command.IsCanceled, Is.False);
    }

    [Test]
    public async Task TestSettingResultAfterAlreadyCompletedDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        TestCommandResult firstResult = new();
        command.Result = firstResult;
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));

        Assert.That(() => command.Result = new TestCommandResult(), Throws.Nothing);
        Assert.That(command.Result, Is.EqualTo(firstResult with { }));
    }

    [Test]
    public async Task TestSettingExceptionAfterAlreadyCompletedDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        TestCommandResult result = new();
        command.Result = result;
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));

        Assert.That(() => command.ThrownException = new WebDriverBiDiException("late exception"), Throws.Nothing);
        Assert.That(command.Result, Is.Not.Null);
        Assert.That(command.ThrownException, Is.Null);
    }

    [Test]
    public async Task TestCancelAfterAlreadyCompletedDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        TestCommandResult result = new();
        command.Result = result;
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));

        Assert.That(() => command.Cancel(), Throws.Nothing);
        Assert.That(command.Result, Is.Not.Null);
        Assert.That(command.IsCanceled, Is.False);
    }

    [Test]
    public async Task TestSettingResultAfterCancelDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        command.Cancel();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));

        Assert.That(() => command.Result = new TestCommandResult(), Throws.Nothing);
        Assert.That(command.Result, Is.Null);
        Assert.That(command.IsCanceled, Is.True);
    }

    [Test]
    public async Task TestSettingExceptionAfterCancelDoesNotThrow()
    {
        string commandName = "module.command";
        TestCommandParameters commandParams = new TestCommandParameters(commandName);
        Command command = new(1, commandParams);

        command.Cancel();
        await command.WaitForCompletionAsync(TimeSpan.FromMilliseconds(50));

        Assert.That(() => command.ThrownException = new WebDriverBiDiException("late exception"), Throws.Nothing);
        Assert.That(command.ThrownException, Is.Null);
        Assert.That(command.IsCanceled, Is.True);
    }
}
