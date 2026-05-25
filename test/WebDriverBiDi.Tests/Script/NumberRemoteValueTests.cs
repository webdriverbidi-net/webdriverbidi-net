namespace WebDriverBiDi.Script;

using System.Text.Json;

public class NumberRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeNumberRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Number, result.Type);
        Assert.Equal(3.14159, result.Value);
    }

    [Fact]
    public void TestCanDeserializeNumberRemoteValueWithInteger()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Number, result.Type);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("number", argumentLocalValue.Type);
        Assert.IsType<double>(argumentLocalValue.Value);
        Assert.Equal(3.14159, argumentLocalValue.Value);
    }

    [Fact]
    public void TestCanConvertToLong()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(42, result.ToLong());
    }

    [Fact]
    public void TestCanConvertToInt()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(42, result.ToInt());
    }

    [Fact]
    public void TestCanUseImplicitConversionToInt()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        int intValue = result;
        Assert.Equal(42, intValue);
    }

    [Fact]
    public void TestCanUseImplicitConversionToLong()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        long longValue = result;
        Assert.Equal(42, longValue);
    }

    [Fact]
    public void TestCanUseImplicitConversionToDouble()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        double doubleValue = result;
        Assert.Equal(42, doubleValue);
    }

    [Fact]
    public void TestDeserializingNumberRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "number"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NumberRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingNumberRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "number",
                        "value": {}
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NumberRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingNumberRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-number",
                        "value": 3.14159
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NumberRemoteValue>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);
        Assert.NotNull(result);
        NumberRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
