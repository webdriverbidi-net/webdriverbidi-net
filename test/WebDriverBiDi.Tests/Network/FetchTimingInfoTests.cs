namespace WebDriverBiDi.Network;

using Newtonsoft.Json;

[TestFixture]
public class FetchTimingInfoTests
{
    [Test]
    public void TestCanDeserializeFetChTimingInfo()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        FetchTimingInfo? info = JsonConvert.DeserializeObject<FetchTimingInfo>(json);
        Assert.That(info, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(info!.TimeOrigin, Is.EqualTo(1));
            Assert.That(info!.RequestTime, Is.EqualTo(2));
            Assert.That(info!.RedirectStart, Is.EqualTo(3));
            Assert.That(info!.RedirectEnd, Is.EqualTo(4));
            Assert.That(info!.FetchStart, Is.EqualTo(5));
            Assert.That(info!.DnsStart, Is.EqualTo(6));
            Assert.That(info!.DnsEnd, Is.EqualTo(7));
            Assert.That(info!.ConnectStart, Is.EqualTo(8));
            Assert.That(info!.ConnectEnd, Is.EqualTo(9));
            Assert.That(info!.TlsStart, Is.EqualTo(10));
            Assert.That(info!.RequestStart, Is.EqualTo(11));
            Assert.That(info!.ResponseStart, Is.EqualTo(12));
            Assert.That(info!.ResponseEnd, Is.EqualTo(13));
        });
    }

    [Test]
    public void TestDeserializingWithMissingTimeOriginThrows()
    {
        string json = @"{ ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'timeOrigin' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingRequestTimeThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'requestTime' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingRedirectStartThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'redirectStart' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingRedirectEndThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'redirectEnd' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingFetchStartThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'fetchStart' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingDnsStartThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'dnsStart' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingDnsEndThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'dnsEnd' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingConnectStartThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'connectStart' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingConnectEndThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'connectEnd' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingTlsStartThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""requestStart"": 11, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'tlsStart' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingRequestStartThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""responseStart"": 12, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'requestStart' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingResponseStartThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseEnd"": 13 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'responseStart' not found in JSON"));
    }

    [Test]
    public void TestDeserializingWithMissingResponseEndThrows()
    {
        string json = @"{ ""timeOrigin"": 1, ""requestTime"": 2, ""redirectStart"": 3, ""redirectEnd"": 4, ""fetchStart"": 5, ""dnsStart"": 6, ""dnsEnd"": 7, ""connectStart"": 8, ""connectEnd"": 9, ""tlsStart"": 10, ""requestStart"": 11, ""responseStart"": 12 }";
        Assert.That(() => JsonConvert.DeserializeObject<FetchTimingInfo>(json), Throws.InstanceOf<JsonSerializationException>().With.Message.Contains("Required property 'responseEnd' not found in JSON"));
    }
}