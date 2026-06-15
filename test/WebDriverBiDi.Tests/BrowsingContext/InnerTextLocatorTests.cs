namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class InnerTextLocatorTests
{
    [Fact]
    public void TestCanSerializeUsingBaseType()
    {
        Locator locator = new InnerTextLocator("locator text");
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
    }

    [Fact]
    public void TestCanSerializeInnerTextLocator()
    {
        InnerTextLocator value = new("text to locate");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("innerText", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("text to locate", valueProperty.Value<string>());
    }

    [Fact]
    public void TestCanSerializeInnerTextLocatorWithMaxDepthZero()
    {
        InnerTextLocator value = new("text to locate")
        {
            MaxDepth = 0
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(3, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("innerText", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("text to locate", valueProperty.Value<string>());

        Assert.True(parsed.ContainsKey("maxDepth"));
        JToken? maxDepthProperty = parsed["maxDepth"];
        Assert.NotNull(maxDepthProperty);
        Assert.Equal(JTokenType.Integer, maxDepthProperty.Type);
        Assert.Equal(0UL, maxDepthProperty.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeInnerTextLocatorWithMaxDepthNonZero()
    {
        InnerTextLocator value = new("text to locate")
        {
            MaxDepth = 10
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(3, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("innerText", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("text to locate", valueProperty.Value<string>());

        Assert.True(parsed.ContainsKey("maxDepth"));
        JToken? maxDepthProperty = parsed["maxDepth"];
        Assert.NotNull(maxDepthProperty);
        Assert.Equal(JTokenType.Integer, maxDepthProperty.Type);
        Assert.Equal(10UL, maxDepthProperty.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeInnerTextLocatorWithIgnoreCaseTrue()
    {
        InnerTextLocator value = new("text to locate")
        {
            IgnoreCase = true
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(3, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("innerText", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("text to locate", valueProperty.Value<string>());

        Assert.True(parsed.ContainsKey("ignoreCase"));
        JToken? ignoreCaseProperty = parsed["ignoreCase"];
        Assert.NotNull(ignoreCaseProperty);
        Assert.Equal(JTokenType.Boolean, ignoreCaseProperty.Type);
        Assert.True(ignoreCaseProperty.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeInnerTextLocatorWithIgnoreCaseFalse()
    {
        InnerTextLocator value = new("text to locate")
        {
            IgnoreCase = false
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(3, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("innerText", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("text to locate", valueProperty.Value<string>());

        Assert.True(parsed.ContainsKey("ignoreCase"));
        JToken? ignoreCaseProperty = parsed["ignoreCase"];
        Assert.NotNull(ignoreCaseProperty);
        Assert.Equal(JTokenType.Boolean, ignoreCaseProperty.Type);
        Assert.False(ignoreCaseProperty.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeInnerTextLocatorWithMatchTypeFull()
    {
        InnerTextLocator value = new("text to locate")
        {
            MatchType = InnerTextMatchType.Full
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(3, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("innerText", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("text to locate", valueProperty.Value<string>());

        Assert.True(parsed.ContainsKey("matchType"));
        JToken? matchTypeProperty = parsed["matchType"];
        Assert.NotNull(matchTypeProperty);
        Assert.Equal(JTokenType.String, matchTypeProperty.Type);
        Assert.Equal("full", matchTypeProperty.Value<string>());
    }

    [Fact]
    public void TestCanSerializeInnerTextLocatorWithMatchTypePartial()
    {
        InnerTextLocator value = new("text to locate")
        {
            MatchType = InnerTextMatchType.Partial
        };
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(3, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("innerText", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("text to locate", valueProperty.Value<string>());

        Assert.True(parsed.ContainsKey("matchType"));
        JToken? matchTypeProperty = parsed["matchType"];
        Assert.NotNull(matchTypeProperty);
        Assert.Equal(JTokenType.String, matchTypeProperty.Type);
        Assert.Equal("partial", matchTypeProperty.Value<string>());
    }
}
