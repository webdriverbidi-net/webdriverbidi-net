namespace WebDriverBiDi.Script;

using System.Text.Json;

public class ServiceWorkerRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "service-worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<ServiceWorkerRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.ServiceWorker, info.Type);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "service-worker"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        ServiceWorkerRealmInfo realmInfo = Assert.IsType<ServiceWorkerRealmInfo>(info);
        ServiceWorkerRealmInfo copy = realmInfo with { };
        Assert.Equal(realmInfo, copy);
    }
}
