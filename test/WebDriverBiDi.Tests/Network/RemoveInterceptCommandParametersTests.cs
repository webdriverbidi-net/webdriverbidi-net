namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemoveInterceptCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        RemoveInterceptCommandParameters properties = new("interceptId");
        Assert.That(properties.MethodName, Is.EqualTo("network.removeIntercept"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        RemoveInterceptCommandParameters properties = new("interceptId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("intercept"));
            Assert.That(serialized["intercept"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["intercept"]!.Value<string>(), Is.EqualTo("interceptId"));
        });
    }
}