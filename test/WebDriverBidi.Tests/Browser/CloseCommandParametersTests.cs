namespace WebDriverBidi.Browser;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CloseCommandParametersTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        CloseCommandParameters properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }
}