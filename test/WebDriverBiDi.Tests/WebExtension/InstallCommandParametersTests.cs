namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class InstallCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        InstallCommandParameters properties = new(new ExtensionPath("myExtension"));
        Assert.That(properties.MethodName, Is.EqualTo("webExtension.install"));
    }

    [Test]
    public void TestCanSerializeParametersWithExtensionPath()
    {
        InstallCommandParameters properties = new(new ExtensionPath("myExtension"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("extensionData"));
            Assert.That(serialized["extensionData"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject extensionDataObject = (JObject)serialized["extensionData"]!;
            Assert.That(extensionDataObject, Has.Count.EqualTo(2));
            Assert.That(extensionDataObject, Contains.Key("type"));
            Assert.That(extensionDataObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(extensionDataObject["type"]!.Value<string>(), Is.EqualTo("path"));
            Assert.That(extensionDataObject, Contains.Key("path"));
            Assert.That(extensionDataObject["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(extensionDataObject["path"]!.Value<string>(), Is.EqualTo("myExtension"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithExtensionArchivePath()
    {
        InstallCommandParameters properties = new(new ExtensionArchivePath("myExtensionArchive"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("extensionData"));
            Assert.That(serialized["extensionData"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject extensionDataObject = (JObject)serialized["extensionData"]!;
            Assert.That(extensionDataObject, Has.Count.EqualTo(2));
            Assert.That(extensionDataObject, Contains.Key("type"));
            Assert.That(extensionDataObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(extensionDataObject["type"]!.Value<string>(), Is.EqualTo("archivePath"));
            Assert.That(extensionDataObject, Contains.Key("path"));
            Assert.That(extensionDataObject["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(extensionDataObject["path"]!.Value<string>(), Is.EqualTo("myExtensionArchive"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithBase64Extension()
    {
        InstallCommandParameters properties = new(new ExtensionBase64Encoded("Some base64 encoded data"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("extensionData"));
            Assert.That(serialized["extensionData"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject extensionDataObject = (JObject)serialized["extensionData"]!;
            Assert.That(extensionDataObject, Has.Count.EqualTo(2));
            Assert.That(extensionDataObject, Contains.Key("type"));
            Assert.That(extensionDataObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(extensionDataObject["type"]!.Value<string>(), Is.EqualTo("base64"));
            Assert.That(extensionDataObject, Contains.Key("value"));
            Assert.That(extensionDataObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(extensionDataObject["value"]!.Value<string>(), Is.EqualTo("Some base64 encoded data"));
        }
    }
}
