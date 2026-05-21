namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetScreenOrientationOverrideCoordinatesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetScreenOrientationOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setScreenOrientationOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetScreenOrientationOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("screenOrientation"));
        JToken? screenOrientation = serialized["screenOrientation"];
        Assert.NotNull(screenOrientation);
        Assert.Equal(JTokenType.Null, screenOrientation.Type);
        Assert.Null(screenOrientation.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithScreenOrientation()
    {
        SetScreenOrientationOverrideCommandParameters properties = new()
        {
            ScreenOrientation = new(ScreenOrientationNatural.Landscape, ScreenOrientationType.PortraitSecondary),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("screenOrientation"));
        JToken? screenOrientationToken = serialized["screenOrientation"];
        Assert.NotNull(screenOrientationToken);
        Assert.Equal(JTokenType.Object, screenOrientationToken.Type);
        JObject? coordinatesObject = screenOrientationToken as JObject;
        Assert.NotNull(coordinatesObject);
        Assert.Equal(2, coordinatesObject.Count);

        Assert.True(coordinatesObject.ContainsKey("natural"));
        JToken? natural = coordinatesObject["natural"];
        Assert.NotNull(natural);
        Assert.Equal(JTokenType.String, natural.Type);
        Assert.Equal("landscape", natural.Value<string>());

        Assert.True(coordinatesObject.ContainsKey("type"));
        JToken? type = coordinatesObject["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("portrait-secondary", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetScreenOrientationOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("screenOrientation"));
        JToken? screenOrientation = serialized["screenOrientation"];
        Assert.NotNull(screenOrientation);
        Assert.Equal(JTokenType.Null, screenOrientation.Type);
        Assert.Null(screenOrientation.Value<JObject?>());

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
        SetScreenOrientationOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("screenOrientation"));
        JToken? screenOrientation = serialized["screenOrientation"];
        Assert.NotNull(screenOrientation);
        Assert.Equal(JTokenType.Null, screenOrientation.Type);
        Assert.Null(screenOrientation.Value<JObject?>());

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
        SetScreenOrientationOverrideCommandParameters properties = SetScreenOrientationOverrideCommandParameters.ResetScreenOrientationOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.ScreenOrientation);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetScreenOrientationOverrideCommandParameters firstInstance = SetScreenOrientationOverrideCommandParameters.ResetScreenOrientationOverride;
        SetScreenOrientationOverrideCommandParameters secondInstance = SetScreenOrientationOverrideCommandParameters.ResetScreenOrientationOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
