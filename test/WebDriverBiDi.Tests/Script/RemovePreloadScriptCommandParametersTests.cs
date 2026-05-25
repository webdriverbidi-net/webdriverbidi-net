namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class RemovePreloadScriptCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        RemovePreloadScriptCommandParameters properties = new("myLoadScriptId");
        Assert.Equal("script.removePreloadScript", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeProperties()
    {
        RemovePreloadScriptCommandParameters properties = new("myLoadScriptId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("script"));
        JToken? script = serialized["script"];
        Assert.NotNull(script);
        Assert.Equal(JTokenType.String, script.Type);
        Assert.Equal("myLoadScriptId", script.Value<string>());
    }
}
