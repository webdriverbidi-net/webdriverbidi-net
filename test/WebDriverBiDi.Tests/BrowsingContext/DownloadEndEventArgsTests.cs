namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class DownloadEndEventArgsTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);
        DownloadEndEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": {},
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullContextThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": null,
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingUrlThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidUrlTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": {},
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullUrlThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": null,
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingTimestampThrows()
    {
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidTimestampDataTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": "invalid timestamp",
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithnullTimestampThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": null,
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNavigationThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "navigation": "myNavigationId",
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithInvalidNavigationTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": {},
                        "status": "complete",
                        "download": "myDownloadId",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingDownloadIdThrows()
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullDownloadIdThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": null,
                        "status": "complete"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingStatusThrows()
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullStatusValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "status": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json, this.options));
    }
}
