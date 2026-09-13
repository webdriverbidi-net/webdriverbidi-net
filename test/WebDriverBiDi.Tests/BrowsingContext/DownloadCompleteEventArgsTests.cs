namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class DownloadCompleteEventArgsTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestCanDeserialize()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
        DownloadCompleteEventArgs completeEventArgs = Assert.IsType<DownloadCompleteEventArgs>(eventArgs);

        Assert.Equal("myContextId", completeEventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", completeEventArgs.Url);
        Assert.Equal((ulong)epochTimestamp, completeEventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), completeEventArgs.Timestamp);
        Assert.Equal("myNavigationId", completeEventArgs.NavigationId);
        Assert.Equal("myDownloadId", completeEventArgs.DownloadId);
        Assert.Equal(DownloadEndStatus.Complete, completeEventArgs.Status);
        Assert.Equal("myFile.file", completeEventArgs.FilePath);
    }

    [Fact]
    public void TestCanDeserializeWithNullFilePath()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
        DownloadCompleteEventArgs completeEventArgs = Assert.IsType<DownloadCompleteEventArgs>(eventArgs);

        Assert.Equal("myDownloadId", completeEventArgs.DownloadId);
        Assert.Equal(DownloadEndStatus.Complete, completeEventArgs.Status);
        Assert.Null(completeEventArgs.FilePath);
    }

    [Fact]
    public void TestNewInstanceHasCompleteStatus()
    {
        DownloadCompleteEventArgs eventArgs = new();
        Assert.Equal(DownloadEndStatus.Complete, eventArgs.Status);
        Assert.Null(eventArgs.FilePath);
    }

    [Fact]
    public void TestDeserializingWithMissingFilePathThrows()
    {
        // The protocol requires filepath on every completed download; only its value may be null.
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "status": "complete"
                      }
                      """;
        Assert.Contains("missing required properties including: 'filepath'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options)).Message);
    }

    [Fact]
    public void TestDeserializingWithInvalidFilePathTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestCopySemantics()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
        DownloadCompleteEventArgs completeEventArgs = Assert.IsType<DownloadCompleteEventArgs>(eventArgs);
        DownloadCompleteEventArgs copy = completeEventArgs with { };
        Assert.Equal(completeEventArgs, copy);
    }
}
