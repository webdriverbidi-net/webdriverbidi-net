namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class WheelSourceActionsTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        WheelSourceActions properties = new();
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
        Assert.Equal("wheel", type.Value<string>());

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
        WheelSourceActions properties = new();
        properties.Actions.Add(new WheelScrollAction());
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
        Assert.Equal("wheel", type.Value<string>());

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
        Assert.Equal(5, action.Count);

        Assert.True(action.ContainsKey("type"));
        JToken? actionType = action["type"];
        Assert.NotNull(actionType);
        Assert.Equal(JTokenType.String, actionType.Type);
        Assert.Equal("scroll", actionType.Value<string>());

        Assert.True(action.ContainsKey("x"));
        JToken? x = action["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(0, x.Value<long>());

        Assert.True(action.ContainsKey("y"));
        JToken? y = action["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(0L, y.Value<long>());

        Assert.True(action.ContainsKey("deltaX"));
        JToken? deltaX = action["deltaX"];
        Assert.NotNull(deltaX);
        Assert.Equal(JTokenType.Integer, deltaX.Type);
        Assert.Equal(0L, deltaX.Value<long>());

        Assert.True(action.ContainsKey("deltaY"));
        JToken? deltaY = action["deltaY"];
        Assert.NotNull(deltaY);
        Assert.Equal(JTokenType.Integer, deltaY.Type);
        Assert.Equal(0, deltaY.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithExplicitSourceId()
    {
        WheelSourceActions properties = new("my wheel source");
        Assert.Equal("my wheel source", properties.Id);

        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);
        Assert.Equal("my wheel source", id.Value<string>());

        Assert.True(serialized.ContainsKey("type"));
        Assert.Equal("wheel", serialized["type"]!.Value<string>());
    }

    [Fact]
    public void TestDefaultConstructorGeneratesUniqueSourceIds()
    {
        WheelSourceActions first = new();
        WheelSourceActions second = new();
        Assert.False(string.IsNullOrEmpty(first.Id));
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void TestCanSerializeEveryRegisteredWheelSourceActionType()
    {
        // See the note in KeySourceActionsTests: an unregistered derived type is a serialization
        // failure at send time, not a compile or unit-test failure.
        WheelSourceActions properties = new();
        properties.Actions.Add(new WheelScrollAction());
        properties.Actions.Add(new PauseAction());

        JObject serialized = JObject.Parse(JsonSerializer.Serialize(properties));
        Assert.Equal("wheel", serialized["type"]?.Value<string>());
        Assert.Equal(2, serialized["actions"]?.Value<JArray>()?.Count);
        AssertActionType(serialized, 0, "scroll");
        AssertActionType(serialized, 1, "pause");
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
