namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class CookieHeaderTests
{
    [Test]
    public void TestCanSerialize()
    {
        CookieHeader cookieHeader = new();
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.Empty);
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject valueObject = (JObject)serialized["value"]!;
            Assert.That(valueObject, Has.Count.EqualTo(2));
            Assert.That(valueObject, Contains.Key("type"));
            Assert.That(valueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(valueObject, Contains.Key("value"));
            Assert.That(valueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["value"]!.Value<string>(), Is.Empty);
        });
    }

    [Test]
    public void TestCanSerializeWithConstructorValues()
    {
        CookieHeader cookieHeader = new("cookieName", "cookieValue");
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject valueObject = (JObject)serialized["value"]!;
            Assert.That(valueObject, Has.Count.EqualTo(2));
            Assert.That(valueObject, Contains.Key("type"));
            Assert.That(valueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(valueObject, Contains.Key("value"));
            Assert.That(valueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["value"]!.Value<string>(), Is.EqualTo("cookieValue"));
        });
    }

    [Test]
    public void TestCanSerializeWithPropertySetValues()
    {
        CookieHeader cookieHeader = new()
        {
            Name = "cookieName",
            Value = BytesValue.FromString("cookieValue")
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject valueObject = (JObject)serialized["value"]!;
            Assert.That(valueObject, Has.Count.EqualTo(2));
            Assert.That(valueObject, Contains.Key("type"));
            Assert.That(valueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(valueObject, Contains.Key("value"));
            Assert.That(valueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["value"]!.Value<string>(), Is.EqualTo("cookieValue"));
        });
    }

    [Test]
    public void TestCanSerializeWithPropertySetAndBynaryValue()
    {
        byte[] cookieValue = new byte[] { 0x41, 0x42, 0x43 };
        string base64Value = Convert.ToBase64String(cookieValue);
        CookieHeader cookieHeader = new()
        {
            Name = "cookieName",
            Value = BytesValue.FromByteArray(cookieValue)
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject valueObject = (JObject)serialized["value"]!;
            Assert.That(valueObject, Has.Count.EqualTo(2));
            Assert.That(valueObject, Contains.Key("type"));
            Assert.That(valueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["type"]!.Value<string>(), Is.EqualTo("base64"));
            Assert.That(valueObject, Contains.Key("value"));
            Assert.That(valueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["value"]!.Value<string>(), Is.EqualTo(base64Value));
        });
    }
}
