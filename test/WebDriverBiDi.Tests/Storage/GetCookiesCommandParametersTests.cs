namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetCookiesCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        GetCookiesCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("storage.getCookies"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        GetCookiesCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(0));
    }

    [Test]
    public void TestCanSerializeParametersWithCookieFilter()
    {
        GetCookiesCommandParameters properties = new()
        {
            Filter = new CookieFilter()
            {
                Name = "cookieName"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("filter"));
            Assert.That(serialized["filter"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject filterObject = serialized["filter"]!.Value<JObject>()!;
            Assert.That(filterObject, Has.Count.EqualTo(1));
            Assert.That(filterObject, Contains.Key("name"));
            Assert.That(filterObject["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(filterObject["name"]!.Value<string>(), Is.EqualTo("cookieName"));
        });        
    }

    [Test]
    public void TestCanSerializeParametersWithBrowsingContextPartitionDescriptor()
    {
        GetCookiesCommandParameters properties = new()
        {
            Partition = new BrowsingContextPartitionDescriptor("myContext")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("partition"));
            Assert.That(serialized["partition"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partitionObject = serialized["partition"]!.Value<JObject>()!;
            Assert.That(partitionObject, Has.Count.EqualTo(2));
            Assert.That(partitionObject, Contains.Key("type"));
            Assert.That(partitionObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partitionObject["type"]!.Value<string>(), Is.EqualTo("context"));
            Assert.That(partitionObject, Contains.Key("context"));
            Assert.That(partitionObject["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partitionObject["context"]!.Value<string>(), Is.EqualTo("myContext"));
        });        
    }

    [Test]
    public void TestCanSerializeParametersWithStorageKeyPartitionDescriptor()
    {
        GetCookiesCommandParameters properties = new()
        {
            Partition = new StorageKeyPartitionDescriptor()
            {
                UserContextId = "myUserContext"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("partition"));
            Assert.That(serialized["partition"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partitionObject = serialized["partition"]!.Value<JObject>()!;
            Assert.That(partitionObject, Has.Count.EqualTo(2));
            Assert.That(partitionObject, Contains.Key("type"));
            Assert.That(partitionObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partitionObject["type"]!.Value<string>(), Is.EqualTo("storageKey"));
            Assert.That(partitionObject, Contains.Key("userContext"));
            Assert.That(partitionObject["userContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partitionObject["userContext"]!.Value<string>(), Is.EqualTo("myUserContext"));
        });        
    }

    [Test]
    public void TestCanSerializeParametersWithAllValues()
    {
        GetCookiesCommandParameters properties = new()
        {
            Filter = new CookieFilter()
            {
                Name = "cookieName",
            },
            Partition = new BrowsingContextPartitionDescriptor("myContext")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("filter"));
            Assert.That(serialized["filter"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject filterObject = serialized["filter"]!.Value<JObject>()!;
            Assert.That(filterObject, Has.Count.EqualTo(1));
            Assert.That(filterObject, Contains.Key("name"));
            Assert.That(filterObject["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(filterObject["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(serialized, Contains.Key("partition"));
            Assert.That(serialized["partition"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partitionObject = serialized["partition"]!.Value<JObject>()!;
            Assert.That(partitionObject, Has.Count.EqualTo(2));
            Assert.That(partitionObject, Contains.Key("type"));
            Assert.That(partitionObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partitionObject["type"]!.Value<string>(), Is.EqualTo("context"));
            Assert.That(partitionObject, Contains.Key("context"));
            Assert.That(partitionObject["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partitionObject["context"]!.Value<string>(), Is.EqualTo("myContext"));
        });        
    }
}
