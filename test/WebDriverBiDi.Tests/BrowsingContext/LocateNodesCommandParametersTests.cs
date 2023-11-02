namespace WebDriverBiDi.BrowsingContext;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Script;

[TestFixture]
public class LocateNodesCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"));
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.locateNodes"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"));
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("locator"));
            Assert.That(serialized["locator"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["locator"]!, Contains.Key("type"));
            Assert.That(serialized["locator"]!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["type"]!.Value<string>, Is.EqualTo("css"));
            Assert.That(serialized["locator"]!, Contains.Key("value"));
            Assert.That(serialized["locator"]!["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["value"]!.Value<string>, Is.EqualTo(".selector"));
        });
    }

    [Test]
    public void TestCanSerializeWithMaxNodeCount()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"))
        {
            MaxNodeCount = 10
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("locator"));
            Assert.That(serialized["locator"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["locator"]!, Contains.Key("type"));
            Assert.That(serialized["locator"]!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["type"]!.Value<string>, Is.EqualTo("css"));
            Assert.That(serialized["locator"]!, Contains.Key("value"));
            Assert.That(serialized["locator"]!["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["value"]!.Value<string>, Is.EqualTo(".selector"));
            Assert.That(serialized, Contains.Key("maxNodeCount"));
            Assert.That(serialized["maxNodeCount"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxNodeCount"]!.Value<ulong>(), Is.EqualTo(10));
        });
    }

    [Test]
    public void TestCanSerializeWithResultOwnership()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"))
        {
            ResultOwnership = ResultOwnership.Root
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("locator"));
            Assert.That(serialized["locator"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["locator"]!, Contains.Key("type"));
            Assert.That(serialized["locator"]!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["type"]!.Value<string>, Is.EqualTo("css"));
            Assert.That(serialized["locator"]!, Contains.Key("value"));
            Assert.That(serialized["locator"]!["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["value"]!.Value<string>, Is.EqualTo(".selector"));
            Assert.That(serialized, Contains.Key("resultOwnership"));
            Assert.That(serialized["resultOwnership"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["resultOwnership"]!.Value<string>(), Is.EqualTo("root"));
        });
    }

    [Test]
    public void TestCanSerializeWithSandbox()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"))
        {
            Sandbox = "mySandbox"
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("locator"));
            Assert.That(serialized["locator"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["locator"]!, Contains.Key("type"));
            Assert.That(serialized["locator"]!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["type"]!.Value<string>, Is.EqualTo("css"));
            Assert.That(serialized["locator"]!, Contains.Key("value"));
            Assert.That(serialized["locator"]!["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["value"]!.Value<string>, Is.EqualTo(".selector"));
            Assert.That(serialized, Contains.Key("sandbox"));
            Assert.That(serialized["sandbox"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sandbox"]!.Value<string>(), Is.EqualTo("mySandbox"));
        });
    }

    [Test]
    public void TestCanSerializeWithSerializationOptions()
    {
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"))
        {
            SerializationOptions = new()
            {
                IncludeShadowTree = IncludeShadowTreeSerializationOption.All,
                MaxDomDepth = 10,
                MaxObjectDepth = 0,
            }
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("locator"));
            Assert.That(serialized["locator"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["locator"]!, Contains.Key("type"));
            Assert.That(serialized["locator"]!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["type"]!.Value<string>, Is.EqualTo("css"));
            Assert.That(serialized["locator"]!, Contains.Key("value"));
            Assert.That(serialized["locator"]!["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["value"]!.Value<string>, Is.EqualTo(".selector"));
            Assert.That(serialized, Contains.Key("serializationOptions"));
            Assert.That(serialized["serializationOptions"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject serializationOptions = (serialized["serializationOptions"]! as JObject)!;
            Assert.That(serializationOptions, Has.Count.EqualTo(3));
            Assert.That(serializationOptions, Contains.Key("includeShadowTree"));
            Assert.That(serializationOptions["includeShadowTree"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serializationOptions["includeShadowTree"]!.Value<string>, Is.EqualTo("all"));
            Assert.That(serializationOptions, Contains.Key("maxDomDepth"));
            Assert.That(serializationOptions["maxDomDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serializationOptions["maxDomDepth"]!.Value<ulong>, Is.EqualTo(10));
            Assert.That(serializationOptions, Contains.Key("maxObjectDepth"));
            Assert.That(serializationOptions["maxObjectDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serializationOptions["maxObjectDepth"]!.Value<ulong>, Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeWithContextNode()
    {
        string nodeJson = @"{ ""type"": ""node"", ""sharedId"": ""mySharedId"", ""value"": { ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0 } }";
        RemoteValue nodeValue = JsonConvert.DeserializeObject<RemoteValue>(nodeJson)!;
        LocateNodesCommandParameters properties = new("myContextId", new CssLocator(".selector"));
        properties.ContextNodes.Add(nodeValue.ToSharedReference());

        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
            Assert.That(serialized, Contains.Key("locator"));
            Assert.That(serialized["locator"]!.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized["locator"]!, Contains.Key("type"));
            Assert.That(serialized["locator"]!["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["type"]!.Value<string>, Is.EqualTo("css"));
            Assert.That(serialized["locator"]!, Contains.Key("value"));
            Assert.That(serialized["locator"]!["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["locator"]!["value"]!.Value<string>, Is.EqualTo(".selector"));
            Assert.That(serialized, Contains.Key("contextNodes"));
            Assert.That(serialized["contextNodes"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["contextNodes"]! as JArray, Has.Count.EqualTo(1));
        });
    }
}
