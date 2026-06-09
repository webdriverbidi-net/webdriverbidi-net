namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class DownloadEndEventArgsTests
{
    [Fact]
    public void TestCanDeserializeComplete()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal((ulong)epochTimestamp, eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal("myDownloadId", eventArgs.DownloadId);
        Assert.Equal(DownloadEndStatus.Complete, eventArgs.Status);
        Assert.Equal("myFile.file", eventArgs.FilePath);
    }

    [Fact]
    public void TestCanDeserializeCompleteWithNullFilePath()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "status": "complete",
                        "filepath": null
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal((ulong)epochTimestamp, eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal("myDownloadId", eventArgs.DownloadId);
        Assert.Equal(DownloadEndStatus.Complete, eventArgs.Status);
        Assert.Null(eventArgs.FilePath);
    }

    [Fact]
    public void TestCanDeserializeCanceled()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "status": "canceled"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal((ulong)epochTimestamp, eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal("myDownloadId", eventArgs.DownloadId);
        Assert.Equal(DownloadEndStatus.Canceled, eventArgs.Status);
        Assert.Null(eventArgs.FilePath);
    }

    [Fact]
    public void TestCanDeserializingWithMissingDownloadIdThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "filepath": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json));
    }

    [Fact]
    public void TestCanDeserializingWithMissingStatusThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "filepath": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "status": "complete",
                        "filepath": "myFile.file"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json);
        Assert.NotNull(eventArgs);
        DownloadEndEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingStatusValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidStatusValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "status": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingDownloadIdValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidDownloadIdTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": {},
                        "status": "complete"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidStatusTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": {},
                        "download": "myDownloadId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidFilePathValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }
}
