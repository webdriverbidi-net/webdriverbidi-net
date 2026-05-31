namespace WebDriverBiDi.Session;

using System.Reflection;
using System.Text.Json;

public class CapabilitiesResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.True(result.SetWindowRect);
        Assert.Null(result.Proxy);
        Assert.Null(result.UnhandledPromptBehavior);
        Assert.Null(result.WebSocketUrl);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCanDeserializeWithWebSocketUrl()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "webSocketUrl": "ws://socket:1234",
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.Null(result.Proxy);
        Assert.Null(result.UnhandledPromptBehavior);
        Assert.True(result.SetWindowRect);
        Assert.Equal("ws://socket:1234", result.WebSocketUrl);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCanDeserializeManualWithFullProxyProperties()
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

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.Proxy);
        ManualProxyConfigurationResult proxyResult = result.Proxy.ProxyConfigurationResultAs<ManualProxyConfigurationResult>();
        Assert.Equal(ProxyType.Manual, proxyResult.ProxyType);
        Assert.Equal("http.proxy", proxyResult.HttpProxy);
        Assert.Equal("ssl.proxy", proxyResult.SslProxy);
        Assert.Equal("socks.proxy", proxyResult.SocksProxy);
        Assert.Equal(5, proxyResult.SocksVersion);
        Assert.NotNull(proxyResult.NoProxyAddresses);
        Assert.Single(proxyResult.NoProxyAddresses);
        Assert.Empty(proxyResult.AdditionalData);
        Assert.True(result.SetWindowRect);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCanDeserializeProxyAutoConfigWithFullProxyProperties()
    {
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

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.Proxy);
        PacProxyConfigurationResult proxyResult = result.Proxy.ProxyConfigurationResultAs<PacProxyConfigurationResult>();
        Assert.Equal(ProxyType.ProxyAutoConfig, proxyResult.ProxyType);
        Assert.Equal("proxy.autoconfig.url", proxyResult.ProxyAutoConfigUrl);
        Assert.Empty(proxyResult.AdditionalData);
        Assert.True(result.SetWindowRect);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCanDeserializeAutoDetectWithFullProxyProperties()
    {
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

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.Proxy);
        AutoDetectProxyConfigurationResult proxyResult = result.Proxy.ProxyConfigurationResultAs<AutoDetectProxyConfigurationResult>();
        Assert.Equal(ProxyType.AutoDetect, proxyResult.ProxyType);
        Assert.Empty(proxyResult.AdditionalData);
        Assert.True(result.SetWindowRect);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCanDeserializeSystemWithFullProxyProperties()
    {
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

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.Proxy);
        SystemProxyConfigurationResult proxyResult = result.Proxy.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
        Assert.Equal(ProxyType.System, proxyResult.ProxyType);
        Assert.Empty(proxyResult.AdditionalData);
        Assert.True(result.SetWindowRect);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCanDeserializeDirectWithFullProxyProperties()
    {
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

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.Proxy);
        DirectProxyConfigurationResult proxyResult = result.Proxy.ProxyConfigurationResultAs<DirectProxyConfigurationResult>();
        Assert.Equal(ProxyType.Direct, proxyResult.ProxyType);
        Assert.Empty(proxyResult.AdditionalData);
        Assert.True(result.SetWindowRect);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCanDeserializeWithProxyContainingAdditionalData()
    {
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

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.Proxy);
        SystemProxyConfigurationResult proxyResult = result.Proxy.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
        Assert.Equal(ProxyType.System, proxyResult.ProxyType);
        Assert.Single(proxyResult.AdditionalData);
        Assert.True(proxyResult.AdditionalData.ContainsKey("additionalName"));
        Assert.IsType<string>(proxyResult.AdditionalData["additionalName"]);
        Assert.Equal("additionalValue", proxyResult.AdditionalData["additionalName"]);
        Assert.True(result.SetWindowRect);
        Assert.True(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.Equal("capValue", result.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        CapabilitiesResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestCanDeserializeWithInvalidProxyTypeThrows()
    {
        // spell-checker: disable
        // Disable spell checker for specifically disallowed value proxyautocomplete
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "proxyType": "proxyautoconfig",
                          "httpProxy": "http.proxy"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.Contains("JSON for 'ProxyConfiguration' proxyType property contains unknown value 'proxyautoconfig'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json)).Message);
        // spell-checker: enable
    }

    [Fact]
    public void TestCannotDeserializeWithEmptyProxy()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {},
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.Contains("must contain a 'proxyType' property", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json)).Message);
    }

    [Fact]
    public void TestCanDeserializeWithFullUnhandledPromptBehavior()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": {
                          "default": "accept",
                          "alert": "accept",
                          "confirm": "dismiss",
                          "prompt": "dismiss",
                          "beforeunload": "ignore",
                          "file": "ignore"
                        }
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.UnhandledPromptBehavior);
        UserPromptHandlerResult userPromptHandler = result.UnhandledPromptBehavior;
        Assert.Equal(UserPromptHandlerType.Accept, userPromptHandler.Default);
        Assert.Equal(UserPromptHandlerType.Accept, userPromptHandler.Alert);
        Assert.Equal(UserPromptHandlerType.Dismiss, userPromptHandler.Confirm);
        Assert.Equal(UserPromptHandlerType.Dismiss, userPromptHandler.Prompt);
        Assert.Equal(UserPromptHandlerType.Ignore, userPromptHandler.BeforeUnload);
        Assert.Equal(UserPromptHandlerType.Ignore, userPromptHandler.File);
        Assert.True(result.SetWindowRect);
    }

    [Fact]
    public void TestCanDeserializeWithNoUnhandledPromptBehavior()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": {}
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.UnhandledPromptBehavior);
        UserPromptHandlerResult userPromptHandler = result.UnhandledPromptBehavior;
        Assert.Null(userPromptHandler.Default);
        Assert.Null(userPromptHandler.Alert);
        Assert.Null(userPromptHandler.Confirm);
        Assert.Null(userPromptHandler.Prompt);
        Assert.Null(userPromptHandler.BeforeUnload);
        Assert.True(result.SetWindowRect);
    }

    [Fact]
    public void TestCanDeserializeWithPartialUnhandledPromptBehavior()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": {
                          "default": "accept"
                        }
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.UnhandledPromptBehavior);
        UserPromptHandlerResult userPromptHandler = result.UnhandledPromptBehavior;
        Assert.Equal(UserPromptHandlerType.Accept, userPromptHandler.Default);
        Assert.Null(userPromptHandler.Alert);
        Assert.Null(userPromptHandler.Confirm);
        Assert.Null(userPromptHandler.Prompt);
        Assert.Null(userPromptHandler.BeforeUnload);
        Assert.True(result.SetWindowRect);
    }

    [Fact]
    public void TestCanDeserializingWithInvalidUnhandledPromptBehaviorThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": "accept"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestCanDeserializeWithNoAdditionalProperties()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "proxy": {
                          "proxyType": "system"
                        }
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);

        Assert.True(result.AcceptInsecureCertificates);
        Assert.Equal("greatBrowser", result.BrowserName);
        Assert.Equal("101.5b", result.BrowserVersion);
        Assert.Equal("otherOS", result.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.UserAgent);
        Assert.NotNull(result.Proxy);
        Assert.True(result.SetWindowRect);
        Assert.Empty(result.AdditionalCapabilities);
    }

    [Fact]
    public void TestDeserializingWithMissingBrowserNameThrows()
    {
        string json = """
                      {
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "httpProxy": "http.proxy"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingBrowserVersionThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "httpProxy": "http.proxy"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingPlatformNameThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "httpProxy": "http.proxy"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingUserAgentThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "httpProxy": "http.proxy"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingAcceptInsecureCertificatesThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "proxy": {
                          "httpProxy": "http.proxy"
                        },
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingSetWindowRectThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": {
                          "httpProxy": "http.proxy"
                        },
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidProxyTypeThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "proxy": "invalidProxyType",
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidUnhandledPromptTypeThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": "invalidPromptBehavior",
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestProxyReturnsCachedInstanceOnRepeatedAccess()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "proxy": {
                          "proxyType": "direct"
                        }
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ProxyConfigurationResult? first = result.Proxy;
        ProxyConfigurationResult? second = result.Proxy;
        Assert.NotNull(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void TestAdditionalCapabilitiesReturnsCachedInstanceOnRepeatedAccess()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "capName": "capValue"
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        ReceivedDataDictionary first = result.AdditionalCapabilities;
        ReceivedDataDictionary second = result.AdditionalCapabilities;
        Assert.Same(second, first);
    }

    [Fact]
    public void TestUnhandledPromptBehaviorReturnsCachedInstanceOnRepeatedAccess()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "unhandledPromptBehavior": {
                          "default": "accept"
                        }
                      }
                      """;
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
        Assert.NotNull(result);
        UserPromptHandlerResult? first = result.UnhandledPromptBehavior;
        UserPromptHandlerResult? second = result.UnhandledPromptBehavior;
        Assert.NotNull(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void TestProxyReturnsCachedInstanceForEachProxyType()
    {
        string[] proxyJsonValues = ["\"proxyType\": \"direct\"", "\"proxyType\": \"system\"", "\"proxyType\": \"autodetect\"", "\"proxyType\": \"pac\", \"proxyAutoconfigUrl\": \"http://proxy.example\"", "\"proxyType\": \"manual\", \"httpProxy\": \"http.proxy\""];
        foreach (string proxyJson in proxyJsonValues)
        {
            string json = $$"""
                          {
                            "browserName": "greatBrowser",
                            "browserVersion": "101.5b",
                            "platformName": "otherOS",
                            "userAgent": "WebDriverBidi.NET/1.0",
                            "acceptInsecureCerts": true,
                            "setWindowRect": true,
                            "proxy": { {{proxyJson}} }
                          }
                          """;
            CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json);
            Assert.NotNull(result);
            ProxyConfigurationResult? first = result.Proxy;
            ProxyConfigurationResult? second = result.Proxy;
            Assert.NotNull(first);
            Assert.Same(first, second);
        }
    }

    [Fact]
    public void TestDeserializingWithInvalidWebSocketUrlTypeThrows()
    {
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true,
                        "proxy": {
                          "proxyType": "system"
                        },
                        "webSocketUrl": 123,
                        "capName": "capValue"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CapabilitiesResult>(json));
    }

    [Fact]
    public void TestProxyReturnsNullForUnknownProxyType()
    {
        // The _ => null branch in the Proxy switch expression cannot be reached through normal
        // JSON deserialization: the DiscriminatedUnionJsonConverter throws for any unknown
        // proxyType string before control reaches the switch. This test exercises it directly
        // by injecting a ProxyConfiguration whose ProxyType is an out-of-range enum value via
        // reflection, simulating what would happen if a future spec version added a new proxy
        // type before the library was updated.
        // Deserialize without a proxy so that the Proxy cached field stays null, ensuring
        // the switch expression is entered when we inject an unknown ProxyType below.
        string json = """
                      {
                        "browserName": "greatBrowser",
                        "browserVersion": "101.5b",
                        "platformName": "otherOS",
                        "userAgent": "WebDriverBidi.NET/1.0",
                        "acceptInsecureCerts": true,
                        "setWindowRect": true
                      }
                      """;
        CapabilitiesResult result = JsonSerializer.Deserialize<CapabilitiesResult>(json)!;

        // Inject a ProxyConfiguration with an out-of-range ProxyType value, simulating a
        // future spec proxy type the library does not yet handle.
        ProxyConfiguration unknownProxy = new DirectProxyConfiguration();
        PropertyInfo proxyTypeProperty = typeof(ProxyConfiguration).GetProperty("ProxyType", BindingFlags.Public | BindingFlags.Instance)!;
        proxyTypeProperty.SetValue(unknownProxy, (ProxyType)99);

        PropertyInfo serializableProxyProperty = typeof(CapabilitiesResult).GetProperty("SerializableProxy", BindingFlags.NonPublic | BindingFlags.Instance)!;
        serializableProxyProperty.SetValue(result, unknownProxy);

        Assert.Null(result.Proxy);
    }
}
