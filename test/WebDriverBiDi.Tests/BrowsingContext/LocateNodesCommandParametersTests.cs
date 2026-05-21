namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

public class LocateNodesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"));
        Assert.Equal("browsingContext.locateNodes", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("locator"));
        JToken? locatorToken = serialized["locator"];
        Assert.NotNull(locatorToken);
        Assert.Equal(JTokenType.Object, locatorToken.Type);
        JObject? locator = locatorToken.Value<JObject>();
        Assert.NotNull(locator);

        Assert.True(locator.ContainsKey("type"));
        JToken? locatorType = locator["type"];
        Assert.NotNull(locatorType);
        Assert.Equal(JTokenType.String, locatorType.Type);
        Assert.Equal("css", locatorType.Value<string>());

        Assert.True(locator.ContainsKey("value"));
        JToken? locatorValue = locator["value"];
        Assert.NotNull(locatorValue);
        Assert.Equal(JTokenType.String, locatorValue.Type);
        Assert.Equal(".selector", locatorValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithMaxNodeCount()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"))
        {
            MaxNodeCount = 10
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("locator"));
        JToken? locatorToken = serialized["locator"];
        Assert.NotNull(locatorToken);
        Assert.Equal(JTokenType.Object, locatorToken.Type);
        JObject? locator = locatorToken.Value<JObject>();
        Assert.NotNull(locator);

        Assert.True(locator.ContainsKey("type"));
        JToken? locatorType = locator["type"];
        Assert.NotNull(locatorType);
        Assert.Equal(JTokenType.String, locatorType.Type);
        Assert.Equal("css", locatorType.Value<string>());

        Assert.True(locator.ContainsKey("value"));
        JToken? locatorValue = locator["value"];
        Assert.NotNull(locatorValue);
        Assert.Equal(JTokenType.String, locatorValue.Type);
        Assert.Equal(".selector", locatorValue.Value<string>());

        Assert.True(serialized.ContainsKey("maxNodeCount"));
        JToken? maxNodeCount = serialized["maxNodeCount"];
        Assert.NotNull(maxNodeCount);
        Assert.Equal(JTokenType.Integer, maxNodeCount.Type);
        Assert.Equal(10UL, maxNodeCount.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeWithSerializationOptions()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"))
        {
            SerializationOptions = new()
            {
                IncludeShadowTree = IncludeShadowTreeSerializationOption.All,
                MaxDomDepth = 10,
                MaxObjectDepth = 0,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("locator"));
        JToken? locatorToken = serialized["locator"];
        Assert.NotNull(locatorToken);
        Assert.Equal(JTokenType.Object, locatorToken.Type);
        JObject? locator = locatorToken.Value<JObject>();
        Assert.NotNull(locator);

        Assert.True(locator.ContainsKey("type"));
        JToken? locatorType = locator["type"];
        Assert.NotNull(locatorType);
        Assert.Equal(JTokenType.String, locatorType.Type);
        Assert.Equal("css", locatorType.Value<string>());

        Assert.True(locator.ContainsKey("value"));
        JToken? locatorValue = locator["value"];
        Assert.NotNull(locatorValue);
        Assert.Equal(JTokenType.String, locatorValue.Type);
        Assert.Equal(".selector", locatorValue.Value<string>());

        Assert.True(serialized.ContainsKey("serializationOptions"));
        JToken? serializationOptionsToken = serialized["serializationOptions"];
        Assert.NotNull(serializationOptionsToken);
        Assert.Equal(JTokenType.Object, serializationOptionsToken.Type);
        JObject? serializationOptions = serializationOptionsToken.Value<JObject>();
        Assert.NotNull(serializationOptions);
        Assert.Equal(3, serializationOptions.Count);

        Assert.True(serializationOptions.ContainsKey("includeShadowTree"));
        JToken? includeShadowTree = serializationOptions["includeShadowTree"];
        Assert.NotNull(includeShadowTree);
        Assert.Equal(JTokenType.String, includeShadowTree.Type);
        Assert.Equal("all", includeShadowTree.Value<string>());

        Assert.True(serializationOptions.ContainsKey("maxDomDepth"));
        JToken? maxDomDepth = serializationOptions["maxDomDepth"];
        Assert.NotNull(maxDomDepth);
        Assert.Equal(JTokenType.Integer, maxDomDepth.Type);
        Assert.Equal(10UL, maxDomDepth.Value<ulong>());

        Assert.True(serializationOptions.ContainsKey("maxObjectDepth"));
        JToken? maxObjectDepth = serializationOptions["maxObjectDepth"];
        Assert.NotNull(maxObjectDepth);
        Assert.Equal(JTokenType.Integer, maxObjectDepth.Type);
        Assert.Equal(0UL, maxObjectDepth.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeWithStartNode()
    {
        string nodeJson = @"{ ""type"": ""node"", ""sharedId"": ""mySharedId"", ""value"": { ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0 } }";
        RemoteValue? nodeValue = JsonSerializer.Deserialize<RemoteValue>(nodeJson);
        Assert.NotNull(nodeValue);
        NodeRemoteValue? nodeRemoteValue = nodeValue as NodeRemoteValue;
        Assert.NotNull(nodeRemoteValue);
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"));
        properties.StartNodes.Add(nodeRemoteValue.ToSharedReference());

        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("locator"));
        JToken? locatorToken = serialized["locator"];
        Assert.NotNull(locatorToken);
        Assert.Equal(JTokenType.Object, locatorToken.Type);
        JObject? locator = locatorToken.Value<JObject>();
        Assert.NotNull(locator);

        Assert.True(locator.ContainsKey("type"));
        JToken? locatorType = locator["type"];
        Assert.NotNull(locatorType);
        Assert.Equal(JTokenType.String, locatorType.Type);
        Assert.Equal("css", locatorType.Value<string>());

        Assert.True(locator.ContainsKey("value"));
        JToken? locatorValue = locator["value"];
        Assert.NotNull(locatorValue);
        Assert.Equal(JTokenType.String, locatorValue.Type);
        Assert.Equal(".selector", locatorValue.Value<string>());

        Assert.True(serialized.ContainsKey("startNodes"));
        JToken? startNodesToken = serialized["startNodes"];
        Assert.NotNull(startNodesToken);
        Assert.Equal(JTokenType.Array, startNodesToken.Type);
        JArray? startNodes = startNodesToken as JArray;
        Assert.NotNull(startNodes);
        Assert.Single(startNodes);
    }
}
