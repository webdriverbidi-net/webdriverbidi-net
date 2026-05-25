namespace WebDriverBiDi.Speculation;

using System.Text.Json;

public class PrefetchStatusUpdatedEventArgsTests
{
    [Fact]
    public void TestCanDeserializeWithPendingStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("https://example.com/index.html", eventArgs.Url);
        Assert.Equal(PreloadingStatus.Pending, eventArgs.Status);
    }

    [Fact]
    public void TestCanDeserializeWithReadyStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "ready"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("https://example.com/index.html", eventArgs.Url);
        Assert.Equal(PreloadingStatus.Ready, eventArgs.Status);
    }

    [Fact]
    public void TestCanDeserializeWithSuccessStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "success"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("https://example.com/index.html", eventArgs.Url);
        Assert.Equal(PreloadingStatus.Success, eventArgs.Status);
    }

    [Fact]
    public void TestCanDeserializeWithFailureStatus()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "failure"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("https://example.com/index.html", eventArgs.Url);
        Assert.Equal(PreloadingStatus.Failure, eventArgs.Status);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        PrefetchStatusUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);
        PrefetchStatusUpdatedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "url": "https://example.com/index.html",
                        "status": "pending"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingUrlThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "status": "pending"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": {},
                        "status": "pending"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingStatusThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidStatusTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidStatusValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "url": "https://example.com/index.html",
                        "status": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<PrefetchStatusUpdatedEventArgs>(json));
    }
}
