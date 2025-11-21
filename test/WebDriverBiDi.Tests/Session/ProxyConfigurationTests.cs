namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ProxyConfigurationTests
{
    [Test]
    public void TestCanSerializeWithHttpProxy()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            HttpProxy = "http.proxy"
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
            Assert.That(serialized, Contains.Key("httpProxy"));
            Assert.That(serialized["httpProxy"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["httpProxy"]!.Value<string>(), Is.EqualTo("http.proxy"));
        });
    }

    [Test]
    public void TestCanSerializeWithSslProxy()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            SslProxy = "ssl.proxy"
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
            Assert.That(serialized, Contains.Key("sslProxy"));
            Assert.That(serialized["sslProxy"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sslProxy"]!.Value<string>(), Is.EqualTo("ssl.proxy"));
        });
    }

    [Test]
    public void TestCanSerializeWithSocksProxy()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            SocksProxy = "socks.proxy"
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
            Assert.That(serialized, Contains.Key("socksProxy"));
            Assert.That(serialized["socksProxy"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["socksProxy"]!.Value<string>(), Is.EqualTo("socks.proxy"));
        });
    }

    [Test]
    public void TestCanSerializeWithSocksVersion()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            SocksVersion = 4
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
            Assert.That(serialized, Contains.Key("socksVersion"));
            Assert.That(serialized["socksVersion"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["socksVersion"]!.Value<long>(), Is.EqualTo(4));
        });
    }

    [Test]
    public void TestCanSerializeWithNoProxyAddresses()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            NoProxyAddresses = ["no.proxy.address"]
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
            Assert.That(serialized, Contains.Key("noProxy"));
            Assert.That(serialized["noProxy"]!.Type, Is.EqualTo(JTokenType.Array));
        });
        JArray? noProxyArray = serialized["noProxy"] as JArray;
        Assert.That(noProxyArray, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(noProxyArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(noProxyArray![0].Value<string>(), Is.EqualTo("no.proxy.address"));
        });
    }

    [Test]
    public void TestCanSerializeWithEmptyNoProxyAddresses()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            NoProxyAddresses = []
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
            Assert.That(serialized, Contains.Key("noProxy"));
            Assert.That(serialized["noProxy"]!.Type, Is.EqualTo(JTokenType.Array));
        });
        JArray? noProxyArray = serialized["noProxy"] as JArray;
        Assert.That(noProxyArray, Is.Empty);
    }

    [Test]
    public void TestCanSerializeWithProxyTypeDirect()
    {
        ProxyConfiguration proxy = new DirectProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("direct"));
        });
    }

    [Test]
    public void TestCanSerializeWithProxyTypeManual()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("manual"));
        });
    }

    [Test]
    public void TestCanSerializeWithProxyTypeSystem()
    {
        ProxyConfiguration proxy = new SystemProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("system"));
        });
    }

    [Test]
    public void TestCanSerializeWithProxyTypeAutoDetect()
    {
        ProxyConfiguration proxy = new AutoDetectProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("autodetect"));
        });
    }

    [Test]
    public void TestCanSerializeWithProxyTypeProxyAutoconfig()
    {
        ProxyConfiguration proxy = new PacProxyConfiguration("proxy.autoconfig.url");
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("pac"));
            Assert.That(serialized, Contains.Key("proxyAutoconfigUrl"));
            Assert.That(serialized["proxyAutoconfigUrl"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyAutoconfigUrl"]!.Value<string>(), Is.EqualTo("proxy.autoconfig.url"));
        });
    }

    [Test]
    public void TestCanSerializeWithAdditionalData()
    {
        ProxyConfiguration proxy = new DirectProxyConfiguration();
        proxy.AdditionalData["additionalName"] = "additionalValue";
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("proxyType"));
            Assert.That(serialized["proxyType"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["proxyType"]!.Value<string>(), Is.EqualTo("direct"));
            Assert.That(serialized, Contains.Key("additionalName"));
            Assert.That(serialized["additionalName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["additionalName"]!.Value<string>(), Is.EqualTo("additionalValue"));
        });
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeManual()
    {
        string json = """
                      {
                        "proxyType": "manual"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!.ProxyType, Is.EqualTo(ProxyType.Manual));
            Assert.That(deserialized, Is.InstanceOf<ManualProxyConfiguration>());
            ManualProxyConfiguration deserializedResult = (ManualProxyConfiguration)deserialized;
            Assert.That(deserializedResult.HttpProxy, Is.Null);
            Assert.That(deserializedResult.SslProxy, Is.Null);
            Assert.That(deserializedResult.SocksProxy, Is.Null);
            Assert.That(deserializedResult.SocksVersion, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeSystem()
    {
        string json = """
                      {
                        "proxyType": "system"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!.ProxyType, Is.EqualTo(ProxyType.System));
            Assert.That(deserialized, Is.InstanceOf<SystemProxyConfiguration>());
        });
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeAutoDetect()
    {
        string json = """
                      {
                        "proxyType": "autodetect"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!.ProxyType, Is.EqualTo(ProxyType.AutoDetect));
            Assert.That(deserialized, Is.InstanceOf<AutoDetectProxyConfiguration>());
        });
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeProxyAutoconfig()
    {
        string json = """
                      {
                        "proxyType": "pac",
                        "proxyAutoconfigUrl": "proxy.autoconfig.url"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!.ProxyType, Is.EqualTo(ProxyType.ProxyAutoConfig));
            Assert.That(deserialized, Is.InstanceOf<PacProxyConfiguration>());
            PacProxyConfiguration deserializedResult = (PacProxyConfiguration)deserialized;
            Assert.That(deserializedResult.ProxyAutoConfigUrl, Is.EqualTo("proxy.autoconfig.url"));
        });
    }

    [Test]
    public void TestCanDeserializeWithProxyTypeDirect()
    {
        string json = """
                      {
                        "proxyType": "direct"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!.ProxyType, Is.EqualTo(ProxyType.Direct));
            Assert.That(deserialized, Is.InstanceOf<DirectProxyConfiguration>());
        });
    }

    [Test]
    public void TestCanDeserializeWithAdditionalData()
    {
        string json = """
                      {
                        "proxyType": "direct",
                        "additionalName": "additionalValue"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized!.ProxyType, Is.EqualTo(ProxyType.Direct));
            Assert.That(deserialized, Is.InstanceOf<DirectProxyConfiguration>());
            Assert.That(deserialized.AdditionalData, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void TestDeserializeWithNonObjectJsonThrows()
    {
        string json = @"""proxyType""";
        Assert.That(() => JsonSerializer.Deserialize<ProxyConfiguration>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must be an object"));
    }

    [Test]
    public void TestDeserializeWithProxyTypeProxyAutoconfigWithMissingUrlThrows()
    {
        string json = """
                      {
                        "proxyType": "pac"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ProxyConfiguration>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("proxyAutoconfigUrl"));
    }

    [Test]
    public void TestDeserializeWithInvalidProxyTypeThrows()
    {
        string json = """
                      {
                        "proxyType": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ProxyConfiguration>(json), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("value 'invalid' is not valid for enum type"));
    }

    [Test]
    public void TestDeserializeWithNonStringProxyTypeThrows()
    {
        string json = """
                      {
                        "proxyType": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<ProxyConfiguration>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must be a string"));
    }
}
