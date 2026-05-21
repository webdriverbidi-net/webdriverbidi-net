namespace WebDriverBiDi.Network;

using System.Text.Json;

public class BeforeRequestSentEventArgsTests
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
        BeforeRequestSentEventArgs? eventArgs = JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        // Note that proper deserialization of base class properties is tested in BaseNetworkEventArgsTests.
        Assert.Null(eventArgs.Initiator);
    }

    [Fact]
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
                            "request": {{this.requestDataJson}},
                            "initiator": {
                                "type": "parser"
                            }
                           }
                           """;
        BeforeRequestSentEventArgs? eventArgs = JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson);
        Assert.NotNull(eventArgs);

        // Note that proper deserialization of base class properties is tested in BaseNetworkEventArgsTests.
        // Also proper deserialization of the Initiator object is handled in InitiatorTests.
        Assert.NotNull(eventArgs.Initiator);
        Assert.Equal(InitiatorType.Parser, eventArgs.Initiator.Type);
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
        BeforeRequestSentEventArgs? eventArgs = JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson);
        Assert.NotNull(eventArgs);
        BeforeRequestSentEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
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
                            "request": {{this.requestDataJson}},
                            "initiator": []
                           }
                           """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<BeforeRequestSentEventArgs>(eventJson));
    }
}
