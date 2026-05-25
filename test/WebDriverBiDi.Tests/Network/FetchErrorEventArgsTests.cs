namespace WebDriverBiDi.Network;

using System.Text.Json;

public class FetchErrorEventArgsTests
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
                             "request": {{this.requestDataJson}},
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal(milliseconds, eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
        Assert.Equal(0u, eventArgs.RedirectCount);
        Assert.Equal("My error", eventArgs.ErrorText);

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
                             "request": {{this.requestDataJson}},
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson);
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
        Assert.Equal("My error", eventArgs.ErrorText);
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
                             "request": {{this.requestDataJson}},
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson);
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
        Assert.Equal("My error", eventArgs.ErrorText);
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
                             "request": {{this.requestDataJson}},
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson);
        Assert.NotNull(eventArgs);
        FetchErrorEventArgs copy = eventArgs with { };
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
                             "context": "myContextId",
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson));
    }

    [Fact]
    public void TestDeserializingWithInvalidErrorTextTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $$"""
                           {
                             "context": "myContextId,
                             "navigation": "myNavigationId",
                             "isBlocked": false,
                             "redirectCount": 0,
                             "timestamp": {{milliseconds}},
                             "request": {{this.requestDataJson}},
                             "errorText": {}
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson));
    }
}
