namespace WebDriverBiDi.DigitalCredentials;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetVirtualWalletBehaviorCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond);
        Assert.That(properties.MethodName, Is.EqualTo("digitalCredentials.setVirtualWalletBehavior"));
    }

    [Test]
    public void TestCanSerializeParametersForRespondAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("respond"));
        }
    }

    [Test]
    public void TestCanSerializeParametersForWaitAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Wait);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("wait"));
        }
    }

    [Test]
    public void TestCanSerializeParametersForDeclineAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Decline);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("decline"));
        }
    }

    [Test]
    public void TestCanSerializeParametersForClearAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Clear);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("clear"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithContext()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond)
        {
            Context = "myContextId"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("respond"));
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContextId"));
        }
    }

    [Test]
    public void TestCanSerializeParametersWithProtocol()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond)
        {
            Protocol = "myProtocol"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("respond"));
            Assert.That(serialized, Contains.Key("protocol"));
            Assert.That(serialized["protocol"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["protocol"]!.Value<string>(), Is.EqualTo("myProtocol"));
        }
    }


    [Test]
    public void TestCanSerializeParametersWithResponse()
    {
        Dictionary<string, object?> response = new()
        {
            { "name", "value" },
        };
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond)
        {
            Response = response,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("action"));
            Assert.That(serialized["action"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["action"]!.Value<string>(), Is.EqualTo("respond"));
            Assert.That(serialized, Contains.Key("response"));
            Assert.That(serialized["response"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? responseObject = serialized["response"] as JObject;
            Assert.That(responseObject, Is.Not.Null);
            Assert.That(responseObject, Has.Count.EqualTo(1));
            Assert.That(responseObject, Contains.Key("name"));
            Assert.That(responseObject!["name"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(responseObject!["name"]!.Value<string>(), Is.EqualTo("value"));
        }
    }
}
