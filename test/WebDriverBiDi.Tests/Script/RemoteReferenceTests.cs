namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemoteReferenceTests
{
    [Test]
    public void TestCanSerializeRemoteObjectReference()
    {
        RemoteObjectReference reference = new("myHandle");
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myNewHandle"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
        });
    }

    [Test]
    public void TestRemoteObjectReferenceCopySemantics()
    {
        RemoteObjectReference reference = new("myHandle");
        RemoteObjectReference copy = reference with { };
        Assert.That(copy, Is.EqualTo(reference));
    }

    [Test]
    public void TestCanSerializeSharedReference()
    {
        SharedReference reference = new("mySharedId");
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("myNewSharedId"));
        });
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
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject, Contains.Key("sharedId"));
            Assert.That(referenceObject["sharedId"]!.Value<string>(), Is.EqualTo("mySharedId"));
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
        });
    }

    [Test]
    public void TestSharedReferenceCopySemantics()
    {
        SharedReference reference = new("mySharedId");
        SharedReference copy = reference with { };
        Assert.That(copy, Is.EqualTo(reference));
    }

    [Test]
    public void TestCanSerializeRemoteObjectReferenceWithAdditionalProperties()
    {
        RemoteObjectReference reference = new("myHandle");
        reference.AdditionalData["myPropertyName"] = "myValue";
        string json = JsonSerializer.Serialize(reference);
        JObject referenceObject = JObject.Parse(json);
        Assert.That(referenceObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(referenceObject, Contains.Key("handle"));
            Assert.That(referenceObject["handle"]!.Value<string>(), Is.EqualTo("myHandle"));
            Assert.That(referenceObject, Contains.Key("myPropertyName"));
            Assert.That(referenceObject["myPropertyName"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(referenceObject["myPropertyName"]!.Value<string>(), Is.EqualTo("myValue"));
        });
    }
}
