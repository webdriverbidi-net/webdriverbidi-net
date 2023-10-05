namespace WebDriverBiDi.Session;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class StatusCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        StatusCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("session.status"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        StatusCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}
