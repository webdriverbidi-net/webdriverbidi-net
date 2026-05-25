namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ActivateCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        ActivateCommandParameters properties = new("myContextId");
        Assert.Equal("browsingContext.activate", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        ActivateCommandParameters properties = new("myContextId");
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
