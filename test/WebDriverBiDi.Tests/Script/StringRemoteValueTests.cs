namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class StringRemoteValueTests
{
    [Test]
    public void TestCanDeserializeStringRemoteValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.String));
        Assert.That(result.Value, Is.EqualTo("my string value"));
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("string"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<string>());
        Assert.That(argumentLocalValue.Value, Is.EqualTo("my string value"));
    }

    [Test]
    public void TestCanUseImplicitConversionToString()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        string stringValue = result;
        Assert.That(stringValue, Is.EqualTo("my string value"));
    }

    [Test]
    public void TestDeserializingStringRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "string"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<StringRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingStringRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "string",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<StringRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingStringRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-string",
                        "value": "my string value"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<StringRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "my string value"
                      }
                      """;

        StringRemoteValue? result = JsonSerializer.Deserialize<StringRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        StringRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
