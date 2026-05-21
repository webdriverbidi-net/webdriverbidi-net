namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ReleaseActionsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        ReleaseActionsCommandParameters properties = new("myContextId");
        Assert.Equal("input.releaseActions", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        ReleaseActionsCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());
    }
}
