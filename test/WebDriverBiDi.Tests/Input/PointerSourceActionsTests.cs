namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PointerSourceActionsTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        PointerSourceActions properties = new();
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
        Assert.Equal("pointer", type.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalMousePointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = PointerType.Mouse,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointer", type.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);

        Assert.True(serialized.ContainsKey("parameters"));
        JToken? parametersToken = serialized["parameters"];
        Assert.NotNull(parametersToken);
        Assert.Equal(JTokenType.Object, parametersToken.Type);

        JObject parametersObject = (JObject)parametersToken;
        Assert.Single(parametersObject);
        Assert.True(parametersObject.ContainsKey("pointerType"));

        JToken? pointerType = parametersObject["pointerType"];
        Assert.NotNull(pointerType);
        Assert.Equal(JTokenType.String, pointerType.Type);
        Assert.Equal("mouse", pointerType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalPenPointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = PointerType.Pen,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointer", type.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);

        Assert.True(serialized.ContainsKey("parameters"));
        JToken? parametersToken = serialized["parameters"];
        Assert.NotNull(parametersToken);
        Assert.Equal(JTokenType.Object, parametersToken.Type);

        JObject parametersObject = (JObject)parametersToken;
        Assert.Single(parametersObject);
        Assert.True(parametersObject.ContainsKey("pointerType"));

        JToken? pointerType = parametersObject["pointerType"];
        Assert.NotNull(pointerType);
        Assert.Equal(JTokenType.String, pointerType.Type);
        Assert.Equal("pen", pointerType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalTouchPointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
            {
                PointerType = PointerType.Touch,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointer", type.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);

        Assert.True(serialized.ContainsKey("parameters"));
        JToken? parametersToken = serialized["parameters"];
        Assert.NotNull(parametersToken);
        Assert.Equal(JTokenType.Object, parametersToken.Type);

        JObject parametersObject = (JObject)parametersToken;
        Assert.Single(parametersObject);
        Assert.True(parametersObject.ContainsKey("pointerType"));

        JToken? pointerType = parametersObject["pointerType"];
        Assert.NotNull(pointerType);
        Assert.Equal(JTokenType.String, pointerType.Type);
        Assert.Equal("touch", pointerType.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalUnspecifiedPointerType()
    {
        PointerSourceActions properties = new()
        {
            Parameters = new PointerParameters()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("id"));
        JToken? id = serialized["id"];
        Assert.NotNull(id);
        Assert.Equal(JTokenType.String, id.Type);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointer", type.Value<string>());

        Assert.True(serialized.ContainsKey("actions"));
        JToken? actionsToken = serialized["actions"];
        Assert.NotNull(actionsToken);
        Assert.Equal(JTokenType.Array, actionsToken.Type);
        JArray? actionsArray = actionsToken.Value<JArray>();
        Assert.NotNull(actionsArray);
        Assert.Empty(actionsArray);

        Assert.True(serialized.ContainsKey("parameters"));
        JToken? parametersToken = serialized["parameters"];
        Assert.NotNull(parametersToken);
        Assert.Equal(JTokenType.Object, parametersToken.Type);

        JObject parametersObject = (JObject)parametersToken;
        Assert.Empty(parametersObject);
    }

    [Fact]
    public void TestCanSerializeParametersWithActions()
    {
        PointerSourceActions properties = new();
        properties.Actions.Add(new PointerDownAction(0));
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
        Assert.Equal("pointer", type.Value<string>());

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
        Assert.Equal("pointerDown", actionType.Value<string>());

        Assert.True(action.ContainsKey("button"));
        JToken? button = action["button"];
        Assert.NotNull(button);
        Assert.Equal(JTokenType.Integer, button.Type);
        Assert.Equal(0, button.Value<long>());
    }
}
