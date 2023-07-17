namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[TestFixture]
public class CapabilitiesResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.Proxy, Is.Not.Null);
            Assert.That(result!.Proxy.Type, Is.Null);
            Assert.That(result!.Proxy.HttpProxy, Is.EqualTo("http.proxy"));
            Assert.That(result!.Proxy.SslProxy, Is.Null);
            Assert.That(result!.Proxy.FtpProxy, Is.Null);
            Assert.That(result!.Proxy.SocksProxy, Is.Null);
            Assert.That(result!.Proxy.SocksVersion, Is.Null);
            Assert.That(result!.Proxy.ProxyAutoConfigUrl, Is.Null);
            Assert.That(result!.Proxy.NoProxyAddresses, Is.Null);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithFullProxyProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""manual"", ""httpProxy"": ""http.proxy"", ""sslProxy"": ""ssl.proxy"", ""ftpProxy"": ""ftp.proxy"", ""socksProxy"": ""socks.proxy"", ""socksVersion"": 5, ""proxyAutoconfigUrl"": ""http://example.com"", ""noProxy"": [ ""example.com"" ] }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result!.Proxy, Is.Not.Null);
            Assert.That(result!.Proxy.Type, Is.EqualTo(ProxyType.Manual));
            Assert.That(result!.Proxy.HttpProxy, Is.EqualTo("http.proxy"));
            Assert.That(result!.Proxy.SslProxy, Is.EqualTo("ssl.proxy"));
            Assert.That(result!.Proxy.FtpProxy, Is.EqualTo("ftp.proxy"));
            Assert.That(result!.Proxy.SocksProxy, Is.EqualTo("socks.proxy"));
            Assert.That(result!.Proxy.SocksVersion, Is.EqualTo(5));
            Assert.That(result!.Proxy.ProxyAutoConfigUrl, Is.EqualTo("http://example.com"));
            Assert.That(result!.Proxy.NoProxyAddresses, Has.Count.EqualTo(1));
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
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""proxyType"": ""proxyautoconfig"", ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("value 'proxyautoconfig' is not valid for enum type"));
        // spell-checker: ensable
    }

    [Test]
    public void TestCanDeserializeWithEmptyProxy()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result.Proxy, Is.Not.Null);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithNoAdditionalProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { }, ""setWindowRect"": true }";
        CapabilitiesResult? result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result.Proxy, Is.Not.Null);
            Assert.That(result.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.AdditionalCapabilities, Is.Empty);
        });
    }

    [Test]
    public void TestDeserializingWithMissingBrowserNameThrows()
    {
        string json = @"{ ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingBrowserVersionThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingPlatformNameThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingAcceptInsecureCertificatesThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingProxyThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingSetWindowRectThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}