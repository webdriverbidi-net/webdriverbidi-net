namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class NavigationResultTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "url": "http://example.com",
                        "navigation": null
                      }
                      """;
        NavigateCommandResult? result = JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options);
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
        NavigateCommandResult? result = JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options);
        Assert.NotNull(result);

        Assert.Equal("http://example.com", result.Url);
        Assert.Equal("myNavigationId", result.NavigationId);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "url": "http://example.com",
                        "navigation": "myNavigationId"
                      }
                      """;
        NavigateCommandResult? result = JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options);
        Assert.NotNull(result);
        NavigateCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingNavigationThrows()
    {
        string json = """
                      {
                        "url": "http://example.com"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingUrlThrows()
    {
        string json = "{}";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "url": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullUrlThrows()
    {
        string json = """
                      {
                        "url": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigateCommandResult>(json, this.options));
    }
}
