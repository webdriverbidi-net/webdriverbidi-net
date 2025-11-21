namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class KeyDownActionTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        KeyDownAction properties = new("a");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("keyDown"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["value"]!.Value<string>(), Is.EqualTo("a"));
        }
    }

    [Test]
    public void TestCreatingActionWithEmptyValueThrows()
    {
        Assert.That(() => new KeyDownAction(string.Empty), Throws.InstanceOf<ArgumentException>().With.Message.Contains("Action value cannot be null or the empty string"));
    }
}
