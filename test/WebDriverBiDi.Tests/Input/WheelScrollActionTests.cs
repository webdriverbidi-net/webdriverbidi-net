namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

using WebDriverBiDi.Script;

public class WheelScrollActionTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        WheelScrollAction properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(5, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("scroll", type.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(0L, x.Value<long>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(0L, y.Value<long>());

        Assert.True(serialized.ContainsKey("deltaX"));
        JToken? deltaX = serialized["deltaX"];
        Assert.NotNull(deltaX);
        Assert.Equal(JTokenType.Integer, deltaX.Type);
        Assert.Equal(0L, deltaX.Value<long>());

        Assert.True(serialized.ContainsKey("deltaY"));
        JToken? deltaY = serialized["deltaY"];
        Assert.NotNull(deltaY);
        Assert.Equal(JTokenType.Integer, deltaY.Type);
        Assert.Equal(0L, deltaY.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalDuration()
    {
        WheelScrollAction properties = new()
        {
            Duration = TimeSpan.FromMilliseconds(1),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(6, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("scroll", type.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(0L, x.Value<long>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(0L, y.Value<long>());

        Assert.True(serialized.ContainsKey("deltaX"));
        JToken? deltaX = serialized["deltaX"];
        Assert.NotNull(deltaX);
        Assert.Equal(JTokenType.Integer, deltaX.Type);
        Assert.Equal(0L, deltaX.Value<long>());

        Assert.True(serialized.ContainsKey("deltaY"));
        JToken? deltaY = serialized["deltaY"];
        Assert.NotNull(deltaY);
        Assert.Equal(JTokenType.Integer, deltaY.Type);
        Assert.Equal(0L, deltaY.Value<long>());

        Assert.True(serialized.ContainsKey("duration"));
        JToken? duration = serialized["duration"];
        Assert.NotNull(duration);
        Assert.Equal(JTokenType.Integer, duration.Type);
        Assert.Equal(1L, duration.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalViewportOrigin()
    {
        WheelScrollAction properties = new()
        {
            Origin = Origin.Viewport
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(6, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("scroll", type.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(0L, x.Value<long>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(0L, y.Value<long>());

        Assert.True(serialized.ContainsKey("deltaX"));
        JToken? deltaX = serialized["deltaX"];
        Assert.NotNull(deltaX);
        Assert.Equal(JTokenType.Integer, deltaX.Type);
        Assert.Equal(0L, deltaX.Value<long>());

        Assert.True(serialized.ContainsKey("deltaY"));
        JToken? deltaY = serialized["deltaY"];
        Assert.NotNull(deltaY);
        Assert.Equal(JTokenType.Integer, deltaY.Type);
        Assert.Equal(0L, deltaY.Value<long>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("viewport", origin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalPointerOrigin()
    {
        WheelScrollAction properties = new()
        {
            Origin = Origin.Pointer
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(6, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("scroll", type.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(0L, x.Value<long>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(0L, y.Value<long>());

        Assert.True(serialized.ContainsKey("deltaX"));
        JToken? deltaX = serialized["deltaX"];
        Assert.NotNull(deltaX);
        Assert.Equal(JTokenType.Integer, deltaX.Type);
        Assert.Equal(0L, deltaX.Value<long>());

        Assert.True(serialized.ContainsKey("deltaY"));
        JToken? deltaY = serialized["deltaY"];
        Assert.NotNull(deltaY);
        Assert.Equal(JTokenType.Integer, deltaY.Type);
        Assert.Equal(0L, deltaY.Value<long>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("pointer", origin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalElementOrigin()
    {
        string nodeJson = """
                          {
                            "type": "node",
                            "value": {
                              "nodeType": 1,
                              "childNodeCount": 0
                            },
                            "sharedId": "testSharedId"
                          }
                          """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(nodeJson);
        Assert.NotNull(remoteValue);
        Assert.IsType<NodeRemoteValue>(remoteValue);
        SharedReference node = ((NodeRemoteValue)remoteValue).ToSharedReference();
        WheelScrollAction properties = new()
        {
            Origin = Origin.Element(new ElementOrigin(node))
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(6, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("scroll", type.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(0L, x.Value<long>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(0L, y.Value<long>());

        Assert.True(serialized.ContainsKey("deltaX"));
        JToken? deltaX = serialized["deltaX"];
        Assert.NotNull(deltaX);
        Assert.Equal(JTokenType.Integer, deltaX.Type);
        Assert.Equal(0L, deltaX.Value<long>());

        Assert.True(serialized.ContainsKey("deltaY"));
        JToken? deltaY = serialized["deltaY"];
        Assert.NotNull(deltaY);
        Assert.Equal(JTokenType.Integer, deltaY.Type);
        Assert.Equal(0L, deltaY.Value<long>());

        Assert.True(serialized.ContainsKey("origin"));
        JToken? originToken = serialized["origin"];
        Assert.NotNull(originToken);
        Assert.Equal(JTokenType.Object, originToken.Type);

        JObject originObject = (JObject)originToken;
        Assert.Equal(2, originObject.Count);
        Assert.True(originObject.ContainsKey("type"));

        JToken? originType = originObject["type"];
        Assert.NotNull(originType);
        Assert.Equal(JTokenType.String, originType.Type);
        Assert.Equal("element", originType.Value<string>());

        Assert.True(originObject.ContainsKey("element"));
        JToken? elementToken = originObject["element"];
        Assert.NotNull(elementToken);
        Assert.Equal(JTokenType.Object, elementToken.Type);

        JObject elementObject = (JObject)elementToken;
        Assert.Single(elementObject);
        Assert.True(elementObject.ContainsKey("sharedId"));

        JToken? sharedId = elementObject["sharedId"];
        Assert.NotNull(sharedId);
        Assert.Equal(JTokenType.String, sharedId.Type);
        Assert.Equal("testSharedId", sharedId.Value<string>());
    }
}
