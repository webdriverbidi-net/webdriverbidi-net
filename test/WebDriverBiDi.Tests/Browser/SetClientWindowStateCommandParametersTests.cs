namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetClientWindowStateCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId");
        Assert.That(properties.MethodName, Is.EqualTo("browser.setClientWindowState"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("clientWindow"));
            Assert.That(serialized["clientWindow"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["clientWindow"]!.Value<string>(), Is.EqualTo("myWindowId"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("normal"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithSpecifiedSizeAndLocation()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId")
        {
            X = 100,
            Y = 200,
            Width = 300,
            Height = 400,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(6));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("clientWindow"));
            Assert.That(serialized["clientWindow"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["clientWindow"]!.Value<string>(), Is.EqualTo("myWindowId"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("normal"));
            Assert.That(serialized, Contains.Key("x"));
            Assert.That(serialized["x"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["x"]!.Value<ulong>(), Is.EqualTo(100));
            Assert.That(serialized, Contains.Key("y"));
            Assert.That(serialized["y"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["y"]!.Value<ulong>(), Is.EqualTo(200));
            Assert.That(serialized, Contains.Key("width"));
            Assert.That(serialized["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["width"]!.Value<ulong>(), Is.EqualTo(300));
            Assert.That(serialized, Contains.Key("height"));
            Assert.That(serialized["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(serialized["height"]!.Value<ulong>(), Is.EqualTo(400));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithMaximizedState()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId")
        {
            State = ClientWindowState.Maximized,
            X = 100,
            Y = 200,
            Width = 300,
            Height = 400,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("clientWindow"));
            Assert.That(serialized["clientWindow"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["clientWindow"]!.Value<string>(), Is.EqualTo("myWindowId"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("maximized"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithMinimizedState()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId")
        {
            State = ClientWindowState.Minimized,
            X = 100,
            Y = 200,
            Width = 300,
            Height = 400,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("clientWindow"));
            Assert.That(serialized["clientWindow"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["clientWindow"]!.Value<string>(), Is.EqualTo("myWindowId"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("minimized"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithFullscreenState()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId")
        {
            State = ClientWindowState.Fullscreen,
            X = 100,
            Y = 200,
            Width = 300,
            Height = 400,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("clientWindow"));
            Assert.That(serialized["clientWindow"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["clientWindow"]!.Value<string>(), Is.EqualTo("myWindowId"));
            Assert.That(serialized, Contains.Key("state"));
            Assert.That(serialized["state"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["state"]!.Value<string>(), Is.EqualTo("fullscreen"));
        });
    }
}