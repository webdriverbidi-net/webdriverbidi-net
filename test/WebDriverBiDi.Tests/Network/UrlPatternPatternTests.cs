namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class UrlPatternPatternTests
{
    [Fact]
    public void TestCanSerializeValue()
    {
        UrlPattern value = new UrlPatternPattern();
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pattern", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeValueWithProtocol()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            Protocol = "https"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pattern", type.Value<string>());

        Assert.True(serialized.ContainsKey("protocol"));
        JToken? protocol = serialized["protocol"];
        Assert.NotNull(protocol);
        Assert.Equal(JTokenType.String, protocol.Type);
        Assert.Equal("https", protocol.Value<string>());
    }

    [Fact]
    public void TestCanSerializeValueWithHostName()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            HostName = "example.com"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pattern", type.Value<string>());

        Assert.True(serialized.ContainsKey("hostname"));
        JToken? hostname = serialized["hostname"];
        Assert.NotNull(hostname);
        Assert.Equal(JTokenType.String, hostname.Type);
        Assert.Equal("example.com", hostname.Value<string>());
    }

    [Fact]
    public void TestCanSerializeValueWithPort()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            Port = "2222"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pattern", type.Value<string>());

        Assert.True(serialized.ContainsKey("port"));
        JToken? port = serialized["port"];
        Assert.NotNull(port);
        Assert.Equal(JTokenType.String, port.Type);
        Assert.Equal("2222", port.Value<string>());
    }

    [Fact]
    public void TestCanSerializeValueWithPathName()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            PathName = "/data/*"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pattern", type.Value<string>());

        Assert.True(serialized.ContainsKey("pathname"));
        JToken? pathname = serialized["pathname"];
        Assert.NotNull(pathname);
        Assert.Equal(JTokenType.String, pathname.Type);
        Assert.Equal("/data/*", pathname.Value<string>());
    }

    [Fact]
    public void TestCanSerializeValueWithSearch()
    {
        UrlPattern value = new UrlPatternPattern()
        {
            Search = "?user=foo"
        };
        string json = JsonSerializer.Serialize(value);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pattern", type.Value<string>());

        Assert.True(serialized.ContainsKey("search"));
        JToken? search = serialized["search"];
        Assert.NotNull(search);
        Assert.Equal(JTokenType.String, search.Type);
        Assert.Equal("?user=foo", search.Value<string>());
    }
}
