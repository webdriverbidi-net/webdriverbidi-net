namespace WebDriverBiDi.Network;

using System.Text.Json;

public class ResponseStartedEventArgsTests
{
    private readonly string requestDataJson = """
                                             {
                                               "request": "myRequestId",
                                               "url": "https://example.com",
                                               "method": "get",
                                               "headers": [],
                                               "cookies": [],
                                               "initiatorType": "other",
                                               "destination": "document",
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
                             "response": {
                               "url": "https://example.com",
                               "protocol": "https",
                               "status": 200,
                               "statusText": "OK",
                               "fromCache": false,
                               "headers": [
                                 {
                                   "name": "headerName",
                                   "value": {
                                     "type": "string",
                                     "value": "headerValue"
                                   }
                                 }
                               ],
                               "mimeType": "text/html",
                               "bytesReceived": 400,
                               "headersSize": 100,
                               "bodySize": 300,
                               "content": {
                                 "size": 300
                               }
                             }
                           }
                           """;
        ResponseStartedEventArgs? eventArgs = JsonSerializer.Deserialize<ResponseStartedEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myNavigationId", eventArgs.NavigationId);
        Assert.Equal(milliseconds, eventArgs.EpochTimestamp);
        Assert.Equal(DateTime.UnixEpoch.AddMilliseconds(milliseconds), eventArgs.Timestamp);
        Assert.Equal(0u, eventArgs.RedirectCount);

        // Note that proper RequestData deserialization is tested elsewhere.
        // Also proper ResponseData deserialization is tested elsewhere.
        Assert.NotNull(eventArgs.Request);
        Assert.False(eventArgs.IsBlocked);
        Assert.Null(eventArgs.Intercepts);
        Assert.NotNull(eventArgs.Response);
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
                             "response": {
                               "url": "https://example.com",
                               "protocol": "https",
                               "status": 200,
                               "statusText": "OK",
                               "fromCache": false,
                               "headers": [
                                 {
                                   "name": "headerName",
                                   "value": {
                                     "type": "string",
                                     "value": "headerValue"
                                   }
                                 }
                               ],
                               "mimeType": "text/html",
                               "bytesReceived": 400,
                               "headersSize": 100,
                               "bodySize": 300,
                               "content": {
                                 "size": 300
                               }
                             }
                           }
                           """;
        ResponseStartedEventArgs? eventArgs = JsonSerializer.Deserialize<ResponseStartedEventArgs>(eventJson);
        Assert.NotNull(eventArgs);
        ResponseStartedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingResponseThrows()
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ResponseStartedEventArgs>(eventJson));
    }
}
