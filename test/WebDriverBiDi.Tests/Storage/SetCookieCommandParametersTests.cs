namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Network;

[TestFixture]
public class SetCookieCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"));
        Assert.That(properties.MethodName, Is.EqualTo("storage.setCookie"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("cookie"));
            Assert.That(serialized["cookie"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject cookieObject = serialized["cookie"]!.Value<JObject>()!;
            Assert.That(cookieObject, Has.Count.EqualTo(3));
            Assert.That(cookieObject, Contains.Key("name"));
            Assert.That(cookieObject["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieObject["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(cookieObject, Contains.Key("value"));
            Assert.That(cookieObject["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject cookieValueObject = cookieObject["value"]!.Value<JObject>()!;
            Assert.That(cookieValueObject, Has.Count.EqualTo(2));
            Assert.That(cookieValueObject, Contains.Key("type"));
            Assert.That(cookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(cookieValueObject, Contains.Key("value"));
            Assert.That(cookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["value"]!.Value<string>(), Is.EqualTo("cookieValue"));
            Assert.That(cookieObject, Contains.Key("domain"));
            Assert.That(cookieObject["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieObject["domain"]!.Value<string>(), Is.EqualTo("cookieDomain"));
        }        
    }

    [Test]
    public void TestCanSerializeParametersWithBrowsingContextPartitionDescriptor()
    {
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"))
        {
            Partition = new BrowsingContextPartitionDescriptor("myContext")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("cookie"));
            Assert.That(serialized["cookie"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject cookieObject = serialized["cookie"]!.Value<JObject>()!;
            Assert.That(cookieObject, Has.Count.EqualTo(3));
            Assert.That(cookieObject, Contains.Key("name"));
            Assert.That(cookieObject["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieObject["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(cookieObject, Contains.Key("value"));
            Assert.That(cookieObject["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject cookieValueObject = cookieObject["value"]!.Value<JObject>()!;
            Assert.That(cookieValueObject, Has.Count.EqualTo(2));
            Assert.That(cookieValueObject, Contains.Key("type"));
            Assert.That(cookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(cookieValueObject, Contains.Key("value"));
            Assert.That(cookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["value"]!.Value<string>(), Is.EqualTo("cookieValue"));
            Assert.That(cookieObject, Contains.Key("domain"));
            Assert.That(cookieObject["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieObject["domain"]!.Value<string>(), Is.EqualTo("cookieDomain"));
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
        }        
    }

    [Test]
    public void TestCanSerializeParametersWithStorageKeyPartitionDescriptor()
    {
        SetCookieCommandParameters properties = new(new PartialCookie("cookieName", BytesValue.FromString("cookieValue"), "cookieDomain"))
        {
            Partition = new StorageKeyPartitionDescriptor()
            {
                UserContextId = "myUserContext"
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("cookie"));
            Assert.That(serialized["cookie"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject cookieObject = serialized["cookie"]!.Value<JObject>()!;
            Assert.That(cookieObject, Has.Count.EqualTo(3));
            Assert.That(cookieObject, Contains.Key("name"));
            Assert.That(cookieObject["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieObject["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(cookieObject, Contains.Key("value"));
            Assert.That(cookieObject["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject cookieValueObject = cookieObject["value"]!.Value<JObject>()!;
            Assert.That(cookieValueObject, Has.Count.EqualTo(2));
            Assert.That(cookieValueObject, Contains.Key("type"));
            Assert.That(cookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(cookieValueObject, Contains.Key("value"));
            Assert.That(cookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["value"]!.Value<string>(), Is.EqualTo("cookieValue"));
            Assert.That(cookieObject, Contains.Key("domain"));
            Assert.That(cookieObject["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieObject["domain"]!.Value<string>(), Is.EqualTo("cookieDomain"));
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
        }        
    }
}
