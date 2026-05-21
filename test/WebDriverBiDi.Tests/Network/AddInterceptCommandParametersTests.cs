namespace WebDriverBiDi.Network;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class AddInterceptCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent);
        Assert.Equal("network.addIntercept", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("phases"));
        JToken? phasesToken = serialized["phases"];
        Assert.NotNull(phasesToken);
        Assert.Equal(JTokenType.Array, phasesToken.Type);
        JArray? interceptArray = phasesToken as JArray;
        Assert.NotNull(interceptArray);
        Assert.Single(interceptArray);
        Assert.Equal(JTokenType.String, interceptArray[0].Type);
        Assert.Equal("beforeRequestSent", interceptArray[0].Value<string>());
    }

    [Fact]
    public void TestCanConstructWithMultiplePhases()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent, InterceptPhase.ResponseStarted);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("phases"));
        JToken? phasesToken = serialized["phases"];
        Assert.NotNull(phasesToken);
        Assert.Equal(JTokenType.Array, phasesToken.Type);
        JArray? interceptArray = phasesToken as JArray;
        Assert.NotNull(interceptArray);
        Assert.Equal(2, interceptArray.Count);
        Assert.Equal(JTokenType.String, interceptArray[0].Type);
        Assert.Equal("beforeRequestSent", interceptArray[0].Value<string>());
        Assert.Equal(JTokenType.String, interceptArray[1].Type);
        Assert.Equal("responseStarted", interceptArray[1].Value<string>());
    }

    [Fact]
    public void TestDuplicatePhaseArgumentOnlySerializesOnce()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent, InterceptPhase.BeforeRequestSent);
        properties.Phases.Add(InterceptPhase.AuthRequired);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Single(serialized);
        Assert.True(serialized.ContainsKey("phases"));
        JToken? phasesToken = serialized["phases"];
        Assert.NotNull(phasesToken);
        Assert.Equal(JTokenType.Array, phasesToken.Type);
        JArray? phases = phasesToken as JArray;
        Assert.NotNull(phases);
        Assert.Equal(2, phases.Count);
        Assert.Equal(JTokenType.String, phases[0].Type);
        Assert.Equal("beforeRequestSent", phases[0].Value<string>());
        Assert.Equal(JTokenType.String, phases[1].Type);
        Assert.Equal("authRequired", phases[1].Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithAllProperties()
    {
        AddInterceptCommandParameters properties = new(InterceptPhase.BeforeRequestSent)
        {
            BrowsingContextIds = ["myContext"],
            UrlPatterns = [new UrlPatternString("https://example.com/*")]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);

        Assert.Equal(3, serialized.Count);
        Assert.True(serialized.ContainsKey("phases"));
        JToken? phasesToken = serialized["phases"];
        Assert.NotNull(phasesToken);
        Assert.Equal(JTokenType.Array, phasesToken.Type);
        JArray? phases = phasesToken as JArray;
        Assert.NotNull(phases);
        Assert.Single(phases);
        Assert.Equal(JTokenType.String, phases[0].Type);
        Assert.Equal("beforeRequestSent", phases[0].Value<string>());

        Assert.True(serialized.ContainsKey("contexts"));
        JToken? contextsToken = serialized["contexts"];
        Assert.NotNull(contextsToken);
        JArray? contexts = contextsToken as JArray;
        Assert.NotNull(contexts);
        Assert.Single(contexts);
        Assert.Equal(JTokenType.String, contexts[0].Type);
        Assert.Equal("myContext", contexts[0].Value<string>());

        Assert.True(serialized.ContainsKey("urlPatterns"));
        JToken? urlPatternsToken = serialized["urlPatterns"];
        Assert.NotNull(urlPatternsToken);
        Assert.Equal(JTokenType.Array, urlPatternsToken.Type);
        JArray? patterns = urlPatternsToken as JArray;
        Assert.NotNull(patterns);
        Assert.Single(patterns);
        Assert.Equal(JTokenType.Object, patterns[0].Type);
        JObject? urlPatternObject = patterns[0] as JObject;
        Assert.NotNull(urlPatternObject);
        Assert.Equal(2, urlPatternObject.Count);
        Assert.True(urlPatternObject.ContainsKey("type"));
        JToken? patternType = urlPatternObject["type"];
        Assert.NotNull(patternType);
        Assert.Equal(JTokenType.String, patternType.Type);
        Assert.Equal("string", patternType.Value<string>());
        Assert.True(urlPatternObject.ContainsKey("pattern"));
        JToken? pattern = urlPatternObject["pattern"];
        Assert.NotNull(pattern);
        Assert.Equal(JTokenType.String, pattern.Type);
        Assert.Equal("https://example.com/*", pattern.Value<string>());
    }

    [Fact]
    public void TestOmittingPhaseInConstructorThrows()
    {
        Assert.StartsWith("You must supply at least one phase for the intercept", Assert.ThrowsAny<ArgumentException>(() => new AddInterceptCommandParameters()).Message);
    }
}
