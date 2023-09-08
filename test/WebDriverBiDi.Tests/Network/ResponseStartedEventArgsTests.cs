namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

[TestFixture]
public class ResponseStartedEventArgsTests
{
    private readonly string requestDataJson = @"{
    ""request"": ""myRequestId"",
    ""url"": ""https://example.com"",
    ""method"": ""get"",
    ""headers"": [],
    ""cookies"": [],
    ""headersSize"": 100,
    ""bodySize"": 300,
    ""timings"": {
        ""timeOrigin"": 1,
        ""requestTime"": 2,
        ""redirectStart"": 3,
        ""redirectEnd"": 4,
        ""fetchStart"": 5,
        ""dnsStart"": 6,
        ""dnsEnd"": 7,
        ""connectStart"": 8,
        ""connectEnd"": 9,
        ""tlsStart"": 10,
        ""requestStart"": 11,
        ""responseStart"": 12,
        ""responseEnd"": 13
    }
}";

    [Test]
    public void TestCanDeserialize()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson},
    ""response"": {{
        ""url"": ""https://example.com"",
        ""protocol"": ""https"",
        ""status"": 200,
        ""statusText"": ""OK"",
        ""fromCache"": false,
        ""headers"": [
            {{
                ""name"": ""headerName"",
                ""value"": {{
                    ""type"": ""string"",
                    ""value"": ""headerValue""
                }}
            }}
        ],
        ""mimeType"": ""text/html"",
        ""bytesReceived"": 400,
        ""headersSize"": 100,
        ""bodySize"": 300,
        ""content"": {{
            ""size"": 300
        }}       
    }}
}}";
        ResponseStartedEventArgs? eventArgs = JsonConvert.DeserializeObject<ResponseStartedEventArgs>(eventJson);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs!.RedirectCount, Is.EqualTo(0));

            // Note that proper RequestData deserialization is tested elsewhere.
            // Also proper ResponseData deserialization is tested elsewhere.
            Assert.That(eventArgs!.Request, Is.Not.Null);
            Assert.That(eventArgs!.IsBlocked, Is.False);
            Assert.That(eventArgs!.Intercepts, Is.Null);
            Assert.That(eventArgs.Response, Is.Not.Null);
        });
    }

    [Test]
    public void TestDeserializeWithMissingResponseThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<ResponseStartedEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }
}