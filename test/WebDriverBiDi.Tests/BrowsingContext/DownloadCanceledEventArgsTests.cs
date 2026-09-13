namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class DownloadCanceledEventArgsTests
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
                        "download": "myDownloadId",
                        "status": "canceled"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
        DownloadCanceledEventArgs canceledEventArgs = Assert.IsType<DownloadCanceledEventArgs>(eventArgs);

        Assert.Equal("myContextId", canceledEventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", canceledEventArgs.Url);
        Assert.Equal((ulong)epochTimestamp, canceledEventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), canceledEventArgs.Timestamp);
        Assert.Equal("myNavigationId", canceledEventArgs.NavigationId);
        Assert.Equal("myDownloadId", canceledEventArgs.DownloadId);
        Assert.Equal(DownloadEndStatus.Canceled, canceledEventArgs.Status);
    }

    [Fact]
    public void TestCanDeserializeWithFilePathProperty()
    {
        // A canceled download has no filepath in the protocol. A remote end that sends one anyway is not
        // rejected: like any other property a received type does not define, it is not consumed by the
        // type, and the transport exposes it as extension data.
        long epochTimestamp = Convert.ToInt64((DateTime.UtcNow - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "download": "myDownloadId",
                        "status": "canceled",
                        "filepath": "myFile.file"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
        DownloadCanceledEventArgs canceledEventArgs = Assert.IsType<DownloadCanceledEventArgs>(eventArgs);
        Assert.Equal(DownloadEndStatus.Canceled, canceledEventArgs.Status);
    }

    [Fact]
    public void TestNewInstanceHasCanceledStatus()
    {
        DownloadCanceledEventArgs eventArgs = new();
        Assert.Equal(DownloadEndStatus.Canceled, eventArgs.Status);
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
                        "status": "canceled"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json, this.options);
        DownloadCanceledEventArgs canceledEventArgs = Assert.IsType<DownloadCanceledEventArgs>(eventArgs);
        DownloadCanceledEventArgs copy = canceledEventArgs with { };
        Assert.Equal(canceledEventArgs, copy);
    }
}
