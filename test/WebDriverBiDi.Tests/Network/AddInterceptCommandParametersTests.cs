namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class AddInterceptCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent);
        Assert.That(properties.MethodName, Is.EqualTo("network.addIntercept"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("phases"));
            Assert.That(serialized["phases"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray interceptArray = (JArray)serialized["phases"]!;
            Assert.That(interceptArray, Has.Count.EqualTo(1));
            Assert.That(interceptArray[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(interceptArray[0].Value<string>(), Is.EqualTo("beforeRequestSent"));
        }
    }

    [Test]
    public void TestCanConstructWithMultiplePhases()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent, InterceptPhase.ResponseStarted);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("phases"));
            Assert.That(serialized["phases"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray interceptArray = (JArray)serialized["phases"]!;
            Assert.That(interceptArray, Has.Count.EqualTo(2));
            Assert.That(interceptArray[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(interceptArray[0].Value<string>(), Is.EqualTo("beforeRequestSent"));
            Assert.That(interceptArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(interceptArray[1].Value<string>(), Is.EqualTo("responseStarted"));
        }
    }

    [Test]
    public void TestDuplicatePhaseArgumentOnlySerializesOnce()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent, InterceptPhase.BeforeRequestSent);
        properties.Phases.Add(InterceptPhase.AuthRequired);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("phases"));
            Assert.That(serialized["phases"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray phases = (JArray)serialized["phases"]!;
            Assert.That(phases, Has.Count.EqualTo(2));
            Assert.That(phases[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(phases[0].Value<string>(), Is.EqualTo("beforeRequestSent"));
            Assert.That(phases[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(phases[1].Value<string>(), Is.EqualTo("authRequired"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithAllProperties()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent)
        {
            BrowsingContextIds = ["myContext"],
            UrlPatterns = [new UrlPatternString("https://example.com/*")]
        };
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

    [Test]
    public void TestOmittingPhaseInConstructorThrows()
    {
        Assert.That(() => new AddInterceptCommandParameters(), Throws.InstanceOf<ArgumentException>().With.Message.StartsWith("You must supply at least one phase for the intercept"));
    }
}
