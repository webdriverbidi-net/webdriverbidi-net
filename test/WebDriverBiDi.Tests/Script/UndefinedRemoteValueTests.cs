namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class UndefinedRemoteValueTests
{
    [Test]
    public void TestCanDeserializeUndefinedRemoteValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;

        UndefinedRemoteValue? result = JsonSerializer.Deserialize<UndefinedRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Undefined));
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;

        UndefinedRemoteValue? result = JsonSerializer.Deserialize<UndefinedRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("undefined"));
        Assert.That(argumentLocalValue.Value, Is.Null);
    }

    [Test]
    public void TestDeserializingStringRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-undefined"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<UndefinedRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;

        UndefinedRemoteValue? result = JsonSerializer.Deserialize<UndefinedRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        UndefinedRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
