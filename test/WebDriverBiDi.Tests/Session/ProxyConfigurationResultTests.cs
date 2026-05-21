namespace WebDriverBiDi.Session;

using System.Text.Json;

public class ProxyConfigurationResultTests
{
    [Fact]
    public void TestCanDeserializeManualProxyConfigurationResult()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
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
    public void TestCanDeserializeManualProxyConfigurationResultWithNullNoProxyList()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
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
    public void TestManualProxyConfigurationResultCopySemantics()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
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

    [Fact]
    public void TestCanDeserializeProxyAutoConfigProxyConfigurationResult()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
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
    public void TestCanDeserializeProxyAutoConfigProxyConfigurationResultCopySemantics()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
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

    [Fact]
    public void TestCanDeserializeAutoDetectProxyConfigurationResult()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "autodetect"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<AutoDetectProxyConfigurationResult>(proxyResult);
        AutoDetectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<AutoDetectProxyConfigurationResult>();

        Assert.Equal(ProxyType.AutoDetect, proxyConfig.ProxyType);
        Assert.Empty(proxyConfig.AdditionalData);
    }

    [Fact]
    public void TestAutoDetectProxyConfigurationResultCopySemantics()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "autodetect"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<AutoDetectProxyConfigurationResult>(proxyResult);
        AutoDetectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<AutoDetectProxyConfigurationResult>();
        AutoDetectProxyConfigurationResult copy = proxyConfig with { };
        Assert.Equal(proxyConfig, copy);
    }

    [Fact]
    public void TestCanDeserializeSystemProxyConfigurationResult()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "system"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<SystemProxyConfigurationResult>(proxyResult);
        SystemProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();

        Assert.Equal(ProxyType.System, proxyConfig.ProxyType);
        Assert.Empty(proxyConfig.AdditionalData);
        Assert.True(result.SetWindowRect);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestSystemProxyConfigurationResultCopySemantics()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "system"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<SystemProxyConfigurationResult>(proxyResult);
        SystemProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
        SystemProxyConfigurationResult copy = proxyConfig with { };
        Assert.Equal(proxyConfig, copy);
    }

    [Fact]
    public void TestCanDeserializeDirectProxyConfigurationResult()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
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
    public void TestDirectProxyConfigurationResultCopySemantics()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
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

    [Fact]
    public void TestCanDeserializeWithProxyContainingAdditionalData()
    {
        // ProxyConfigurationResult constructor is internal, and the only
        // place it is called is in the deserialization of a CapabilitiesResult,
        // so we will go through that mechanism to get the ProxyConfigurationResult.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "system",
                          "additionalName": "additionalValue"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.NotNull(proxyResult);
        Assert.IsType<SystemProxyConfigurationResult>(proxyResult);
        SystemProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();

        Assert.Equal(ProxyType.System, proxyConfig.ProxyType);
        Assert.Single(proxyConfig.AdditionalData);
        Assert.True(proxyConfig.AdditionalData.ContainsKey("additionalName"));
        Assert.IsType<string>(proxyConfig.AdditionalData["additionalName"]);
        Assert.Equal("additionalValue", proxyConfig.AdditionalData["additionalName"]);
    }
}
