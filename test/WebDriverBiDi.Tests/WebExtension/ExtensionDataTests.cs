namespace WebDriverBiDi.WebExtension;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ExtensionDataTests
{
    [Test]
    public void TestCanSerializeExtensionPathExtensionData()
    {
        ExtensionPath value = new("myPath");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("path"));
            Assert.That(parsed, Contains.Key("path"));
            Assert.That(parsed["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["path"]!.Value<string>(), Is.EqualTo("myPath"));
        });
    }

    [Test]
    public void TestCanSerializeExtensionPathExtensionDataWithNoArgs()
    {
        ExtensionPath value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("path"));
            Assert.That(parsed, Contains.Key("path"));
            Assert.That(parsed["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["path"]!.Value<string>(), Is.Empty);
        });
    }
    [Test]
    public void TestCanSerializeExtensionArchivePathExtensionData()
    {
        ExtensionArchivePath value = new("myPath.zip");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("archivePath"));
            Assert.That(parsed, Contains.Key("path"));
            Assert.That(parsed["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["path"]!.Value<string>(), Is.EqualTo("myPath.zip"));
        });
    }

    [Test]
    public void TestCanSerializeExtensionArchivePathExtensionDataWithNoArgs()
    {
        ExtensionArchivePath value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("archivePath"));
            Assert.That(parsed, Contains.Key("path"));
            Assert.That(parsed["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["path"]!.Value<string>(), Is.Empty);
        });
    }

    [Test]
    public void TestCanSerializeExtensionBase64EncodedExtensionData()
    {
        ExtensionBase64Encoded value = new("Base64 Encoded Data");
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("base64"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("Base64 Encoded Data"));
        });
    }

    [Test]
    public void TestCanSerializeExtensionBase64EncodedExtensionDataWithNoArgs()
    {
        ExtensionBase64Encoded value = new();
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(2));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("base64"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.Empty);
        });
    }
}