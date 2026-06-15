namespace WebDriverBiDi.Script;

using System.Text.Json;

public class SharedWorkerRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "shared-worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<SharedWorkerRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.SharedWorker, info.Type);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "shared-worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        SharedWorkerRealmInfo realmInfo = Assert.IsType<SharedWorkerRealmInfo>(info);
        SharedWorkerRealmInfo copy = realmInfo with { };
        Assert.Equal(realmInfo, copy);
    }
}
