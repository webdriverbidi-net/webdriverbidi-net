namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class TargetTests
{
    [Test]
    public void TestCanDeserializeRealmTarget()
    {
        string json = @"{ ""realm"": ""myRealm"" }";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<RealmTarget>());
        RealmTarget realmTarget = (RealmTarget)target!;
        Assert.That(realmTarget.RealmId, Is.EqualTo("myRealm"));
    }

    [Test]
    public void TestCanDeserializeContextTarget()
    {
        string json = @"{ ""context"": ""myContext"" }";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<ContextTarget>());
        ContextTarget contextTarget = (ContextTarget)target!;
        Assert.Multiple(() =>
        {
            Assert.That(contextTarget.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(contextTarget.Sandbox, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeContextTargetWithSandbox()
    {
        string json = @"{ ""context"": ""myContext"", ""sandbox"": ""mySandbox"" }";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<ContextTarget>());
        ContextTarget contextTarget = (ContextTarget)target!;
        Assert.Multiple(() =>
        {
            Assert.That(contextTarget.BrowsingContextId, Is.EqualTo("myContext"));
            Assert.That(contextTarget.Sandbox, Is.EqualTo("mySandbox"));
        });
    }

    [Test]
    public void TestDeserializationOfInvalidJsonThrows()
    {
        string json = @"{ ""invalid"": ""invalidValue"" }";
        Assert.That(() => JsonSerializer.Deserialize<ContextTarget>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: context"));
        Assert.That(() => JsonSerializer.Deserialize<RealmTarget>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: realm"));
        Assert.That(() => JsonSerializer.Deserialize<Target>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("ScriptTarget must contain either a 'realm' or a 'context' property"));
        Assert.That(() => JsonSerializer.Deserialize<Target>(@"[ ""invalid target"" ]"), Throws.InstanceOf<JsonException>().With.Message.Contains("Script target JSON must be an object"));
    }

    [Test]
    public void TestCanSerializeRealmTarget()
    {
        Target target = new RealmTarget("myRealm");
        string json = JsonSerializer.Serialize(target);
        JObject deserialized = JObject.Parse(json);
        Assert.That(deserialized, Has.Count.EqualTo(1));
        Assert.That(deserialized, Contains.Key("realm"));
        JToken realmValue = deserialized.GetValue("realm")!;
        Assert.Multiple(() =>
        {
            Assert.That(realmValue.Type, Is.EqualTo(JTokenType.String));
            Assert.That((string?)realmValue, Is.EqualTo("myRealm"));
        });
    }

    [Test]
    public void TestCanSerializeContextTarget()
    {
        Target target = new ContextTarget("myContext", "mySandbox");
        string json = JsonSerializer.Serialize(target);
        JObject deserialized = JObject.Parse(json);
        Assert.That(deserialized, Has.Count.EqualTo(2));
        Assert.That(deserialized, Contains.Key("context"));
        JToken contextValue = deserialized.GetValue("context")!;
        JToken sandboxValue = deserialized.GetValue("sandbox")!;
        
        Assert.Multiple(() =>
        {
            Assert.That(contextValue.Type, Is.EqualTo(JTokenType.String));
            Assert.That((string?)contextValue, Is.EqualTo("myContext"));
            Assert.That(sandboxValue.Type, Is.EqualTo(JTokenType.String));
            Assert.That((string?)sandboxValue, Is.EqualTo("mySandbox"));
        });
    }
}
