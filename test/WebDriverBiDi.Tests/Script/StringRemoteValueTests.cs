namespace WebDriverBiDi.Script;

using System.Text.Json;

public class StringRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeStringRemoteValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.String, result.Type);
        Assert.Equal("my string value", result.Value);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("string", argumentLocalValue.Type);
        Assert.IsType<string>(argumentLocalValue.Value);
        Assert.Equal("my string value", argumentLocalValue.Value);
    }

    [Fact]
    public void TestCanUseImplicitConversionToString()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);

        Assert.NotNull(result);
        string stringValue = result;
        Assert.Equal("my string value", stringValue);
    }

    [Fact]
    public void TestDeserializingStringRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "string"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StringRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingStringRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "string",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StringRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingStringRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-string",
                        "value": "my string value"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<StringRemoteValue>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);
        Assert.NotNull(result);
        StringRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
