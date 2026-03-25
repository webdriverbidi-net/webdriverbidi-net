namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemoteObjectReferenceTests
{
    [Test]
    public void TestCanSerializeRemoteObjectReference()
    {
        RemoteObjectReference reference = new("myHandle");
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
        }
    }

    [Test]
    public void TestCanEditRemoteObjectReferenceHandle()
    {
        RemoteObjectReference reference = new("myHandle")
        {
            Handle = "myNewHandle"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myNewHandle"));
        }
    }

    [Test]
    public void TestCanSerializeRemoteObjectReferenceWithSharedId()
    {
        RemoteObjectReference reference = new("myHandle")
        {
            SharedId = "mySharedId"
        };
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
        }
    }

    [Test]
    public void TestRemoteObjectReferenceCopySemantics()
    {
        RemoteObjectReference reference = new("myHandle");
        RemoteObjectReference copy = reference with { };
        Assert.That(copy, Is.EqualTo(reference));
    }

    [Test]
    public void TestRemoteObjectReferenceHandleGetterReturnsValue()
    {
        RemoteObjectReference reference = new("myHandle");
        Assert.That(reference.Handle, Is.EqualTo("myHandle"));
    }

    [Test]
    public void TestRemoteObjectReferenceHandleSetterSetsValue()
    {
        RemoteObjectReference reference = new("myHandle");
        reference.Handle = "myNewHandle";
        Assert.That(reference.Handle, Is.EqualTo("myNewHandle"));
    }

    [Test]
    public void TestSettingRemoteObjectReferenceSharedIdToNullThrows()
    {
        RemoteObjectReference reference = new("myHandle");
        Assert.That(() => reference.Handle = null!, Throws.InstanceOf<ArgumentNullException>());;
    }

    [Test]
    public void TestDeserializingRemoteObjectReferenceThrowsWhenHandleIsMissing()
    {
        string json = "{}";
        Assert.That(() => JsonSerializer.Deserialize<RemoteObjectReference>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingRemoteObjectReferenceThrowsWhenHandleIsInvalidType()
    {
        string json = """
                      {
                        "handle": 123
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteObjectReference>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanSerializeRemoteObjectReferenceWithAdditionalProperties()
    {
        RemoteObjectReference reference = new("myHandle");
        reference.AdditionalData["myPropertyName"] = "myValue";
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
            Assert.That(referenceObject, Contains.Key("myPropertyName"));
            Assert.That(referenceObject["myPropertyName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(referenceObject["myPropertyName"]!.Value<string>(), Is.EqualTo("myValue"));
        }
    }
}
