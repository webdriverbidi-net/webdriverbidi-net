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
        ManualProxyConfigurationResult proxyConfig = proxyResult.As<ManualProxyConfigurationResult>();

        Assert.Equal(ProxyType.Manual, proxyConfig.ProxyType);
        Assert.Equal("http.proxy", proxyConfig.HttpProxy);
        Assert.Equal("ssl.proxy", proxyConfig.SslProxy);
        Assert.Equal("socks.proxy", proxyConfig.SocksProxy);
        Assert.Equal(5UL, proxyConfig.SocksVersion);
        Assert.NotNull(proxyConfig.NoProxyAddresses);
        Assert.Single(proxyConfig.NoProxyAddresses);
        Assert.Empty(proxyConfig.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializeWithMissingNoProxyList()
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
        ManualProxyConfigurationResult proxyConfig = proxyResult.As<ManualProxyConfigurationResult>();

        Assert.Equal(ProxyType.Manual, proxyConfig.ProxyType);
        Assert.Equal("http.proxy", proxyConfig.HttpProxy);
        Assert.Equal("ssl.proxy", proxyConfig.SslProxy);
        Assert.Equal("socks.proxy", proxyConfig.SocksProxy);
        Assert.Equal(5UL, proxyConfig.SocksVersion);
        // Omitting noProxy and sending an empty array mean the same thing to the remote end, so a
        // payload without the member reports an empty list rather than null.
        Assert.Empty(proxyConfig.NoProxyAddresses);
        Assert.Empty(proxyConfig.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializeWithNullNoProxyList()
    {
        // The protocol defines no array member whose value may be null, so this payload is malformed;
        // it is accepted as "no addresses" rather than faulting the session, and the projection stays
        // non-null either way. ProxyConfigurationResult constructor is internal; go through
        // CapabilitiesResult deserialization.
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
                          "noProxy": null
                        },
                        "setWindowRect": true
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        ManualProxyConfigurationResult proxyConfig = proxyResult.As<ManualProxyConfigurationResult>();

        Assert.Equal("http.proxy", proxyConfig.HttpProxy);
        Assert.Empty(proxyConfig.NoProxyAddresses);
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
        ManualProxyConfigurationResult proxyConfig = proxyResult.As<ManualProxyConfigurationResult>();
        ManualProxyConfigurationResult copy = proxyConfig with { };
        Assert.Equal(proxyConfig, copy);
    }
}
