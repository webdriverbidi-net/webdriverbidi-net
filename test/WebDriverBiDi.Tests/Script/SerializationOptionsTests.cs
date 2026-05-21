namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SerializationOptionsTests
{
    [Fact]
    public void TestCanSerializeOptions()
    {
        SerializationOptions options = new();
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(JTokenType.Object, serialized.Type);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeOptionsWithOptionalMaxDomDepth()
    {
        SerializationOptions options = new()
        {
            MaxDomDepth = 1
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(JTokenType.Object, serialized.Type);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("maxDomDepth"));
        JToken? maxDomDepth = serialized["maxDomDepth"];
        Assert.NotNull(maxDomDepth);
        Assert.Equal(JTokenType.Integer, maxDomDepth.Type);
        Assert.Equal(1L, maxDomDepth.Value<long>());
    }

    [Fact]
    public void TestCanSerializeOptionsWithOptionalInfiniteMaxDomDepth()
    {
        SerializationOptions options = new()
        {
            MaxDomDepth = SerializationOptions.InfiniteMaxDomDepth
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(JTokenType.Object, serialized.Type);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("maxDomDepth"));
        JToken? maxDomDepth = serialized["maxDomDepth"];
        Assert.NotNull(maxDomDepth);
        Assert.Equal(JTokenType.Null, maxDomDepth.Type);
    }

    [Fact]
    public void TestCanSerializeOptionsWithOptionalMaxObjectDepth()
    {
        SerializationOptions options = new()
        {
            MaxObjectDepth = 1
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(JTokenType.Object, serialized.Type);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("maxObjectDepth"));
        JToken? maxObjectDepth = serialized["maxObjectDepth"];
        Assert.NotNull(maxObjectDepth);
        Assert.Equal(JTokenType.Integer, maxObjectDepth.Type);
        Assert.Equal(1L, maxObjectDepth.Value<long>());
    }

    [Fact]
    public void TestCanSerializeOptionsWithOptionalIncludeShadowTree()
    {
        SerializationOptions options = new()
        {
            IncludeShadowTree = IncludeShadowTreeSerializationOption.None
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(JTokenType.Object, serialized.Type);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("includeShadowTree"));
        JToken? includeShadowTree = serialized["includeShadowTree"];
        Assert.NotNull(includeShadowTree);
        Assert.Equal(JTokenType.String, includeShadowTree.Type);
        Assert.Equal("none", includeShadowTree.Value<string>());
    }

    [Fact]
    public void TestCanSerializeOptionsWithOptionalIncludeOpenShadowTree()
    {
        SerializationOptions options = new()
        {
            IncludeShadowTree = IncludeShadowTreeSerializationOption.Open
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(JTokenType.Object, serialized.Type);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("includeShadowTree"));
        JToken? includeShadowTree = serialized["includeShadowTree"];
        Assert.NotNull(includeShadowTree);
        Assert.Equal(JTokenType.String, includeShadowTree.Type);
        Assert.Equal("open", includeShadowTree.Value<string>());
    }

    [Fact]
    public void TestCanSerializeOptionsWithOptionalIncludeAllShadowTree()
    {
        SerializationOptions options = new()
        {
            IncludeShadowTree = IncludeShadowTreeSerializationOption.All
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(JTokenType.Object, serialized.Type);
        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("includeShadowTree"));
        JToken? includeShadowTree = serialized["includeShadowTree"];
        Assert.NotNull(includeShadowTree);
        Assert.Equal(JTokenType.String, includeShadowTree.Type);
        Assert.Equal("all", includeShadowTree.Value<string>());
    }
}
