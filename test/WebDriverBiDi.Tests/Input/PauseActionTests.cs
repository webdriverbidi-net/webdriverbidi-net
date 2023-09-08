namespace WebDriverBiDi.Input;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PauseActionTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        PauseAction properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pause"));
       });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalDuration()
    {
        PauseAction properties = new()
        {
            Duration = TimeSpan.FromMilliseconds(1),
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pause"));
            Assert.That(serialized, Contains.Key("duration"));
            Assert.That(serialized["duration"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["duration"]!.Value<long>(), Is.EqualTo(1));
        });
    }
}