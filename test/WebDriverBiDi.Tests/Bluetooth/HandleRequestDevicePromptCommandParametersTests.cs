namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class HandleRequestDevicePromptCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        HandleRequestDevicePromptCommandParameters acceptParameters = new HandleRequestDevicePromptAcceptCommandParameters("myContext", "myPrompt", "myDevice");
        Assert.Equal("bluetooth.handleRequestDevicePrompt", acceptParameters.MethodName);
        HandleRequestDevicePromptCommandParameters cancelParameters = new HandleRequestDevicePromptCancelCommandParameters("myContext", "myPrompt");
        Assert.Equal("bluetooth.handleRequestDevicePrompt", cancelParameters.MethodName);
    }

    [Fact]
    public void TestCanSerializeAcceptParameters()
    {
        HandleRequestDevicePromptCommandParameters properties = new HandleRequestDevicePromptAcceptCommandParameters("myContext", "myPrompt", "myDevice");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("prompt"));
        JToken? prompt = serialized["prompt"];
        Assert.NotNull(prompt);
        Assert.Equal(JTokenType.String, prompt.Type);
        Assert.Equal("myPrompt", prompt.Value<string>());

        Assert.True(serialized.ContainsKey("accept"));
        JToken? accept = serialized["accept"];
        Assert.NotNull(accept);
        Assert.Equal(JTokenType.Boolean, accept.Type);
        Assert.True(accept.Value<bool>());

        Assert.True(serialized.ContainsKey("device"));
        JToken? device = serialized["device"];
        Assert.NotNull(device);
        Assert.Equal(JTokenType.String, device.Type);
        Assert.Equal("myDevice", device.Value<string>());
    }

    [Fact]
    public void TestCanModifyPropertiesInAcceptParameters()
    {
        HandleRequestDevicePromptAcceptCommandParameters properties = new("myContext", "myPrompt", "myDevice")
        {
            BrowsingContextId = "myOtherContext",
            PromptId = "myOtherPrompt",
            DeviceId = "myOtherDevice"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myOtherContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("prompt"));
        JToken? prompt = serialized["prompt"];
        Assert.NotNull(prompt);
        Assert.Equal(JTokenType.String, prompt.Type);
        Assert.Equal("myOtherPrompt", prompt.Value<string>());

        Assert.True(serialized.ContainsKey("accept"));
        JToken? accept = serialized["accept"];
        Assert.NotNull(accept);
        Assert.Equal(JTokenType.Boolean, accept.Type);
        Assert.True(accept.Value<bool>());

        Assert.True(serialized.ContainsKey("device"));
        JToken? device = serialized["device"];
        Assert.NotNull(device);
        Assert.Equal(JTokenType.String, device.Type);
        Assert.Equal("myOtherDevice", device.Value<string>());
    }

    [Fact]
    public void TestCanSerializeCancelParameters()
    {
        HandleRequestDevicePromptCommandParameters properties = new HandleRequestDevicePromptCancelCommandParameters("myContext", "myPrompt");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("prompt"));
        JToken? prompt = serialized["prompt"];
        Assert.NotNull(prompt);
        Assert.Equal(JTokenType.String, prompt.Type);
        Assert.Equal("myPrompt", prompt.Value<string>());

        Assert.True(serialized.ContainsKey("accept"));
        JToken? accept = serialized["accept"];
        Assert.NotNull(accept);
        Assert.Equal(JTokenType.Boolean, accept.Type);
        Assert.False(accept.Value<bool>());
    }

    [Fact]
    public void TestCanModifyPropertiesInCancelParameters()
    {
        HandleRequestDevicePromptCancelCommandParameters properties = new("myContext", "myPrompt")
        {
            BrowsingContextId = "myOtherContext",
            PromptId = "myOtherPrompt"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myOtherContext", context.Value<string>());

        Assert.True(serialized.ContainsKey("prompt"));
        JToken? prompt = serialized["prompt"];
        Assert.NotNull(prompt);
        Assert.Equal(JTokenType.String, prompt.Type);
        Assert.Equal("myOtherPrompt", prompt.Value<string>());

        Assert.True(serialized.ContainsKey("accept"));
        JToken? accept = serialized["accept"];
        Assert.NotNull(accept);
        Assert.Equal(JTokenType.Boolean, accept.Type);
        Assert.False(accept.Value<bool>());
    }

    [Fact]
    public void TestAcceptParametersDeviceIdGetterReturnsValue()
    {
        HandleRequestDevicePromptAcceptCommandParameters parameters = new("myContext", "myPrompt", "myDevice");
        Assert.Equal("myDevice", parameters.DeviceId);
    }

    [Fact]
    public void TestAcceptParametersDeviceIdSetterSetsValue()
    {
        HandleRequestDevicePromptAcceptCommandParameters parameters = new("myContext", "myPrompt", "myDevice");
        parameters.DeviceId = "myNewDevice";
        Assert.Equal("myNewDevice", parameters.DeviceId);
    }

    [Fact]
    public void TestAcceptParametersThrowsWhenGettingDeviceIdWhenNull()
    {
        HandleRequestDevicePromptAcceptCommandParameters parameters = new("myContext", "myPrompt", null!);
        Assert.Equal("DeviceId cannot be null", Assert.ThrowsAny<InvalidOperationException>(() => _ = parameters.DeviceId).Message);
    }

    [Fact]
    public void TestAcceptParametersThrowsWhenSettingDeviceIdToNull()
    {
        HandleRequestDevicePromptAcceptCommandParameters parameters = new("myContext", "myPrompt", "myDevice");
        Assert.Throws<ArgumentNullException>(() => parameters.DeviceId = null!);
    }
}
