namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class DisownCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        DisownCommandParameters properties = new(new RealmTarget("myRealm"));
        Assert.Equal("script.disown", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        DisownCommandParameters properties = new(new RealmTarget("myRealm"));
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("target"));
        JToken? target = serialized["target"];
        Assert.NotNull(target);
        Assert.Equal(JTokenType.Object, target.Type);

        Assert.True(serialized.ContainsKey("handles"));
        JToken? handles = serialized["handles"];
        Assert.NotNull(handles);
        Assert.Equal(JTokenType.Array, handles.Type);
        Assert.Empty(handles);
    }

    [Fact]
    public void TestCanSerializeParametersWithHandles()
    {
        DisownCommandParameters properties = new(new RealmTarget("myRealm"), "myHandle");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("target"));
        JToken? target = serialized["target"];
        Assert.NotNull(target);
        Assert.Equal(JTokenType.Object, target.Type);

        Assert.True(serialized.ContainsKey("handles"));
        JToken? handles = serialized["handles"];
        Assert.NotNull(handles);
        Assert.Equal(JTokenType.Array, handles.Type);
        Assert.Single(handles);
    }
}
