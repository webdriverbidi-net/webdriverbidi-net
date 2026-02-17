namespace WebDriverBiDi.UserAgentClientHints;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class BrandVersionTests
{
    [Test]
    public void TestCanSerialize()
    {
        BrandVersion brandVersion = new("myBrand", "myVersion");
        string json = JsonSerializer.Serialize(brandVersion);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("brand"));
            Assert.That(serialized["brand"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["brand"]!.Value<string>, Is.EqualTo("myBrand"));
            Assert.That(serialized, Contains.Key("version"));
            Assert.That(serialized["version"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["version"]!.Value<string>, Is.EqualTo("myVersion"));
        }
    }
}
