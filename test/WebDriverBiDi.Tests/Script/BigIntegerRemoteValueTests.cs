namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json;

[TestFixture]
public class BigIntegerRemoteValueTests
{
    [Test]
    public void TestCanDeserializeBigIntegerRemoteValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        BigIntegerRemoteValue? result = JsonSerializer.Deserialize<BigIntegerRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.BigInt));
        Assert.That(result.Value, Is.EqualTo(BigInteger.Parse("123456789012345678901234567890")));
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        BigIntegerRemoteValue? result = JsonSerializer.Deserialize<BigIntegerRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("bigint"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<BigInteger>());
        Assert.That(argumentLocalValue.Value, Is.EqualTo(BigInteger.Parse("123456789012345678901234567890")));
    }

    [Test]
    public void TestDeserializingBigIntegerRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "bigint"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBigIntegerRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBigIntegerRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBigIntegerRemoteValueWithInvalidValueThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "not-a-number"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<BigIntegerRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123456789012345678901234567890"
                      }
                      """;

        BigIntegerRemoteValue? result = JsonSerializer.Deserialize<BigIntegerRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        BigIntegerRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
