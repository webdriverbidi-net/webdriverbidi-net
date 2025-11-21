namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class HandleRequestDevicePromptCommandParametersTests
{
   [Test]
    public void TestCommandName()
    {
        HandleRequestDevicePromptCommandParameters acceptParameters = new HandleRequestDevicePromptAcceptCommandParameters("myContext", "myPrompt", "myDevice");
        Assert.That(acceptParameters.MethodName, Is.EqualTo("bluetooth.handleRequestDevicePrompt"));
        HandleRequestDevicePromptCommandParameters cancelParameters = new HandleRequestDevicePromptCancelCommandParameters("myContext", "myPrompt");
        Assert.That(cancelParameters.MethodName, Is.EqualTo("bluetooth.handleRequestDevicePrompt"));
    }

    [Test]
    public void TestCanSerializeAcceptParameters()
    {
        HandleRequestDevicePromptCommandParameters properties = new HandleRequestDevicePromptAcceptCommandParameters("myContext", "myPrompt", "myDevice");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("prompt"));
            Assert.That(serialized["prompt"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["prompt"]!.Value<string>(), Is.EqualTo("myPrompt"));
            Assert.That(serialized, Contains.Key("accept"));
            Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["accept"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("device"));
            Assert.That(serialized["device"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["device"]!.Value<string>(), Is.EqualTo("myDevice"));
        }
    }

    [Test]
    public void TestCanModifyPropertiesInAcceptParameters()
    {
        HandleRequestDevicePromptAcceptCommandParameters properties = new("myContext", "myPrompt", "myDevice");
        properties.BrowsingContextId = "myOtherContext";
        properties.PromptId = "myOtherPrompt";
        properties.DeviceId = "myOtherDevice";
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myOtherContext"));
            Assert.That(serialized, Contains.Key("prompt"));
            Assert.That(serialized["prompt"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["prompt"]!.Value<string>(), Is.EqualTo("myOtherPrompt"));
            Assert.That(serialized, Contains.Key("accept"));
            Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["accept"]!.Value<bool>(), Is.True);
            Assert.That(serialized, Contains.Key("device"));
            Assert.That(serialized["device"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["device"]!.Value<string>(), Is.EqualTo("myOtherDevice"));
        }
    }

    [Test]
    public void TestCanSerializeCancelParameters()
    {
        HandleRequestDevicePromptCommandParameters properties = new HandleRequestDevicePromptCancelCommandParameters("myContext", "myPrompt");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
            Assert.That(serialized, Contains.Key("prompt"));
            Assert.That(serialized["prompt"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["prompt"]!.Value<string>(), Is.EqualTo("myPrompt"));
            Assert.That(serialized, Contains.Key("accept"));
            Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["accept"]!.Value<bool>(), Is.False);
        }
    }

    [Test]
    public void TestCanModifyPropertiesInCancelParameters()
    {
        HandleRequestDevicePromptCancelCommandParameters properties = new("myContext", "myPrompt");
        properties.BrowsingContextId = "myOtherContext";
        properties.PromptId = "myOtherPrompt";
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myOtherContext"));
            Assert.That(serialized, Contains.Key("prompt"));
            Assert.That(serialized["prompt"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["prompt"]!.Value<string>(), Is.EqualTo("myOtherPrompt"));
            Assert.That(serialized, Contains.Key("accept"));
            Assert.That(serialized["accept"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(serialized["accept"]!.Value<bool>(), Is.False);
        }
    }
}
