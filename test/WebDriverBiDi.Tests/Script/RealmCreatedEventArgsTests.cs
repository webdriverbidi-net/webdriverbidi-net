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
                        "context": "myContext"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        RealmCreatedEventArgs eventArgs = new(info);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.RealmId, Is.EqualTo("myRealm"));
            Assert.That(eventArgs.Origin, Is.EqualTo("myOrigin"));
            Assert.That(eventArgs.Type, Is.EqualTo(RealmType.Window));
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
        Assert.That(info, Is.Not.Null);
        RealmCreatedEventArgs eventArgs = new(info);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.RealmId, Is.EqualTo("myRealm"));
            Assert.That(eventArgs.Origin, Is.EqualTo("myOrigin"));
            Assert.That(eventArgs.Type, Is.EqualTo(RealmType.Worker));
        });
    }

    [Test]
    public void TestCanCastToSpecificRealmType()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        RealmCreatedEventArgs eventArgs = new(info);
        WindowRealmInfo castInfo = eventArgs.As<WindowRealmInfo>();
        Assert.Multiple(() =>
        {
            Assert.That(castInfo.RealmId, Is.EqualTo("myRealm"));
            Assert.That(castInfo.Origin, Is.EqualTo("myOrigin"));
            Assert.That(castInfo.Type, Is.EqualTo(RealmType.Window));
        });
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.That(info, Is.Not.Null);
        RealmCreatedEventArgs eventArgs = new(info);
        RealmCreatedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

}
