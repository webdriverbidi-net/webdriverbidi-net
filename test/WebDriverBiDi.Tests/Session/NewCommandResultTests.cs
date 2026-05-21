namespace WebDriverBiDi.Session;

using System.Text.Json;

public class NewCommandResultTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "sessionId": "mySession",
                        "capabilities": {
                          "browserName": "greatBrowser",
                          "browserVersion": "101.5b",
                          "platformName": "otherOS",
                          "userAgent": "WebDriverBidi.NET/1.0",
                          "acceptInsecureCerts": true,
                          "proxy": {
                            "proxyType": "manual",
                            "httpProxy": "http.proxy"
                          },
                          "setWindowRect": true,
                          "capName": "capValue"
                        }
                      }
                      """;
        NewCommandResult? result = JsonSerializer.Deserialize<NewCommandResult>(json);
        Assert.NotNull(result);

        Assert.Equal("mySession", result.SessionId);
        Assert.Equal("greatBrowser", result.Capabilities.BrowserName);
        Assert.Equal("101.5b", result.Capabilities.BrowserVersion);
        Assert.Equal("otherOS", result.Capabilities.PlatformName);
        Assert.Equal("WebDriverBidi.NET/1.0", result.Capabilities.UserAgent);
        Assert.True(result.Capabilities.AcceptInsecureCertificates);
        Assert.NotNull(result.Capabilities.Proxy);
        ProxyConfigurationResult proxyResult = result.Capabilities.Proxy;
        Assert.Equal(ProxyType.Manual, proxyResult.ProxyType);
        Assert.Equal("http.proxy", proxyResult.ProxyConfigurationResultAs<ManualProxyConfigurationResult>().HttpProxy);
        Assert.True(result.Capabilities.SetWindowRect);
        Assert.Single(result.Capabilities.AdditionalCapabilities);
        Assert.True(result.Capabilities.AdditionalCapabilities.ContainsKey("capName"));
        Assert.NotNull(result.Capabilities.AdditionalCapabilities["capName"]);
        Assert.Equal("capValue", result.Capabilities.AdditionalCapabilities["capName"]);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "sessionId": "mySession",
                        "capabilities": {
                          "browserName": "greatBrowser",
                          "browserVersion": "101.5b",
                          "platformName": "otherOS",
                          "userAgent": "WebDriverBidi.NET/1.0",
                          "acceptInsecureCerts": true,
                          "proxy": {
                            "proxyType": "manual",
                            "httpProxy": "http.proxy"
                          },
                          "setWindowRect": true,
                          "capName": "capValue"
                        }
                      }
                      """;
        NewCommandResult? result = JsonSerializer.Deserialize<NewCommandResult>(json);
        Assert.NotNull(result);
        NewCommandResult copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingSessionIdThrows()
    {
        string json = """
                      {
                        "capabilities": {
                          "browserName": "greatBrowser",
                          "browserVersion": "101.5b",
                          "platformName": "otherOS",
                          "userAgent": "WebDriverBidi.NET/1.0",
                          "acceptInsecureCerts": true,
                          "proxy": {
                            "proxyType": "manual",
                            "httpProxy": "http.proxy"
                          },
                          "setWindowRect": true,
                          "capName": "capValue"
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NewCommandResult>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingCapabilitiesThrows()
    {
        string json = """
                      {
                        "sessionId": "mySession"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NewCommandResult>(json));
    }
}
