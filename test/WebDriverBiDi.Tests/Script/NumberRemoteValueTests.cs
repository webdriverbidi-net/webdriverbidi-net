namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class NumberRemoteValueTests
{
    [Test]
    public void TestCanDeserializeNumberRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Number));
        Assert.That(result.Value, Is.EqualTo(3.14159));
    }

    [Test]
    public void TestCanDeserializeNumberRemoteValueWithInteger()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Number));
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("number"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<double>());
        Assert.That(argumentLocalValue.Value, Is.EqualTo(3.14159));
    }

    [Test]
    public void TestCanConvertToLong()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ToLong(), Is.EqualTo(42));
    }

    [Test]
    public void TestCanConvertToInt()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ToInt(), Is.EqualTo(42));
    }

    [Test]
    public void TestCanUseImplicitConversionToInt()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        int intValue = result;
        Assert.That(intValue, Is.EqualTo(42));
    }

    [Test]
    public void TestCanUseImplicitConversionToLong()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        long longValue = result;
        Assert.That(longValue, Is.EqualTo(42));
    }

    [Test]
    public void TestCanUseImplicitConversionToDouble()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        double doubleValue = result;
        Assert.That(doubleValue, Is.EqualTo(42));
    }

    [Test]
    public void TestDeserializingNumberRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "number"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<NumberRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingNumberRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "number",
                        "value": {}
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<NumberRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingNumberRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-number",
                        "value": 3.14159
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<NumberRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        NumberRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
