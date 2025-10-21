namespace WebDriverBiDi.Network;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class CookieTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserializeCookie()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "strict",
                        "size": 100
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(cookie.Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Strict));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
            Assert.That(cookie.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithSameSiteLax()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(cookie.Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
            Assert.That(cookie.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithSameSiteNone()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "none",
                        "size": 100
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(cookie.Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.None));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
            Assert.That(cookie.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithBinaryValue()
    {
        byte[] byteArray = new byte[] { 0x41, 0x42, 0x43 };
        string base64Value = Convert.ToBase64String(byteArray);
        string json = $$"""
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "base64",
                          "value": "{{base64Value}}"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie.Value.Type, Is.EqualTo(BytesValueType.Base64));
            Assert.That(cookie.Value.Value, Is.EqualTo(base64Value));
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
            Assert.That(cookie.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithExpiration()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100,
                        "expiry": {{milliseconds}}
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(cookie.Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Lax));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.EqualTo(expireTime));
            Assert.That(cookie.EpochExpires, Is.EqualTo(milliseconds));
            Assert.That(cookie.AdditionalData, Is.Empty);
        });
    }

    [Test]
    public void TestCanDeserializeCookieWithAdditionalData()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "strict",
                        "size": 100,
                        "extraData": "myExtraData"
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        Assert.That(cookie, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(cookie!.Name, Is.EqualTo("cookieName"));
            Assert.That(cookie.Value.Type, Is.EqualTo(BytesValueType.String));
            Assert.That(cookie.Value.Value, Is.EqualTo("cookieValue"));
            Assert.That(cookie.Domain, Is.EqualTo("cookieDomain"));
            Assert.That(cookie.Path, Is.EqualTo("/cookiePath"));
            Assert.That(cookie.Secure, Is.False);
            Assert.That(cookie.HttpOnly, Is.False);
            Assert.That(cookie.SameSite, Is.EqualTo(CookieSameSiteValue.Strict));
            Assert.That(cookie.Size, Is.EqualTo(100));
            Assert.That(cookie.Expires, Is.Null);
            Assert.That(cookie.EpochExpires, Is.Null);
            Assert.That(cookie.AdditionalData, Has.Count.EqualTo(1));
            Assert.That(cookie.AdditionalData, Contains.Key("extraData"));
            Assert.That(cookie.AdditionalData["extraData"]!.GetType, Is.EqualTo(typeof(string)));
            Assert.That(cookie.AdditionalData["extraData"]!, Is.EqualTo("myExtraData"));
        });
    }

    [Test]
    public void TestCanConvertDeserializeCookieToSetCookieHeader()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerMillisecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds);
        string json = $$"""
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100,
                        "expiry": {{milliseconds}}
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json, deserializationOptions);
        Assert.That(cookie, Is.Not.Null);
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

    [Test]
    public void TestDeserializeWithMissingNameThrows()
    {
        string json = """
                      {
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: name"));
    }

    [Test]
    public void TestDeserializeWithMissingValueThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: value"));
    }

    [Test]
    public void TestDeserializeWithMissingDomainThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: domain"));
    }

    [Test]
    public void TestDeserializeWithMissingPathThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: path"));
    }

    [Test]
    public void TestDeserializeWithMissingSecureThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "httpOnly": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: secure"));
    }

    [Test]
    public void TestDeserializeWithMissingHttpOnlyThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "sameSite": "lax",
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: httpOnly"));
    }

    [Test]
    public void TestDeserializeWithMissingSameSiteThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: sameSite"));
    }

    [Test]
    public void TestDeserializeWithMissingSizeThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "lax"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<JsonException>().With.Message.Contains("missing required properties, including the following: size"));
    }

    [Test]
    public void TestDeserializeWithInvalidSameSiteValueThrows()
    {
        string json = """
                      {
                        "name": "cookieName",
                        "value": {
                          "type": "string",
                          "value": "cookieValue"
                        },
                        "domain": "cookieDomain",
                        "path": "/cookiePath",
                        "secure": false,
                        "httpOnly": false,
                        "sameSite": "invalid",
                        "size": 100
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<Cookie>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("value 'invalid' is not valid for enum type"));
    }
}
