namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ExtensionDataTests
{
    [Fact]
    public void TestCanSerializeExtensionPathExtensionData()
    {
        ExtensionPath value = new("myPath");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("path", type.Value<string>());

        Assert.True(parsed.ContainsKey("path"));
        JToken? path = parsed["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal("myPath", path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeExtensionPathExtensionDataWithNoArgs()
    {
        ExtensionPath value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("path", type.Value<string>());

        Assert.True(parsed.ContainsKey("path"));
        JToken? path = parsed["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal(string.Empty, path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeExtensionArchivePathExtensionData()
    {
        ExtensionArchivePath value = new("myPath.zip");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("archivePath", type.Value<string>());

        Assert.True(parsed.ContainsKey("path"));
        JToken? path = parsed["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal("myPath.zip", path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeExtensionArchivePathExtensionDataWithNoArgs()
    {
        ExtensionArchivePath value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("archivePath", type.Value<string>());

        Assert.True(parsed.ContainsKey("path"));
        JToken? path = parsed["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal(string.Empty, path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeExtensionBase64EncodedExtensionData()
    {
        ExtensionBase64Encoded value = new("Base64 Encoded Data");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("base64", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueToken = parsed["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.String, valueToken.Type);
        Assert.Equal("Base64 Encoded Data", valueToken.Value<string>());
    }

    [Fact]
    public void TestCanSerializeExtensionBase64EncodedExtensionDataWithNoArgs()
    {
        ExtensionBase64Encoded value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("base64", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? valueToken = parsed["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.String, valueToken.Type);
        Assert.Equal(string.Empty, valueToken.Value<string>());
    }
}
