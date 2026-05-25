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
    public void TestCanGetResetParameters()
    {
        SetForcedColorsModeThemeOverrideCommandParameters properties = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.Theme);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetForcedColorsModeThemeOverrideCommandParameters firstInstance = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        SetForcedColorsModeThemeOverrideCommandParameters secondInstance = SetForcedColorsModeThemeOverrideCommandParameters.ResetForcedColorsModeThemeOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
