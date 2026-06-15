namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PacProxyConfigurationTests
{
    [Fact]
    public void TestCanSerialize()
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
    public void TestCanDeserialize()
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
    public void TestDeserializeWithMissingUrlThrows()
    {
        string json = """
                      {
                        "proxyType": "pac"
                      }
                      """;
        Assert.Contains("proxyAutoconfigUrl", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ProxyConfiguration>(json)).Message);
    }
}
