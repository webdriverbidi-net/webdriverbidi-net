namespace WebDriverBidi.Session;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class StatusCommandSettingsTests
{
    [Test]
    public void TestCanSerializeSettings()
    {
        var properties = new StatusCommandSettings();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized.Count, Is.EqualTo(0));
    }
}