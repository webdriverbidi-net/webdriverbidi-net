namespace WebDriverBiDi.Network;

using System.Text.Json;
using Microsoft.VisualBasic;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class FetchErrorEventArgsTests
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
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs.RedirectCount, Is.EqualTo(0));
            Assert.That(eventArgs.ErrorText, Is.EqualTo("My error"));

            // Note that proper RequestData deserialization is tested elsewhere.
            Assert.That(eventArgs.Request, Is.Not.Null);
            Assert.That(eventArgs.IsBlocked, Is.False);
            Assert.That(eventArgs.Intercepts, Is.Null);
        });
    }

    [Test]
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
                             "request": {{requestDataJson}},
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs.RedirectCount, Is.EqualTo(0));

            // Note that proper RequestData deserialization is tested elsewhere.
            Assert.That(eventArgs.Request, Is.Not.Null);
            Assert.That(eventArgs.IsBlocked, Is.True);
            Assert.That(eventArgs.Intercepts, Is.Not.Null);
            Assert.That(eventArgs.Intercepts, Has.Count.EqualTo(1));
            Assert.That(eventArgs.Intercepts![0], Is.EqualTo("myInterceptId"));
            Assert.That(eventArgs.ErrorText, Is.EqualTo("My error"));
        });
    }

    [Test]
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
                             "request": {{requestDataJson}},
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.BrowsingContextId, Is.Null);
            Assert.That(eventArgs.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs.RedirectCount, Is.EqualTo(0));

            // Note that proper RequestData deserialization is tested elsewhere.
            Assert.That(eventArgs.Request, Is.Not.Null);
            Assert.That(eventArgs.IsBlocked, Is.False);
            Assert.That(eventArgs.Intercepts, Is.Null);
            Assert.That(eventArgs.ErrorText, Is.EqualTo("My error"));
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
                             "errorText": "My error"
                           }
                           """;
        FetchErrorEventArgs? eventArgs = JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        FetchErrorEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
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
                             "request": {{requestDataJson}}
                           }
                           """;
        Assert.That(() => JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
                             "request": {{requestDataJson}},
                             "errorText": {}
                           }
                           """;
        Assert.That(() => JsonSerializer.Deserialize<FetchErrorEventArgs>(eventJson, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
