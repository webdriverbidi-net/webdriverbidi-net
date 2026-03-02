namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Session;

[TestFixture]
public class ProxyConfigurationJsonConverterTests
{
    [Test]
    public void TestDeserializeAutodetectProxyReturnsAutoDetectProxyConfiguration()
    {
        string json = """{"proxyType": "autodetect"}""";
        ProxyConfiguration? config = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(config, Is.Not.Null);
        Assert.That(config, Is.InstanceOf<AutoDetectProxyConfiguration>());
        Assert.That(config!.ProxyType, Is.EqualTo(ProxyType.AutoDetect));
    }

    [Test]
    public void TestDeserializeDirectProxyReturnsDirectProxyConfiguration()
    {
        string json = """{"proxyType": "direct"}""";
        ProxyConfiguration? config = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(config, Is.Not.Null);
        Assert.That(config, Is.InstanceOf<DirectProxyConfiguration>());
        Assert.That(config!.ProxyType, Is.EqualTo(ProxyType.Direct));
    }

    [Test]
    public void TestDeserializeManualProxyWithFieldsReturnsManualProxyConfiguration()
    {
        string json = """
                      {"proxyType": "manual", "httpProxy": "http://proxy:8080", "sslProxy": "https://proxy:8443"}
                      """;
        ProxyConfiguration? config = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(config, Is.Not.Null);
        Assert.That(config, Is.InstanceOf<ManualProxyConfiguration>());
        ManualProxyConfiguration manualConfig = (ManualProxyConfiguration)config!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(manualConfig.ProxyType, Is.EqualTo(ProxyType.Manual));
            Assert.That(manualConfig.HttpProxy, Is.EqualTo("http://proxy:8080"));
            Assert.That(manualConfig.SslProxy, Is.EqualTo("https://proxy:8443"));
        }
    }

    [Test]
    public void TestDeserializePacProxyReturnsPacProxyConfiguration()
    {
        string json = """{"proxyType": "pac", "proxyAutoconfigUrl": "https://proxy.example.com/pac"}""";
        ProxyConfiguration? config = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(config, Is.Not.Null);
        Assert.That(config, Is.InstanceOf<PacProxyConfiguration>());
        PacProxyConfiguration pacConfig = (PacProxyConfiguration)config!;
        Assert.That(pacConfig.ProxyAutoConfigUrl, Is.EqualTo("https://proxy.example.com/pac"));
    }

    [Test]
    public void TestDeserializeSystemProxyReturnsSystemProxyConfiguration()
    {
        string json = """{"proxyType": "system"}""";
        ProxyConfiguration? config = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(config, Is.Not.Null);
        Assert.That(config, Is.InstanceOf<SystemProxyConfiguration>());
        Assert.That(config!.ProxyType, Is.EqualTo(ProxyType.System));
    }

    [Test]
    public void TestDeserializeWithMissingProxyTypeReturnsNull()
    {
        string json = """{"httpProxy": "http://proxy:8080"}""";
        ProxyConfiguration? config = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(config, Is.Null);
    }

    [Test]
    public void TestDeserializeWithNonStringProxyTypeThrowsJsonException()
    {
        string json = """{"proxyType": {}}""";
        Assert.That(() => JsonSerializer.Deserialize<ProxyConfiguration>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must be a string"));
    }

    [Test]
    public void TestDeserializeNonObjectThrowsJsonException()
    {
        string json = """["invalid proxy"]""";
        Assert.That(() => JsonSerializer.Deserialize<ProxyConfiguration>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must be an object"));
    }

    [Test]
    public void TestDeserializeCapturesAdditionalUnknownPropertiesInAdditionalData()
    {
        string json = """{"proxyType": "direct", "unknownKey": "unknownValue"}""";
        ProxyConfiguration? config = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(config, Is.Not.Null);
        Assert.That(config!.AdditionalData, Has.Count.EqualTo(1));
        Assert.That(config.AdditionalData, Contains.Key("unknownKey"));
        JsonElement value = (JsonElement)config.AdditionalData["unknownKey"]!;
        Assert.That(value.GetString(), Is.EqualTo("unknownValue"));
    }

    [Test]
    public void TestCanSerialize()
    {
        ProxyConfiguration config = new ManualProxyConfiguration
        {
            HttpProxy = "http://proxy:8080",
            SslProxy = "https://proxy:8443"
        };
        string json = JsonSerializer.Serialize(config);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
        Assert.That(serialized["httpProxy"]!.Value<string>(), Is.EqualTo("http://proxy:8080"));
        Assert.That(serialized["sslProxy"]!.Value<string>(), Is.EqualTo("https://proxy:8443"));
    }

    [Test]
    public void TestWriteWithNullValueThrowsArgumentNullException()
    {
        ProxyConfigurationJsonConverter converter = new();
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        Assert.That(() => converter.Write(writer, null!, new JsonSerializerOptions()), Throws.InstanceOf<ArgumentNullException>());
    }
}
