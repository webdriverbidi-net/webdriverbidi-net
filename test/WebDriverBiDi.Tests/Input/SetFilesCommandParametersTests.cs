namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

[TestFixture]
public class SetFilesCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        SharedReference element = new("mySharedId");
        SetFilesCommandParameters properties = new("myContextId", element);
        Assert.That(properties.MethodName, Is.EqualTo("input.setFiles"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SharedReference element = new("mySharedId");
        SetFilesCommandParameters properties = new("myContextId", element);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("element"));
            Assert.That(serialized["element"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["element"]!.Value<JObject>(), Is.Not.Null);
            JObject elementObject = serialized["element"]!.Value<JObject>()!;
            Assert.That(elementObject, Has.Count.EqualTo(1));
            Assert.That(elementObject, Contains.Key("sharedId"));
            Assert.That(elementObject["sharedId"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(elementObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
            Assert.That(serialized, Contains.Key("files"));
            Assert.That(serialized["files"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["files"]!, Is.Empty);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithFileList()
    {
        SharedReference element = new("mySharedId");
        SetFilesCommandParameters properties = new("myContextId", element);
        properties.Files.Add("path/to/file1.txt");
        properties.Files.Add("path/to/file2.txt");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("element"));
            Assert.That(serialized["element"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["element"]!.Value<JObject>(), Is.Not.Null);
            JObject elementObject = serialized["element"]!.Value<JObject>()!;
            Assert.That(elementObject, Has.Count.EqualTo(1));
            Assert.That(elementObject, Contains.Key("sharedId"));
            Assert.That(elementObject["sharedId"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(elementObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
            Assert.That(serialized, Contains.Key("files"));
            Assert.That(serialized["files"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["files"]!.Value<JArray>(), Is.Not.Null);
            JArray filesArray = serialized["files"]!.Value<JArray>()!;
            Assert.That(filesArray, Has.Count.EqualTo(2));
            Assert.That(filesArray[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(filesArray[0].Value<string>(), Is.EqualTo("path/to/file1.txt"));
            Assert.That(filesArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(filesArray[1].Value<string>(), Is.EqualTo("path/to/file2.txt"));
        }
    }
}
