namespace WebDriverBiDi.Script;

using System.Text.Json;

public class BooleanRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeBooleanRemoteValueWithTrue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Boolean, result.Type);
        Assert.True(result.Value);
    }

    [Fact]
    public void TestCanDeserializeBooleanRemoteValueWithFalse()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": false
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Boolean, result.Type);
        Assert.False(result.Value);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("boolean", argumentLocalValue.Type);
        Assert.IsType<bool>(argumentLocalValue.Value);
        Assert.True((bool)argumentLocalValue.Value);
    }

    [Fact]
    public void TestCanUseImplicitConversionToBool()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.NotNull(result);
        bool boolValue = result;
        Assert.True(boolValue);
    }

    [Fact]
    public void TestDeserializingBooleanRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "boolean"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BooleanRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingBooleanRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BooleanRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingBooleanRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-boolean",
                        "value": true
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BooleanRemoteValue>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);
        Assert.NotNull(result);
        BooleanRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
