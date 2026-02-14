namespace WebDriverBiDi.Session;

using System.Text.Json;

[TestFixture]
public class ProxyConfigurationResultTests
{
    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<ManualProxyConfigurationResult>());
        ManualProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<ManualProxyConfigurationResult>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(proxyConfig.ProxyType, Is.EqualTo(ProxyType.Manual));
            Assert.That(proxyConfig.HttpProxy, Is.EqualTo("http.proxy"));
            Assert.That(proxyConfig.SslProxy, Is.EqualTo("ssl.proxy"));
            Assert.That(proxyConfig.SocksProxy, Is.EqualTo("socks.proxy"));
            Assert.That(proxyConfig.SocksVersion, Is.EqualTo(5));
            Assert.That(proxyConfig.NoProxyAddresses, Has.Count.EqualTo(1));
            Assert.That(proxyConfig.AdditionalData, Is.Empty);
        }
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<ManualProxyConfigurationResult>());
        ManualProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<ManualProxyConfigurationResult>();
        ManualProxyConfigurationResult copy = proxyConfig with { };
        Assert.That(copy, Is.EqualTo(proxyConfig));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<PacProxyConfigurationResult>());
        PacProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<PacProxyConfigurationResult>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(proxyConfig.ProxyType, Is.EqualTo(ProxyType.ProxyAutoConfig));
            Assert.That(proxyConfig.ProxyAutoConfigUrl, Is.EqualTo("proxy.autoconfig.url"));
            Assert.That(proxyConfig.AdditionalData, Is.Empty);
        }
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<PacProxyConfigurationResult>());
        PacProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<PacProxyConfigurationResult>();
        PacProxyConfigurationResult copy = proxyConfig with { };
        Assert.That(copy, Is.EqualTo(proxyConfig));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<AutoDetectProxyConfigurationResult>());
        AutoDetectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<AutoDetectProxyConfigurationResult>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(proxyConfig.ProxyType, Is.EqualTo(ProxyType.AutoDetect));
            Assert.That(proxyConfig.AdditionalData, Is.Empty);
        }
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<AutoDetectProxyConfigurationResult>());
        AutoDetectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<AutoDetectProxyConfigurationResult>();
        AutoDetectProxyConfigurationResult copy = proxyConfig with { };
        Assert.That(copy, Is.EqualTo(proxyConfig));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<SystemProxyConfigurationResult>());
        SystemProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(proxyConfig.ProxyType, Is.EqualTo(ProxyType.System));
            Assert.That(proxyConfig.AdditionalData, Is.Empty);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        }
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<SystemProxyConfigurationResult>());
        SystemProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
        SystemProxyConfigurationResult copy = proxyConfig with { };
        Assert.That(copy, Is.EqualTo(proxyConfig));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<DirectProxyConfigurationResult>());
        DirectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<DirectProxyConfigurationResult>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(proxyConfig.ProxyType, Is.EqualTo(ProxyType.Direct));
            Assert.That(proxyConfig.AdditionalData, Is.Empty);
        }
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<DirectProxyConfigurationResult>());
        DirectProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<DirectProxyConfigurationResult>();
        DirectProxyConfigurationResult copy = proxyConfig with { };
        Assert.That(copy, Is.EqualTo(proxyConfig));
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        ProxyConfigurationResult? proxyResult = result.Proxy;
        Assert.That(proxyResult, Is.Not.Null);
        Assert.That(proxyResult, Is.InstanceOf<SystemProxyConfigurationResult>());
        SystemProxyConfigurationResult proxyConfig = proxyResult.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(proxyConfig.ProxyType, Is.EqualTo(ProxyType.System));
            Assert.That(proxyConfig.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(proxyConfig.AdditionalData, Contains.Key("additionalName"));
            Assert.That(proxyConfig.AdditionalData["additionalName"], Is.InstanceOf<string>());
            Assert.That(proxyConfig.AdditionalData["additionalName"], Is.EqualTo("additionalValue"));
        }
    }
}
