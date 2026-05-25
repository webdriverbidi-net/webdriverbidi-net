namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class InstallCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        InstallCommandParameters properties = new(new ExtensionPath("myExtension"));
        Assert.Equal("webExtension.install", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersWithExtensionPath()
    {
        InstallCommandParameters properties = new(new ExtensionPath("myExtension"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("extensionData"));
        JToken? extensionDataToken = serialized["extensionData"];
        Assert.NotNull(extensionDataToken);
        Assert.Equal(JTokenType.Object, extensionDataToken.Type);
        JObject? extensionDataObject = extensionDataToken as JObject;
        Assert.NotNull(extensionDataObject);
        Assert.Equal(2, extensionDataObject.Count);

        Assert.True(extensionDataObject.ContainsKey("type"));
        JToken? type = extensionDataObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("path", type.Value<string>());

        Assert.True(extensionDataObject.ContainsKey("path"));
        JToken? path = extensionDataObject["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal("myExtension", path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithExtensionArchivePath()
    {
        InstallCommandParameters properties = new(new ExtensionArchivePath("myExtensionArchive"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("extensionData"));
        JToken? extensionDataToken = serialized["extensionData"];
        Assert.NotNull(extensionDataToken);
        Assert.Equal(JTokenType.Object, extensionDataToken.Type);
        JObject? extensionDataObject = extensionDataToken as JObject;
        Assert.NotNull(extensionDataObject);
        Assert.Equal(2, extensionDataObject.Count);

        Assert.True(extensionDataObject.ContainsKey("type"));
        JToken? type = extensionDataObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("archivePath", type.Value<string>());

        Assert.True(extensionDataObject.ContainsKey("path"));
        JToken? path = extensionDataObject["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal("myExtensionArchive", path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithBase64Extension()
    {
        InstallCommandParameters properties = new(new ExtensionBase64Encoded("Some base64 encoded data"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("extensionData"));
        JToken? extensionDataToken = serialized["extensionData"];
        Assert.NotNull(extensionDataToken);
        Assert.Equal(JTokenType.Object, extensionDataToken.Type);
        JObject? extensionDataObject = extensionDataToken as JObject;
        Assert.NotNull(extensionDataObject);
        Assert.Equal(2, extensionDataObject.Count);

        Assert.True(extensionDataObject.ContainsKey("type"));
        JToken? type = extensionDataObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("base64", type.Value<string>());

        Assert.True(extensionDataObject.ContainsKey("value"));
        JToken? value = extensionDataObject["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.String, value.Type);
        Assert.Equal("Some base64 encoded data", value.Value<string>());
    }
}
