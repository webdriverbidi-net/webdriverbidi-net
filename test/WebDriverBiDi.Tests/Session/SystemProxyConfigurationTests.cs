namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SystemProxyConfigurationTests
{
    [Fact]
    public void TestCanSerialize()
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
    public void TestCanDeserialize()
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
}
