namespace WebDriverBiDi.Network;

using System.Runtime.CompilerServices;
using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetCacheBehaviorCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default);
        Assert.That(properties.MethodName, Is.EqualTo("network.setCacheBehavior"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("cacheBehavior"));
            Assert.That(serialized["cacheBehavior"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["cacheBehavior"]!.Value<string>(), Is.EqualTo("default"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithBypass()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Bypass);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("cacheBehavior"));
            Assert.That(serialized["cacheBehavior"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["cacheBehavior"]!.Value<string>(), Is.EqualTo("bypass"));
        }
    }

    public void TestCanSetCacheBehaviorViaProperty()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default);
        properties.CacheBehavior = CacheBehavior.Bypass;
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("cacheBehavior"));
            Assert.That(serialized["cacheBehavior"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["cacheBehavior"]!.Value<string>(), Is.EqualTo("bypass"));
        }
    } 

    [Test]
    public void TestCanSerializeWithContexts()
    {
        SetCacheBehaviorCommandParameters properties = new(CacheBehavior.Default)
        {
            Contexts = ["myContext"]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("cacheBehavior"));
            Assert.That(serialized["cacheBehavior"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["cacheBehavior"]!.Value<string>(), Is.EqualTo("default"));
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray contextsObject = (JArray)serialized["contexts"]!;
            Assert.That(contextsObject, Has.Count.EqualTo(1));
            Assert.That(contextsObject[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsObject[0].Value<string>(), Is.EqualTo("myContext"));
        }
    }
}
