namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetMediaFeaturesOverrideCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetMediaFeaturesOverrideCommandParameters properties = new();
        Assert.Equal("emulation.setMediaFeaturesOverride", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetMediaFeaturesOverrideCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("features"));
        JToken? features = serialized["features"];
        Assert.NotNull(features);
        Assert.Equal(JTokenType.Null, features.Type);
        Assert.Null(features.Value<JArray?>());
    }

    [Fact]
    public void TestCanSerializeParametersWithMediaFeatures()
    {
        SetMediaFeaturesOverrideCommandParameters properties = new()
        {
            Features = new()
            {
                Pointer = PointerMediaFeatureValue.Coarse,
            },
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("features"));
        JToken? featuresToken = serialized["features"];
        Assert.NotNull(featuresToken);
        Assert.Equal(JTokenType.Object, featuresToken.Type);
        JObject? featuresObject = featuresToken as JObject;
        Assert.NotNull(featuresObject);
        Assert.Single(featuresObject);
        Assert.True(featuresObject.ContainsKey("pointer"));

        JToken? featureToken = featuresObject["pointer"];
        Assert.NotNull(featureToken);
        Assert.Equal(JTokenType.String, featureToken.Type);
        Assert.Equal("coarse", featureToken.Value<string>());
    }

    [Fact]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetMediaFeaturesOverrideCommandParameters properties = new()
        {
            Contexts =
            {
                "context1",
                "context2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("features"));
        JToken? features = serialized["features"];
        Assert.NotNull(features);
        Assert.Equal(JTokenType.Null, features.Type);
        Assert.Null(features.Value<JArray?>());

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
        SetMediaFeaturesOverrideCommandParameters properties = new()
        {
            UserContexts =
            {
                "userContext1",
                "userContext2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("features"));
        JToken? features = serialized["features"];
        Assert.NotNull(features);
        Assert.Equal(JTokenType.Null, features.Type);
        Assert.Null(features.Value<JArray?>());

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
        SetMediaFeaturesOverrideCommandParameters properties = SetMediaFeaturesOverrideCommandParameters.ResetMediaFeaturesOverride;
        Assert.NotNull(properties);

        Assert.Null(properties.Features);
        Assert.Empty(properties.Contexts);
        Assert.Empty(properties.UserContexts);
    }

    [Fact]
    public void TestResetParametersPropertyReturnsNewInstance()
    {
        SetMediaFeaturesOverrideCommandParameters firstInstance = SetMediaFeaturesOverrideCommandParameters.ResetMediaFeaturesOverride;
        SetMediaFeaturesOverrideCommandParameters secondInstance = SetMediaFeaturesOverrideCommandParameters.ResetMediaFeaturesOverride;
        Assert.NotSame(secondInstance, firstInstance);
    }
}
