namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PerformActionsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        PerformActionsCommandParameters properties = new("myContextId");
        Assert.Equal("input.performActions", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        PerformActionsCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);
    }
}
