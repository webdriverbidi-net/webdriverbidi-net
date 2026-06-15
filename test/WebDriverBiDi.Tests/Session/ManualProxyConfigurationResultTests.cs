namespace WebDriverBiDi.Session;

using System.Text.Json;

public class ManualProxyConfigurationResultTests
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
                          "proxyType": "manual",
                          "httpProxy": "http.proxy",
                          "sslProxy": "ssl.proxy",
                          "socksProxy": "socks.proxy",
                          "socksVersion": 5,
                          "noProxy": [ "example.com" ]
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<ManualProxyConfigurationResult>(proxyResult);
        ManualProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<ManualProxyConfigurationResult>();

        Assert.Equal(ProxyType.Manual, proxyConfig.ProxyType);
        Assert.Equal("http.proxy", proxyConfig.HttpProxy);
        Assert.Equal("ssl.proxy", proxyConfig.SslProxy);
        Assert.Equal("socks.proxy", proxyConfig.SocksProxy);
        Assert.Equal(5, proxyConfig.SocksVersion);
        Assert.NotNull(proxyConfig.NoProxyAddresses);
        Assert.Single(proxyConfig.NoProxyAddresses);
        Assert.Empty(proxyConfig.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializeWithNullNoProxyList()
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
                          "proxyType": "manual",
                          "httpProxy": "http.proxy",
                          "sslProxy": "ssl.proxy",
                          "socksProxy": "socks.proxy",
                          "socksVersion": 5
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<ManualProxyConfigurationResult>(proxyResult);
        ManualProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<ManualProxyConfigurationResult>();

        Assert.Equal(ProxyType.Manual, proxyConfig.ProxyType);
        Assert.Equal("http.proxy", proxyConfig.HttpProxy);
        Assert.Equal("ssl.proxy", proxyConfig.SslProxy);
        Assert.Equal("socks.proxy", proxyConfig.SocksProxy);
        Assert.Equal(5, proxyConfig.SocksVersion);
        Assert.Null(proxyConfig.NoProxyAddresses);
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
                          "proxyType": "manual",
                          "httpProxy": "http.proxy",
                          "sslProxy": "ssl.proxy",
                          "socksProxy": "socks.proxy",
                          "socksVersion": 5,
                          "noProxy": [ "example.com" ]
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<ManualProxyConfigurationResult>(proxyResult);
        ManualProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<ManualProxyConfigurationResult>();
        ManualProxyConfigurationResult copy = proxyConfig with { };
        Assert.Equal(proxyConfig, copy);
    }
}
