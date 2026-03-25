namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SharedReferenceTests
{
    [Test]
    public void TestCanSerializeSharedReference()
    {
        SharedReference reference = new("mySharedId");
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
        }
    }

    [Test]
    public void TestCanEditSharedReferenceSharedId()
    {
        SharedReference reference = new("mySharedId")
        {
            SharedId = "myNewSharedId"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("myNewSharedId"));
        }
    }

    [Test]
    public void TestCanSerializeSharedReferenceWithHandle()
    {
        SharedReference reference = new("mySharedId")
        {
            Handle = "myHandle"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
        }
    }

    [Test]
    public void TestSharedReferenceCopySemantics()
    {
        SharedReference reference = new("mySharedId");
        SharedReference copy = reference with { };
        Assert.That(copy, Is.EqualTo(reference));
    }

    [Test]
    public void TestSharedReferenceSharedIdGetterReturnsValue()
    {
        SharedReference reference = new("mySharedId");
        Assert.That(reference.SharedId, Is.EqualTo("mySharedId"));
    }

    [Test]
    public void TestSharedReferenceSharedIdSetterSetsValue()
    {
        SharedReference reference = new("mySharedId");
        reference.SharedId = "myNewSharedId";
        Assert.That(reference.SharedId, Is.EqualTo("myNewSharedId"));
    }

    [Test]
    public void TestSettingSharedReferenceHandleToNullThrows()
    {
        SharedReference reference = new("mySharedId");
        Assert.That(() => reference.SharedId = null!, Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public void TestDeserializingSharedReferenceThrowsWhenSharedIdIsMissing()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<SharedReference>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingSharedReferenceThrowsWhenSharedIdIsInvalidType()
    {
        string json = """
                      {
                        "sharedId": 123
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<SharedReference>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanSerializeSharedReferenceWithAdditionalProperties()
    {
        SharedReference reference = new("mySharedId");
        reference.AdditionalData["myPropertyName"] = "myValue";
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
            Assert.That(referenceObject, Contains.Key("myPropertyName"));
            Assert.That(referenceObject["myPropertyName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(referenceObject["myPropertyName"]!.Value<string>(), Is.EqualTo("myValue"));
        }
    }
}
