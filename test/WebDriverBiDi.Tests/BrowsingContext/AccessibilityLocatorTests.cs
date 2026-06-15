namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class AccessibilityLocatorTests
{
    [Fact]
    public void TestCanSerializeUsingBaseType()
    {
        Locator locator = new AccessibilityLocator();
        Assert.NotEmpty(JsonSerializer.Serialize(locator));
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
}
