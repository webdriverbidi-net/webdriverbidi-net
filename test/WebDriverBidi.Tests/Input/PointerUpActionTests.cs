namespace WebDriverBidi.Input;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class PointerUpActionTests
{
    [Test]
    public void TestCanSerializeParameters()
    {
        PointerUpAction properties = new();
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalWidthAndHeight()
    {
        PointerUpAction properties = new()
        {
            Width = 1,
            Height = 1,
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("width"));
            Assert.That(serialized["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["width"]!.Value<long>(), Is.EqualTo(1));
            Assert.That(serialized, Contains.Key("height"));
            Assert.That(serialized["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["height"]!.Value<long>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalPressureProperties()
    {
        PointerUpAction properties = new()
        {
            Pressure = 1,
            TangentialPressure = 1,
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("pressure"));
            Assert.That(serialized["pressure"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["pressure"]!.Value<double>(), Is.EqualTo(1));
            Assert.That(serialized, Contains.Key("tangentialPressure"));
            Assert.That(serialized["tangentialPressure"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["tangentialPressure"]!.Value<double>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalTwistProperty()
    {
        PointerUpAction properties = new()
        {
            Twist = 1,
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("twist"));
            Assert.That(serialized["twist"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["twist"]!.Value<ulong>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestSettingTwistPropertyToInvalidValueThrows()
    {
        PointerUpAction properties = new();
        Assert.That(() => properties.Twist = 360, Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("Twist value must be between 0 and 359"));
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalAngleProperties()
    {
        PointerUpAction properties = new()
        {
            AzimuthAngle = 1,
            AltitudeAngle = 1,
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("altitudeAngle"));
            Assert.That(serialized["altitudeAngle"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["altitudeAngle"]!.Value<double>(), Is.EqualTo(1));
            Assert.That(serialized, Contains.Key("azimuthAngle"));
            Assert.That(serialized["azimuthAngle"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["azimuthAngle"]!.Value<double>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithOptionalTiltProperties()
    {
        PointerUpAction properties = new()
        {
            TiltX = 1,
            TiltY = 1,
        };
        string json = JsonConvert.SerializeObject(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("type"));
            Assert.That(serialized["type"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["type"]!.Value<string>(), Is.EqualTo("pointerUp"));
            Assert.That(serialized, Contains.Key("button"));
            Assert.That(serialized["button"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["button"]!.Value<long>(), Is.EqualTo(0));
            Assert.That(serialized, Contains.Key("tiltX"));
            Assert.That(serialized["tiltX"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["tiltX"]!.Value<long>(), Is.EqualTo(1));
            Assert.That(serialized, Contains.Key("tiltY"));
            Assert.That(serialized["tiltY"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["tiltY"]!.Value<long>(), Is.EqualTo(1));
        });
    }

    [Test]
    public void TestSettingTiltPropertiesToInvalidValueThrows()
    {
        PointerUpAction properties = new();
        Assert.Multiple(() =>
        {
            Assert.That(() => properties.TiltX = -91, Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("TiltX value must be between -90 and 90 inclusive"));
            Assert.That(() => properties.TiltX = 91, Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("TiltX value must be between -90 and 90 inclusive"));
            Assert.That(() => properties.TiltY = -91, Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("TiltY value must be between -90 and 90 inclusive"));
            Assert.That(() => properties.TiltY = 91, Throws.InstanceOf<WebDriverBidiException>().With.Message.EqualTo("TiltY value must be between -90 and 90 inclusive"));
        });
    }
}