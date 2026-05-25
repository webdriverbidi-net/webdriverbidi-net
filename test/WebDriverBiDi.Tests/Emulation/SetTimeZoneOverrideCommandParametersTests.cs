namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetTimeZoneOverrideCoordinatesCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetTimeZoneOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setTimezoneOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetTimeZoneOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("timezone"));
        JToken? timezone = serialized["timezone"];
        Assert.NotNull(timezone);
        Assert.Equal(JTokenType.Null, timezone.Type);
        Assert.Null(timezone.Value<JObject?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithLocale()
    {
        SetTimeZoneOverrideCommandParameters properties = new()
        {
            TimeZone = "US/Eastern"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("timezone"));
        JToken? timezone = serialized["timezone"];
        Assert.NotNull(timezone);
        Assert.Equal(JTokenType.String, timezone.Type);
        Assert.Equal("US/Eastern", timezone.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetTimeZoneOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("timezone"));
        JToken? timezone = serialized["timezone"];
        Assert.NotNull(timezone);
        Assert.Equal(JTokenType.Null, timezone.Type);
        Assert.Null(timezone.Value<JObject?>());

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
        SetTimeZoneOverrideCommandParameters properties = new()
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

        Assert.True(serialized.ContainsKey("timezone"));
        JToken? timezone = serialized["timezone"];
        Assert.NotNull(timezone);
        Assert.Equal(JTokenType.Null, timezone.Type);
        Assert.Null(timezone.Value<JObject?>());

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
        SetTimeZoneOverrideCommandParameters properties = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.TimeZone);
        Assert.Null(properties.Contexts);
        Assert.Null(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetTimeZoneOverrideCommandParameters firstInstance = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
        SetTimeZoneOverrideCommandParameters secondInstance = SetTimeZoneOverrideCommandParameters.ResetTimeZoneOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
