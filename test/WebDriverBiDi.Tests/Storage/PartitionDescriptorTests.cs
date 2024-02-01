namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PartitionDescriptorTests
{
    [Test]
    public void TestCanSerializeBrowsingContextPartitionDescriptor()
    {
        BrowsingContextPartitionDescriptor properties = new("myBrowsingContext");
        string json = JsonSerializer.Serialize(properties);
        var serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("context"));
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myBrowsingContext"));
        });
    }

    [Test]
    public void TestCanSerializeStorageKeyPartitionDescriptor()
    {
        StorageKeyPartitionDescriptor properties = new();
        string json = JsonSerializer.Serialize(properties);
        var serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("storageKey"));
        });
    }

    [Test]
    public void TestCanSerializeStorageKeyPartitionDescriptorWithUserContext()
    {
        StorageKeyPartitionDescriptor properties = new()
        {
            UserContextId = "myUserContext"
        };
        string json = JsonSerializer.Serialize(properties);
        var serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("storageKey"));
            Assert.That(serialized, Contains.Key("userContext"));
            Assert.That(serialized["userContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["userContext"]!.Value<string>(), Is.EqualTo("myUserContext"));
        });
    }

    [Test]
    public void TestCanSerializeStorageKeyPartitionDescriptorWithSourceOrigin()
    {
        StorageKeyPartitionDescriptor properties = new()
        {
            SourceOrigin = "mySourceOrigin"
        };
        string json = JsonSerializer.Serialize(properties);
        var serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("storageKey"));
            Assert.That(serialized, Contains.Key("sourceOrigin"));
            Assert.That(serialized["sourceOrigin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sourceOrigin"]!.Value<string>(), Is.EqualTo("mySourceOrigin"));
        });
    }

    [Test]
    public void TestCanSerializeStorageKeyPartitionDescriptorWithAllProperties()
    {
        StorageKeyPartitionDescriptor properties = new()
        {
            UserContextId = "myUserContext",
            SourceOrigin = "mySourceOrigin"
        };
        string json = JsonSerializer.Serialize(properties);
        var serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("storageKey"));
            Assert.That(serialized, Contains.Key("userContext"));
            Assert.That(serialized["userContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["userContext"]!.Value<string>(), Is.EqualTo("myUserContext"));
            Assert.That(serialized, Contains.Key("sourceOrigin"));
            Assert.That(serialized["sourceOrigin"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sourceOrigin"]!.Value<string>(), Is.EqualTo("mySourceOrigin"));
        });
    }
}
