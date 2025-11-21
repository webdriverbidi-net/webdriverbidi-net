namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ReleaseActionsCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        ReleaseActionsCommandParameters properties = new("myContextId");
        Assert.That(properties.MethodName, Is.EqualTo("input.releaseActions"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        ReleaseActionsCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        }
    }
}
