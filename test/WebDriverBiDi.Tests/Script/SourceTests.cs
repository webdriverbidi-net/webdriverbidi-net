namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SourceTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "realmId"
                      }
                      """;
        Source? source = JsonSerializer.Deserialize<Source>(json, deserializationOptions);
        Assert.That(source, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(source.RealmId, Is.EqualTo("realmId"));
            Assert.That(source.Context, Is.Null);
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "realmId"
                      }
                      """;
        Source? source = JsonSerializer.Deserialize<Source>(json, deserializationOptions);
        Assert.That(source, Is.Not.Null);
        Source copy = source with { };
        Assert.That(copy, Is.EqualTo(source));
    }

    [Test]
    public void TestDeserializeWithMissingRealmThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<Source>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidRealmTypeThrows()
    {
        string json = """
                      {
                        "realm": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Source>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalContext()
    {
        string json = """
                      {
                        "realm": "realmId",
                        "context": "contextId"
                      }
                      """;
        Source? source = JsonSerializer.Deserialize<Source>(json, deserializationOptions);
        Assert.That(source, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(source.RealmId, Is.EqualTo("realmId"));
            Assert.That(source.Context, Is.Not.Null);
            Assert.That(source.Context, Is.EqualTo("contextId"));
        }
    }

    [Test]
    public void TestDeserializeWithInvalidFlagsTypeThrows()
    {
        string json = """
                      {
                        "realm": "realmId",
                        "context": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Source>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
