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
        Assert.Null(source.Context);
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
        Assert.NotNull(source.Context);
        Assert.Equal("contextId", source.Context);
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
        Assert.NotNull(source.UserContext);
        Assert.Equal("userContextId", source.UserContext);
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
