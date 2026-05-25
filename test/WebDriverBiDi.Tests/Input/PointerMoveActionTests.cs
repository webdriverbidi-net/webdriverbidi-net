namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

using WebDriverBiDi.Script;

public class PointerMoveActionTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        PointerMoveAction properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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
    }

    [Fact]
    public void TestCanSerializeParametersWithIntegerPosition()
    {
        PointerMoveAction properties = new()
        {
            X = 2,
            Y = 3,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(2L, x.Value<long>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(3L, y.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithFractionalPosition()
    {
        PointerMoveAction properties = new()
        {
            X = 2.1,
            Y = 3.4,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Float, x.Type);
        Assert.Equal(2.1m, x.Value<decimal>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Float, y.Type);
        Assert.Equal(3.4m, y.Value<decimal>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalDuration()
    {
        PointerMoveAction properties = new()
        {
            Duration = TimeSpan.FromMilliseconds(1),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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

        Assert.True(serialized.ContainsKey("duration"));
        JToken? duration = serialized["duration"];
        Assert.NotNull(duration);
        Assert.Equal(JTokenType.Integer, duration.Type);
        Assert.Equal(1L, duration.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalViewportOrigin()
    {
        PointerMoveAction properties = new()
        {
            Origin = Origin.Viewport
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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

        Assert.True(serialized.ContainsKey("origin"));
        JToken? origin = serialized["origin"];
        Assert.NotNull(origin);
        Assert.Equal(JTokenType.String, origin.Type);
        Assert.Equal("viewport", origin.Value<string>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalPointerOrigin()
    {
        PointerMoveAction properties = new()
        {
            Origin = Origin.Pointer
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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
        PointerMoveAction properties = new()
        {
            Origin = Origin.Element(new ElementOrigin(node))
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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

    [Fact]
    public void TestCanSerializeParametersWithOptionalWidthAndHeight()
    {
        PointerMoveAction properties = new()
        {
            Width = 1,
            Height = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(5, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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

        Assert.True(serialized.ContainsKey("width"));
        JToken? width = serialized["width"];
        Assert.NotNull(width);
        Assert.Equal(JTokenType.Integer, width.Type);
        Assert.Equal(1L, width.Value<long>());

        Assert.True(serialized.ContainsKey("height"));
        JToken? height = serialized["height"];
        Assert.NotNull(height);
        Assert.Equal(JTokenType.Integer, height.Type);
        Assert.Equal(1L, height.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalPressureProperties()
    {
        PointerMoveAction properties = new()
        {
            Pressure = 1,
            TangentialPressure = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(5, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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

        Assert.True(serialized.ContainsKey("pressure"));
        JToken? pressure = serialized["pressure"];
        Assert.NotNull(pressure);
        Assert.Equal(JTokenType.Float, pressure.Type);
        Assert.Equal(1, pressure.Value<double>());

        Assert.True(serialized.ContainsKey("tangentialPressure"));
        JToken? tangentialPressure = serialized["tangentialPressure"];
        Assert.NotNull(tangentialPressure);
        Assert.Equal(JTokenType.Float, tangentialPressure.Type);
        Assert.Equal(1, tangentialPressure.Value<double>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalTwistProperty()
    {
        PointerMoveAction properties = new()
        {
            Twist = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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

        Assert.True(serialized.ContainsKey("twist"));
        JToken? twist = serialized["twist"];
        Assert.NotNull(twist);
        Assert.Equal(JTokenType.Integer, twist.Type);
        Assert.Equal(1UL, twist.Value<ulong>());
    }

    [Fact]
    public void TestSettingTwistPropertyToInvalidValueThrows()
    {
        PointerMoveAction properties = new();
        Assert.Contains("Twist value must be between 0 and 359", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Twist = 360).Message);
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalAngleProperties()
    {
        PointerMoveAction properties = new()
        {
            AzimuthAngle = 1,
            AltitudeAngle = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(5, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerMove", type.Value<string>());

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

        Assert.True(serialized.ContainsKey("altitudeAngle"));
        JToken? altitudeAngle = serialized["altitudeAngle"];
        Assert.NotNull(altitudeAngle);
        Assert.Equal(JTokenType.Float, altitudeAngle.Type);
        Assert.Equal(1, altitudeAngle.Value<double>());

        Assert.True(serialized.ContainsKey("azimuthAngle"));
        JToken? azimuthAngle = serialized["azimuthAngle"];
        Assert.NotNull(azimuthAngle);
        Assert.Equal(JTokenType.Float, azimuthAngle.Type);
        Assert.Equal(1, azimuthAngle.Value<double>());
    }

    [Fact]
    public void TestSettingAnglePropertiesToInvalidValueThrows()
    {
        PointerDownAction properties = new(0);

        Assert.Contains("AltitudeAngle value must be between 0 and 1.5707963267948966 (pi / 2) inclusive", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.AltitudeAngle = -0.01).Message);
        Assert.Contains("AltitudeAngle value must be between 0 and 1.5707963267948966 (pi / 2) inclusive", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.AltitudeAngle = 1.58).Message);
        Assert.Contains("AzimuthAngle value must be between 0 and 6.283185307179586 (2 * pi) inclusive", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.AzimuthAngle = -0.01).Message);
        Assert.Contains("AzimuthAngle value must be between 0 and 6.283185307179586 (2 * pi) inclusive", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.AzimuthAngle = 6.29).Message);
    }
}
