namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PointerUpActionTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        PointerUpAction properties = new(0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
        }
    }
}
