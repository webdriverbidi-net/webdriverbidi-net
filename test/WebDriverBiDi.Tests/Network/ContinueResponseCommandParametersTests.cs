namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ContinueResponseCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        ContinueResponseCommandParameters properties = new("myRequestId");
        Assert.Equal("network.continueResponse", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        ContinueResponseCommandParameters properties = new("myRequestId");
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
    public void TestCanSerializeWithAuthCredentials()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            Credentials = new AuthCredentials("myUserName", "myPassword")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("credentials"));
        JToken? credentialsToken = serialized["credentials"];
        Assert.NotNull(credentialsToken);
        Assert.Equal(JTokenType.Object, credentialsToken.Type);
        JObject? bodyObject = credentialsToken as JObject;
        Assert.NotNull(bodyObject);
        Assert.Equal(3, bodyObject.Count);
        Assert.True(bodyObject.ContainsKey("type"));
        JToken? credType = bodyObject["type"];
        Assert.NotNull(credType);
        Assert.Equal(JTokenType.String, credType.Type);
        Assert.Equal("password", credType.Value<string>());
        Assert.True(bodyObject.ContainsKey("username"));
        JToken? username = bodyObject["username"];
        Assert.NotNull(username);
        Assert.Equal(JTokenType.String, username.Type);
        Assert.Equal("myUserName", username.Value<string>());
        Assert.True(bodyObject.ContainsKey("password"));
        JToken? password = bodyObject["password"];
        Assert.NotNull(password);
        Assert.Equal(JTokenType.String, password.Type);
        Assert.Equal("myPassword", password.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithCookies()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            Cookies = [new SetCookieHeader("cookieName", "cookieValue")]
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
        ContinueResponseCommandParameters properties = new("myRequestId")
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
    public void TestCanSerializeWithReasonPhrase()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            ReasonPhrase = "Not Found"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("reasonPhrase"));
        JToken? reasonPhrase = serialized["reasonPhrase"];
        Assert.NotNull(reasonPhrase);
        Assert.Equal(JTokenType.String, reasonPhrase.Type);
        Assert.Equal("Not Found", reasonPhrase.Value<string>());
    }

    [Fact]
    public void TestCanSerializeWithStatusCode()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            StatusCode = 404
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(2, serialized.Count);
        Assert.True(serialized.ContainsKey("request"));
        JToken? request = serialized["request"];
        Assert.NotNull(request);
        Assert.Equal(JTokenType.String, request.Type);
        Assert.Equal("myRequestId", request.Value<string>());

        Assert.True(serialized.ContainsKey("statusCode"));
        JToken? statusCode = serialized["statusCode"];
        Assert.NotNull(statusCode);
        Assert.Equal(JTokenType.Integer, statusCode.Type);
        Assert.Equal(404UL, statusCode.Value<ulong>());
    }

    [Fact]
    public void TestEmptyHeadersListSerializesAsEmptyArray()
    {
        // A present-but-empty array is not the same as an omitted field, and this is the property's
        // defining behaviour. The remote end steps for network.continueResponse start from an empty
        // header list and append each element, so sending [] clears the intercepted response's
        // headers while omitting the field leaves them as they were. A regression to "omit when
        // empty" would silently turn the first into the second, and every other test here passes
        // either a populated list or none at all.
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            Headers = [],
        };
        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));

        Assert.True(serialized.ContainsKey("headers"));
        JToken? token = serialized["headers"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Array, token.Type);
        JArray? array = token as JArray;
        Assert.NotNull(array);
        Assert.Empty(array);

        // The contrast that gives the empty array its meaning: null omits the field entirely.
        properties.Headers = null;
        JObject withoutList = JObject.Parse(JsonSerializer.Serialize(properties));
        Assert.False(withoutList.ContainsKey("headers"));
    }

    [Fact]
    public void TestEmptyCookiesListSerializesAsEmptyArray()
    {
        // A present-but-empty array is not the same as an omitted field, and this is the property's
        // defining behaviour. The remote end steps for network.continueResponse start from an empty
        // cookie list and append each element, so sending [] clears the intercepted response's
        // cookies while omitting the field leaves them as they were. A regression to "omit when
        // empty" would silently turn the first into the second, and every other test here passes
        // either a populated list or none at all.
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            Cookies = [],
        };
        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));

        Assert.True(serialized.ContainsKey("cookies"));
        JToken? token = serialized["cookies"];
        Assert.NotNull(token);
        Assert.Equal(JTokenType.Array, token.Type);
        JArray? array = token as JArray;
        Assert.NotNull(array);
        Assert.Empty(array);

        // The contrast that gives the empty array its meaning: null omits the field entirely.
        properties.Cookies = null;
        JObject withoutList = JObject.Parse(JsonSerializer.Serialize(properties));
        Assert.False(withoutList.ContainsKey("cookies"));
    }
}
