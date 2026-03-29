namespace WebDriverBiDi.JsonConverters;

using System.Numerics;
using System.Text.Json;

[TestFixture]
public class BigIntegerJsonConverterTests
{
    [Test]
    public void TestDeserializingValidBigInteger()
    {
        string json = "\"123456789012345678901234567890\"";
        BigInteger? result = JsonSerializer.Deserialize<BigInteger>(json, new JsonSerializerOptions { Converters = { new BigIntegerJsonConverter() } });
        Assert.That(result, Is.EqualTo(BigInteger.Parse("123456789012345678901234567890")));
    }

    [Test]
    public void TestDeserializingInvalidBigIntegerThrows()
    {
        string json = "\"not-a-big-integer\"";
        Assert.That(() => JsonSerializer.Deserialize<BigInteger>(json, new JsonSerializerOptions { Converters = { new BigIntegerJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.EqualTo($"Cannot parse invalid value 'not-a-big-integer' for bigint"));
    }

    [Test]
    public void TestSerializationThrows()
    {
        BigInteger value = BigInteger.Parse("123456789012345678901234567890");
        Assert.That(() => JsonSerializer.Serialize(value, new JsonSerializerOptions { Converters = { new BigIntegerJsonConverter() } }), Throws.InstanceOf<NotSupportedException>());
    }
}
