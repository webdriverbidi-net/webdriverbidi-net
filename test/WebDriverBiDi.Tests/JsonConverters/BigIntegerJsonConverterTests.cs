namespace WebDriverBiDi.JsonConverters;

using System.Numerics;
using System.Text.Json;

public class BigIntegerJsonConverterTests
{
    [Fact]
    public void TestDeserializingValidBigInteger()
    {
        string json = "\"123456789012345678901234567890\"";
        BigInteger? result = JsonSerializer.Deserialize<BigInteger>(json, new JsonSerializerOptions { Converters = { new BigIntegerJsonConverter() } });
        Assert.Equal(BigInteger.Parse("123456789012345678901234567890"), result);
    }

    [Fact]
    public void TestDeserializingInvalidBigIntegerThrows()
    {
        string json = "\"not-a-big-integer\"";
        Assert.Equal($"Cannot parse invalid value 'not-a-big-integer' for bigint", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BigInteger>(json, new JsonSerializerOptions { Converters = { new BigIntegerJsonConverter() } })).Message);
    }

    [Fact]
    public void TestSerializationThrows()
    {
        BigInteger value = BigInteger.Parse("123456789012345678901234567890");
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Serialize(value, new JsonSerializerOptions { Converters = { new BigIntegerJsonConverter() } }));
    }
}
