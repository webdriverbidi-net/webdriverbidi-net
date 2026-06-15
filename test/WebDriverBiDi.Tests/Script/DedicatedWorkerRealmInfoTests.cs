namespace WebDriverBiDi.Script;

using System.Text.Json;

public class DedicatedWorkerRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": [ "ownerRealm" ]
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        DedicatedWorkerRealmInfo realmInfo = Assert.IsType<DedicatedWorkerRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.DedicatedWorker, realmInfo.Type);
        Assert.Single(realmInfo.Owners);
        Assert.Equal("ownerRealm", realmInfo.Owners[0]);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": [ "ownerRealm" ]
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<DedicatedWorkerRealmInfo>(info);
        DedicatedWorkerRealmInfo realmInfo = (DedicatedWorkerRealmInfo)info;
        DedicatedWorkerRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingOwnersThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker"
                      }
                      """;
        Assert.Contains("missing required properties including: 'owners'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithInvalidOwnersTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": ""
                      }
                      """;
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWithInvalidOwnersEntryTypeThrows()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "dedicated-worker",
                        "owners": [ 123 ]
                      }
                      """;
        Assert.Contains("value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmInfo>(json)).Message);
    }
}
