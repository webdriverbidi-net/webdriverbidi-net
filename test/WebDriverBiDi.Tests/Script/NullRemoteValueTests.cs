namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class NullRemoteValueTests
{
    [Test]
    public void TestCanDeserializeNullRemoteValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;

        NullRemoteValue? result = JsonSerializer.Deserialize<NullRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Null));
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;

        NullRemoteValue? result = JsonSerializer.Deserialize<NullRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("null"));
        Assert.That(argumentLocalValue.Value, Is.Null);
    }

    [Test]
    public void TestDeserializingStringRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-null"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<NullRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;

        NullRemoteValue? result = JsonSerializer.Deserialize<NullRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        NullRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
