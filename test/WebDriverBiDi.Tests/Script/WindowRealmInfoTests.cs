namespace WebDriverBiDi.Script;

using System.Text.Json;

public class WindowRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
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
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.Window, realmInfo.Type);
        Assert.Equal("myContext", realmInfo.BrowsingContext);
        Assert.Null(realmInfo.Sandbox);
    }

    [Fact]
    public void TestCanDeserializeWithUserContext()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "userContext": "myUserContext"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.Window, realmInfo.Type);
        Assert.Equal("myContext", realmInfo.BrowsingContext);
        Assert.Null(realmInfo.Sandbox);
        Assert.Equal("myUserContext", realmInfo.UserContext);
    }

    [Fact]
    public void TestCanDeserializeWithSandbox()
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
        Assert.NotNull(info);
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.Window, realmInfo.Type);
        Assert.Equal("myContext", realmInfo.BrowsingContext);
        Assert.Equal("mySandbox", realmInfo.Sandbox);
    }

    [Fact]
    public void TestCopySemantics()
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
        Assert.NotNull(info);
        WindowRealmInfo realmInfo = Assert.IsType<WindowRealmInfo>(info);
        WindowRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window"
                      }
                      """;
        Assert.Contains("missing required properties including: 'context'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": 123
                      }
                      """;
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithInvalidSandboxTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "sandbox": 2
                      }
                      """;
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithInvalidUserContextTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "window",
                        "context": "myContext",
                        "userContext": 2
                      }
                      """;
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }
}
