namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

[TestFixture]
public class BaseNetworkEventArgsTests
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
    ""request"": {requestDataJson}
}}";
        BaseNetworkEventArgs? eventArgs = JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs!.RedirectCount, Is.EqualTo(0));

            // Note that proper RequestData deserialization is tested elsewhere.
            Assert.That(eventArgs!.Request, Is.Not.Null);
            Assert.That(eventArgs!.IsBlocked, Is.False);
            Assert.That(eventArgs!.Intercepts, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithIntercepts()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": true,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""intercepts"": [ ""myInterceptId"" ],
    ""request"": {requestDataJson}
}}";
        BaseNetworkEventArgs? eventArgs = JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs!.RedirectCount, Is.EqualTo(0));

            // Note that proper RequestData deserialization is tested elsewhere.
            Assert.That(eventArgs!.Request, Is.Not.Null);
            Assert.That(eventArgs!.IsBlocked, Is.True);
            Assert.That(eventArgs!.Intercepts, Is.Not.Null);
            Assert.That(eventArgs!.Intercepts, Has.Count.EqualTo(1));
            Assert.That(eventArgs!.Intercepts![0], Is.EqualTo("myInterceptId"));
        });
    }

    [Test]
    public void TestCanDeserializeWithNullContextId()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": null,
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        BaseNetworkEventArgs? eventArgs = JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.Null);
            Assert.That(eventArgs!.NavigationId, Is.EqualTo("myNavigationId"));
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs!.RedirectCount, Is.EqualTo(0));

            // Note that proper RequestData deserialization is tested elsewhere.
            Assert.That(eventArgs!.Request, Is.Not.Null);
            Assert.That(eventArgs!.IsBlocked, Is.False);
            Assert.That(eventArgs!.Intercepts, Is.Null);
        });
    }

    [Test]
    public void TestDeserializingWithMissingContextIdThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextIdTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": {{}},
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCanDeserializeWithNullNavigationId()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": null,
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        BaseNetworkEventArgs? eventArgs = JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs!.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs!.NavigationId, Is.Null);
            Assert.That(eventArgs!.EpochTimestamp, Is.EqualTo(milliseconds));
            Assert.That(eventArgs!.Timestamp, Is.EqualTo(DateTime.UnixEpoch.AddMilliseconds(milliseconds)));
            Assert.That(eventArgs!.RedirectCount, Is.EqualTo(0));

            // Note that proper RequestData deserialization is tested elsewhere.
            Assert.That(eventArgs!.Request, Is.Not.Null);
            Assert.That(eventArgs!.IsBlocked, Is.False);
            Assert.That(eventArgs!.Intercepts, Is.Null);
        });
    }

    [Test]
    public void TestDeserializingWithMissingNavigationIdThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidNavigationIdTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": {{}},
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingIsBlockedThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidIsBlockedTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": {{}},
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializingWithMissingRedirectCountThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidRedirectCountTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": {{}},
    ""timestamp"": {milliseconds},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingTimestampThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidTimestampTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {{}},
    ""request"": {requestDataJson}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingRequestDataThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {milliseconds}
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithInvalidRequestDataTypeThrows()
    {
        DateTime now = DateTime.UtcNow;
        DateTime eventTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(eventTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string eventJson = $@"{{
    ""context"": ""myContextId"",
    ""navigation"": ""myNavigationId"",
    ""isBlocked"": false,
    ""redirectCount"": 0,
    ""timestamp"": {{}},
    ""request"": ""requestData""
}}";
        Assert.That(() => JsonConvert.DeserializeObject<BaseNetworkEventArgs>(eventJson), Throws.InstanceOf<JsonSerializationException>());
    }
}
