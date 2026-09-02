namespace WebDriverBiDi.Script;

using System.Text.Json;

public class SourceTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "realm": "realmId"
                      }
                      """;
        Source? source = JsonSerializer.Deserialize<Source>(json);
        Assert.NotNull(source);

        Assert.Equal("realmId", source.RealmId);
        Assert.Null(source.BrowsingContextId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "realm": "realmId"
                      }
                      """;
        Source? source = JsonSerializer.Deserialize<Source>(json);
        Assert.NotNull(source);
        Source copy = source with { };
        Assert.Equal(source, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingRealmThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Source>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidRealmTypeThrows()
    {
        string json = """
                      {
                        "realm": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Source>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalContext()
    {
        string json = """
                      {
                        "realm": "realmId",
                        "context": "contextId"
                      }
                      """;
        Source? source = JsonSerializer.Deserialize<Source>(json);
        Assert.NotNull(source);

        Assert.Equal("realmId", source.RealmId);
        Assert.NotNull(source.BrowsingContextId);
        Assert.Equal("contextId", source.BrowsingContextId);
    }

    [Fact]
    public void TestCanDeserializeWithOptionalUserContext()
    {
        string json = """
                      {
                        "realm": "realmId",
                        "userContext": "userContextId"
                      }
                      """;
        Source? source = JsonSerializer.Deserialize<Source>(json);
        Assert.NotNull(source);

        Assert.Equal("realmId", source.RealmId);
        Assert.NotNull(source.UserContextId);
        Assert.Equal("userContextId", source.UserContextId);
    }

    [Fact]
    public void TestDeserializeWithInvalidFlagsTypeThrows()
    {
        string json = """
                      {
                        "realm": "realmId",
                        "context": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Source>(json));
    }
}
