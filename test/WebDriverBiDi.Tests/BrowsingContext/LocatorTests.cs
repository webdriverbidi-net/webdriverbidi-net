namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class LocatorTests
{
    [Fact]
    public void TestCanSerializeUsingBaseType()
    {
        Locator locator = new CssLocator("locator");
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
        locator = new XPathLocator("//locator");
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
        locator = new InnerTextLocator("locator text");
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
        locator = new AccessibilityLocator();
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
        locator = new ContextLocator("myContext");
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
    }

    [Fact]
    public void TestCanSerializeCssLocator()
    {
        CssLocator value = new(".selector");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("css", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal(".selector", valueProperty.Value<string>());
    }

    [Fact]
    public void TestCanSerializeXPathLocator()
    {
        XPathLocator value = new("//selector");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("xpath", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.String, valueProperty.Type);
        Assert.Equal("//selector", valueProperty.Value<string>());
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

    [Fact]
    public void TestCanSerializeAccessibilityLocator()
    {
        AccessibilityLocator value = new();

        Assert.Null(value.Name);
        Assert.Null(value.Role);

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("accessibility", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.Object, valueProperty.Type);

        JObject? accessibilityValue = valueProperty as JObject;
        Assert.NotNull(accessibilityValue);
        Assert.Empty(accessibilityValue);
    }

    [Fact]
    public void TestCanSerializeAccessibilityWithNameAndRole()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };

        Assert.Equal("accessibleName", value.Name);
        Assert.Equal("accessibleRole", value.Role);

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("accessibility", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.Object, valueProperty.Type);

        JObject? accessibilityValue = valueProperty as JObject;
        Assert.NotNull(accessibilityValue);
        Assert.Equal(2, accessibilityValue.Count);

        Assert.True(accessibilityValue.ContainsKey("name"));
        JToken? accessibilityName = accessibilityValue["name"];
        Assert.NotNull(accessibilityName);
        Assert.Equal(JTokenType.String, accessibilityName.Type);
        Assert.Equal("accessibleName", accessibilityName.Value<string>());

        Assert.True(accessibilityValue.ContainsKey("role"));
        JToken? accessibilityRole = accessibilityValue["role"];
        Assert.NotNull(accessibilityRole);
        Assert.Equal(JTokenType.String, accessibilityRole.Type);
        Assert.Equal("accessibleRole", accessibilityRole.Value<string>());
    }

    [Fact]
    public void TestCanSerializeAccessibilityWithName()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName"
        };

        Assert.Equal("accessibleName", value.Name);
        Assert.Null(value.Role);

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("accessibility", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.Object, valueProperty.Type);

        JObject? accessibilityValue = valueProperty as JObject;
        Assert.NotNull(accessibilityValue);
        Assert.Single(accessibilityValue);

        Assert.True(accessibilityValue.ContainsKey("name"));
        JToken? accessibilityName = accessibilityValue["name"];
        Assert.NotNull(accessibilityName);
        Assert.Equal(JTokenType.String, accessibilityName.Type);
        Assert.Equal("accessibleName", accessibilityName.Value<string>());
    }

    [Fact]
    public void TestCanSerializeAccessibilityWithRole()
    {
        AccessibilityLocator value = new()
        {
            Role = "accessibleRole"
        };

        Assert.Null(value.Name);
        Assert.Equal("accessibleRole", value.Role);

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("accessibility", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.Object, valueProperty.Type);

        JObject? accessibilityValue = valueProperty as JObject;
        Assert.NotNull(accessibilityValue);
        Assert.Single(accessibilityValue);

        Assert.True(accessibilityValue.ContainsKey("role"));
        JToken? accessibilityRole = accessibilityValue["role"];
        Assert.NotNull(accessibilityRole);
        Assert.Equal(JTokenType.String, accessibilityRole.Type);
        Assert.Equal("accessibleRole", accessibilityRole.Value<string>());
    }

    [Fact]
    public void TestCanSerializeAccessibilityWithRemovingName()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };
        value.Name = null;
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("accessibility", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.Object, valueProperty.Type);

        JObject? accessibilityValue = valueProperty as JObject;
        Assert.NotNull(accessibilityValue);
        Assert.Single(accessibilityValue);

        Assert.True(accessibilityValue.ContainsKey("role"));
        JToken? accessibilityRole = accessibilityValue["role"];
        Assert.NotNull(accessibilityRole);
        Assert.Equal(JTokenType.String, accessibilityRole.Type);
        Assert.Equal("accessibleRole", accessibilityRole.Value<string>());
    }

    [Fact]
    public void TestCanSerializeAccessibilityWithRemovingRole()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };
        value.Role = null;
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("accessibility", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.Object, valueProperty.Type);

        JObject? accessibilityValue = valueProperty as JObject;
        Assert.NotNull(accessibilityValue);
        Assert.Single(accessibilityValue);

        Assert.True(accessibilityValue.ContainsKey("name"));
        JToken? accessibilityName = accessibilityValue["name"];
        Assert.NotNull(accessibilityName);
        Assert.Equal(JTokenType.String, accessibilityName.Type);
        Assert.Equal("accessibleName", accessibilityName.Value<string>());
    }

    [Fact]
    public void TestCanSerializeAccessibilityWithRemovingNameAndRole()
    {
        AccessibilityLocator value = new()
        {
            Name = "accessibleName",
            Role = "accessibleRole"
        };
        value.Name = null;
        value.Role = null;
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("accessibility", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
        Assert.Equal(JTokenType.Object, valueProperty.Type);

        JObject? accessibilityValue = valueProperty as JObject;
        Assert.NotNull(accessibilityValue);
        Assert.Empty(accessibilityValue);
    }

    [Fact]
    public void TestCanSerializeContextLocator()
    {
        ContextLocator value = new("myContext");
        Assert.Equal("myContext", value.BrowsingContextId);

        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);
        Assert.True(parsed.ContainsKey("type"));
        JToken? typeProperty = parsed["type"];
        Assert.NotNull(typeProperty);
        Assert.Equal(JTokenType.String, typeProperty.Type);
        Assert.Equal("context", typeProperty.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueProperty = parsed["value"];
        Assert.NotNull(valueProperty);
 
        JObject? contextValue = valueProperty as JObject;
        Assert.NotNull(contextValue);
        Assert.Single(contextValue);
        Assert.True(contextValue.ContainsKey("context"));

        JToken? context = contextValue["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());
    }
}
