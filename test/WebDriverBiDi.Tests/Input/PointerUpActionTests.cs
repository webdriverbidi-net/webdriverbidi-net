namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PointerUpActionTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        PointerUpAction properties = new(0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerUp", type.Value<string>());

        Assert.True(serialized.ContainsKey("button"));
        JToken? button = serialized["button"];
        Assert.NotNull(button);
        Assert.Equal(JTokenType.Integer, button.Type);
        Assert.Equal(0L, button.Value<long>());
    }
}
