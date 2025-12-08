namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Network;

[TestFixture]
public class PartialCookieTests
{
    [Test]
    public void TestCanSerializePartialCookie()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithPath()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Path = "myCookiePath"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("path"));
            Assert.That(serialized["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["path"]!.Value<string>(), Is.EqualTo("myCookiePath"));
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithSize()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Size = 123
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("size"));
            Assert.That(serialized["size"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["size"]!.Value<ulong>(), Is.EqualTo(123));
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithHttpOnlyTrue()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            HttpOnly = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("httpOnly"));
            Assert.That(serialized["httpOnly"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["httpOnly"]!.Value<bool>(), Is.True);
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithHttpOnlyFalse()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            HttpOnly = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("httpOnly"));
            Assert.That(serialized["httpOnly"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["httpOnly"]!.Value<bool>(), Is.False);
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithSecureTrue()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Secure = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("secure"));
            Assert.That(serialized["secure"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["secure"]!.Value<bool>(), Is.True);
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithSecureFalse()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Secure = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("secure"));
            Assert.That(serialized["secure"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["secure"]!.Value<bool>(), Is.False);
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithSameSiteNone()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            SameSite = CookieSameSiteValue.None
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("sameSite"));
            Assert.That(serialized["sameSite"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sameSite"]!.Value<string>(), Is.EqualTo("none"));
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithSameSiteLax()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            SameSite = CookieSameSiteValue.Lax
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("sameSite"));
            Assert.That(serialized["sameSite"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sameSite"]!.Value<string>(), Is.EqualTo("lax"));
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithSameSiteStrict()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            SameSite = CookieSameSiteValue.Strict
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("sameSite"));
            Assert.That(serialized["sameSite"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sameSite"]!.Value<string>(), Is.EqualTo("strict"));
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithExpirationDate()
    {
        DateTime now = DateTime.UtcNow;
        DateTime expirationDate = now.AddDays(1);
        ulong seconds = Convert.ToUInt64(expirationDate.Subtract(DateTime.UnixEpoch).TotalSeconds);
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = expirationDate
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(4));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
            Assert.That(serialized, Contains.Key("expiry"));
            Assert.That(serialized["expiry"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["expiry"]!.Value<ulong>(), Is.EqualTo(seconds));
        }
    }

    [Test]
    public void TestCanSerializePartialCookieWithNullExpirationDate()
    {
        DateTime now = DateTime.UtcNow;
        DateTime expirationDate = now.AddDays(1);
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = null
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject partialCookieValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(partialCookieValueObject, Has.Count.EqualTo(2));
            Assert.That(partialCookieValueObject, Contains.Key("type"));
            Assert.That(partialCookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(partialCookieValueObject, Contains.Key("value"));
            Assert.That(partialCookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(partialCookieValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
        }
    }

    [Test]
    public void TestSettingPartialCookieExpirationDate()
    {
        DateTime now = DateTime.UtcNow.AddDays(1);
        DateTime expirationDate = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = expirationDate
        };
        Assert.That(properties.Expires, Is.EqualTo(expirationDate));
    }

    [Test]
    public void TestSettingPartialCookieExpirationDateNull()
    {
        DateTime now = DateTime.UtcNow;
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = null
        };
        Assert.That(properties.Expires, Is.Null);
    }
}
