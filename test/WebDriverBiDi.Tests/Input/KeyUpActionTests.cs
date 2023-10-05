namespace WebDriverBiDi.Input;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class KeyUpActionTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        KeyUpAction properties = new("a");
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("keyUp"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["value"]!.Value<string>(), Is.EqualTo("a"));
        });
    }

    [Test]
    public void TestCreatingActionWithEmptyValueThrows()
    {
        Assert.That(() => new KeyUpAction(string.Empty), Throws.InstanceOf<ArgumentException>().With.Message.Contains("Action value cannot be null or the empty string"));
    }
}
