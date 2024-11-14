namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class RealmCreatedEventArgsTests
{
    [Test]
    public void TestCanCreateWithWindowRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "sandbox": "mySandbox"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        RealmCreatedEventArgs eventArgs = new(info!);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.RealmId, Is.EqualTo("myRealm"));
            Assert.That(eventArgs.Origin, Is.EqualTo("myOrigin"));
            Assert.That(eventArgs.Type, Is.EqualTo(RealmType.Window));
            Assert.That(eventArgs.BrowsingContext, Is.EqualTo("myContext"));
            Assert.That(eventArgs.Sandbox, Is.EqualTo("mySandbox"));
        });
    }

    [Test]
    public void TestCanCreateWithNonWindowRealmInfo()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        RealmCreatedEventArgs eventArgs = new(info!);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.RealmId, Is.EqualTo("myRealm"));
            Assert.That(eventArgs.Origin, Is.EqualTo("myOrigin"));
            Assert.That(eventArgs.Type, Is.EqualTo(RealmType.Worker));
            Assert.That(eventArgs.BrowsingContext, Is.Null);
            Assert.That(eventArgs.Sandbox, Is.Null);
        });
    }
}
