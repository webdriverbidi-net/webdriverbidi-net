namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetUserContextsCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        GetUserContextsCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("browser.getUserContexts"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        CloseCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}
