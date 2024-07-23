namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class NewCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        NewCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("session.new"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        NewCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Contains.Key("capabilities"));
        Assert.That(serialized["capabilities"], Is.Empty);
    }

    [Test]
    public void TestCanSerializeWithAlwaysMatch()
    {
        NewCommandParameters properties = new()
        {
            Capabilities =
            {
                AlwaysMatch = new CapabilitiesRequestInfo() { BrowserName = "greatBrowser" }
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("capabilities"));
            
            Assert.That(serialized["capabilities"], Contains.Key("alwaysMatch"));
            Assert.That(serialized["capabilities"]!["alwaysMatch"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? alwaysMatch = serialized["capabilities"]["alwaysMatch"] as JObject;
        Assert.That(alwaysMatch, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(alwaysMatch!, Contains.Key("browserName"));
            Assert.That(alwaysMatch!["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
        });
    }

    [Test]
    public void TestCanSerializeWithFirstMatch()
    {
        NewCommandParameters properties = new();
        properties.Capabilities.FirstMatch.Add(new CapabilitiesRequestInfo() { BrowserName = "greatBrowser" });
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("capabilities"));
            Assert.That(serialized["capabilities"], Contains.Key("firstMatch"));
            Assert.That(serialized["capabilities"]!["firstMatch"]!.Type, Is.EqualTo(JTokenType.Array));
        });
        JArray? firstMatch = serialized["capabilities"]["firstMatch"] as JArray;
        Assert.That(firstMatch, Is.Not.Null);
        Assert.That(firstMatch, Has.Count.EqualTo(1));
        Assert.That(firstMatch![0].Type, Is.EqualTo(JTokenType.Object));

        JObject? firstMatchElement = firstMatch[0] as JObject;
        Assert.Multiple(() =>
        {
            Assert.That(firstMatchElement, Has.Count.EqualTo(1));
            Assert.That(firstMatchElement!, Contains.Key("browserName"));
            Assert.That(firstMatchElement!["browserName"]!.Value<string>(), Is.EqualTo("greatBrowser"));
        });
    }
}
