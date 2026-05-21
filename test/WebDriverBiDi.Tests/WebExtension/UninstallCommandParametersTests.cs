namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class UninstallCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        UninstallCommandParameters properties = new("myExtensionId");
        Assert.Equal("webExtension.uninstall", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        UninstallCommandParameters properties = new("myExtensionId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("extension"));
        JToken? extension = serialized["extension"];
        Assert.NotNull(extension);
        Assert.Equal(JTokenType.String, extension.Type);
        Assert.Equal("myExtensionId", extension.Value<string>());
    }
}
