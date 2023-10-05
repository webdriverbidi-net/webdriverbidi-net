namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetCookieHeaderTests
{
    [Test]
    public void TestCanSerialize()
    {
        SetCookieHeader cookieHeader = new();
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
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue");
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
        SetCookieHeader cookieHeader = new()
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
    public void TestCanSerializeWithPropertySetAndBinaryValue()
    {
        byte[] cookieValue = new byte[] { 0x41, 0x42, 0x43 };
        string base64Value = Convert.ToBase64String(cookieValue);
        SetCookieHeader cookieHeader = new()
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

    [Test]
    public void TestCanSerializeWithDomain()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            Domain = "myDomain"
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
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
            Assert.That(serialized, Contains.Key("domain"));
            Assert.That(serialized["domain"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["domain"]!.Value<string>(), Is.EqualTo("myDomain"));
        });
    }

    [Test]
    public void TestCanSerializeWithPath()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            Path = "myPath"
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
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
            Assert.That(serialized, Contains.Key("path"));
            Assert.That(serialized["path"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["path"]!.Value<string>(), Is.EqualTo("myPath"));
        });
    }

    [Test]
    public void TestCanSerializeWithHttpOnly()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            HttpOnly = true
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
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
            Assert.That(serialized, Contains.Key("httpOnly"));
            Assert.That(serialized["httpOnly"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["httpOnly"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeWithSecure()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            Secure = true
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
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
            Assert.That(serialized, Contains.Key("secure"));
            Assert.That(serialized["secure"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["secure"]!.Value<bool>(), Is.True);
        });
    }

    [Test]
    public void TestCanSerializeWithSameSite()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            SameSite = CookieSameSiteValue.Strict
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
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
            Assert.That(serialized, Contains.Key("sameSite"));
            Assert.That(serialized["sameSite"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sameSite"]!.Value<string>(), Is.EqualTo("strict"));
        });
    }

    [Test]
    public void TestCanSerializeWithExpires()
    {
        DateTime expirationTime = DateTime.Now.AddDays(3);
        string expected = $"{expirationTime.ToUniversalTime():ddd, dd MMM yyyy HH:mm:ss} GMT";
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            Expires = expirationTime
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
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
            Assert.That(serialized, Contains.Key("expires"));
            Assert.That(serialized["expires"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["expires"]!.Value<string>(), Is.EqualTo(expected));
        });
    }

    [Test]
    public void TestCanSerializeWithMaxAge()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            MaxAge = 100
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
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
            Assert.That(serialized, Contains.Key("maxAge"));
            Assert.That(serialized["maxAge"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["maxAge"]!.Value<ulong>(), Is.EqualTo(100));
        });
    }

    [Test]
    public void TestConstructionWithCookie()
    {
        JsonSerializerOptions deserializationOptions = new()
        {
            TypeInfoResolver = new PrivateConstructorContractResolver(),
        };

        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = @"{ ""name"": ""cookieName"", ""value"":{ ""type"": ""string"", ""value"": ""cookieValue"" }, ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100, ""expires"": " + milliseconds + @" }";
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        SetCookieHeader header = cookie!.ToSetCookieHeader();
        Assert.Multiple(() =>
        {
            Assert.That(header!.Name, Is.EqualTo("cookieName"));
            Assert.That(header.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(header.Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(header.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(header.Path, Is.EqualTo("/cookiePath"));
            Assert.That(header.Secure, Is.False);
            Assert.That(header.HttpOnly, Is.False);
            Assert.That(header.SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(header.Expires, Is.EqualTo(expireTime));
            Assert.That(header.MaxAge, Is.Null);
        });
    }
}
