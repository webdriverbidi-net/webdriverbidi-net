namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class RealmTargetTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = @"{ ""realm"": ""myRealm"" }";
        Target? target = JsonSerializer.Deserialize<Target>(json);
        Assert.NotNull(target);
        Assert.IsType<RealmTarget>(target);
        RealmTarget realmTarget = (RealmTarget)target;
        Assert.Equal("myRealm", realmTarget.RealmId);
    }

    [Fact]
    public void TestDeserializingWithInvalidJsonThrows()
    {
        string json = @"{ ""invalid"": ""invalidValue"" }";
        Assert.Contains("missing required properties including: 'realm", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RealmTarget>(json)).Message);
    }

    [Fact]
    public void TestCanSerialize()
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
    public void TestCopySemantics()
    {
        Target target = new RealmTarget("myContext");
        Target copy = target with { };
        Assert.IsType<RealmTarget>(copy);
        Assert.Equal(target, copy);
    }
}
