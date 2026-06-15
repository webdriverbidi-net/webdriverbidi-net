namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ManualProxyConfigurationTests
{
    [Fact]
    public void TestCanSerialize()
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
    public void TestCanDeserialize()
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
}
