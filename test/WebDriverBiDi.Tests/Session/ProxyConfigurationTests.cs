namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ProxyConfigurationTests
{
    [Fact]
    public void TestCanSerializeWithHttpProxy()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            HttpProxy = "http.proxy"
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("httpProxy"));
        JToken? httpProxy = serialized["httpProxy"];
        Assert.NotNull(httpProxy);
        Assert.Equal(JTokenType.String, httpProxy.Type);
        Assert.Equal("http.proxy", httpProxy.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithSslProxy()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            SslProxy = "ssl.proxy"
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("sslProxy"));
        JToken? sslProxy = serialized["sslProxy"];
        Assert.NotNull(sslProxy);
        Assert.Equal(JTokenType.String, sslProxy.Type);
        Assert.Equal("ssl.proxy", sslProxy.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithSocksProxy()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            SocksProxy = "socks.proxy"
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("socksProxy"));
        JToken? socksProxy = serialized["socksProxy"];
        Assert.NotNull(socksProxy);
        Assert.Equal(JTokenType.String, socksProxy.Type);
        Assert.Equal("socks.proxy", socksProxy.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithSocksVersion()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            SocksVersion = 4
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("socksVersion"));
        JToken? socksVersion = serialized["socksVersion"];
        Assert.NotNull(socksVersion);
        Assert.Equal(JTokenType.Integer, socksVersion.Type);
        Assert.Equal(4L, socksVersion.Value<long>());
    }

    [Fact]
    public void TestCanSerializeWithNoProxyAddresses()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            NoProxyAddresses = ["no.proxy.address"]
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("noProxy"));
        JToken? noProxyToken = serialized["noProxy"];
        Assert.NotNull(noProxyToken);
        Assert.Equal(JTokenType.Array, noProxyToken.Type);

        JArray? noProxyArray = noProxyToken as JArray;
        Assert.NotNull(noProxyArray);
        Assert.Single(noProxyArray);

        Assert.Equal(JTokenType.String, noProxyArray[0].Type);
        Assert.Equal("no.proxy.address", noProxyArray[0].Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithEmptyNoProxyAddresses()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration()
        {
            NoProxyAddresses = []
        };
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("noProxy"));
        JToken? noProxyToken = serialized["noProxy"];
        Assert.NotNull(noProxyToken);
        Assert.Equal(JTokenType.Array, noProxyToken.Type);

        JArray? noProxyArray = noProxyToken as JArray;
        Assert.NotNull(noProxyArray);
        Assert.Empty(noProxyArray);
    }

    [Fact]
    public void TestCanSerializeWithProxyTypeDirect()
    {
        ProxyConfiguration proxy = new DirectProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("direct", proxyType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithProxyTypeManual()
    {
        ProxyConfiguration proxy = new ManualProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("manual", proxyType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithProxyTypeSystem()
    {
        ProxyConfiguration proxy = new SystemProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("system", proxyType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithProxyTypeAutoDetect()
    {
        ProxyConfiguration proxy = new AutoDetectProxyConfiguration();
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("autodetect", proxyType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithProxyTypeProxyAutoconfig()
    {
        ProxyConfiguration proxy = new PacProxyConfiguration("proxy.autoconfig.url");
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("pac", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("proxyAutoconfigUrl"));
        JToken? proxyAutoconfigUrl = serialized["proxyAutoconfigUrl"];
        Assert.NotNull(proxyAutoconfigUrl);
        Assert.Equal(JTokenType.String, proxyAutoconfigUrl.Type);
        Assert.Equal("proxy.autoconfig.url", proxyAutoconfigUrl.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithAdditionalData()
    {
        ProxyConfiguration proxy = new DirectProxyConfiguration();
        proxy.AdditionalData["additionalName"] = "additionalValue";
        string json = JsonSerializer.Serialize(proxy);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("proxyType"));
        JToken? proxyType = serialized["proxyType"];
        Assert.NotNull(proxyType);
        Assert.Equal(JTokenType.String, proxyType.Type);
        Assert.Equal("direct", proxyType.Value<string>());

        Assert.True(serialized.ContainsKey("additionalName"));
        JToken? additionalName = serialized["additionalName"];
        Assert.NotNull(additionalName);
        Assert.Equal(JTokenType.String, additionalName.Type);
        Assert.Equal("additionalValue", additionalName.Value<string>());
    }

    [Fact]
    public void TestCanDeserializeWithProxyTypeManual()
    {
        string json = """
                      {
                        "proxyType": "manual"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.NotNull(deserialized);

        Assert.Equal(ProxyType.Manual, deserialized.ProxyType);
        Assert.IsType<ManualProxyConfiguration>(deserialized);
        ManualProxyConfiguration deserializedResult = (ManualProxyConfiguration)deserialized;
        Assert.Null(deserializedResult.HttpProxy);
        Assert.Null(deserializedResult.SslProxy);
        Assert.Null(deserializedResult.SocksProxy);
        Assert.Null(deserializedResult.SocksVersion);
    }

    [Fact]
    public void TestCanDeserializeWithProxyTypeSystem()
    {
        string json = """
                      {
                        "proxyType": "system"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.NotNull(deserialized);

        Assert.Equal(ProxyType.System, deserialized.ProxyType);
        Assert.IsType<SystemProxyConfiguration>(deserialized);
    }

    [Fact]
    public void TestCanDeserializeWithProxyTypeAutoDetect()
    {
        string json = """
                      {
                        "proxyType": "autodetect"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.NotNull(deserialized);

        Assert.Equal(ProxyType.AutoDetect, deserialized.ProxyType);
        Assert.IsType<AutoDetectProxyConfiguration>(deserialized);
    }

    [Fact]
    public void TestCanDeserializeWithProxyTypeProxyAutoconfig()
    {
        string json = """
                      {
                        "proxyType": "pac",
                        "proxyAutoconfigUrl": "proxy.autoconfig.url"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.NotNull(deserialized);

        Assert.Equal(ProxyType.ProxyAutoConfig, deserialized.ProxyType);
        Assert.IsType<PacProxyConfiguration>(deserialized);
        PacProxyConfiguration deserializedResult = (PacProxyConfiguration)deserialized;
        Assert.Equal("proxy.autoconfig.url", deserializedResult.ProxyAutoConfigUrl);
    }

    [Fact]
    public void TestCanDeserializeWithProxyTypeDirect()
    {
        string json = """
                      {
                        "proxyType": "direct"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.NotNull(deserialized);

        Assert.Equal(ProxyType.Direct, deserialized.ProxyType);
        Assert.IsType<DirectProxyConfiguration>(deserialized);
    }

    [Fact]
    public void TestCanDeserializeWithAdditionalData()
    {
        string json = """
                      {
                        "proxyType": "direct",
                        "additionalName": "additionalValue"
                      }
                      """;
        ProxyConfiguration? deserialized = JsonSerializer.Deserialize<ProxyConfiguration>(json);
        Assert.NotNull(deserialized);

        Assert.Equal(ProxyType.Direct, deserialized.ProxyType);
        Assert.IsType<DirectProxyConfiguration>(deserialized);
        Assert.Single(deserialized.AdditionalData);
    }

    [Fact]
    public void TestDeserializeWithNonObjectJsonThrows()
    {
        string json = @"""proxyType""";
        Assert.Contains("must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ProxyConfiguration>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithProxyTypeProxyAutoconfigWithMissingUrlThrows()
    {
        string json = """
                      {
                        "proxyType": "pac"
                      }
                      """;
        Assert.Contains("proxyAutoconfigUrl", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ProxyConfiguration>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithInvalidProxyTypeThrows()
    {
        string json = """
                      {
                        "proxyType": "invalid"
                      }
                      """;
        Assert.Contains("JSON for 'ProxyConfiguration' proxyType property contains unknown value 'invalid'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ProxyConfiguration>(json)).Message);
    }

    [Fact]
    public void TestDeserializeWithNonStringProxyTypeThrows()
    {
        string json = """
                      {
                        "proxyType": {}
                      }
                      """;
        Assert.Contains("must be a string", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ProxyConfiguration>(json)).Message);
    }
}
