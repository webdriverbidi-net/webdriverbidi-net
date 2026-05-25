namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetScreenSettingsOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetScreenSettingsOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setScreenSettingsOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetScreenSettingsOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("screenArea"));
        JToken? screenArea = serialized["screenArea"];
        Assert.NotNull(screenArea);
        Assert.Equal(JTokenType.Null, screenArea.Type);
        Assert.Null(screenArea.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDefaultScreenArea()
    {
        SetScreenSettingsOverrideCommandParameters properties = new()
        {
            ScreenArea = new(),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("screenArea"));
        JToken? screenAreaToken = serialized["screenArea"];
        Assert.NotNull(screenAreaToken);
        Assert.Equal(JTokenType.Object, screenAreaToken.Type);
        JObject? screenAreaObject = screenAreaToken as JObject;
        Assert.NotNull(screenAreaObject);
        Assert.Equal(2, screenAreaObject.Count);

        Assert.True(screenAreaObject.ContainsKey("width"));
        JToken? width = screenAreaObject["width"];
        Assert.NotNull(width);
        Assert.Equal(JTokenType.Integer, width.Type);
        Assert.Equal(0UL, width.Value<ulong>());

        Assert.True(screenAreaObject.ContainsKey("height"));
        JToken? height = screenAreaObject["height"];
        Assert.NotNull(height);
        Assert.Equal(JTokenType.Integer, height.Type);
        Assert.Equal(0UL, height.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersWithScreenArea()
    {
        SetScreenSettingsOverrideCommandParameters properties = new()
        {
            ScreenArea = new()
            {
                Width = 1280,
                Height = 1024,
            },
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("screenArea"));
        JToken? screenAreaToken = serialized["screenArea"];
        Assert.NotNull(screenAreaToken);
        Assert.Equal(JTokenType.Object, screenAreaToken.Type);
        JObject? screenAreaObject = screenAreaToken as JObject;
        Assert.NotNull(screenAreaObject);
        Assert.Equal(2, screenAreaObject.Count);

        Assert.True(screenAreaObject.ContainsKey("width"));
        JToken? width = screenAreaObject["width"];
        Assert.NotNull(width);
        Assert.Equal(JTokenType.Integer, width.Type);
        Assert.Equal(1280UL, width.Value<ulong>());

        Assert.True(screenAreaObject.ContainsKey("height"));
        JToken? height = screenAreaObject["height"];
        Assert.NotNull(height);
        Assert.Equal(JTokenType.Integer, height.Type);
        Assert.Equal(1024UL, height.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScreenSettingsOverrideCommandParameters properties = new()
        {
            Contexts =
            [
                "context1",
                "context2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("screenArea"));
        JToken? screenArea = serialized["screenArea"];
        Assert.NotNull(screenArea);
        Assert.Equal(JTokenType.Null, screenArea.Type);
        Assert.Null(screenArea.Value<JObject?>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        Assert.Equal(JTokenType.Array, contextsToken.Type);
        JArray? contextsArray = contextsToken.Value<JArray>();
        Assert.NotNull(contextsArray);
        Assert.Equal(2, contextsArray.Count);
        Assert.Equal(JTokenType.String, contextsArray[0].Type);
        Assert.Equal("context1", contextsArray[0].Value<string>());
        Assert.Equal(JTokenType.String, contextsArray[1].Type);
        Assert.Equal("context2", contextsArray[1].Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithUserContexts()
    {
        SetScreenSettingsOverrideCommandParameters properties = new()
        {
            UserContexts =
            [
                "userContext1",
                "userContext2",
            ]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("screenArea"));
        JToken? screenArea = serialized["screenArea"];
        Assert.NotNull(screenArea);
        Assert.Equal(JTokenType.Null, screenArea.Type);
        Assert.Null(screenArea.Value<JObject?>());

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        Assert.Equal(JTokenType.Array, userContextsToken.Type);
        JArray? userContextsArray = userContextsToken.Value<JArray>();
        Assert.NotNull(userContextsArray);
        Assert.Equal(2, userContextsArray.Count);
        Assert.Equal(JTokenType.String, userContextsArray[0].Type);
        Assert.Equal("userContext1", userContextsArray[0].Value<string>());
        Assert.Equal(JTokenType.String, userContextsArray[1].Type);
        Assert.Equal("userContext2", userContextsArray[1].Value<string>());
    }

    [Fact]
    public void TestCanGetResetParameters()
    {
        SetScreenSettingsOverrideCommandParameters properties = SetScreenSettingsOverrideCommandParameters.ResetScreenSettingsOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.ScreenArea);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetScreenSettingsOverrideCommandParameters firstInstance = SetScreenSettingsOverrideCommandParameters.ResetScreenSettingsOverride;
        SetScreenSettingsOverrideCommandParameters secondInstance = SetScreenSettingsOverrideCommandParameters.ResetScreenSettingsOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
