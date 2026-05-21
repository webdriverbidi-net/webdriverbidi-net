namespace WebDriverBiDi.Network;

using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;

public class BytesValueTests
{
    [Fact]
    public void TestCanSerializeStringValue()
    {
        BytesValue value = BytesValue.FromString("this is my string");
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("string", type.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.String, valueToken.Type);
        Assert.Equal("this is my string", valueToken.Value<string>());
    }

    [Fact]
    public void TestCanSerializeBase64Value()
    {
        string base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes("this is my string"));
        BytesValue value = BytesValue.FromBase64String(base64String);
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("base64", type.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.String, valueToken.Type);
        Assert.Equal(base64String, valueToken.Value<string>());
    }

    [Fact]
    public void TestCanSerializeBase64ValueFromByteArray()
    {
        byte[] byteArray = Encoding.UTF8.GetBytes("this is my string");
        string base64String = Convert.ToBase64String(byteArray);
        BytesValue value = BytesValue.FromByteArray(byteArray);
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("base64", type.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.String, valueToken.Type);
        Assert.Equal(base64String, valueToken.Value<string>());
    }

    [Fact]
    public void TestCanDeserializeStringValue()
    {
        string stringValue = "this is my string";
        byte[] valueArray = Encoding.UTF8.GetBytes(stringValue);
        string json = $$"""
                      {
                        "type": "string",
                        "value": "{{stringValue}}"
                      }
                      """;
        BytesValue? value = JsonSerializer.Deserialize<BytesValue>(json);
        Assert.NotNull(value);

        Assert.Equal(BytesValueType.String, value.Type);
        Assert.Equal("this is my string", value.Value);
        Assert.Equal(valueArray, value.ValueAsByteArray);
    }

    [Fact]
    public void TestCanDeserializeBase64Value()
    {
        // Disable spell checking only for the base64-encoded value.
        // cspell: disable-next
        string base64Value = "dGhpcyBpcyBteSBzdHJpbmc=";
        byte[] valueArray = Convert.FromBase64String(base64Value);
        string json = $$"""
                      {
                        "type": "base64",
                        "value": "{{base64Value}}"
                      }
                      """;
        BytesValue? value = JsonSerializer.Deserialize<BytesValue>(json);
        Assert.NotNull(value);

        Assert.Equal(BytesValueType.Base64, value.Type);

        // Disable spell checking only for the base64-encoded value.
        // cspell: disable-next
        Assert.Equal("dGhpcyBpcyBteSBzdHJpbmc=", value.Value);
        Assert.Equal(valueArray, value.ValueAsByteArray);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string stringValue = "this is my string";
        byte[] valueArray = Encoding.UTF8.GetBytes(stringValue);
        string json = $$"""
                      {
                        "type": "string",
                        "value": "{{stringValue}}"
                      }
                      """;
        BytesValue? value = JsonSerializer.Deserialize<BytesValue>(json);
        Assert.NotNull(value);
        BytesValue copy = value with { };
        Assert.Equal(value, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingTypeThrows()
    {
        string json = """
                      {
                        "value": "this is my string"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BytesValue>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidTypeValueThrows()
    {
        string json = $$"""
                      {
                        "type": "invalid",
                        "value": "this is my string"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BytesValue>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidTypeThrows()
    {
        string json = $$"""
                      {
                        "type": [],
                        "value": "this is my string"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BytesValue>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingValueThrows()
    {
        string json = $$"""
                      {
                        "type": "string"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BytesValue>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidValueThrows()
    {
        string json = $$"""
                      {
                        "type": "base64",
                        "value": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BytesValue>(json));
    }
}
