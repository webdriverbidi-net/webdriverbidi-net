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
}
