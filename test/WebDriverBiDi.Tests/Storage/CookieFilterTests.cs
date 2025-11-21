namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Network;

[TestFixture]
public class CookieFilterTests
{
    [Test]
    public void TestCanSerializeCookieFilterWithName()
    {
        CookieFilter properties = new()
        {
            Name = "myCookieName"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("name"));
            Assert.That(serialized["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["name"]!.Value<string>(), Is.EqualTo("myCookieName"));
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithValue()
    {
        CookieFilter properties = new()
        {
            Value = BytesValue.FromString("myCookieValue")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("value"));
            Assert.That(serialized["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject bytesValueObject = serialized["value"]!.Value<JObject>()!;
            Assert.That(bytesValueObject, Has.Count.EqualTo(2));
            Assert.That(bytesValueObject, Contains.Key("type"));
            Assert.That(bytesValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(bytesValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(bytesValueObject, Contains.Key("value"));
            Assert.That(bytesValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(bytesValueObject["value"]!.Value<string>(), Is.EqualTo("myCookieValue"));
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithDomain()
    {
        CookieFilter properties = new()
        {
            Domain = "myCookieDomain"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myCookieDomain"));
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithPath()
    {
        CookieFilter properties = new()
        {
            Path = "myCookiePath"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("path"));
            Assert.That(serialized["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["path"]!.Value<string>(), Is.EqualTo("myCookiePath"));
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithSize()
    {
        CookieFilter properties = new()
        {
            Size = 3
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("size"));
            Assert.That(serialized["size"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["size"]!.Value<ulong>(), Is.EqualTo(3));
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithHttpOnlyTrue()
    {
        CookieFilter properties = new()
        {
            HttpOnly = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("httpOnly"));
            Assert.That(serialized["httpOnly"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["httpOnly"]!.Value<bool>(), Is.True);
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithHttpOnlyFalse()
    {
        CookieFilter properties = new()
        {
            HttpOnly = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("httpOnly"));
            Assert.That(serialized["httpOnly"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["httpOnly"]!.Value<bool>(), Is.False);
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithSecureTrue()
    {
        CookieFilter properties = new()
        {
            Secure = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("secure"));
            Assert.That(serialized["secure"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["secure"]!.Value<bool>(), Is.True);
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithSecureFalse()
    {
        CookieFilter properties = new()
        {
            Secure = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("secure"));
            Assert.That(serialized["secure"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["secure"]!.Value<bool>(), Is.False);
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithSameSiteValue()
    {
        CookieFilter properties = new()
        {
            SameSite = CookieSameSiteValue.Strict
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("sameSite"));
            Assert.That(serialized["sameSite"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sameSite"]!.Value<string>(), Is.EqualTo("strict"));
        }
    }

    [Test]
    public void TestCanSerializeCookieFilterWithExpirationDate()
    {
        DateTime now = DateTime.UtcNow;
        DateTime expirationDate = now.AddDays(1);
        ulong milliseconds = Convert.ToUInt64(expirationDate.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        CookieFilter properties = new()
        {
            Expires = expirationDate
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("expiry"));
            Assert.That(serialized["expiry"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["expiry"]!.Value<ulong>(), Is.EqualTo(milliseconds));
        }
    }

    [Test]
    public void TestSettingCookieFilterExpirationDate()
    {
        DateTime now = DateTime.UtcNow.AddDays(1);
        DateTime expirationDate = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        CookieFilter properties = new()
        {
            Expires = expirationDate
        };
        Assert.That(properties.Expires, Is.EqualTo(expirationDate));
    }

    [Test]
    public void TestSettingCookieFilterExpirationDateNull()
    {
        DateTime now = DateTime.UtcNow;
        CookieFilter properties = new()
        {
            Expires = null
        };
        Assert.That(properties.Expires, Is.Null);
    }
}
