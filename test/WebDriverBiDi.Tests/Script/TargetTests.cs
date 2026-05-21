namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class TargetTests
{
    [Fact]
    public void TestCanDeserializeRealmTarget()
    {
        string json = @"{ ""realm"": ""myRealm"" }";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.NotNull(target);
        Assert.IsType<RealmTarget>(target);
        RealmTarget realmTarget = (RealmTarget)target;
        Assert.Equal("myRealm", realmTarget.RealmId);
    }

    [Fact]
    public void TestCanDeserializeContextTarget()
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
    public void TestCanDeserializeContextTargetWithSandbox()
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
    public void TestDeserializationOfInvalidJsonThrows()
    {
        string json = @"{ ""invalid"": ""invalidValue"" }";
        Assert.Contains("missing required properties including: 'context", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ContextTarget>(json)).Message);
        Assert.Contains("missing required properties including: 'realm", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmTarget>(json)).Message);
        Assert.Contains("JSON for 'Target' must contain one of the following properties: context, realm", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Target>(json)).Message);
        Assert.Contains("JSON for 'Target' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<Target>(@"[ ""invalid target"" ]")).Message);
    }

    [Fact]
    public void TestCanSerializeRealmTarget()
    {
        Target target = new RealmTarget("myRealm");
        string json = JsonSerializer.Serialize(target);
        JObject deserialized = JObject.Parse(json);
        Assert.Single(deserialized);
        Assert.True(deserialized.ContainsKey("realm"));
        JToken? realmValue = deserialized.GetValue("realm");
        Assert.NotNull(realmValue);

        Assert.Equal(JTokenType.String, realmValue.Type);
        Assert.Equal("myRealm", (string?)realmValue);
    }

    [Fact]
    public void TestCanSerializeContextTarget()
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
    public void TestContextTargetCopySemantics()
    {
        Target target = new ContextTarget("myContext");
        Target copy = target with { };
        Assert.IsType<ContextTarget>(copy);
        Assert.Equal(target, copy);
    }

    [Fact]
    public void TestRealmTargetCopySemantics()
    {
        Target target = new RealmTarget("myContext");
        Target copy = target with { };
        Assert.IsType<RealmTarget>(copy);
        Assert.Equal(target, copy);
    }
}
