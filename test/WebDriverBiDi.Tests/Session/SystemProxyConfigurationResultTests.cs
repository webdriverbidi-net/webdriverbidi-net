namespace WebDriverBiDi.Session;

using System.Text.Json;

public class SystemProxyConfigurationResultTests
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
    public void TestCanDeserializeWithAdditionalData()
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
