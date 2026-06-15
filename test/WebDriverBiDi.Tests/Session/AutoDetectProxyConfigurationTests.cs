namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class AutoDetectProxyConfigurationTests
{
    [Fact]
    public void TestCanSerialize()
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
    public void TestCanDeserialize()
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
}
