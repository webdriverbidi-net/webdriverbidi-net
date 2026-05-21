namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Protocol;
using WebDriverBiDi.TestUtilities;

public class CommandJsonConverterTests
{
    [Fact]
    public void TestReadThrowsNotImplementedException()
    {
        string json = """
                      {
                        "id": 1,
                        "method": "module.command",
                        "params": { "parameterName": "parameterValue" }
                      }
                      """;
        Assert.ThrowsAny<NotImplementedException>(() => JsonSerializer.Deserialize<Command>(json));
    }

    [Fact]
    public void TestWriteSerializesCommandWithIdMethodAndParams()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        Command command = new(1, commandParams);
        string json = JsonSerializer.Serialize(command);
        JObject serialized = JObject.Parse(json);

        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(1L, id.Value<long>());

        JToken? method = serialized["method"];
        Assert.NotNull(method);
        Assert.Equal("module.command", method.Value<string>());

        Assert.NotNull(serialized["params"]);
        JToken? paramsToken = serialized["params"];
        Assert.NotNull(paramsToken);
        JToken? parameterName = paramsToken["parameterName"];
        Assert.NotNull(parameterName);
        Assert.Equal("parameterValue", parameterName.Value<string>());
    }

    [Fact]
    public void TestWriteIncludesAdditionalDataInOutput()
    {
        TestCommandParameters commandParams = new TestCommandParameters("module.command");
        commandParams.AdditionalData["extraKey"] = "extraValue";
        Command command = new(1, commandParams);
        string json = JsonSerializer.Serialize(command);
        JObject serialized = JObject.Parse(json);
        Assert.True(serialized.ContainsKey("extraKey"));
        JToken? extraKey = serialized["extraKey"];
        Assert.NotNull(extraKey);
        Assert.Equal("extraValue", extraKey.Value<string>());
    }

    [Fact]
    public void TestWriteWithNullValueThrowsArgumentNullException()
    {
        CommandJsonConverter converter = new();
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        Assert.ThrowsAny<ArgumentNullException>(() => converter.Write(writer, null!, new JsonSerializerOptions()));
    }
}
