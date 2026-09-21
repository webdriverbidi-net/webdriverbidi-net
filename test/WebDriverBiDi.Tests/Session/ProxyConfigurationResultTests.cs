namespace WebDriverBiDi.Session;

using System.Text.Json;

public class ProxyConfigurationResultTests
{
    [Fact]
    public void TestCanCastToSubclassType()
    {
        ProxyConfigurationResult proxyResult = DeserializeManualProxy();
        ManualProxyConfigurationResult proxyConfig = proxyResult.As<ManualProxyConfigurationResult>();
        Assert.Equal("http.proxy", proxyConfig.HttpProxy);
    }

    [Fact]
    public void TestCannotCastToImproperSubclassType()
    {
        // The cast belongs to the As<T>()/TryAs<T>() family, every member of which reports a wrong type
        // as a WebDriverBiDiException. A caller catching that type around a downcast of any other
        // discriminated result must catch this one too.
        ProxyConfigurationResult proxyResult = DeserializeManualProxy();
        Assert.Contains("cannot be cast", Assert.ThrowsAny<WebDriverBiDiException>(() => proxyResult.As<PacProxyConfigurationResult>()).Message);
    }

    [Fact]
    public void TestCastToBaseTypeSucceeds()
    {
        // T is constrained to ProxyConfigurationResult, which every result satisfies, so the base type
        // is a legal cast target and must not be treated as a mismatch.
        ProxyConfigurationResult proxyResult = DeserializeManualProxy();
        Assert.Same(proxyResult, proxyResult.As<ProxyConfigurationResult>());
    }

    [Fact]
    public void TestTryCastToSubclassTypeReturnsTrue()
    {
        ProxyConfigurationResult proxyResult = DeserializeManualProxy();
        bool result = proxyResult.TryAs(out ManualProxyConfigurationResult? proxyConfig);
        Assert.True(result);
        Assert.NotNull(proxyConfig);
        Assert.Equal("http.proxy", proxyConfig.HttpProxy);
    }

    [Fact]
    public void TestTryCastToImproperSubclassTypeReturnsFalse()
    {
        ProxyConfigurationResult proxyResult = DeserializeManualProxy();
        bool result = proxyResult.TryAs(out PacProxyConfigurationResult? proxyConfig);
        Assert.False(result);
        Assert.Null(proxyConfig);
    }

    // The ProxyConfigurationResult constructor is internal, so an instance is obtained the way a consumer
    // gets one: by deserializing the capabilities of a session.
    private static ProxyConfigurationResult DeserializeManualProxy()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "manual",
                          "httpProxy": "http.proxy"
                        },
                        "setWindowRect": true
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<ManualProxyConfigurationResult>(proxyResult);
        return proxyResult;
    }
}
