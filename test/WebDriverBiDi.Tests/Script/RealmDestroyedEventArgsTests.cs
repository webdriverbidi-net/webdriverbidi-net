namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class RealmDestroyedEventArgsTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealmId"
                      }
                      """;
        RealmDestroyedEventArgs? eventArgs = JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.That(eventArgs.RealmId, Is.EqualTo("myRealmId"));
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealmId"
                      }
                      """;
        RealmDestroyedEventArgs? eventArgs = JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        RealmDestroyedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
    public void TestDeserializeWithMissingRealmValueThrows()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidRealmValueThrows()
    {
        string json = """
                      {
                        "realm": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
