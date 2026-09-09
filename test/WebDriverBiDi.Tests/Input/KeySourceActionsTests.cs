namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class KeySourceActionsTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        KeySourceActions properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("key", type.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);
    }

    [Fact]
    public void TestCanSerializeParametersWithActions()
    {
        KeySourceActions properties = new();
        properties.Actions.Add(new KeyDownAction("a"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("key", type.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Single(actionsArray);

        JToken? actionToken = actionsArray[0];
        Assert.NotNull(actionToken);
        Assert.Equal(JTokenType.Object, actionToken.Type);

        JObject? action = actionToken.Value<JObject>();
        Assert.NotNull(action);
        Assert.Equal(2, action.Count);

        Assert.True(action.ContainsKey("type"));
        JToken? actionType = action["type"];
        Assert.NotNull(actionType);
        Assert.Equal(JTokenType.String, actionType.Type);
        Assert.Equal("keyDown", actionType.Value<string>());

        Assert.True(action.ContainsKey("value"));
        JToken? value = action["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.String, value.Type);
        Assert.Equal("a", value.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithExplicitSourceId()
    {
        KeySourceActions properties = new("my key source");
        Assert.Equal("my key source", properties.Id);

        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);
        Assert.Equal("my key source", id.Value<string>());

        Assert.True(serialized.ContainsKey("type"));
        Assert.Equal("key", serialized["type"]!.Value<string>());
    }

    [Fact]
    public void TestDefaultConstructorGeneratesUniqueSourceIds()
    {
        KeySourceActions first = new();
        KeySourceActions second = new();
        Assert.False(string.IsNullOrEmpty(first.Id));
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void TestCanSerializeEveryRegisteredKeySourceActionType()
    {
        // Every type registered with [JsonDerivedType] on IKeySourceAction must serialize; an
        // unregistered runtime type makes System.Text.Json throw NotSupportedException at send time,
        // which no test of the individual action classes would catch.
        KeySourceActions properties = new();
        properties.Actions.Add(new KeyDownAction("a"));
        properties.Actions.Add(new KeyUpAction("a"));
        properties.Actions.Add(new PauseAction());

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));
        Assert.Equal("key", serialized["type"]?.Value<string>());
        Assert.Equal(3, serialized["actions"]?.Value<JArray>()?.Count);
        AssertActionType(serialized, 0, "keyDown");
        AssertActionType(serialized, 1, "keyUp");
        AssertActionType(serialized, 2, "pause");
    }

    /// <summary>
    /// Asserts that the action at the given index in the serialized "actions" array carries the
    /// expected polymorphic "type" discriminator.
    /// </summary>
    private static void AssertActionType(JObject serialized, int index, string expectedType)
    {
        JArray? actionsArray = serialized["actions"]?.Value<JArray>();
        Assert.NotNull(actionsArray);
        JObject? action = actionsArray[index].Value<JObject>();
        Assert.NotNull(action);
        Assert.Equal(expectedType, action["type"]?.Value<string>());
    }
}
