namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ScriptTargetJsonConverterTests
{
    [Test]
    public void TestCanDeserializeRealmTarget()
    {
        string json = @"{ ""realm"": ""myRealm"" }";
        ScriptTarget? target = JsonConvert.DeserializeObject<ScriptTarget>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<RealmTarget>());
        RealmTarget realmTarget = (RealmTarget)target!;
        Assert.That(realmTarget.RealmId, Is.EqualTo("myRealm"));
    }

    [Test]
    public void TestCanDeserializeContextTarget()
    {
        string json = @"{ ""context"": ""myContext"" }";
        ScriptTarget? target = JsonConvert.DeserializeObject<ScriptTarget>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<ContextTarget>());
        ContextTarget contextTarget = (ContextTarget)target!;
        Assert.That(contextTarget.BrowsingContextId, Is.EqualTo("myContext"));
        Assert.That(contextTarget.Sandbox, Is.Null);
    }

    [Test]
    public void TestCanDeserializeContextTargetWithSandbox()
    {
        string json = @"{ ""context"": ""myContext"", ""sandbox"": ""mySandbox"" }";
        ScriptTarget? target = JsonConvert.DeserializeObject<ScriptTarget>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<ContextTarget>());
        ContextTarget contextTarget = (ContextTarget)target!;
        Assert.That(contextTarget.BrowsingContextId, Is.EqualTo("myContext"));
        Assert.That(contextTarget.Sandbox, Is.EqualTo("mySandbox"));
    }

    [Test]
    public void TestDeserializationOfInvalidJsonThrows()
    {
        string json = @"{ ""invalid"": ""invalidValue"" }";
        Assert.That(() => JsonConvert.DeserializeObject<ScriptTarget>(json), Throws.InstanceOf<WebDriverBidiException>().With.Message.Contains("ScriptTarget must contain either a 'realm' or a 'context' property"));
    }

    [Test]
    public void TestCanSerializeRealmTarget()
    {
        ScriptTarget target = new RealmTarget("myRealm");
        string json = JsonConvert.SerializeObject(target);
        JObject deserialized = JObject.Parse(json);
        Assert.That(deserialized.Count, Is.EqualTo(1));
        Assert.That(deserialized.ContainsKey("realm"));
        JToken realmValue = deserialized.GetValue("realm")!;
        Assert.That(realmValue.Type, Is.EqualTo(JTokenType.String));
        Assert.That((string?)realmValue, Is.EqualTo("myRealm"));
    }

    [Test]
    public void TestCanSerializeContextTarget()
    {
        ScriptTarget target = new ContextTarget("myContext");
        string json = JsonConvert.SerializeObject(target);
        JObject deserialized = JObject.Parse(json);
        Assert.That(deserialized.Count, Is.EqualTo(1));
        Assert.That(deserialized.ContainsKey("context"));
        JToken contextValue = deserialized.GetValue("context")!;
        Assert.That(contextValue.Type, Is.EqualTo(JTokenType.String));
        Assert.That((string?)contextValue, Is.EqualTo("myContext"));
    }
}
