namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetForcedColorsModeThemeOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setForcedColorsModeThemeOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("theme"));
        JToken? theme = serialized["theme"];
        Assert.NotNull(theme);
        Assert.Equal(JTokenType.Null, theme.Type);
        Assert.Null(theme.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithNoneMode()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.None
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("theme"));
        JToken? theme = serialized["theme"];
        Assert.NotNull(theme);
        Assert.Equal(JTokenType.String, theme.Type);
        Assert.Equal("none", theme.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithLightMode()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.Light
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("theme"));
        JToken? theme = serialized["theme"];
        Assert.NotNull(theme);
        Assert.Equal(JTokenType.String, theme.Type);
        Assert.Equal("light", theme.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDarkMode()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.Dark
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("theme"));
        JToken? theme = serialized["theme"];
        Assert.NotNull(theme);
        Assert.Equal(JTokenType.String, theme.Type);
        Assert.Equal("dark", theme.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithContexts()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.Dark,
            Contexts =
            {
                "context1",
                "context2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("theme"));
        JToken? theme = serialized["theme"];
        Assert.NotNull(theme);
        Assert.Equal(JTokenType.String, theme.Type);
        Assert.Equal("dark", theme.Value<string>());

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
    public void TestCanSerializeParametersWithUserContexts()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = new()
        {
            Theme = ForcedColorsModeTheme.Dark,
            UserContexts =
            {
                "userContext1",
                "userContext2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("theme"));
        JToken? theme = serialized["theme"];
        Assert.NotNull(theme);
        Assert.Equal(JTokenType.String, theme.Type);
        Assert.Equal("dark", theme.Value<string>());

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
        SetForcedColorsModeThemeOverrideCommandParameters properties = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.Theme);
        Assert.Empty(properties.Contexts);
        Assert.Empty(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetForcedColorsModeThemeOverrideCommandParameters firstInstance = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        SetForcedColorsModeThemeOverrideCommandParameters secondInstance = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
