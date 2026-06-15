namespace WebDriverBiDi.Script;

using System.Text.Json;

public class WorkerRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
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
        Assert.IsType<WorkerRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.Worker, info.Type);
    }

    [Fact]
    public void TestCopySemantics()
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
        Assert.IsType<RealmInfo>(info, exactMatch: false);
        RealmInfo copy = info with { };
        Assert.Equal(info, copy);
    }
}
