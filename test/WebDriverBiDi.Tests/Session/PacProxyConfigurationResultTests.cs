namespace WebDriverBiDi.Session;

using System.Text.Json;

public class PacProxyConfigurationResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        // ProxyConfigurationResult constructor is internal; go through CapabilitiesResult deserialization.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "pac",
                          "proxyAutoconfigUrl": "proxy.autoconfig.url"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<PacProxyConfigurationResult>(proxyResult);
        PacProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<PacProxyConfigurationResult>();

        Assert.Equal(ProxyType.ProxyAutoConfig, proxyConfig.ProxyType);
        Assert.Equal("proxy.autoconfig.url", proxyConfig.ProxyAutoConfigUrl);
        Assert.Empty(proxyConfig.AdditionalData);
    }

    [Fact]
    public void TestCopySemantics()
    {
        // ProxyConfigurationResult constructor is internal; go through CapabilitiesResult deserialization.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "pac",
                          "proxyAutoconfigUrl": "proxy.autoconfig.url"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<PacProxyConfigurationResult>(proxyResult);
        PacProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<PacProxyConfigurationResult>();
        PacProxyConfigurationResult copy = proxyConfig with { };
        Assert.Equal(proxyConfig, copy);
    }
}
