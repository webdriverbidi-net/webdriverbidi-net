namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class StatusCommandPropertiesTests
{
    [Test]
    public void TestCanSerializeProperties()
    {
        var properties = new StatusCommandProperties();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(0));
    }
}