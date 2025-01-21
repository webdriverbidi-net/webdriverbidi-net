namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class AddPreloadScriptCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration");
        Assert.That(properties.MethodName, Is.EqualTo("script.addPreloadScript"));
    }

    [Test]
    public void TestCanSerializeProperties()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("functionDeclaration"));
            Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["functionDeclaration"]!.Value<string>(), Is.EqualTo("myFunctionDeclaration"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithSandbox()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
        {
            Sandbox = "mySandbox"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("functionDeclaration"));
            Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["functionDeclaration"]!.Value<string>(), Is.EqualTo("myFunctionDeclaration"));
            Assert.That(serialized, Contains.Key("sandbox"));
            Assert.That(serialized["sandbox"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["sandbox"]!.Value<string>(), Is.EqualTo("mySandbox"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithArguments()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
        {
            Arguments = new()
            {
                new ChannelValue(new ChannelProperties("myChannel"))
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("functionDeclaration"));
            Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["functionDeclaration"]!.Value<string>(), Is.EqualTo("myFunctionDeclaration"));
            Assert.That(serialized, Contains.Key("arguments"));
            Assert.That(serialized["arguments"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? argsArray = serialized["arguments"]!.Value<JArray>();
            Assert.That(argsArray, Has.Count.EqualTo(1));
            Assert.That(argsArray![0].Type, Is.EqualTo(JTokenType.Object));
            JObject? argObject = argsArray[0].Value<JObject>();
            Assert.That(argObject, Has.Count.EqualTo(2));
            Assert.That(argObject, Contains.Key("type"));
            Assert.That(argObject!["type"]!.Value<string>(), Is.EqualTo("channel"));
            Assert.That(argObject, Contains.Key("value"));
            Assert.That(argObject["value"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? argValue = argObject["value"]!.Value<JObject>();
            Assert.That(argValue!.Value<JObject>(), Has.Count.EqualTo(1));
            Assert.That(argValue, Contains.Key("channel"));
            Assert.That(argValue!["channel"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(argValue!["channel"]!.Value<string>(), Is.EqualTo("myChannel"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
        {
            Contexts = new()
            {
                "context1",
                "context2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("functionDeclaration"));
            Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["functionDeclaration"]!.Value<string>(), Is.EqualTo("myFunctionDeclaration"));
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["contexts"]!.Value<JArray>();
            Assert.That(contextsArray, Has.Count.EqualTo(2));
            Assert.That(contextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[0].Value<string>(), Is.EqualTo("context1"));
            Assert.That(contextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[1].Value<string>(), Is.EqualTo("context2"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithUserContexts()
    {
        AddPreloadScriptCommandParameters properties = new("myFunctionDeclaration")
        {
            UserContexts = new()
            {
                "userContext1",
                "userContext2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("functionDeclaration"));
            Assert.That(serialized["functionDeclaration"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["functionDeclaration"]!.Value<string>(), Is.EqualTo("myFunctionDeclaration"));
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? userContextsArray = serialized["userContexts"]!.Value<JArray>();
            Assert.That(userContextsArray, Has.Count.EqualTo(2));
            Assert.That(userContextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[0].Value<string>(), Is.EqualTo("userContext1"));
            Assert.That(userContextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[1].Value<string>(), Is.EqualTo("userContext2"));
        });
    }
}
