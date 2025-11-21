namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class AddInterceptCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        AddInterceptCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("network.addIntercept"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        AddInterceptCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("phases"));
            Assert.That(serialized["phases"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["phases"] as JArray, Is.Empty);
        }
    }

    [Test]
    public void TestCanSerializeParametersWithPhases()
    {
        AddInterceptCommandParameters properties = new();
        properties.Phases.Add(InterceptPhase.BeforeRequestSent);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("phases"));
            Assert.That(serialized["phases"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray phases = (JArray)serialized["phases"]!;
            Assert.That(phases, Has.Count.EqualTo(1));
            Assert.That(phases[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(phases[0].Value<string>(), Is.EqualTo("beforeRequestSent"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithAllProperties()
    {
        AddInterceptCommandParameters properties = new()
        {
            BrowsingContextIds = ["myContext"],
            UrlPatterns = [new UrlPatternString("https://example.com/*")]
        };
        properties.Phases.Add(InterceptPhase.BeforeRequestSent);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("phases"));
            Assert.That(serialized["phases"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray phases = (JArray)serialized["phases"]!;
            Assert.That(phases, Has.Count.EqualTo(1));
            Assert.That(phases[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(phases[0].Value<string>(), Is.EqualTo("beforeRequestSent"));
            Assert.That(serialized, Contains.Key("contexts"));
            JArray contexts = (JArray)serialized["contexts"]!;
            Assert.That(contexts, Has.Count.EqualTo(1));
            Assert.That(contexts[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contexts[0].Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("urlPatterns"));
            Assert.That(serialized["urlPatterns"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray patterns = (JArray)serialized["urlPatterns"]!;
            Assert.That(patterns, Has.Count.EqualTo(1));
            Assert.That(patterns[0].Type, Is.EqualTo(JTokenType.Object));
            JObject urlPatternObject = (JObject)patterns[0];
            Assert.That(urlPatternObject, Has.Count.EqualTo(2));
            Assert.That(urlPatternObject, Contains.Key("type"));
            Assert.That(urlPatternObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(urlPatternObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(urlPatternObject, Contains.Key("pattern"));
            Assert.That(urlPatternObject["pattern"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(urlPatternObject["pattern"]!.Value<string>(), Is.EqualTo("https://example.com/*"));
        }
    } 
}
