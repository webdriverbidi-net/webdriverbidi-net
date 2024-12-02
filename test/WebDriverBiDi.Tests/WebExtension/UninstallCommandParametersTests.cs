namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class UninstallCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        UninstallCommandParameters properties = new("myExtensionId");
        Assert.That(properties.MethodName, Is.EqualTo("webExtension.uninstall"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        UninstallCommandParameters properties = new("myExtensionId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("extension"));
            Assert.That(serialized["extension"]!.Type, Is.EqualTo(JTokenType.String));
             Assert.That(serialized["extension"]!.Value<string>(), Is.EqualTo("myExtensionId"));
        });
    }
}
