namespace WebDriverBiDi.Emulation;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetGeolocationOverrideCoordinatesCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("emulation.setGeolocationOverride"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("coordinates"));
            Assert.That(serialized["coordinates"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["coordinates"]!.Value<JObject?>, Is.Null);
        });
    }

    [Test]
    public void TestCanSerializeParametersWithCoordinates()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new()
        {
            Coordinates = new(123.45, -67.89),
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("coordinates"));
            Assert.That(serialized["coordinates"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? coordinatesObject = serialized["coordinates"] as JObject;
            Assert.That(coordinatesObject, Has.Count.EqualTo(2));
            Assert.That(coordinatesObject, Contains.Key("longitude"));
            Assert.That(coordinatesObject!["longitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(coordinatesObject!["longitude"]!.Value<double>, Is.EqualTo(123.45));
            Assert.That(coordinatesObject, Contains.Key("latitude"));
            Assert.That(coordinatesObject!["latitude"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(coordinatesObject!["latitude"]!.Value<double>, Is.EqualTo(-67.89));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithContexts()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new()
        {
            Contexts = new()
            {
                "context1",
                "context2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("coordinates"));
            Assert.That(serialized["coordinates"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["coordinates"]!.Value<JObject?>(), Is.Null);
            Assert.That(serialized, Contains.Key("contexts"));
            Assert.That(serialized["contexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? contextsArray = serialized["contexts"]!.Value<JArray>();
            Assert.That(contextsArray, Has.Count.EqualTo(2));
            Assert.That(contextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[0].Value<string>(), Is.EqualTo("context1"));
            Assert.That(contextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(contextsArray[1].Value<string>(), Is.EqualTo("context2"));
        });
    }

    [Test]
    public void TestCanSerializePropertiesWithUserContexts()
    {
        SetGeolocationOverrideCoordinatesCommandParameters properties = new()
        {
            UserContexts = new()
            {
                "userContext1",
                "userContext2",
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("coordinates"));
            Assert.That(serialized["coordinates"]!.Type, Is.EqualTo(JTokenType.Null));
            Assert.That(serialized["coordinates"]!.Value<JObject?>(), Is.Null);
            Assert.That(serialized, Contains.Key("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? userContextsArray = serialized["userContexts"]!.Value<JArray>();
            Assert.That(userContextsArray, Has.Count.EqualTo(2));
            Assert.That(userContextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[0].Value<string>(), Is.EqualTo("userContext1"));
            Assert.That(userContextsArray[1].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray[1].Value<string>(), Is.EqualTo("userContext2"));
        });
    }
}
