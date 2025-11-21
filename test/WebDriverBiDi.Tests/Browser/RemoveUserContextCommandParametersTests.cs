namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class RemoveUserContextCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        RemoveUserContextCommandParameters properties = new("myUserContext");
        Assert.That(properties.MethodName, Is.EqualTo("browser.removeUserContext"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        RemoveUserContextCommandParameters properties = new("myUserContext");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("userContext"));
            Assert.That(serialized["userContext"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["userContext"]!.Value<string>(), Is.EqualTo("myUserContext"));
        }
    }
}
