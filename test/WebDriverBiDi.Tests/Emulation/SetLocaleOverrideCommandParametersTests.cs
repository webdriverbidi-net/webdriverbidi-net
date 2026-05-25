namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetLocaleOverrideCoordinatesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetLocaleOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setLocaleOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetLocaleOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("locale"));
        JToken? locale = serialized["locale"];
        Assert.NotNull(locale);
        Assert.Equal(JTokenType.Null, locale.Type);
        Assert.Null(locale.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithLocale()
    {
        SetLocaleOverrideCommandParameters properties = new()
        {
            Locale = "en-US"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("locale"));
        JToken? locale = serialized["locale"];
        Assert.NotNull(locale);
        Assert.Equal(JTokenType.String, locale.Type);
        Assert.Equal("en-US", locale.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetLocaleOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("locale"));
        JToken? locale = serialized["locale"];
        Assert.NotNull(locale);
        Assert.Equal(JTokenType.Null, locale.Type);
        Assert.Null(locale.Value<JObject?>());

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
        SetLocaleOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("locale"));
        JToken? locale = serialized["locale"];
        Assert.NotNull(locale);
        Assert.Equal(JTokenType.Null, locale.Type);
        Assert.Null(locale.Value<JObject?>());

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
        SetLocaleOverrideCommandParameters properties = SetLocaleOverrideCommandParameters.ResetLocaleOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.Locale);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetLocaleOverrideCommandParameters firstInstance = SetLocaleOverrideCommandParameters.ResetLocaleOverride;
        SetLocaleOverrideCommandParameters secondInstance = SetLocaleOverrideCommandParameters.ResetLocaleOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
