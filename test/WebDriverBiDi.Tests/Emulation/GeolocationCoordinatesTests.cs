namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class GeolocationCoordinatesTests
{
    [Test]
    public void TestCanSerializeCoordinates()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89);
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("longitude"));
            Assert.That(serialized["longitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["longitude"]!.Value<double>(), Is.EqualTo(123.45));
            Assert.That(serialized, Contains.Key("latitude"));
            Assert.That(serialized["latitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["latitude"]!.Value<double>(), Is.EqualTo(-67.89));
        });
    }

    [Test]
    public void TestCanSerializeCoordinatesWithAccuracy()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Accuracy = 0.95,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("longitude"));
            Assert.That(serialized["longitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["longitude"]!.Value<double>(), Is.EqualTo(123.45));
            Assert.That(serialized, Contains.Key("latitude"));
            Assert.That(serialized["latitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["latitude"]!.Value<double>(), Is.EqualTo(-67.89));
            Assert.That(serialized, Contains.Key("accuracy"));
            Assert.That(serialized["accuracy"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["accuracy"]!.Value<double>(), Is.EqualTo(0.95));
        });
    }

    [Test]
    public void TestCanSerializeCoordinatesWithAltitudeAndAltitudeAccuracy()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Altitude = 1.0,
            AltitudeAccuracy = 2.0,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("longitude"));
            Assert.That(serialized["longitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["longitude"]!.Value<double>(), Is.EqualTo(123.45));
            Assert.That(serialized, Contains.Key("latitude"));
            Assert.That(serialized["latitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["latitude"]!.Value<double>(), Is.EqualTo(-67.89));
            Assert.That(serialized, Contains.Key("altitude"));
            Assert.That(serialized["altitude"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["altitude"]!.Value<double>(), Is.EqualTo(1.0));
            Assert.That(serialized, Contains.Key("altitudeAccuracy"));
            Assert.That(serialized["altitudeAccuracy"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["altitudeAccuracy"]!.Value<double>(), Is.EqualTo(2.0));
        });
    }

    [Test]
    public void TestCanSerializeCoordinatesWithSpeedAndHeading()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Speed = 10.0,
            Heading = 137.5,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("longitude"));
            Assert.That(serialized["longitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["longitude"]!.Value<double>(), Is.EqualTo(123.45));
            Assert.That(serialized, Contains.Key("latitude"));
            Assert.That(serialized["latitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["latitude"]!.Value<double>(), Is.EqualTo(-67.89));
            Assert.That(serialized, Contains.Key("speed"));
            Assert.That(serialized["speed"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["speed"]!.Value<double>(), Is.EqualTo(10.0));
            Assert.That(serialized, Contains.Key("heading"));
            Assert.That(serialized["heading"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["heading"]!.Value<double>(), Is.EqualTo(137.5));
        });
    }

    [Test]
    public void TestCanSerializeCoordinatesWithStationarySpeedAndHeading()
    {
        GeolocationCoordinates coordinates = new(123.45, -67.89)
        {
            Speed = 0,
            Heading = double.NaN,
        };
        string json = JsonSerializer.Serialize(coordinates);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("longitude"));
            Assert.That(serialized["longitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["longitude"]!.Value<double>(), Is.EqualTo(123.45));
            Assert.That(serialized, Contains.Key("latitude"));
            Assert.That(serialized["latitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["latitude"]!.Value<double>(), Is.EqualTo(-67.89));
            Assert.That(serialized, Contains.Key("speed"));
            Assert.That(serialized["speed"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["speed"]!.Value<double>(), Is.EqualTo(0.0));
            Assert.That(serialized, Contains.Key("heading"));
            Assert.That(serialized["heading"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["heading"]!.Value<string>(), Is.EqualTo("NaN"));
        });
    }
}
