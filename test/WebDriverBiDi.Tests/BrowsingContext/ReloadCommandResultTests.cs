namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class ReloadCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "url": "http://example.com"
                      }
                      """;
        ReloadCommandResult? result = JsonSerializer.Deserialize<ReloadCommandResult>(json);
        Assert.NotNull(result);

        Assert.Equal("http://example.com", result.Url);
        Assert.Null(result.NavigationId);
    }

    [Fact]
    public void TestCanDeserializeWithNavigationId()
    {
        string json = """
                      {
                        "url": "http://example.com",
                        "navigation": "myNavigationId"
                      }
                      """;
        ReloadCommandResult? result = JsonSerializer.Deserialize<ReloadCommandResult>(json);
        Assert.NotNull(result);

        Assert.Equal("http://example.com", result.Url);
        Assert.Equal("myNavigationId", result.NavigationId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "url": "http://example.com"
                      }
                      """;
        ReloadCommandResult? result = JsonSerializer.Deserialize<ReloadCommandResult>(json);
        Assert.NotNull(result);
        ReloadCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingUrlThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ReloadCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "url": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ReloadCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidNavigationIdTypeThrows()
    {
        string json = """
                      {
                        "url": "http://example.co",
                        "navigation": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ReloadCommandResult>(json));
    }
}
