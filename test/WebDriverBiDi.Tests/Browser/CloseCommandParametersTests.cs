namespace WebDriverBiDi.Browser;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CloseCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        CloseCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("browser.close"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        CloseCommandParameters properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}