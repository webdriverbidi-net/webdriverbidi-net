namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CapabilityRequestTests
{
    [Fact]
    public void TestCanSerialize()
    {
        CapabilityRequest capabilities = new();
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Empty(result);
    }

    [Fact]
    public void TestCanSerializeWithBrowserName()
    {
        CapabilityRequest capabilities = new()
        {
            BrowserName = "greatBrowser"
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("browserName"));
        JToken? browserName = result["browserName"];
        Assert.NotNull(browserName);
        Assert.Equal(JTokenType.String, browserName.Type);
        Assert.Equal("greatBrowser", browserName.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithBrowserVersion()
    {
        CapabilityRequest capabilities = new()
        {
            BrowserVersion = "101.5b"
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("browserVersion"));
        JToken? browserVersion = result["browserVersion"];
        Assert.NotNull(browserVersion);
        Assert.Equal(JTokenType.String, browserVersion.Type);
        Assert.Equal("101.5b", browserVersion.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithPlatformName()
    {
        CapabilityRequest capabilities = new()
        {
            PlatformName = "oddOS"
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("platformName"));
        JToken? platformName = result["platformName"];
        Assert.NotNull(platformName);
        Assert.Equal(JTokenType.String, platformName.Type);
        Assert.Equal("oddOS", platformName.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithAcceptInsecureCertificatesTrue()
    {
        CapabilityRequest capabilities = new()
        {
            AcceptInsecureCertificates = true
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("acceptInsecureCerts"));
        JToken? acceptInsecureCerts = result["acceptInsecureCerts"];
        Assert.NotNull(acceptInsecureCerts);
        Assert.Equal(JTokenType.Boolean, acceptInsecureCerts.Type);
        Assert.True(acceptInsecureCerts.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeWithAcceptInsecureCertificatesFalse()
    {
        CapabilityRequest capabilities = new()
        {
            AcceptInsecureCertificates = false
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("acceptInsecureCerts"));
        JToken? acceptInsecureCerts = result["acceptInsecureCerts"];
        Assert.NotNull(acceptInsecureCerts);
        Assert.Equal(JTokenType.Boolean, acceptInsecureCerts.Type);
        Assert.False(acceptInsecureCerts.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeWithUnhandledPromptBehavior()
    {
        CapabilityRequest capabilities = new()
        {
            UnhandledPromptBehavior = new UserPromptHandler()
            {
                Alert = UserPromptHandlerType.Accept
            }
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("unhandledPromptBehavior"));
        JToken? unhandledPromptBehaviorToken = result["unhandledPromptBehavior"];
        Assert.NotNull(unhandledPromptBehaviorToken);
        Assert.Equal(JTokenType.Object, unhandledPromptBehaviorToken.Type);

        JObject? proxyObject = unhandledPromptBehaviorToken as JObject;
        Assert.NotNull(proxyObject);
        Assert.Single(proxyObject);

        Assert.True(proxyObject.ContainsKey("alert"));
        JToken? alert = proxyObject["alert"];
        Assert.NotNull(alert);
        Assert.Equal(JTokenType.String, alert.Type);
        Assert.Equal("accept", alert.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithProxy()
    {
        CapabilityRequest capabilities = new()
        {
            Proxy = new ManualProxyConfiguration() { HttpProxy = "http.proxy" }
        };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("proxy"));
        JToken? proxyToken = result["proxy"];
        Assert.NotNull(proxyToken);
        Assert.Equal(JTokenType.Object, proxyToken.Type);

        JObject? proxyObject = proxyToken as JObject;
        Assert.NotNull(proxyObject);
        Assert.Equal(2, proxyObject.Count);

        Assert.True(proxyObject.ContainsKey("proxyType"));
        JToken? proxyType = proxyObject["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(proxyObject.ContainsKey("httpProxy"));
        JToken? httpProxy = proxyObject["httpProxy"];
        Assert.NotNull(httpProxy);
        Assert.Equal(JTokenType.String, httpProxy.Type);
        Assert.Equal("http.proxy", httpProxy.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithAdditionalCapabilities()
    {
        CapabilityRequest capabilities = new();
        capabilities.AdditionalCapabilities["capName"] = "capValue";
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("capName"));
        JToken? capName = result["capName"];
        Assert.NotNull(capName);
        Assert.Equal(JTokenType.String, capName.Type);
        Assert.Equal("capValue", capName.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithAdditionalCapabilitiesObject()
    {
        CapabilityRequest capabilities = new();
        capabilities.AdditionalCapabilities["additional"] = new Dictionary<string, object?>() { { "capName", "capValue" } };
        string json = JsonSerializer.Serialize(capabilities);
        JObject result = JObject.Parse(json);
        Assert.Single(result);

        Assert.True(result.ContainsKey("additional"));
        JToken? additionalToken = result["additional"];
        Assert.NotNull(additionalToken);
        Assert.Equal(JTokenType.Object, additionalToken.Type);

        JObject? additionalObject = additionalToken as JObject;
        Assert.NotNull(additionalObject);
        Assert.Single(additionalObject);

        Assert.True(additionalObject.ContainsKey("capName"));
        JToken? capName = additionalObject["capName"];
        Assert.NotNull(capName);
        Assert.Equal(JTokenType.String, capName.Type);
        Assert.Equal("capValue", capName.Value<string>());
    }
}
