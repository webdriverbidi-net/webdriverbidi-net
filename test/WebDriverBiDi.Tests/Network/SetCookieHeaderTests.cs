namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetCookieHeaderTests
{
    [Fact]
    public void TestCanSerialize()
    {
        SetCookieHeader cookieHeader = new();
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal(string.Empty, name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal(string.Empty, valueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithConstructorValues()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue");
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithPropertySetValues()
    {
        SetCookieHeader cookieHeader = new()
        {
            Name = "cookieName",
            Value = BytesValue.FromString("cookieValue")
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());
    }

    [Fact]
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

        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("base64", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal(base64Value, valueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithDomain()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            Domain = "myDomain"
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("domain"));
        JToken? domain = serialized["domain"];
        Assert.NotNull(domain);
        Assert.Equal(JTokenType.String, domain.Type);
        Assert.Equal("myDomain", domain.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithPath()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            Path = "myPath"
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("path"));
        JToken? path = serialized["path"];
        Assert.NotNull(path);
        Assert.Equal(JTokenType.String, path.Type);
        Assert.Equal("myPath", path.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithHttpOnly()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            HttpOnly = true
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("httpOnly"));
        JToken? httpOnly = serialized["httpOnly"];
        Assert.NotNull(httpOnly);
        Assert.Equal(JTokenType.Boolean, httpOnly.Type);
        Assert.True(httpOnly.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeWithSecure()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            Secure = true
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("secure"));
        JToken? secure = serialized["secure"];
        Assert.NotNull(secure);
        Assert.Equal(JTokenType.Boolean, secure.Type);
        Assert.True(secure.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeWithSameSite()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            SameSite = CookieSameSiteValue.Strict
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("sameSite"));
        JToken? sameSite = serialized["sameSite"];
        Assert.NotNull(sameSite);
        Assert.Equal(JTokenType.String, sameSite.Type);
        Assert.Equal("strict", sameSite.Value<string>());
    }

    [Fact]
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

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("expiry"));
        JToken? expiry = serialized["expiry"];
        Assert.NotNull(expiry);
        Assert.Equal(JTokenType.String, expiry.Type);
        Assert.Equal(expected, expiry.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithMaxAge()
    {
        SetCookieHeader cookieHeader = new("cookieName", "cookieValue")
        {
            MaxAge = 100
        };
        string json = JsonSerializer.Serialize(cookieHeader);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("cookieName", name.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? valueToken = serialized["value"];
        Assert.NotNull(valueToken);
        Assert.Equal(JTokenType.Object, valueToken.Type);
        JObject? valueObject = valueToken.Value<JObject>();
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);

        Assert.True(valueObject.ContainsKey("type"));
        JToken? valueType = valueObject["type"];
        Assert.NotNull(valueType);
        Assert.Equal(JTokenType.String, valueType.Type);
        Assert.Equal("string", valueType.Value<string>());

        Assert.True(valueObject.ContainsKey("value"));
        JToken? valueValue = valueObject["value"];
        Assert.NotNull(valueValue);
        Assert.Equal(JTokenType.String, valueValue.Type);
        Assert.Equal("cookieValue", valueValue.Value<string>());

        Assert.True(serialized.ContainsKey("maxAge"));
        JToken? maxAge = serialized["maxAge"];
        Assert.NotNull(maxAge);
        Assert.Equal(JTokenType.Integer, maxAge.Type);
        Assert.Equal(100UL, maxAge.Value<ulong>());
    }

    [Fact]
    public void TestConstructionWithCookie()
    {
        DateTime now = DateTime.UtcNow.AddSeconds(10);
        DateTime expireTime = new(now.Ticks - (now.Ticks % TimeSpan.TicksPerSecond));
        ulong milliseconds = Convert.ToUInt64(expireTime.Subtract(DateTime.UnixEpoch).TotalSeconds);
        string json = @"{ ""name"": ""cookieName"", ""value"":{ ""type"": ""string"", ""value"": ""cookieValue"" }, ""domain"": ""cookieDomain"", ""path"": ""/cookiePath"", ""secure"": false, ""httpOnly"": false, ""sameSite"": ""lax"", ""size"": 100, ""expiry"": " + milliseconds + @" }";
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
}
