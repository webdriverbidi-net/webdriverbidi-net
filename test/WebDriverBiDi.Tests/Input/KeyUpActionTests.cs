namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class KeyUpActionTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        KeyUpAction properties = new("a");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("keyUp", type.Value<string>());

        Assert.True(serialized.ContainsKey("value"));
        JToken? value = serialized["value"];
        Assert.NotNull(value);
        Assert.Equal(JTokenType.String, value.Type);
        Assert.Equal("a", value.Value<string>());
    }

    [Fact]
    public void TestCreatingActionWithEmptyValueThrows()
    {
        Assert.Contains("Action value cannot be null or the empty string", Assert.ThrowsAny<ArgumentException>(() => new KeyUpAction(string.Empty)).Message);
    }
}
