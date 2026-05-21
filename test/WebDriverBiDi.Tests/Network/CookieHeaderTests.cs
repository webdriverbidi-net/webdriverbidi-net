namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CookieHeaderTests
{
    [Fact]
    public void TestCanSerialize()
    {
        CookieHeader cookieHeader = new();
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal(string.Empty, name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal(string.Empty, valueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithConstructorValues()
    {
        CookieHeader cookieHeader = new("cookieName", "cookieValue");
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithPropertySetValues()
    {
        CookieHeader cookieHeader = new()
        {
            Name = "cookieName",
            Value = BytesValue.FromString("cookieValue")
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithPropertySetAndBynaryValue()
    {
        byte[] cookieValue = new byte[] { 0x41, 0x42, 0x43 };
        string base64Value = Convert.ToBase64String(cookieValue);
        CookieHeader cookieHeader = new()
        {
            Name = "cookieName",
            Value = BytesValue.FromByteArray(cookieValue)
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("base64", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal(base64Value, valueValue.Value<string>());
    }
}
