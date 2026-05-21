namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class GeolocationCoordinatesTests
{
    [Fact]
    public void TestCanSerializeCoordinates()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89);
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("longitude"));
        JToken? longitude = serialized["longitude"];
        Assert.NotNull(longitude);
        Assert.Equal(JTokenType.Float, longitude.Type);
        Assert.Equal(123.45, longitude.Value<double>());

        Assert.True(serialized.ContainsKey("latitude"));
        JToken? latitude = serialized["latitude"];
        Assert.NotNull(latitude);
        Assert.Equal(JTokenType.Float, latitude.Type);
        Assert.Equal(-67.89, latitude.Value<double>());
    }

    [Fact]
    public void TestCanSerializeCoordinatesWithAccuracy()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Accuracy = 0.95,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(3, serialized.Count);

        Assert.True(serialized.ContainsKey("longitude"));
        JToken? longitude = serialized["longitude"];
        Assert.NotNull(longitude);
        Assert.Equal(JTokenType.Float, longitude.Type);
        Assert.Equal(123.45, longitude.Value<double>());

        Assert.True(serialized.ContainsKey("latitude"));
        JToken? latitude = serialized["latitude"];
        Assert.NotNull(latitude);
        Assert.Equal(JTokenType.Float, latitude.Type);
        Assert.Equal(-67.89, latitude.Value<double>());

        Assert.True(serialized.ContainsKey("accuracy"));
        JToken? accuracy = serialized["accuracy"];
        Assert.NotNull(accuracy);
        Assert.Equal(JTokenType.Float, accuracy.Type);
        Assert.Equal(0.95, accuracy.Value<double>());
    }

    [Fact]
    public void TestCanSerializeCoordinatesWithAltitudeAndAltitudeAccuracy()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Altitude = 1.0,
            AltitudeAccuracy = 2.0,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("longitude"));
        JToken? longitude = serialized["longitude"];
        Assert.NotNull(longitude);
        Assert.Equal(JTokenType.Float, longitude.Type);
        Assert.Equal(123.45, longitude.Value<double>());

        Assert.True(serialized.ContainsKey("latitude"));
        JToken? latitude = serialized["latitude"];
        Assert.NotNull(latitude);
        Assert.Equal(JTokenType.Float, latitude.Type);
        Assert.Equal(-67.89, latitude.Value<double>());

        Assert.True(serialized.ContainsKey("altitude"));
        JToken? altitude = serialized["altitude"];
        Assert.NotNull(altitude);
        Assert.Equal(JTokenType.Integer, altitude.Type);
        Assert.Equal(1.0, altitude.Value<double>());

        Assert.True(serialized.ContainsKey("altitudeAccuracy"));
        JToken? altitudeAccuracy = serialized["altitudeAccuracy"];
        Assert.NotNull(altitudeAccuracy);
        Assert.Equal(JTokenType.Integer, altitudeAccuracy.Type);
        Assert.Equal(2.0, altitudeAccuracy.Value<double>());
    }

    [Fact]
    public void TestCanSerializeCoordinatesWithSpeedAndHeading()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Speed = 10.0,
            Heading = 137.5,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("longitude"));
        JToken? longitude = serialized["longitude"];
        Assert.NotNull(longitude);
        Assert.Equal(JTokenType.Float, longitude.Type);
        Assert.Equal(123.45, longitude.Value<double>());

        Assert.True(serialized.ContainsKey("latitude"));
        JToken? latitude = serialized["latitude"];
        Assert.NotNull(latitude);
        Assert.Equal(JTokenType.Float, latitude.Type);
        Assert.Equal(-67.89, latitude.Value<double>());

        Assert.True(serialized.ContainsKey("speed"));
        JToken? speed = serialized["speed"];
        Assert.NotNull(speed);
        Assert.Equal(JTokenType.Integer, speed.Type);
        Assert.Equal(10.0, speed.Value<double>());

        Assert.True(serialized.ContainsKey("heading"));
        JToken? heading = serialized["heading"];
        Assert.NotNull(heading);
        Assert.Equal(JTokenType.Float, heading.Type);
        Assert.Equal(137.5, heading.Value<double>());
    }

    [Fact]
    public void TestCanSerializeCoordinatesWithStationarySpeedAndHeading()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Speed = 0,
            Heading = double.NaN,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        Assert.True(serialized.ContainsKey("longitude"));
        JToken? longitude = serialized["longitude"];
        Assert.NotNull(longitude);
        Assert.Equal(JTokenType.Float, longitude.Type);
        Assert.Equal(123.45, longitude.Value<double>());

        Assert.True(serialized.ContainsKey("latitude"));
        JToken? latitude = serialized["latitude"];
        Assert.NotNull(latitude);
        Assert.Equal(JTokenType.Float, latitude.Type);
        Assert.Equal(-67.89, latitude.Value<double>());

        Assert.True(serialized.ContainsKey("speed"));
        JToken? speed = serialized["speed"];
        Assert.NotNull(speed);
        Assert.Equal(JTokenType.Integer, speed.Type);
        Assert.Equal(0.0, speed.Value<double>());

        Assert.True(serialized.ContainsKey("heading"));
        JToken? heading = serialized["heading"];
        Assert.NotNull(heading);
        Assert.Equal(JTokenType.String, heading.Type);
        Assert.Equal("NaN", heading.Value<string>());
    }
}
