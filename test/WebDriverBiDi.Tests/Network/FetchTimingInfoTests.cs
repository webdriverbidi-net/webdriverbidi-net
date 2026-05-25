namespace WebDriverBiDi.Network;

using System.Text.Json;

public class FetchTimingInfoTests
{
    [Fact]
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
        Assert.NotNull(info);

        Assert.Equal(1, info.TimeOrigin);
        Assert.Equal(2, info.RequestTime);
        Assert.Equal(3, info.RedirectStart);
        Assert.Equal(4, info.RedirectEnd);
        Assert.Equal(5, info.FetchStart);
        Assert.Equal(6, info.DnsStart);
        Assert.Equal(7, info.DnsEnd);
        Assert.Equal(8, info.ConnectStart);
        Assert.Equal(9, info.ConnectEnd);
        Assert.Equal(10, info.TlsStart);
        Assert.Equal(11, info.RequestStart);
        Assert.Equal(12, info.ResponseStart);
        Assert.Equal(13, info.ResponseEnd);
    }

    [Fact]
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
        Assert.NotNull(info);
        FetchTimingInfo copy = info with { };
        Assert.Equal(info, copy);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'timeOrigin", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'requestTime", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'redirectStart", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'redirectEnd", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'fetchStart", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'dnsStart", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'dnsEnd", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'connectStart", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'connectEnd", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'tlsStart", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'requestStart", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'responseStart", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("was missing required properties including: 'responseEnd", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FetchTimingInfo>(json)).Message);
    }
}
