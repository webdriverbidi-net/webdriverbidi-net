namespace WebDriverBiDi.Script;

using System.Text.Json;

public class RealmCreatedEventArgsTests
{
    [Fact]
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
        Assert.NotNull(info);
        RealmCreatedEventArgs eventArgs = new(info);

        Assert.Equal("myRealm", eventArgs.RealmId);
        Assert.Equal("myOrigin", eventArgs.Origin);
        Assert.Equal(RealmType.Window, eventArgs.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        RealmCreatedEventArgs eventArgs = new(info);

        Assert.Equal("myRealm", eventArgs.RealmId);
        Assert.Equal("myOrigin", eventArgs.Origin);
        Assert.Equal(RealmType.Worker, eventArgs.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        RealmCreatedEventArgs eventArgs = new(info);
        WindowRealmInfo castInfo = eventArgs.As<WindowRealmInfo>();

        Assert.Equal("myRealm", castInfo.RealmId);
        Assert.Equal("myOrigin", castInfo.Origin);
        Assert.Equal(RealmType.Window, castInfo.Type);
    }

    [Fact]
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
        Assert.NotNull(info);
        RealmCreatedEventArgs eventArgs = new(info);
        RealmCreatedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }
}
