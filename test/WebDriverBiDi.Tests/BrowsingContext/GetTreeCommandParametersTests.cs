namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class GetTreeCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        GetTreeCommandParameters properties = new();
        Assert.Equal("browsingContext.getTree", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        GetTreeCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeParametersWithMaxDepth()
    {
        GetTreeCommandParameters properties = new()
        {
            MaxDepth = 2
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("maxDepth"));
        JToken? maxDepth = serialized["maxDepth"];
        Assert.NotNull(maxDepth);
        Assert.Equal(JTokenType.Integer, maxDepth.Type);
        Assert.Equal(2L, maxDepth.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithRoot()
    {
        GetTreeCommandParameters properties = new()
        {
            RootBrowsingContextId = "rootBrowsingContext"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("root"));
        JToken? root = serialized["root"];
        Assert.NotNull(root);
        Assert.Equal(JTokenType.String, root.Type);
        Assert.Equal("rootBrowsingContext", root.Value<string>());
    }
}
