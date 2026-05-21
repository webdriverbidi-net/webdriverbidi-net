namespace WebDriverBiDi.Network;

using System.Text.Json;

public class CookieTests
{
    [Fact]
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);

        Assert.Equal("cookieName", cookie.Name);
        Assert.Equal(BytesValueType.String, cookie.Value.Type);
        Assert.Equal("cookieValue", cookie.Value.Value);
        Assert.Equal("cookieDomain", cookie.Domain);
        Assert.Equal("/cookiePath", cookie.Path);
        Assert.False(cookie.Secure);
        Assert.False(cookie.HttpOnly);
        Assert.Equal(CookieSameSiteValue.Strict, cookie.SameSite);
        Assert.Equal(100, cookie.Size);
        Assert.Null(cookie.Expires);
        Assert.Null(cookie.EpochExpires);
        Assert.Empty(cookie.AdditionalData);
    }

    [Fact]
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);

        Assert.Equal("cookieName", cookie.Name);
        Assert.Equal(BytesValueType.String, cookie.Value.Type);
        Assert.Equal("cookieValue", cookie.Value.Value);
        Assert.Equal("cookieDomain", cookie.Domain);
        Assert.Equal("/cookiePath", cookie.Path);
        Assert.False(cookie.Secure);
        Assert.False(cookie.HttpOnly);
        Assert.Equal(CookieSameSiteValue.Lax, cookie.SameSite);
        Assert.Equal(100, cookie.Size);
        Assert.Null(cookie.Expires);
        Assert.Null(cookie.EpochExpires);
        Assert.Empty(cookie.AdditionalData);
    }

    [Fact]
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);

        Assert.Equal("cookieName", cookie.Name);
        Assert.Equal(BytesValueType.String, cookie.Value.Type);
        Assert.Equal("cookieValue", cookie.Value.Value);
        Assert.Equal("cookieDomain", cookie.Domain);
        Assert.Equal("/cookiePath", cookie.Path);
        Assert.False(cookie.Secure);
        Assert.False(cookie.HttpOnly);
        Assert.Equal(CookieSameSiteValue.None, cookie.SameSite);
        Assert.Equal(100, cookie.Size);
        Assert.Null(cookie.Expires);
        Assert.Null(cookie.EpochExpires);
        Assert.Empty(cookie.AdditionalData);
    }

    [Fact]
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);

        Assert.Equal("cookieName", cookie.Name);
        Assert.Equal(BytesValueType.Base64, cookie.Value.Type);
        Assert.Equal(base64Value, cookie.Value.Value);
        Assert.Equal("cookieDomain", cookie.Domain);
        Assert.Equal("/cookiePath", cookie.Path);
        Assert.False(cookie.Secure);
        Assert.False(cookie.HttpOnly);
        Assert.Equal(CookieSameSiteValue.Lax, cookie.SameSite);
        Assert.Equal(100, cookie.Size);
        Assert.Null(cookie.Expires);
        Assert.Null(cookie.EpochExpires);
        Assert.Empty(cookie.AdditionalData);
    }

    [Fact]
    public void TestCanDeserializeCookieWithExpiration()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalSeconds);
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);

        Assert.Equal("cookieName", cookie.Name);
        Assert.Equal(BytesValueType.String, cookie.Value.Type);
        Assert.Equal("cookieValue", cookie.Value.Value);
        Assert.Equal("cookieDomain", cookie.Domain);
        Assert.Equal("/cookiePath", cookie.Path);
        Assert.False(cookie.Secure);
        Assert.False(cookie.HttpOnly);
        Assert.Equal(CookieSameSiteValue.Lax, cookie.SameSite);
        Assert.Equal(100, cookie.Size);
        Assert.Equal(expireTime, cookie.Expires);
        Assert.Equal(milliseconds, cookie.EpochExpires);
        Assert.Empty(cookie.AdditionalData);
    }

    [Fact]
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);

        Assert.Equal("cookieName", cookie.Name);
        Assert.Equal(BytesValueType.String, cookie.Value.Type);
        Assert.Equal("cookieValue", cookie.Value.Value);
        Assert.Equal("cookieDomain", cookie.Domain);
        Assert.Equal("/cookiePath", cookie.Path);
        Assert.False(cookie.Secure);
        Assert.False(cookie.HttpOnly);
        Assert.Equal(CookieSameSiteValue.Strict, cookie.SameSite);
        Assert.Equal(100, cookie.Size);
        Assert.Null(cookie.Expires);
        Assert.Null(cookie.EpochExpires);
        Assert.Single(cookie.AdditionalData);
        Assert.True(cookie.AdditionalData.ContainsKey("extraData"));
        object? extraData = cookie.AdditionalData["extraData"];
        Assert.NotNull(extraData);
        Assert.Equal(typeof(string), extraData.GetType());
        Assert.Equal("myExtraData", extraData);
    }

    [Fact]
    public void TestCanConvertDeserializeCookieToSetCookieHeader()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalSeconds);
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);
        SetCookieHeader header = cookie.ToSetCookieHeader();

        Assert.Equal("cookieName", header.Name);
        Assert.Equal(BytesValueType.String, header.Value.Type);
        Assert.Equal("cookieValue", header.Value.Value);
        Assert.Equal("cookieDomain", header.Domain);
        Assert.Equal("/cookiePath", header.Path);
        Assert.False(header.Secure);
        Assert.False(header.HttpOnly);
        Assert.Equal(CookieSameSiteValue.Lax, header.SameSite);
        Assert.Equal(expireTime, header.Expires);
        Assert.Null(header.MaxAge);
    }

    [Fact]
    public void TestCopySemantics()
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
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);
        Cookie copy = cookie with { };
        Assert.Equal(cookie, copy);
    }

    [Fact]
    public void TestCookieWithNullExpiryDoesNotSetExpires()
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
                        "expiry": null
                      }
                      """;
        Cookie? cookie = JsonSerializer.Deserialize<Cookie>(json);
        Assert.NotNull(cookie);

        Assert.Null(cookie.EpochExpires);
        Assert.Null(cookie.Expires);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'name'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'value'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'domain'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'path'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'secure'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'httpOnly'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'sameSite'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("missing required properties including: 'size'", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }

    [Fact]
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
        Assert.Contains("value 'invalid' is not valid for enum type", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Cookie>(json)).Message);
    }
}
