namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SerializationOptionsTests
{
    [Test]
    public void TestCanSerializeOptions()
    {
        SerializationOptions options = new();
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Is.Empty);
        }
    }

    [Test]
    public void TestCanSerializeOptionsWithOptionalMaxDomDepth()
    {
        SerializationOptions options = new()
        {
            MaxDomDepth = 1
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("maxDomDepth"));
            Assert.That(serialized["maxDomDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxDomDepth"]!.Value<long>, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestCanSerializeOptionsWithOptionalMaxObjectDepth()
    {
        SerializationOptions options = new()
        {
            MaxObjectDepth = 1
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("maxObjectDepth"));
            Assert.That(serialized["maxObjectDepth"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxObjectDepth"]!.Value<long>, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestCanSerializeOptionsWithOptionalIncludeShadowTree()
    {
        SerializationOptions options = new()
        {
            IncludeShadowTree = IncludeShadowTreeSerializationOption.None
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("includeShadowTree"));
            Assert.That(serialized["includeShadowTree"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["includeShadowTree"]!.Value<string>, Is.EqualTo("none"));
        }
    }

    [Test]
    public void TestCanSerializeOptionsWithOptionalIncludeOpenShadowTree()
    {
        SerializationOptions options = new()
        {
            IncludeShadowTree = IncludeShadowTreeSerializationOption.Open
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("includeShadowTree"));
            Assert.That(serialized["includeShadowTree"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["includeShadowTree"]!.Value<string>, Is.EqualTo("open"));
        }
    }

    [Test]
    public void TestCanSerializeOptionsWithOptionalIncludeAllShadowTree()
    {
        SerializationOptions options = new()
        {
            IncludeShadowTree = IncludeShadowTreeSerializationOption.All
        };
        string json = JsonSerializer.Serialize(options);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized.Type, Is.EqualTo(JTokenType.Object));
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("includeShadowTree"));
            Assert.That(serialized["includeShadowTree"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["includeShadowTree"]!.Value<string>, Is.EqualTo("all"));
        }
    }
}
