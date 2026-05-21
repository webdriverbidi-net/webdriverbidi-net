namespace WebDriverBiDi.Script;

using System.Text.Json;

public class RealmDestroyedEventArgsTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "myRealmId"
                      }
                      """;
        RealmDestroyedEventArgs? eventArgs = JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json);
        Assert.NotNull(eventArgs);
        Assert.Equal("myRealmId", eventArgs.RealmId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "myRealmId"
                      }
                      """;
        RealmDestroyedEventArgs? eventArgs = JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json);
        Assert.NotNull(eventArgs);
        RealmDestroyedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingRealmValueThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidRealmValueThrows()
    {
        string json = """
                      {
                        "realm": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmDestroyedEventArgs>(json));
    }
}
