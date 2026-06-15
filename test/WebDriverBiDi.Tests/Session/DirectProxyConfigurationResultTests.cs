namespace WebDriverBiDi.Session;

using System.Text.Json;

public class DirectProxyConfigurationResultTests
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
                          "proxyType": "direct"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<DirectProxyConfigurationResult>(proxyResult);
        DirectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<DirectProxyConfigurationResult>();

        Assert.Equal(ProxyType.Direct, proxyConfig.ProxyType);
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
                          "proxyType": "direct"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<DirectProxyConfigurationResult>(proxyResult);
        DirectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<DirectProxyConfigurationResult>();
        DirectProxyConfigurationResult copy = proxyConfig with { };
        Assert.Equal(proxyConfig, copy);
    }
}
