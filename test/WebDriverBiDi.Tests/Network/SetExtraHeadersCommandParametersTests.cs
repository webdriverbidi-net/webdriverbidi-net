namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetExtraHeadersCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetExtraHeadersCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("network.setExtraHeaders"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetExtraHeadersCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(1));
            Assert.That(serialized, Contains.Key("headers"));
            Assert.That(serialized["headers"]!.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(serialized["headers"], Is.Empty);
        }
    }

    [Test]
    public void TestCanSerializeWithContexts()
    {
        SetExtraHeadersCommandParameters properties = new()
        {
            Contexts = ["myContext"]
        };
        properties.Headers.Add("X-Extra-Header: headerValue");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("headers"));
            Assert.That(serialized["headers"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray headersArray = (JArray)serialized["headers"]!;
            Assert.That(headersArray, Has.Count.EqualTo(1));
            Assert.That(headersArray[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(headersArray[0].Value<string>(), Is.EqualTo("X-Extra-Header: headerValue"));
            JArray contextsObject = (JArray)serialized["contexts"]!;
            Assert.That(contextsObject, Has.Count.EqualTo(1));
            Assert.That(contextsObject[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsObject[0].Value<string>(), Is.EqualTo("myContext"));
        }
    }

    [Test]
    public void TestCanSerializeWithUserContexts()
    {
        SetExtraHeadersCommandParameters properties = new()
        {
            UserContexts = ["myUserContext"]
        };
        properties.Headers.Add("X-Extra-Header: headerValue");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("headers"));
            Assert.That(serialized["headers"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray headersArray = (JArray)serialized["headers"]!;
            Assert.That(headersArray, Has.Count.EqualTo(1));
            Assert.That(headersArray[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(headersArray[0].Value<string>(), Is.EqualTo("X-Extra-Header: headerValue"));
            JArray contextsObject = (JArray)serialized["userContexts"]!;
            Assert.That(contextsObject, Has.Count.EqualTo(1));
            Assert.That(contextsObject[0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsObject[0].Value<string>(), Is.EqualTo("myUserContext"));
        }
    }
}
