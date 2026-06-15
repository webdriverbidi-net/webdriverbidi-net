namespace WebDriverBiDi.Script;

using System.Text.Json;

public class AudioWorkletRealmInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "audio-worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        AudioWorkletRealmInfo realmInfo = Assert.IsType<AudioWorkletRealmInfo>(info);

        Assert.Equal("myRealm", realmInfo.RealmId);
        Assert.Equal("myOrigin", realmInfo.Origin);
        Assert.Equal(RealmType.AudioWorklet, realmInfo.Type);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealm",
                        "origin": "myOrigin",
                        "type": "audio-worklet"
                      }
                      """;
        RealmInfo? info = JsonSerializer.Deserialize<RealmInfo>(json);
        Assert.NotNull(info);
        AudioWorkletRealmInfo realmInfo = Assert.IsType<AudioWorkletRealmInfo>(info);
        AudioWorkletRealmInfo copy = realmInfo with { };
        Assert.Equal(realmInfo, copy);
    }
}
