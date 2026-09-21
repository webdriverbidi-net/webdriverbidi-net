namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Network;

public class PartialCookieTests
{
    [Fact]
    public void TestCanSerializePartialCookie()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithPath()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Path = "myCookiePath"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("path"));
        JToken? path = serialized["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal("myCookiePath", path.Value<string>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithHttpOnlyTrue()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            HttpOnly = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("httpOnly"));
        JToken? httpOnly = serialized["httpOnly"];
        Assert.NotNull(httpOnly);
        Assert.Equal(JTokenType.Boolean, httpOnly.Type);
        Assert.True(httpOnly.Value<bool>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithHttpOnlyFalse()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            HttpOnly = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("httpOnly"));
        JToken? httpOnly = serialized["httpOnly"];
        Assert.NotNull(httpOnly);
        Assert.Equal(JTokenType.Boolean, httpOnly.Type);
        Assert.False(httpOnly.Value<bool>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithSecureTrue()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Secure = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("secure"));
        JToken? secure = serialized["secure"];
        Assert.NotNull(secure);
        Assert.Equal(JTokenType.Boolean, secure.Type);
        Assert.True(secure.Value<bool>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithSecureFalse()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Secure = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("secure"));
        JToken? secure = serialized["secure"];
        Assert.NotNull(secure);
        Assert.Equal(JTokenType.Boolean, secure.Type);
        Assert.False(secure.Value<bool>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithSameSiteNone()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            SameSite = CookieSameSiteValue.None
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("sameSite"));
        JToken? sameSite = serialized["sameSite"];
        Assert.NotNull(sameSite);
        Assert.Equal(JTokenType.String, sameSite.Type);
        Assert.Equal("none", sameSite.Value<string>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithSameSiteLax()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            SameSite = CookieSameSiteValue.Lax
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("sameSite"));
        JToken? sameSite = serialized["sameSite"];
        Assert.NotNull(sameSite);
        Assert.Equal(JTokenType.String, sameSite.Type);
        Assert.Equal("lax", sameSite.Value<string>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithSameSiteStrict()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            SameSite = CookieSameSiteValue.Strict
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("sameSite"));
        JToken? sameSite = serialized["sameSite"];
        Assert.NotNull(sameSite);
        Assert.Equal(JTokenType.String, sameSite.Type);
        Assert.Equal("strict", sameSite.Value<string>());
    }

    [Fact]
    public void TestExpiresOfDateTimeMaxValueRoundTripsWithoutThrowing()
    {
        // DateTime.MaxValue is the usual way to say "never expires". It is a fraction of a second before the end
        // of 9999, so it is stored as the last whole second, and reads back as that second rather than throwing.
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
        };

        Assert.Equal(new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc), properties.Expires);
        Assert.Equal(DateTimeKind.Utc, properties.Expires?.Kind);
        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));
        Assert.Equal(253402300799UL, serialized["expiry"]?.Value<ulong>());
    }

    [Fact]
    public void TestExpiresTruncatesAFractionOfASecond()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(999),
        };

        Assert.Equal(new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), properties.Expires);
    }

    [Fact]
    public void TestCanSerializePartialCookieWithExpirationDate()
    {
        DateTime now = DateTime.UtcNow;
        DateTime expirationDate = now.AddDays(1);
        // Whole seconds, truncated: the setter never rounds up to the next second.
        ulong seconds = (ulong)(expirationDate.Subtract(DateTime.UnixEpoch).Ticks / TimeSpan.TicksPerSecond);
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = expirationDate
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("expiry"));
        JToken? expiry = serialized["expiry"];
        Assert.NotNull(expiry);
        Assert.Equal(JTokenType.Integer, expiry.Type);
        Assert.Equal(seconds, expiry.Value<ulong>());
    }

    [Fact]
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

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());
    }

    [Fact]
    public void TestCanSerializePartialCookieWithAdditionalData()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain");
        properties.AdditionalData["additionalName"] = "additionalValue";
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? partialCookieValueObject = value.Value<JObject>();
        Assert.NotNull(partialCookieValueObject);
        Assert.Equal(2, partialCookieValueObject.Count);

        Assert.True(partialCookieValueObject.ContainsKey("type"));
        JToken? valueType = partialCookieValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(partialCookieValueObject.ContainsKey("value"));
        JToken? valueValue = partialCookieValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());

        Assert.True(serialized.ContainsKey("additionalName"));
        JToken? additionalName = serialized["additionalName"];
        Assert.NotNull(additionalName);
        Assert.Equal(JTokenType.String, additionalName.Type);
        Assert.Equal("additionalValue", additionalName.Value<string>());
    }

    [Fact]
    public void TestSettingPartialCookieExpirationDate()
    {
        DateTime now = DateTime.UtcNow.AddDays(1);
        DateTime expirationDate = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = expirationDate
        };
        Assert.Equal(expirationDate, properties.Expires);
    }

    [Fact]
    public void TestSettingPartialCookieExpirationDateNull()
    {
        DateTime now = DateTime.UtcNow;
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = null
        };
        Assert.Null(properties.Expires);
    }

    [Fact]
    public void TestSettingPartialCookieExpirationDateBeforeUnixEpochThrows()
    {
        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            properties.Expires = new DateTime(1969, 12, 31, 23, 59, 59, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void TestSettingPartialCookieExpirationDateNormalizesToUtc(DateTimeKind kind)
    {
        // A Local DateTime names a wall-clock reading, not an instant, and ToUniversalTime treats an
        // Unspecified one the same way; the setter converts before computing epoch seconds, so that the
        // remote end is told the instant the caller meant rather than one a UTC offset away. The
        // expectation is computed with the same conversion, keeping the test independent of the machine
        // time zone, and the property always reads back UTC.
        DateTime expirationDate = new(2030, 1, 1, 12, 0, 0, kind);
        DateTime expectedUtc = expirationDate.ToUniversalTime();
        ulong expectedEpochSeconds = (ulong)(expectedUtc.Subtract(DateTime.UnixEpoch).Ticks / TimeSpan.TicksPerSecond);

        PartialCookie properties = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = expirationDate,
        };

        Assert.Equal(expectedUtc, properties.Expires);
        Assert.Equal(DateTimeKind.Utc, properties.Expires!.Value.Kind);

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));
        Assert.Equal(expectedEpochSeconds, serialized["expiry"]?.Value<ulong>());

        // The same instant, given as UTC, must reach the wire as the same value: the two differ only in
        // how the caller spelled the time.
        PartialCookie fromUtc = new("myCookieName", BytesValue.FromString("myCookieValue"), "myCookieDomain")
        {
            Expires = expectedUtc,
        };
        Assert.Equal(serialized["expiry"]?.Value<ulong>(), JObject.Parse(JsonSerializer.Serialize(fromUtc))["expiry"]?.Value<ulong>());
    }

}
