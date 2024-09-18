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
        string json = @"{ ""id"": 1, ""method"": ""module.command"", ""params"": { ""paramName"": ""paramValue"" }}";
        Assert.That(() => JsonSerializer.Deserialize<Command>(json), Throws.InstanceOf<NotImplementedException>());
    }
}