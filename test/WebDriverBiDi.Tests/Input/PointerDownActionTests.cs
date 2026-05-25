namespace WebDriverBiDi.Input;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class PointerDownActionTests
{
    [Fact]
    public void TestCanSerializeParameters()
    {
        PointerDownAction properties = new(0);
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerDown", type.Value<string>());

        Assert.True(serialized.ContainsKey("button"));
        JToken? button = serialized["button"];
        Assert.NotNull(button);
        Assert.Equal(JTokenType.Integer, button.Type);
        Assert.Equal(0L, button.Value<long>());
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalWidthAndHeight()
    {
        PointerDownAction properties = new(0)
        {
            Width = 1,
            Height = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerDown", type.Value<string>());

        Assert.True(serialized.ContainsKey("button"));
        JToken? button = serialized["button"];
        Assert.NotNull(button);
        Assert.Equal(JTokenType.Integer, button.Type);
        Assert.Equal(0L, button.Value<long>());

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
        PointerDownAction properties = new(0)
        {
            Pressure = 1,
            TangentialPressure = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerDown", type.Value<string>());

        Assert.True(serialized.ContainsKey("button"));
        JToken? button = serialized["button"];
        Assert.NotNull(button);
        Assert.Equal(JTokenType.Integer, button.Type);
        Assert.Equal(0L, button.Value<long>());

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
        PointerDownAction properties = new(0)
        {
            Twist = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerDown", type.Value<string>());

        Assert.True(serialized.ContainsKey("button"));
        JToken? button = serialized["button"];
        Assert.NotNull(button);
        Assert.Equal(JTokenType.Integer, button.Type);
        Assert.Equal(0L, button.Value<long>());

        Assert.True(serialized.ContainsKey("twist"));
        JToken? twist = serialized["twist"];
        Assert.NotNull(twist);
        Assert.Equal(JTokenType.Integer, twist.Type);
        Assert.Equal(1UL, twist.Value<ulong>());
    }

    [Fact]
    public void TestSettingTwistPropertyToInvalidValueThrows()
    {
        PointerDownAction properties = new(0);
        Assert.Contains("Twist value must be between 0 and 359", Assert.ThrowsAny<ArgumentOutOfRangeException>(() => properties.Twist = 360).Message);
    }

    [Fact]
    public void TestCanSerializeParametersWithOptionalAngleProperties()
    {
        PointerDownAction properties = new(0)
        {
            AzimuthAngle = 1,
            AltitudeAngle = 1,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("type"));
        JToken? type = serialized["type"];
        Assert.NotNull(type);
        Assert.Equal(JTokenType.String, type.Type);
        Assert.Equal("pointerDown", type.Value<string>());

        Assert.True(serialized.ContainsKey("button"));
        JToken? button = serialized["button"];
        Assert.NotNull(button);
        Assert.Equal(JTokenType.Integer, button.Type);
        Assert.Equal(0L, button.Value<long>());

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
