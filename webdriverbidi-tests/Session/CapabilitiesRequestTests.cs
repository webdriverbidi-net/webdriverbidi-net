namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ExceptionDetailsTests
{
    [Test]
    public void TestCanSerialize()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestCanSerializeWithBrowserName()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.BrowserName = "greatBrowser";
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("browserName"));
        Assert.That(result["browserName"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(result["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
    }

    [Test]
    public void TestCanSerializeWithBrowserVersion()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.BrowserVersion = "101.5b";
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("browserVersion"));
        Assert.That(result["browserVersion"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(result["browserVersion"]!.Value<string>(), Is.EqualTo("101.5b"));
    }

    [Test]
    public void TestCanSerializeWithPlatformName()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.PlatformName = "oddOS";
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("platformName"));
        Assert.That(result["platformName"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(result["platformName"]!.Value<string>(), Is.EqualTo("oddOS"));
    }

    [Test]
    public void TestCanSerializeWithAcceptInsecureCertificatesTrue()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.AcceptInsecureCertificates = true;
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("acceptInsecureCerts"));
        Assert.That(result["acceptInsecureCerts"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(result["acceptInsecureCerts"]!.Value<bool>(), Is.True);
    }

    [Test]
    public void TestCanSerializeWithAcceptInsecureCertificatesFalse()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.AcceptInsecureCertificates = false;
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("acceptInsecureCerts"));
        Assert.That(result["acceptInsecureCerts"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(result["acceptInsecureCerts"]!.Value<bool>(), Is.False);
    }

    [Test]
    public void TestCanSerializeWithEmptyProxy()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.Proxy = Proxy.EmptyProxy;
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("proxy"));
        Assert.That(result["proxy"]!.Type, Is.EqualTo(JTokenType.Object));
        JObject? proxyObject = result["proxy"] as JObject;
        Assert.That(proxyObject!.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestCanSerializeWithProxy()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.Proxy = new Proxy() { HttpProxy = "http.proxy" };
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("proxy"));
        Assert.That(result["proxy"]!.Type, Is.EqualTo(JTokenType.Object));
        JObject? proxyObject = result["proxy"] as JObject;
        Assert.That(proxyObject!.Count, Is.EqualTo(1));
        Assert.That(proxyObject.ContainsKey("httpProxy"));
        Assert.That(proxyObject["httpProxy"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(proxyObject["httpProxy"]!.Value<string>(), Is.EqualTo("http.proxy"));
    }

    [Test]
    public void TestCanSerializeWithAdditionalCapabilities()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.AdditionalCapabilities["capName"] = "capValue";
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("capName"));
        Assert.That(result["capName"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(result["capName"]!.Value<string>(), Is.EqualTo("capValue"));
    }

    [Test]
    public void TestCanSerializeWithAdditionalCapabilitiesObject()
    {
        CapabilitiesRequest capabilities = new CapabilitiesRequest();
        capabilities.AdditionalCapabilities["additional"] = new Dictionary<string, object?>() { { "capName", "capValue" } };
        string json = JsonConvert.SerializeObject(capabilities);
        JObject result = JObject.Parse(json);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result.ContainsKey("additional"));
        Assert.That(result["additional"]!.Type, Is.EqualTo(JTokenType.Object));
        JObject? additionalObject = result["additional"] as JObject;
        Assert.That(additionalObject!.Count, Is.EqualTo(1));
        Assert.That(additionalObject.ContainsKey("capName"));
        Assert.That(additionalObject["capName"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(additionalObject["capName"]!.Value<string>(), Is.EqualTo("capValue"));
    }
}