namespace WebDriverBiDi.Storage;

using System.Text.Json;
using Newtonsoft.Json.Linq;
using WebDriverBiDi.Network;

public class CookieFilterTests
{
    [Fact]
    public void TestCanSerializeCookieFilterWithName()
    {
        CookieFilter properties = new()
        {
            Name = "myCookieName"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myCookieName", name.Value<string>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithValue()
    {
        CookieFilter properties = new()
        {
            Value = BytesValue.FromString("myCookieValue")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.Object, value.Type);
        JObject? bytesValueObject = value.Value<JObject>();
        Assert.NotNull(bytesValueObject);
        Assert.Equal(2, bytesValueObject.Count);

        Assert.True(bytesValueObject.ContainsKey("type"));
        JToken? valueType = bytesValueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(bytesValueObject.ContainsKey("value"));
        JToken? valueValue = bytesValueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("myCookieValue", valueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithDomain()
    {
        CookieFilter properties = new()
        {
            Domain = "myCookieDomain"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myCookieDomain", domain.Value<string>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithPath()
    {
        CookieFilter properties = new()
        {
            Path = "myCookiePath"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("path"));
        JToken? path = serialized["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal("myCookiePath", path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithSize()
    {
        CookieFilter properties = new()
        {
            Size = 3
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("size"));
        JToken? size = serialized["size"];
        Assert.NotNull(size);
        Assert.Equal(JTokenType.Integer, size.Type);
        Assert.Equal(3UL, size.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithHttpOnlyTrue()
    {
        CookieFilter properties = new()
        {
            HttpOnly = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("httpOnly"));
        JToken? httpOnly = serialized["httpOnly"];
        Assert.NotNull(httpOnly);
        Assert.Equal(JTokenType.Boolean, httpOnly.Type);
        Assert.True(httpOnly.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithHttpOnlyFalse()
    {
        CookieFilter properties = new()
        {
            HttpOnly = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("httpOnly"));
        JToken? httpOnly = serialized["httpOnly"];
        Assert.NotNull(httpOnly);
        Assert.Equal(JTokenType.Boolean, httpOnly.Type);
        Assert.False(httpOnly.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithSecureTrue()
    {
        CookieFilter properties = new()
        {
            Secure = true
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("secure"));
        JToken? secure = serialized["secure"];
        Assert.NotNull(secure);
        Assert.Equal(JTokenType.Boolean, secure.Type);
        Assert.True(secure.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithSecureFalse()
    {
        CookieFilter properties = new()
        {
            Secure = false
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("secure"));
        JToken? secure = serialized["secure"];
        Assert.NotNull(secure);
        Assert.Equal(JTokenType.Boolean, secure.Type);
        Assert.False(secure.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithSameSiteValue()
    {
        CookieFilter properties = new()
        {
            SameSite = CookieSameSiteValue.Strict
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("sameSite"));
        JToken? sameSite = serialized["sameSite"];
        Assert.NotNull(sameSite);
        Assert.Equal(JTokenType.String, sameSite.Type);
        Assert.Equal("strict", sameSite.Value<string>());
    }

    [Fact]
    public void TestCanSerializeCookieFilterWithExpirationDate()
    {
        DateTime now = DateTime.UtcNow;
        DateTime expirationDate = now.AddDays(1);
        ulong seconds = Convert.ToUInt64(expirationDate.Subtract(DateTime.UnixEpoch).TotalSeconds);
        CookieFilter properties = new()
        {
            Expires = expirationDate
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("expiry"));
        JToken? expiry = serialized["expiry"];
        Assert.NotNull(expiry);
        Assert.Equal(JTokenType.Integer, expiry.Type);
        Assert.Equal(seconds, expiry.Value<ulong>());
    }

    [Fact]
    public void TestSettingCookieFilterExpirationDate()
    {
        DateTime now = DateTime.UtcNow.AddDays(1);
        DateTime expirationDate = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        CookieFilter properties = new()
        {
            Expires = expirationDate
        };
        Assert.Equal(expirationDate, properties.Expires);
    }

    [Fact]
    public void TestSettingCookieFilterExpirationDateNull()
    {
        DateTime now = DateTime.UtcNow;
        CookieFilter properties = new()
        {
            Expires = null
        };
        Assert.Null(properties.Expires);
    }
}
