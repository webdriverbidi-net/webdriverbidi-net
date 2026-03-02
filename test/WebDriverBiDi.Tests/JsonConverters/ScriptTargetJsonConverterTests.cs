namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

[TestFixture]
public class ScriptTargetJsonConverterTests
{
    [Test]
    public void TestReadWithRealmPropertyReturnsRealmTarget()
    {
        string json = """{ "realm": "myRealmId" }""";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<RealmTarget>());
        RealmTarget realmTarget = (RealmTarget)target!;
        Assert.That(realmTarget.RealmId, Is.EqualTo("myRealmId"));
    }

    [Test]
    public void TestReadWithContextPropertyReturnsContextTarget()
    {
        string json = """{ "context": "myContextId" }""";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.That(target, Is.Not.Null);
        Assert.That(target, Is.InstanceOf<ContextTarget>());
        ContextTarget contextTarget = (ContextTarget)target!;
        Assert.That(contextTarget.BrowsingContextId, Is.EqualTo("myContextId"));
    }

    [Test]
    public void TestReadWithNeitherRealmNorContextThrowsJsonException()
    {
        string json = """{ "invalid": "invalidValue" }""";
        Assert.That(() => JsonSerializer.Deserialize<Target>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("ScriptTarget must contain either a 'realm' or a 'context' property"));
    }

    [Test]
    public void TestReadWithNonObjectJsonThrowsJsonException()
    {
        string json = """[ "invalid target" ]""";
        Assert.That(() => JsonSerializer.Deserialize<Target>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("Script target JSON must be an object"));
    }

    [Test]
    public void TestWriteRealmTargetProducesCorrectJson()
    {
        Target target = new RealmTarget("myRealm");
        string json = JsonSerializer.Serialize(target);
        JObject deserialized = JObject.Parse(json);
        Assert.That(deserialized, Contains.Key("realm"));
        Assert.That(deserialized["realm"]!.Value<string>(), Is.EqualTo("myRealm"));
    }

    [Test]
    public void TestWriteContextTargetProducesCorrectJson()
    {
        Target target = new ContextTarget("myContext");
        string json = JsonSerializer.Serialize(target);
        JObject deserialized = JObject.Parse(json);
        Assert.That(deserialized, Contains.Key("context"));
        Assert.That(deserialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
    }

    [Test]
    public void TestWriteWithNullValueThrowsArgumentNullException()
    {
        ScriptTargetJsonConverter converter = new();
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);
        Assert.That(() => converter.Write(writer, null!, new JsonSerializerOptions()), Throws.InstanceOf<ArgumentNullException>());
    }
}
