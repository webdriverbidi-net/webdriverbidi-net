namespace WebDriverBiDi.Script;

using System.Text.Json;

public class PaintWorkletRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "paint-worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        PaintWorkletRealmInfo realmInfo = Assert.IsType<PaintWorkletRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.PaintWorklet, realmInfo.Type);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "paint-worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        PaintWorkletRealmInfo realmInfo = Assert.IsType<PaintWorkletRealmInfo>(info);
        PaintWorkletRealmInfo copy = realmInfo with { };
        Assert.Equal(realmInfo, copy);
    }
}
