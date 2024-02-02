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
        CloseCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}
