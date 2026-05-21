namespace WebDriverBiDi.Network;

using System.Text.Json;

public class BaseNetworkEventArgsTests
{
    private readonly string requestDataJson = """
                                              {
                                                "request": "myRequestId",
                                                "url": "https://example.com",
                                                "method": "get",
                                                "headers": [],
                                                "cookies": [],
                                                "destination": "document",
                                                "initiatorType": "other",
                                                "headersSize": 100,
                                                "bodySize": 300,
                                                "timings": {
                                                  "timeOrigin": 1,
                                                  "requestTime": 2,
                                                  "redirectStart": 3,
                                                  "redirectEnd": 4,
                                                  "fetchStart": 5,
                                                  "dnsStart": 6,
                                                  "dnsEnd": 7,
                                                  "connectStart": 8,
                                                  "connectEnd": 9,
                                                  "tlsStart": 10,
                                                  "requestStart": 11,
                                                  "responseStart": 12,
                                                  "responseEnd": 13
                                                }
                                              }
                                              """;

    [Fact]
    public void TestCanDeserialize()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        BaseNetworkEventArgs? eventArgs = JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal(milliseconds, eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
        Assert.Equal(0u, eventArgs.RedirectCount);

        // Note that proper RequestData deserialization is tested elsewhere.
        Assert.NotNull(eventArgs.Request);
        Assert.False(eventArgs.IsBlocked);
        Assert.Null(eventArgs.Intercepts);
    }

    [Fact]
    public void TestCanDeserializeWithIntercepts()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": true,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "intercepts": [ "myInterceptId" ],
                             "request": {{this.requestDataJson}}
                           }
                           """;
        BaseNetworkEventArgs? eventArgs = JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal((ulong)((ulong)(milliseconds)), eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
        Assert.Equal(0u, eventArgs.RedirectCount);

        // Note that proper RequestData deserialization is tested elsewhere.
        Assert.NotNull(eventArgs.Request);
        Assert.True(eventArgs.IsBlocked);
        Assert.NotNull(eventArgs.Intercepts);
        Assert.Single(eventArgs.Intercepts);
        Assert.Equal("myInterceptId", eventArgs.Intercepts[0]);
    }

    [Fact]
    public void TestCanDeserializeWithNullContextId()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": null,
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        BaseNetworkEventArgs? eventArgs = JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        Assert.Null(eventArgs.BrowsingContextId);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal((ulong)((ulong)(milliseconds)), eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
        Assert.Equal(0u, eventArgs.RedirectCount);

        // Note that proper RequestData deserialization is tested elsewhere.
        Assert.NotNull(eventArgs.Request);
        Assert.False(eventArgs.IsBlocked);
        Assert.Null(eventArgs.Intercepts);
    }

    [Fact]
    public void TestCopySemantics()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        BaseNetworkEventArgs? eventArgs = JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson);
        Assert.NotNull(eventArgs);
        BaseNetworkEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextIdThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextIdTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": {},
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestCanDeserializeWithNullNavigationId()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": null,
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        BaseNetworkEventArgs? eventArgs = JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Null(eventArgs.NavigationId);
        Assert.Equal((ulong)((ulong)(milliseconds)), eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
        Assert.Equal(0u, eventArgs.RedirectCount);

        // Note that proper RequestData deserialization is tested elsewhere.
        Assert.NotNull(eventArgs.Request);
        Assert.False(eventArgs.IsBlocked);
        Assert.Null(eventArgs.Intercepts);
    }

    [Fact]
    public void TestDeserializingWithMissingNavigationIdThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithInvalidNavigationIdTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": {},
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithMissingIsBlockedThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithInvalidIsBlockedTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": {},
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithMissingRedirectCountThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithInvalidRedirectCountTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": {},
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithMissingTimestampThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithInvalidTimestampTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithMissingRequestDataThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithInvalidRequestDataTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": "requestData"
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BaseNetworkEventArgs>(eventJson));
    }
}
