namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class BeforeRequestSentEventArgsTests
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
                            "request": {{requestDataJson}}
                           }
                           """;
        BeforeRequestSentEventArgs? eventArgs = JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            // Note that proper deserialization of base class properties is tested in BaseNetworkEventArgsTests.
            Assert.That(eventArgs.Initiator, Is.Null);
        }
    }

    [Test]
    public void TestCanDeserializeWithOptionalValues()
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
                            "initiator": {
                                "type": "parser"
                            }
                           }
                           """;
        BeforeRequestSentEventArgs? eventArgs = JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            // Note that proper deserialization of base class properties is tested in BaseNetworkEventArgsTests.
            // Also proper deserialization of the Initiator object is handled in InitiatorTests.
            Assert.That(eventArgs.Initiator, Is.Not.Null);
            Assert.That(eventArgs.Initiator!.Type, Is.EqualTo(InitiatorType.Parser));
        }
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
                            "request": {{requestDataJson}}
                           }
                           """;
        BeforeRequestSentEventArgs? eventArgs = JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        BeforeRequestSentEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
    public void TestDeserializingWithInvalidInitiatorTypeThrows()
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
                            "initiator": []
                           }
                           """;
        Assert.That(() => JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
