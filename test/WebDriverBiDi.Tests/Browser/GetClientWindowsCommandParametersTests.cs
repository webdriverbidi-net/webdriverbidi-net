namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GetClientWindowsCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        GetClientWindowsCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("browser.getClientWindows"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        GetClientWindowsCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}
