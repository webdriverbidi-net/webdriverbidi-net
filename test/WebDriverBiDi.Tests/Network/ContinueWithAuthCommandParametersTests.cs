namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class ContinueWithAuthCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        ContinueWithAuthCommandParameters properties = new("requestId");
        Assert.That(properties.MethodName, Is.EqualTo("network.continueWithAuth"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        ContinueWithAuthCommandParameters properties = new("requestId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("requestId"));
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("default"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithCancelAction()
    {
        ContinueWithAuthCommandParameters properties = new("requestId")
        {
            Action = ContinueWithAuthActionType.Cancel
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(2));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("requestId"));
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("cancel"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithProvideCredentialsAction()
    {
        ContinueWithAuthCommandParameters properties = new("requestId")
        {
            Action = ContinueWithAuthActionType.ProvideCredentials
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("requestId"));
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("provideCredentials"));
            Assert.That(serialized, Contains.Key("credentials"));
            Assert.That(serialized["credentials"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject credentialsObject = (JObject)serialized["credentials"]!;
            Assert.That(credentialsObject, Has.Count.EqualTo(3));
            Assert.That(credentialsObject, Contains.Key("type"));
            Assert.That(credentialsObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(credentialsObject["type"]!.Value<string>(), Is.EqualTo("password"));
            Assert.That(credentialsObject, Contains.Key("username"));
            Assert.That(credentialsObject["username"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(credentialsObject["username"]!.Value<string>(), Is.Empty);
            Assert.That(credentialsObject, Contains.Key("password"));
            Assert.That(credentialsObject["password"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(credentialsObject["password"]!.Value<string>(), Is.Empty);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithProvideCredentialsActionAndCredentials()
    {
        ContinueWithAuthCommandParameters properties = new("requestId")
        {
            Action = ContinueWithAuthActionType.ProvideCredentials,
            Credentials = new AuthCredentials("myUserName", "myPassword")
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Has.Count.EqualTo(3));
            Assert.That(serialized, Contains.Key("request"));
            Assert.That(serialized["request"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["request"]!.Value<string>(), Is.EqualTo("requestId"));
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("provideCredentials"));
            Assert.That(serialized, Contains.Key("credentials"));
            Assert.That(serialized["credentials"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject credentialsObject = (JObject)serialized["credentials"]!;
            Assert.That(credentialsObject, Has.Count.EqualTo(3));
            Assert.That(credentialsObject, Contains.Key("type"));
            Assert.That(credentialsObject["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(credentialsObject["type"]!.Value<string>(), Is.EqualTo("password"));
            Assert.That(credentialsObject, Contains.Key("username"));
            Assert.That(credentialsObject["username"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(credentialsObject["username"]!.Value<string>(), Is.EqualTo("myUserName"));
            Assert.That(credentialsObject, Contains.Key("password"));
            Assert.That(credentialsObject["password"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(credentialsObject["password"]!.Value<string>(), Is.EqualTo("myPassword"));
        });
    }
}