namespace WebDriverBiDi.DigitalCredentials;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetVirtualWalletBehaviorCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond);
        Assert.Equal("digitalCredentials.setVirtualWalletBehavior", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParametersForRespondAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("respond", action.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersForWaitAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Wait);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("wait", action.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersForDeclineAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Decline);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("decline", action.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersForClearAction()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Clear);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("clear", action.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithContext()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond)
        {
            Context = "myContextId"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("respond", action.Value<string>());

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithProtocol()
    {
        SetVirtualWalletBehaviorCommandParameters properties = new(VirtualWalletAction.Respond)
        {
            Protocol = "myProtocol"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("respond", action.Value<string>());

        Assert.True(serialized.ContainsKey("protocol"));
        JToken? protocol = serialized["protocol"];
        Assert.NotNull(protocol);
        Assert.Equal(JTokenType.String, protocol.Type);
        Assert.Equal("myProtocol", protocol.Value<string>());
    }

    [Fact]
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
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("action"));
        JToken? action = serialized["action"];
        Assert.NotNull(action);
        Assert.Equal(JTokenType.String, action.Type);
        Assert.Equal("respond", action.Value<string>());

        Assert.True(serialized.ContainsKey("response"));
        JToken? responseToken = serialized["response"];
        Assert.NotNull(responseToken);
        Assert.Equal(JTokenType.Object, responseToken.Type);
        JObject? responseObject = responseToken as JObject;
        Assert.NotNull(responseObject);
        Assert.Single(responseObject);
        Assert.True(responseObject.ContainsKey("name"));
        JToken? responseName = responseObject["name"];
        Assert.NotNull(responseName);
        Assert.Equal(JTokenType.String, responseName.Type);
        Assert.Equal("value", responseName.Value<string>());
    }
}
