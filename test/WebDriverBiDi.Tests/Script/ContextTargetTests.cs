namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ContextTargetTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = @"{ ""context"": ""myContext"" }";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.NotNull(target);
        Assert.IsType<ContextTarget>(target);
        ContextTarget contextTarget = (ContextTarget)target;

        Assert.Equal("myContext", contextTarget.BrowsingContextId);
        Assert.Null(contextTarget.Sandbox);
    }

    [Fact]
    public void TestCanDeserializeWithSandbox()
    {
        string json = @"{ ""context"": ""myContext"", ""sandbox"": ""mySandbox"" }";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.NotNull(target);
        Assert.IsType<ContextTarget>(target);
        ContextTarget contextTarget = (ContextTarget)target;

        Assert.Equal("myContext", contextTarget.BrowsingContextId);
        Assert.Equal("mySandbox", contextTarget.Sandbox);
    }

    [Fact]
    public void TestDeserializingWithInvalidJsonThrows()
    {
        string json = @"{ ""invalid"": ""invalidValue"" }";
        Assert.Contains("missing required properties including: 'context", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ContextTarget>(json)).Message);
    }

    [Fact]
    public void TestCanSerialize()
    {
        Target target = new ContextTarget("myContext")
        {
            Sandbox = "mySandbox"
        };
        string json = JsonSerializer.Serialize(target);
        JObject deserialized = JObject.Parse(json);
        Assert.Equal(2, deserialized.Count);
        Assert.True(deserialized.ContainsKey("context"));
        JToken? contextValue = deserialized.GetValue("context");
        Assert.NotNull(contextValue);
        JToken? sandboxValue = deserialized.GetValue("sandbox");
        Assert.NotNull(sandboxValue);

        Assert.Equal(JTokenType.String, contextValue.Type);
        Assert.Equal("myContext", (string?)contextValue);
        Assert.Equal(JTokenType.String, sandboxValue.Type);
        Assert.Equal("mySandbox", (string?)sandboxValue);
    }

    [Fact]
    public void TestCopySemantics()
    {
        Target target = new ContextTarget("myContext");
        Target copy = target with { };
        Assert.IsType<ContextTarget>(copy);
        Assert.Equal(target, copy);
    }
}
