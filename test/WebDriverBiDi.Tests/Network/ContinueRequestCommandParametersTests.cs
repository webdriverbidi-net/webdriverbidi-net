namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ContinueRequestCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        ContinueRequestCommandParameters properties = new("myRequestId");
        Assert.Equal("network.continueRequest", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        ContinueRequestCommandParameters properties = new("myRequestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithBody()
    {
        ContinueRequestCommandParameters properties = new("myRequestId")
        {
            Body = BytesValue.FromString("test body")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("body"));
        JToken? bodyToken = serialized["body"];
        Assert.NotNull(bodyToken);
        Assert.Equal(JTokenType.Object, bodyToken.Type);
        JObject? bodyObject = bodyToken as JObject;
        Assert.NotNull(bodyObject);
        Assert.Equal(2, bodyObject.Count);
        Assert.True(bodyObject.ContainsKey("type"));
        JToken? bodyType = bodyObject["type"];
        Assert.NotNull(bodyType);
        Assert.Equal(JTokenType.String, bodyType.Type);
        Assert.Equal("string", bodyType.Value<string>());
        Assert.True(bodyObject.ContainsKey("value"));
        JToken? bodyValue = bodyObject["value"];
        Assert.NotNull(bodyValue);
        Assert.Equal(JTokenType.String, bodyValue.Type);
        Assert.Equal("test body", bodyValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithCookies()
    {
        ContinueRequestCommandParameters properties = new("myRequestId")
        {
            Cookies = [new CookieHeader("cookieName", "cookieValue")]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("cookies"));
        JToken? cookiesToken = serialized["cookies"];
        Assert.NotNull(cookiesToken);
        Assert.Equal(JTokenType.Array, cookiesToken.Type);
        JArray? cookieHeaderArray = cookiesToken as JArray;
        Assert.NotNull(cookieHeaderArray);
        Assert.Single(cookieHeaderArray);
        Assert.Equal(JTokenType.Object, cookieHeaderArray[0].Type);
        JObject? cookieHeaderObject = cookieHeaderArray[0] as JObject;
        Assert.NotNull(cookieHeaderObject);
        Assert.Equal(2, cookieHeaderObject.Count);
        Assert.True(cookieHeaderObject.ContainsKey("name"));
        JToken? cookieName = cookieHeaderObject["name"];
        Assert.NotNull(cookieName);
        Assert.Equal(JTokenType.String, cookieName.Type);
        Assert.Equal("cookieName", cookieName.Value<string>());
        Assert.True(cookieHeaderObject.ContainsKey("value"));
        JToken? cookieValueToken = cookieHeaderObject["value"];
        Assert.NotNull(cookieValueToken);
        Assert.Equal(JTokenType.Object, cookieValueToken.Type);
        JObject? cookieValueObject = cookieValueToken as JObject;
        Assert.NotNull(cookieValueObject);
        Assert.Equal(2, cookieValueObject.Count);
        Assert.True(cookieValueObject.ContainsKey("type"));
        JToken? cookieValueType = cookieValueObject["type"];
        Assert.NotNull(cookieValueType);
        Assert.Equal(JTokenType.String, cookieValueType.Type);
        Assert.Equal("string", cookieValueType.Value<string>());
        Assert.True(cookieValueObject.ContainsKey("value"));
        JToken? cookieValueValue = cookieValueObject["value"];
        Assert.NotNull(cookieValueValue);
        Assert.Equal(JTokenType.String, cookieValueValue.Type);
        Assert.Equal("cookieValue", cookieValueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithHeaders()
    {
        ContinueRequestCommandParameters properties = new("myRequestId")
        {
            Headers = [new Header("headerName", "headerValue")]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("headers"));
        JToken? headersToken = serialized["headers"];
        Assert.NotNull(headersToken);
        Assert.Equal(JTokenType.Array, headersToken.Type);
        JArray? headerArray = headersToken as JArray;
        Assert.NotNull(headerArray);
        Assert.Single(headerArray);
        Assert.Equal(JTokenType.Object, headerArray[0].Type);
        JObject? headerObject = headerArray[0] as JObject;
        Assert.NotNull(headerObject);
        Assert.Equal(2, headerObject.Count);
        Assert.True(headerObject.ContainsKey("name"));
        JToken? headerName = headerObject["name"];
        Assert.NotNull(headerName);
        Assert.Equal(JTokenType.String, headerName.Type);
        Assert.Equal("headerName", headerName.Value<string>());
        Assert.True(headerObject.ContainsKey("value"));
        JToken? headerValueToken = headerObject["value"];
        Assert.NotNull(headerValueToken);
        Assert.Equal(JTokenType.Object, headerValueToken.Type);
        JObject? headerValueObject = headerValueToken as JObject;
        Assert.NotNull(headerValueObject);
        Assert.Equal(2, headerValueObject.Count);
        Assert.True(headerValueObject.ContainsKey("type"));
        JToken? headerValueType = headerValueObject["type"];
        Assert.NotNull(headerValueType);
        Assert.Equal(JTokenType.String, headerValueType.Type);
        Assert.Equal("string", headerValueType.Value<string>());
        Assert.True(headerValueObject.ContainsKey("value"));
        JToken? headerValueValue = headerValueObject["value"];
        Assert.NotNull(headerValueValue);
        Assert.Equal(JTokenType.String, headerValueValue.Type);
        Assert.Equal("headerValue", headerValueValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithMethod()
    {
        ContinueRequestCommandParameters properties = new("myRequestId")
        {
            Method = "get"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("method"));
        JToken? method = serialized["method"];
        Assert.NotNull(method);
        Assert.Equal(JTokenType.String, method.Type);
        Assert.Equal("get", method.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithUrl()
    {
        ContinueRequestCommandParameters properties = new("myRequestId")
        {
            Url = "https://example.com"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("url"));
        JToken? url = serialized["url"];
        Assert.NotNull(url);
        Assert.Equal(JTokenType.String, url.Type);
        Assert.Equal("https://example.com", url.Value<string>());
    }
}
