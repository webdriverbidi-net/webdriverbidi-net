namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class AuthRequiredEventArgsTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

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

    [Test]
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
                             "request": {{requestDataJson}},
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
        AuthRequiredEventArgs? eventArgs = JsonSerializer.Deserialize<AuthRequiredEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            // Note that proper deserialization of base class properties is tested in BaseNetworkEventArgsTests.
            // Also proper deserialization of the ResponseData object is handled in InitiatorTests.
            Assert.That(eventArgs.Response, Is.Not.Null);
        });
    }

    [Test]
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
                             "request": {{requestDataJson}},
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
        AuthRequiredEventArgs? eventArgs = JsonSerializer.Deserialize<AuthRequiredEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        AuthRequiredEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
    public void TestDeserializingWithMissingResponseThrows()
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
                             "request": {{requestDataJson}}
                           }
                           """;
        Assert.That(() => JsonSerializer.Deserialize<AuthRequiredEventArgs>(eventJson, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
