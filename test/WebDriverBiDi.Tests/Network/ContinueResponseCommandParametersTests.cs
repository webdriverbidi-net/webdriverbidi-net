namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ContinueResponseCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        ContinueResponseCommandParameters properties = new("myRequestId");
        Assert.That(properties.MethodName, Is.EqualTo("network.continueResponse"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        ContinueResponseCommandParameters properties = new("myRequestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
        }
    }

    [Test]
    public void TestCanSerializeWithAuthCredentials()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            Credentials = new AuthCredentials("myUserName", "myPassword")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("credentials"));
            Assert.That(serialized["credentials"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject bodyObject = (JObject)serialized["credentials"]!;
            Assert.That(bodyObject, Has.Count.EqualTo(3));
            Assert.That(bodyObject, Contains.Key("type"));
            Assert.That(bodyObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(bodyObject["type"]!.Value<string>(), Is.EqualTo("password"));
            Assert.That(bodyObject, Contains.Key("username"));
            Assert.That(bodyObject["username"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(bodyObject["username"]!.Value<string>(), Is.EqualTo("myUserName"));
            Assert.That(bodyObject, Contains.Key("password"));
            Assert.That(bodyObject["password"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(bodyObject["password"]!.Value<string>(), Is.EqualTo("myPassword"));
        }
    }

    [Test]
    public void TestCanSerializeWithCookies()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            Cookies = [new SetCookieHeader("cookieName", "cookieValue")]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("cookies"));
            Assert.That(serialized["cookies"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray cookieHeaderArray = (JArray)serialized["cookies"]!;
            Assert.That(cookieHeaderArray, Has.Count.EqualTo(1));
            Assert.That(cookieHeaderArray[0].Type, Is.EqualTo(JTokenType.Object));
            JObject cookieHeaderObject = (JObject)cookieHeaderArray[0];
            Assert.That(cookieHeaderObject, Has.Count.EqualTo(2));
            Assert.That(cookieHeaderObject, Contains.Key("name"));
            Assert.That(cookieHeaderObject["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieHeaderObject["name"]!.Value<string>(), Is.EqualTo("cookieName"));
            Assert.That(cookieHeaderObject, Contains.Key("value"));
            Assert.That(cookieHeaderObject["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject cookieValueObject = (JObject)cookieHeaderObject["value"]!;
            Assert.That(cookieValueObject, Has.Count.EqualTo(2));
            Assert.That(cookieValueObject, Contains.Key("type"));
            Assert.That(cookieValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(cookieValueObject, Contains.Key("value"));
            Assert.That(cookieValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(cookieValueObject["value"]!.Value<string>(), Is.EqualTo("cookieValue"));
        }
    }

    [Test]
    public void TestCanSerializeWithHeaders()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            Headers = [new Header("headerName", "headerValue")]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("headers"));
            Assert.That(serialized["headers"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray headerArray = (JArray)serialized["headers"]!;
            Assert.That(headerArray, Has.Count.EqualTo(1));
            Assert.That(headerArray[0].Type, Is.EqualTo(JTokenType.Object));
            JObject headerObject = (JObject)headerArray[0];
            Assert.That(headerObject, Has.Count.EqualTo(2));
            Assert.That(headerObject, Contains.Key("name"));
            Assert.That(headerObject["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(headerObject["name"]!.Value<string>(), Is.EqualTo("headerName"));
            Assert.That(headerObject, Contains.Key("value"));
            Assert.That(headerObject["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject headerValueObject = (JObject)headerObject["value"]!;
            Assert.That(headerValueObject, Has.Count.EqualTo(2));
            Assert.That(headerValueObject, Contains.Key("type"));
            Assert.That(headerValueObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(headerValueObject["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(headerValueObject, Contains.Key("value"));
            Assert.That(headerValueObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(headerValueObject["value"]!.Value<string>(), Is.EqualTo("headerValue"));
        }
    }

    [Test]
    public void TestCanSerializeWithReasonPhrase()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            ReasonPhrase = "Not Found"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("reasonPhrase"));
            Assert.That(serialized["reasonPhrase"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["reasonPhrase"]!.Value<string>(), Is.EqualTo("Not Found"));
        }
    }

    [Test]
    public void TestCanSerializeWithStatusCode()
    {
        ContinueResponseCommandParameters properties = new("myRequestId")
        {
            StatusCode = 404
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("myRequestId"));
            Assert.That(serialized, Contains.Key("statusCode"));
            Assert.That(serialized["statusCode"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["statusCode"]!.Value<ulong>(), Is.EqualTo(404));
        }
    }
}
