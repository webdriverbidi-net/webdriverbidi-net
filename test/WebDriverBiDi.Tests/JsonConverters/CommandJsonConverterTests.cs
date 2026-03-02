namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

[TestFixture]
public class CommandJsonConverterTests
{
    [Test]
    public void TestReadThrowsNotImplementedException()
    {
        string json = """
                      {
                        "id": 1,
                        "method": "module.command",
                        "params": { "parameterName": "parameterValue" }
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Command>(json), Throws.InstanceOf<NotImplementedException>());
    }

    [Test]
    public void TestWriteSerializesCommandWithIdMethodAndParams()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        Command command = new(1, commandParams);
        string json = JsonSerializer.Serialize(command);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized["id"]!.Value<long>(), Is.EqualTo(1));
        Assert.That(serialized["method"]!.Value<string>(), Is.EqualTo("module.command"));
        Assert.That(serialized["params"], Is.Not.Null);
        Assert.That(serialized["params"]!["parameterName"]!.Value<string>(), Is.EqualTo("parameterValue"));
    }

    [Test]
    public void TestWriteIncludesAdditionalDataInOutput()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        commandParams.AdditionalData["extraKey"] = "extraValue";
        Command command = new(1, commandParams);
        string json = JsonSerializer.Serialize(command);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Contains.Key("extraKey"));
        Assert.That(serialized["extraKey"]!.Value<string>(), Is.EqualTo("extraValue"));
    }

    [Test]
    public void TestWriteWithNullValueThrowsArgumentNullException()
    {
        CommandJsonConverter converter = new();
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        Assert.That(() => converter.Write(writer, null!, new JsonSerializerOptions()), Throws.InstanceOf<ArgumentNullException>());
    }
}
