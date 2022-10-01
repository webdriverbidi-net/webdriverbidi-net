namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[TestFixture]
public class CapabilitiesResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCertificates"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
        Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
        Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
        Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
        Assert.That(result!.Proxy, Is.Not.Null);
        Assert.That(result!.Proxy.HttpProxy, Is.EqualTo("http.proxy"));
        Assert.That(result.SetWindowRect, Is.EqualTo(true));
        Assert.That(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
    }

    [Test]
    public void TestCanDeserializeWithEmptyProxy()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCertificates"": true, ""proxy"": { }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        CapabilitiesResult? result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
        Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
        Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
        Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
        Assert.That(result!.Proxy, Is.Not.Null);
        Assert.That(result.SetWindowRect, Is.EqualTo(true));
        Assert.That(result.AdditionalCapabilities.ContainsKey("capName"));
        Assert.That(result.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
    }
    
    [Test]
    public void TestCanDeserializeWithNoAdditionalProperties()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCertificates"": true, ""proxy"": { }, ""setWindowRect"": true }";
        CapabilitiesResult? result = JsonConvert.DeserializeObject<CapabilitiesResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AcceptInsecureCertificates, Is.EqualTo(true));
        Assert.That(result!.BrowserName, Is.EqualTo("greatBrowser"));
        Assert.That(result!.BrowserVersion, Is.EqualTo("101.5b"));
        Assert.That(result!.PlatformName, Is.EqualTo("otherOS"));
        Assert.That(result!.Proxy, Is.Not.Null);
        Assert.That(result.SetWindowRect, Is.EqualTo(true));
        Assert.That(result.AdditionalCapabilities.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestDeserializingWithMissingBrowserNameThrows()
    {
        string json = @"{ ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCertificates"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingBrowserVersionThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""platformName"": ""otherOS"", ""acceptInsecureCertificates"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingPlatformNameThrows()
    {
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""acceptInsecureCertificates"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" }";
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
        string json = @"{ ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCertificates"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""capName"": ""capValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<CapabilitiesResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}