namespace WebDriverBiDi.Session;

using Newtonsoft.Json;

[TestFixture]
public class NewCommandResultTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""sessionId"": ""mySession"", ""capabilities"": { ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" } }";
        NewCommandResult? result = JsonConvert.DeserializeObject<NewCommandResult>(json);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.SessionId, Is.EqualTo("mySession"));
            Assert.That(result.Capabilities.BrowserName, Is.EqualTo("greatBrowser"));
            Assert.That(result.Capabilities.BrowserVersion, Is.EqualTo("101.5b"));
            Assert.That(result.Capabilities.PlatformName, Is.EqualTo("otherOS"));
            Assert.That(result.Capabilities.AcceptInsecureCertificates, Is.EqualTo(true));
            Assert.That(result.Capabilities.Proxy, Is.Not.Null);
            Assert.That(result.Capabilities.Proxy.HttpProxy, Is.EqualTo("http.proxy"));
            Assert.That(result.Capabilities.SetWindowRect, Is.EqualTo(true));
            Assert.That(result.Capabilities.AdditionalCapabilities, Has.Count.EqualTo(1));
            Assert.That(result.Capabilities.AdditionalCapabilities, Contains.Key("capName"));
            Assert.That(result.Capabilities.AdditionalCapabilities["capName"], Is.Not.Null);
            Assert.That(result.Capabilities.AdditionalCapabilities["capName"], Is.EqualTo("capValue"));
        });
    }

    [Test]
    public void TestDeserializingWithMissingSessionIdThrows()
    {
        string json = @"{ ""capabilities"": { ""browserName"": ""greatBrowser"", ""browserVersion"": ""101.5b"", ""platformName"": ""otherOS"", ""acceptInsecureCerts"": true, ""proxy"": { ""httpProxy"": ""http.proxy"" }, ""setWindowRect"": true, ""capName"": ""capValue"" } }";
        Assert.That(() => JsonConvert.DeserializeObject<NewCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializingWithMissingCapabilitiesThrows()
    {
        string json = @"{ ""sessionId"": ""mySession"" }";
        Assert.That(() => JsonConvert.DeserializeObject<NewCommandResult>(json), Throws.InstanceOf<JsonSerializationException>());
    }
}
