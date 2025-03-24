namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

[TestFixture]
public class NavigationEventArgsTests
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
                        "navigation": null
                      }
                      """;
        NavigationEventArgs? eventArgs = JsonSerializer.Deserialize<NavigationEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(epochTimestamp));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp)));
            Assert.That(eventArgs.NavigationId, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithNavigationId()
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
        NavigationEventArgs? eventArgs = JsonSerializer.Deserialize<NavigationEventArgs>(json);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.Url, Is.EqualTo("http://example.com"));
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
        });
    }

    [Test]
    public void TestDeserializeWithMissingContextValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidContextValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": {},
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingUrlValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "timestamp": {{epochTimestamp}},
                        "navigation": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidUrlValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": {},
                        "timestamp": {{epochTimestamp}},
                        "navigation": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingTimestampValueThrows()
    {
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "navigation": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidTimestampValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {},
                        "navigation": null
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingNavigationValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidNavigationValueThrows()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NavigationEventArgs>(json), Throws.InstanceOf<JsonException>());
    }
}
