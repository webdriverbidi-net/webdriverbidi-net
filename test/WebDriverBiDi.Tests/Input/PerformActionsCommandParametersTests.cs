namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PerformActionsCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        PerformActionsCommandParameters properties = new("myContextId");
        Assert.Equal("input.performActions", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        PerformActionsCommandParameters properties = new("myContextId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContextId", context.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);
    }

    [Fact]
    public void TestCanSerializeEveryRegisteredSourceActionsType()
    {
        // Every type registered with [JsonDerivedType] on the abstract SourceActions must serialize.
        // A missing registration is not a compile error; it makes System.Text.Json throw
        // NotSupportedException when input.performActions is actually sent.
        PerformActionsCommandParameters properties = new("myContextId");
        properties.Actions.Add(new NoneSourceActions());
        properties.Actions.Add(new KeySourceActions());
        properties.Actions.Add(new PointerSourceActions());
        properties.Actions.Add(new WheelSourceActions());

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));
        JArray? actionsArray = serialized["actions"]?.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Equal(4, actionsArray.Count);
        Assert.Equal("none", actionsArray[0].Value<JObject>()?["type"]?.Value<string>());
        Assert.Equal("key", actionsArray[1].Value<JObject>()?["type"]?.Value<string>());
        Assert.Equal("pointer", actionsArray[2].Value<JObject>()?["type"]?.Value<string>());
        Assert.Equal("wheel", actionsArray[3].Value<JObject>()?["type"]?.Value<string>());
    }

    [Fact]
    public void TestCanSerializeMixedSourceActionsWithPauseInEachSource()
    {
        // The canonical "tick" idiom: a pause in every source so the devices stay in step. It is the
        // shape most likely to be written by a caller and the one a missing PauseAction registration
        // on any of the three interfaces would break.
        PerformActionsCommandParameters properties = new("myContextId");
        KeySourceActions keyActions = new();
        keyActions.Actions.Add(new KeyDownAction("a"));
        keyActions.Actions.Add(new PauseAction());
        PointerSourceActions pointerActions = new();
        pointerActions.Actions.Add(new PauseAction());
        pointerActions.Actions.Add(new PointerMoveAction());
        WheelSourceActions wheelActions = new();
        wheelActions.Actions.Add(new PauseAction());
        properties.Actions.Add(keyActions);
        properties.Actions.Add(pointerActions);
        properties.Actions.Add(wheelActions);

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));
        JArray? actionsArray = serialized["actions"]?.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Equal(3, actionsArray.Count);
        Assert.Equal("pause", actionsArray[0]["actions"]?[1]?["type"]?.Value<string>());
        Assert.Equal("pause", actionsArray[1]["actions"]?[0]?["type"]?.Value<string>());
        Assert.Equal("pause", actionsArray[2]["actions"]?[0]?["type"]?.Value<string>());
    }
}
