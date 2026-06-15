namespace WebDriverBiDi.Session;

using System.Text.Json;

public class ProxyConfigurationTests
{
    [Fact]
    public void TestDeserializeWithNonObjectJsonThrows()
    {
        string json = @"""proxyType""";
        Assert.Contains("must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ProxyConfiguration>(json)).Message);
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
