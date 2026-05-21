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
}
