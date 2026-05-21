namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class DownloadWillBeginEventArgsTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "suggestedFileName": "myFile.file"
                      }
                      """;
        DownloadWillBeginEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal((ulong)((ulong)(epochTimestamp)), eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal("myDownloadId", eventArgs.DownloadId);
        Assert.Equal("myFile.file", eventArgs.SuggestedFileName);
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
                        "suggestedFileName": "myFile.file"
                      }
                      """;
        DownloadWillBeginEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json);
        Assert.NotNull(eventArgs);
        DownloadWillBeginEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingSuggestedFileNameValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "download": "myDownloadId",
                        "navigation": "myNavigationId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidSuggestedFileNameValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "suggestedFileName": {}
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
                        "suggestedFileName": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidDownloadIdThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": {},
                        "suggestedFileName": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json));
    }
}
