namespace WebDriverBiDi.Session;

using System.Runtime;
using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class CapabilitiesResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.Proxy, Is.Null);
            Assert.That(result.UnhandledPromptBehavior, Is.Null);
            Assert.That(result.WebSocketUrl, Is.Null);
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithWebSocketUrl()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""setWindowRect"": true, ""webSocketUrl"": ""ws://socket:1234"", ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.Proxy, Is.Null);
            Assert.That(result!.UnhandledPromptBehavior, Is.Null);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.WebSocketUrl, Is.EqualTo("ws://socket:1234"));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeManualWithFullProxyProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""manual"", ""httpProxy"": ""http.proxy"", ""sslProxy"": ""ssl.proxy"", ""ftpProxy"": ""ftp.proxy"", ""socksProxy"": ""socks.proxy"", ""socksVersion"": 5, ""noProxy"": [ ""example.com"" ] }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.Proxy, Is.Not.Null);
            ManualProxyConfigurationResult proxyResult = result!.Proxy!.ProxyConfigurationResultAs<ManualProxyConfigurationResult>();
            Assert.That(proxyResult.ProxyType, Is.EqualTo(ProxyType.Manual));
            Assert.That(proxyResult.HttpProxy, Is.EqualTo("http.proxy"));
            Assert.That(proxyResult.SslProxy, Is.EqualTo("ssl.proxy"));
            Assert.That(proxyResult.FtpProxy, Is.EqualTo("ftp.proxy"));
            Assert.That(proxyResult.SocksProxy, Is.EqualTo("socks.proxy"));
            Assert.That(proxyResult.SocksVersion, Is.EqualTo(5));
            Assert.That(proxyResult.NoProxyAddresses, Has.Count.EqualTo(1));
            Assert.That(proxyResult.AdditionalData, Is.Empty);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeProxyAutoConfigWithFullProxyProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""pac"", ""proxyAutoconfigUrl"": ""proxy.autoconfig.url"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.Proxy, Is.Not.Null);
            PacProxyConfigurationResult proxyResult = result!.Proxy!.ProxyConfigurationResultAs<PacProxyConfigurationResult>();
            Assert.That(proxyResult.ProxyType, Is.EqualTo(ProxyType.ProxyAutoConfig));
            Assert.That(proxyResult.ProxyAutoConfigUrl, Is.EqualTo("proxy.autoconfig.url"));
            Assert.That(proxyResult.AdditionalData, Is.Empty);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeAutoDetectWithFullProxyProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""autodetect"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.Proxy, Is.Not.Null);
            AutoDetectProxyConfigurationResult proxyResult = result!.Proxy!.ProxyConfigurationResultAs<AutoDetectProxyConfigurationResult>();
            Assert.That(proxyResult.ProxyType, Is.EqualTo(ProxyType.AutoDetect));
            Assert.That(proxyResult.AdditionalData, Is.Empty);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeSystemWithFullProxyProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""system"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.Proxy, Is.Not.Null);
            SystemProxyConfigurationResult proxyResult = result!.Proxy!.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
            Assert.That(proxyResult.ProxyType, Is.EqualTo(ProxyType.System));
            Assert.That(proxyResult.AdditionalData, Is.Empty);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeDirectWithFullProxyProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""direct"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.Proxy, Is.Not.Null);
            DirectProxyConfigurationResult proxyResult = result!.Proxy!.ProxyConfigurationResultAs<DirectProxyConfigurationResult>();
            Assert.That(proxyResult.ProxyType, Is.EqualTo(ProxyType.Direct));
            Assert.That(proxyResult.AdditionalData, Is.Empty);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithProxyContainingAdditionalData()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""system"", ""additionalName"": ""additionalValue"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.Proxy, Is.Not.Null);
            SystemProxyConfigurationResult proxyResult = result!.Proxy!.ProxyConfigurationResultAs<SystemProxyConfigurationResult>();
            Assert.That(proxyResult.ProxyType, Is.EqualTo(ProxyType.System));
            Assert.That(proxyResult.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(proxyResult.AdditionalData, Contains.Key("additionalName"));
            Assert.That(proxyResult.AdditionalData["additionalName"], Is.InstanceOf<string>());
            Assert.That(proxyResult.AdditionalData["additionalName"], Is.EqualTo("additionalValue"));
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithInvalidProxyTypeThrows()
    {
        // spell-checker: disable
        // Disable spell checker for specifically disallowed value proxyautocomplete
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""proxyautoconfig"", ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("value 'proxyautoconfig' is not valid for enum type"));
        // spell-checker: enable
    }

    [Test]
    public void TestCanDeserializeWithFullUnhandledPromptBehavior()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""setWindowRect"": true, ""unhandledPromptBehavior"": { ""default"": ""accept"", ""alert"": ""accept"", ""confirm"": ""dismiss"", ""prompt"": ""dismiss"", ""beforeunload"": ""ignore"" } }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.UnhandledPromptBehavior, Is.Not.Null);
            UserPromptHandlerResult userPromptHandler = result!.UnhandledPromptBehavior!;
            Assert.That(userPromptHandler.Default, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(userPromptHandler.Alert, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(userPromptHandler.Confirm, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(userPromptHandler.Prompt, Is.EqualTo(UserPromptHandlerType.Dismiss));
            Assert.That(userPromptHandler.BeforeUnload, Is.EqualTo(UserPromptHandlerType.Ignore));
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanDeserializeWithNoUnhandledPromptBehavior()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""setWindowRect"": true, ""unhandledPromptBehavior"": { } }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.UnhandledPromptBehavior, Is.Not.Null);
            UserPromptHandlerResult userPromptHandler = result!.UnhandledPromptBehavior!;
            Assert.That(userPromptHandler.Default, Is.Null);
            Assert.That(userPromptHandler.Alert, Is.Null);
            Assert.That(userPromptHandler.Confirm, Is.Null);
            Assert.That(userPromptHandler.Prompt, Is.Null);
            Assert.That(userPromptHandler.BeforeUnload, Is.Null);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanDeserializeWithPartialUnhandledPromptBehavior()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""setWindowRect"": true, ""unhandledPromptBehavior"": { ""default"": ""accept"" } }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result!.UnhandledPromptBehavior, Is.Not.Null);
            UserPromptHandlerResult userPromptHandler = result!.UnhandledPromptBehavior!;
            Assert.That(userPromptHandler.Default, Is.EqualTo(UserPromptHandlerType.Accept));
            Assert.That(userPromptHandler.Alert, Is.Null);
            Assert.That(userPromptHandler.Confirm, Is.Null);
            Assert.That(userPromptHandler.Prompt, Is.Null);
            Assert.That(userPromptHandler.BeforeUnload, Is.Null);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanDeserializingWithInvalidUnhandledPromptBehaviorThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""setWindowRect"": true, ""unhandledPromptBehavior"": ""accept"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithNoAdditionalProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""system"" }, ""setWindowRect"": true }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result.UserAgent, Is.EqualTo("WebDriverBidi.NET/1.0"));
            Assert.That(result.Proxy, Is.Not.Null);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Is.Empty);
        });
    }

    [Test]
    public void TestDeserializingWithMissingBrowserNameThrows()
    {
        string json = @"{ ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingBrowserVersionThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingPlatformNameThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingUserAgentThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingAcceptInsecureCertificatesThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingSetWindowRectThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidProxyTypeThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": ""invalidProxyType"", ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidUnhandledPromptTypeThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""unhandledPromptBehavior"": ""invalidProxyType"", ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidWebSocketUrlTypeThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""webSocketUrl"": 123, ""capName"": ""capValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
    
    [Test]
    public void TestProxyTypeIsOptional()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""userAgent"": ""WebDriverBidi.NET/1.0"", ""acceptInsecureCerts"": true, ""proxy"": {}, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonSerializer.Deserialize<CapabilitiesResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Proxy, Is.Null);
    }
}
