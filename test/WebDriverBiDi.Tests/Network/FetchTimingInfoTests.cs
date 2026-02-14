namespace WebDriverBiDi.Network;

using System.Text.Json;

[TestFixture]
public class FetchTimingInfoTests
{
    [Test]
    public void TestCanDeserializeFetchTimingInfo()
    {
        string json = """
                      {
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
                      """;
        FetchTimingInfo? info = JsonSerializer.Deserialize<FetchTimingInfo>(json);
        Assert.That(info, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(info.TimeOrigin, Is.EqualTo(1));
            Assert.That(info.RequestTime, Is.EqualTo(2));
            Assert.That(info.RedirectStart, Is.EqualTo(3));
            Assert.That(info.RedirectEnd, Is.EqualTo(4));
            Assert.That(info.FetchStart, Is.EqualTo(5));
            Assert.That(info.DnsStart, Is.EqualTo(6));
            Assert.That(info.DnsEnd, Is.EqualTo(7));
            Assert.That(info.ConnectStart, Is.EqualTo(8));
            Assert.That(info.ConnectEnd, Is.EqualTo(9));
            Assert.That(info.TlsStart, Is.EqualTo(10));
            Assert.That(info.RequestStart, Is.EqualTo(11));
            Assert.That(info.ResponseStart, Is.EqualTo(12));
            Assert.That(info.ResponseEnd, Is.EqualTo(13));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
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
                      """;
        FetchTimingInfo? info = JsonSerializer.Deserialize<FetchTimingInfo>(json);
        Assert.That(info, Is.Not.Null);
        FetchTimingInfo copy = info with { };
        Assert.That(copy, Is.EqualTo(info));
    }

    [Test]
    public void TestDeserializingWithMissingTimeOriginThrows()
    {
        string json = """
                      {
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
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'timeOrigin"));
    }

    [Test]
    public void TestDeserializingWithMissingRequestTimeThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
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
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'requestTime"));
    }

    [Test]
    public void TestDeserializingWithMissingRedirectStartThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
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
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'redirectStart"));
    }

    [Test]
    public void TestDeserializingWithMissingRedirectEndThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
                        "redirectStart": 3,
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
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'redirectEnd"));
    }

    [Test]
    public void TestDeserializingWithMissingFetchStartThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
                        "redirectStart": 3,
                        "redirectEnd": 4,
                        "dnsStart": 6,
                        "dnsEnd": 7,
                        "connectStart": 8,
                        "connectEnd": 9,
                        "tlsStart": 10,
                        "requestStart": 11,
                        "responseStart": 12,
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'fetchStart"));
    }

    [Test]
    public void TestDeserializingWithMissingDnsStartThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
                        "redirectStart": 3,
                        "redirectEnd": 4,
                        "fetchStart": 5,
                        "dnsEnd": 7,
                        "connectStart": 8,
                        "connectEnd": 9,
                        "tlsStart": 10,
                        "requestStart": 11,
                        "responseStart": 12,
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'dnsStart"));
    }

    [Test]
    public void TestDeserializingWithMissingDnsEndThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
                        "redirectStart": 3,
                        "redirectEnd": 4,
                        "fetchStart": 5,
                        "dnsStart": 6,
                        "connectStart": 8,
                        "connectEnd": 9,
                        "tlsStart": 10,
                        "requestStart": 11,
                        "responseStart": 12,
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'dnsEnd"));
    }

    [Test]
    public void TestDeserializingWithMissingConnectStartThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
                        "redirectStart": 3,
                        "redirectEnd": 4,
                        "fetchStart": 5,
                        "dnsStart": 6,
                        "dnsEnd": 7,
                        "connectEnd": 9,
                        "tlsStart": 10,
                        "requestStart": 11,
                        "responseStart": 12,
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'connectStart"));
    }

    [Test]
    public void TestDeserializingWithMissingConnectEndThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
                        "redirectStart": 3,
                        "redirectEnd": 4,
                        "fetchStart": 5,
                        "dnsStart": 6,
                        "dnsEnd": 7,
                        "connectStart": 8,
                        "tlsStart": 10,
                        "requestStart": 11,
                        "responseStart": 12,
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'connectEnd"));
    }

    [Test]
    public void TestDeserializingWithMissingTlsStartThrows()
    {
        string json = """
                      {
                        "timeOrigin": 1,
                        "requestTime": 2,
                        "redirectStart": 3,
                        "redirectEnd": 4,
                        "fetchStart": 5,
                        "dnsStart": 6,
                        "dnsEnd": 7,
                        "connectStart": 8,
                        "connectEnd": 9,
                        "requestStart": 11,
                        "responseStart": 12,
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'tlsStart"));
    }

    [Test]
    public void TestDeserializingWithMissingRequestStartThrows()
    {
        string json = """
                      {
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
                        "responseStart": 12,
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'requestStart"));
    }

    [Test]
    public void TestDeserializingWithMissingResponseStartThrows()
    {
        string json = """
                      {
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
                        "responseEnd": 13
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'responseStart"));
    }

    [Test]
    public void TestDeserializingWithMissingResponseEndThrows()
    {
        string json = """
                      {
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
                        "responseStart": 12
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<FetchTimingInfo>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("was missing required properties including: 'responseEnd"));
    }
}
