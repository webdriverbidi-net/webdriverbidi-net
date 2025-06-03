namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class DownloadEndEventArgsTests
{
    [Test]
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
                        "filepath": "myFile.file"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs.Status, Is.EqualTo(DownloadEndStatus.Complete));
            Assert.That(eventArgs.FilePath, Is.EqualTo("myFile.file"));
        });
    }

    [Test]
    public void TestCanDeserializeCompleteWithNullFilePath()
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
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs.Status, Is.EqualTo(DownloadEndStatus.Complete));
            Assert.That(eventArgs.FilePath, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeCanceled()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "canceled"
                      }
                      """;
        DownloadEndEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadEndEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs.Status, Is.EqualTo(DownloadEndStatus.Canceled));
            Assert.That(eventArgs.FilePath, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithMissingStatusValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidStatusValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidStatusTypeThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "status": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
                        "filepath": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}