namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json;

public class BigIntegerRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeBigIntegerRemoteValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        BigIntegerRemoteValue? result = JsonSerializer.Deserialize<BigIntegerRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.BigInt, result.Type);
        Assert.Equal(BigInteger.Parse("123456789012345678901234567890"), result.Value);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        BigIntegerRemoteValue? result = JsonSerializer.Deserialize<BigIntegerRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("bigint", argumentLocalValue.Type);
        Assert.IsType<BigInteger>(argumentLocalValue.Value);
        Assert.Equal(BigInteger.Parse("123456789012345678901234567890"), argumentLocalValue.Value);
    }

    [Fact]
    public void TestDeserializingBigIntegerRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "bigint"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingBigIntegerRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingBigIntegerRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingBigIntegerRemoteValueWithInvalidValueThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "not-a-number"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        BigIntegerRemoteValue? result = JsonSerializer.Deserialize<BigIntegerRemoteValue>(json);
        Assert.NotNull(result);
        BigIntegerRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
