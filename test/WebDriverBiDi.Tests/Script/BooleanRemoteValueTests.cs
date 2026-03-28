namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class BooleanRemoteValueTests
{
    [Test]
    public void TestCanDeserializeBooleanRemoteValueWithTrue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Boolean));
        Assert.That(result.Value, Is.True);
    }

    [Test]
    public void TestCanDeserializeBooleanRemoteValueWithFalse()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": false
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Boolean));
        Assert.That(result.Value, Is.False);
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("boolean"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<bool>());
        Assert.That(argumentLocalValue.Value, Is.True);
    }

    [Test]
    public void TestCanUseImplicitConversionToBool()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        bool boolValue = result;
        Assert.That(boolValue, Is.True);
    }

    [Test]
    public void TestDeserializingBooleanRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "boolean"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<BooleanRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBooleanRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<BooleanRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingBooleanRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-boolean",
                        "value": true
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<BooleanRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;

        BooleanRemoteValue? result = JsonSerializer.Deserialize<BooleanRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        BooleanRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
