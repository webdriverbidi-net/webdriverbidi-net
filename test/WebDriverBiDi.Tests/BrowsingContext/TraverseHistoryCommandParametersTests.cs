namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class TraverseHistoryCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", 0);
        Assert.Equal("browsingContext.traverseHistory", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", 0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("delta"));
        JToken? delta = serialized["delta"];
        Assert.NotNull(delta);
        Assert.Equal(JTokenType.Integer, delta.Type);
        Assert.Equal(0L, delta.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithPositiveDelta()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", 1);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("delta"));
        JToken? delta = serialized["delta"];
        Assert.NotNull(delta);
        Assert.Equal(JTokenType.Integer, delta.Type);
        Assert.Equal(1L, delta.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNegativeDelta()
    {
        TraverseHistoryCommandParameters properties = new("myContextId", -1);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("delta"));
        JToken? delta = serialized["delta"];
        Assert.NotNull(delta);
        Assert.Equal(JTokenType.Integer, delta.Type);
        Assert.Equal(-1L, delta.Value<long>());
    }
}
