namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CreateUserContextCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        CreateUserContextCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("browser.createUserContext"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        CreateUserContextCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptInsecureCertsTrue()
    {
        CreateUserContextCommandParameters properties = new()
        {
            AcceptInsecureCerts = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("acceptInsecureCerts"));
            Assert.That(serialized["acceptInsecureCerts"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["acceptInsecureCerts"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithAcceptInsecureCertsFalse()
    {
        CreateUserContextCommandParameters properties = new()
        {
            AcceptInsecureCerts = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("acceptInsecureCerts"));
            Assert.That(serialized["acceptInsecureCerts"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["acceptInsecureCerts"]!.Value<bool>(), Is.False);
        });
    }
}
