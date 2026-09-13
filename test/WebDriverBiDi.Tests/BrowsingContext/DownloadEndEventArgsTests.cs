namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class DownloadEndEventArgsTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
    public void TestDeserializingWithNullTimestampThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": {},
                        "status": "complete",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullDownloadIdThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": null,
                        "status": "complete",
                        "filepath": "myFile.file"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithMissingStatusThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithNullStatusValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializeWithInvalidStatusTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNonObjectThrows()
    {
        string json = @"[ ""invalid download end"" ]";
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options));
    }

    [Fact]
    public void TestCanCastToProperSubclassType()
    {
        DownloadEndEventArgs eventArgs = DeserializeCompleteEventArgs(this.options);
        Assert.Same(eventArgs, eventArgs.As<DownloadCompleteEventArgs>());
    }

    [Fact]
    public void TestCannotCastToImproperSubclassType()
    {
        DownloadEndEventArgs eventArgs = DeserializeCompleteEventArgs(this.options);
        Assert.Contains("cannot be cast", Assert.ThrowsAny<WebDriverBiDiException>(() => eventArgs.As<DownloadCanceledEventArgs>()).Message);
    }

    [Fact]
    public void TestTryCastToSubclassTypeReturnsTrue()
    {
        DownloadEndEventArgs eventArgs = DeserializeCompleteEventArgs(this.options);
        bool result = eventArgs.TryAs(out DownloadCompleteEventArgs? completeEventArgs);
        Assert.True(result);
        Assert.Same(eventArgs, completeEventArgs);
    }

    [Fact]
    public void TestTryCastToImproperSubclassTypeReturnsFalse()
    {
        DownloadEndEventArgs eventArgs = DeserializeCompleteEventArgs(this.options);
        bool result = eventArgs.TryAs(out DownloadCanceledEventArgs? canceledEventArgs);
        Assert.False(result);
        Assert.Null(canceledEventArgs);
    }

    private static DownloadEndEventArgs DeserializeCompleteEventArgs(JsonSerializerOptions options)
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, options);
        Assert.NotNull(eventArgs);
        return eventArgs;
    }
}
