namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;

public class NavigationEventArgsTests
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
                        "navigation": null
                      }
                      """;
        NavigationEventArgs? eventArgs = JsonSerializer.Deserialize<NavigationEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal((ulong)((ulong)(epochTimestamp)), eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(epochTimestamp), eventArgs.Timestamp);
        Assert.Null(eventArgs.NavigationId);
    }

    [Fact]
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
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
    }

    [Fact]
    public void TestCanDeserializeWithUserContext()
    {
        long epochTimestamp = Convert.ToInt64((DateTime.Now - DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "timestamp": {{epochTimestamp}},
                        "navigation": "myNavigationId",
                        "userContext": "myUserContextId"
                      }
                      """;
        NavigationEventArgs? eventArgs = JsonSerializer.Deserialize<NavigationEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("http://example.com", eventArgs.Url);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal("myUserContextId", eventArgs.UserContextId);
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
                        "navigation": null
                      }
                      """;
        NavigationEventArgs? eventArgs = JsonSerializer.Deserialize<NavigationEventArgs>(json);
        Assert.NotNull(eventArgs);
        NavigationEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingTimestampValueThrows()
    {
        string json = $$"""
                      {
                        "context": "myContextId",
                        "url": "http://example.com",
                        "navigation": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }

    [Fact]
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NavigationEventArgs>(json));
    }
}
