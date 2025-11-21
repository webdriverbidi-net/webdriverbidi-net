namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class DownloadWillBeginEventArgsTests
{
    [Test]
    public void TestCanDeserialize()
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
        DownloadWillBeginEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(eventArgs.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs.SuggestedFileName, Is.EqualTo("myFile.file"));
        });
    }

    [Test]
    public void TestCopySemantics()
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
        DownloadWillBeginEventArgs? eventArgs = JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        DownloadWillBeginEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
    public void TestDeserializeWithMissingSuggestedFileNameValueThrows()
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
    public void TestDeserializeWithInvalidSuggestedFileNameValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "suggestedFileName": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<DownloadWillBeginEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}