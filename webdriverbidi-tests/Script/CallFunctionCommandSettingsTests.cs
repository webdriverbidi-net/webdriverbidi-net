namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CallFunctionCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        var properties = new CallFunctionCommandSettings("myFunction", new RealmTarget("myRealm"), true);
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(3));
        Assert.That(serialized.ContainsKey("functionDeclaration"));
        Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized.ContainsKey("target"));
        Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(serialized.ContainsKey("awaitPromise"));
        Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
    }

    [Test]
    public void TestCanSerializeSettingsWithOptionalValues()
    {
        var properties = new CallFunctionCommandSettings("myFunction", new RealmTarget("myRealm"), true);
        properties.Arguments.Add(LocalValue.String("myArgument"));
        properties.ThisObject = LocalValue.String("thisObject");
        properties.OwnershipModel = OwnershipModel.None;
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(6));
        Assert.That(serialized.ContainsKey("functionDeclaration"));
        Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(serialized.ContainsKey("target"));
        Assert.That(serialized["target"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(serialized.ContainsKey("awaitPromise"));
        Assert.That(serialized["awaitPromise"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(serialized.ContainsKey("arguments"));
        Assert.That(serialized["arguments"]!.Type, Is.EqualTo(JTokenType.Array));
        Assert.That(serialized.ContainsKey("this"));
        Assert.That(serialized["this"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(serialized.ContainsKey("resultOwnership"));
        Assert.That(serialized["resultOwnership"]!.Type, Is.EqualTo(JTokenType.String));
    }
}