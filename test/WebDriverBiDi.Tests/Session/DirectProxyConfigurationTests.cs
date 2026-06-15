namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class DirectProxyConfigurationTests
{
    [Fact]
    public void TestCanSerialize()
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
    public void TestCanDeserialize()
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
}
