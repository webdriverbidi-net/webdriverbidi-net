namespace WebDriverBiDi.Script;

using System.Text.Json;

public class WorkletRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        Assert.IsType<WorkletRealmInfo>(info);

        Assert.Equal("myRealm", info.RealmId);
        Assert.Equal("myOrigin", info.Origin);
        Assert.Equal(RealmType.Worklet, info.Type);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        WorkletRealmInfo realmInfo = Assert.IsType<WorkletRealmInfo>(info);
        WorkletRealmInfo copy = realmInfo with { };
        Assert.Equal(info, copy);
    }
}
