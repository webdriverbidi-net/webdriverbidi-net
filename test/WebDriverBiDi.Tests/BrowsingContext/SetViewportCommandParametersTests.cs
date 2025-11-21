namespace WebDriverBiDi.BrowsingContext;

using System.Globalization;
using System.Text.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class SetViewportCommandParametersTests
{
    [Test]
    public void TestCommandName()
    {
        SetViewportCommandParameters properties = new();
        Assert.That(properties.MethodName, Is.EqualTo("browsingContext.setViewport"));
    }

    [Test]
    public void TestCanSerializeParameters()
    {
        SetViewportCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Is.Empty);
    }

    [Test]
    public void TestCanSerializeParametersWithBrowsingContext()
    {
        SetViewportCommandParameters properties = new()
        {
            BrowsingContextId = "myContext",
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("context"));
            Assert.That(serialized["context"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(serialized["context"]!.Value<string>(), Is.EqualTo("myContext"));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithViewport()
    {
        SetViewportCommandParameters properties = new()
        {
            Viewport = new Viewport()
            {
                Height = 1024,
                Width = 1280,
            }
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("viewport"));
            Assert.That(serialized["viewport"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? viewportObject = serialized["viewport"]!.Value<JObject>();
            Assert.That(viewportObject, Is.Not.Null);
            Assert.That(viewportObject, Has.Count.EqualTo(2));
            Assert.That(viewportObject, Contains.Key("width"));
            Assert.That(viewportObject!["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["width"]!.Value<ulong>(), Is.EqualTo(1280));
            Assert.That(viewportObject, Contains.Key("height"));
            Assert.That(viewportObject!["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["height"]!.Value<ulong>(), Is.EqualTo(1024));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithDefaultViewport()
    {
        SetViewportCommandParameters properties = new()
        {
            Viewport = new Viewport()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("viewport"));
            Assert.That(serialized["viewport"]!.Type, Is.EqualTo(JTokenType.Object));
            JObject? viewportObject = serialized["viewport"]!.Value<JObject>();
            Assert.That(viewportObject, Is.Not.Null);
            Assert.That(viewportObject, Has.Count.EqualTo(2));
            Assert.That(viewportObject, Contains.Key("width"));
            Assert.That(viewportObject!["width"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["width"]!.Value<ulong>(), Is.EqualTo(0));
            Assert.That(viewportObject, Contains.Key("height"));
            Assert.That(viewportObject!["height"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(viewportObject!["height"]!.Value<ulong>(), Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithViewportReset()
    {
        SetViewportCommandParameters properties = new()
        {
            Viewport = SetViewportCommandParameters.ResetToDefaultViewport,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("viewport"));
            Assert.That(serialized["viewport"]!.Type, Is.EqualTo(JTokenType.Null));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithDevicePixelRatio()
    {
        SetViewportCommandParameters properties = new()
        {
            DevicePixelRatio = 1.5
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("devicePixelRatio"));
            Assert.That(serialized["devicePixelRatio"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(serialized["devicePixelRatio"]!.Value<double>(), Is.EqualTo(1.5));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithResetDevicePixelRatio()
    {
        SetViewportCommandParameters properties = new()
        {
            DevicePixelRatio = SetViewportCommandParameters.ResetToDefaultDevicePixelRatio,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized, Contains.Key("devicePixelRatio"));
            Assert.That(serialized["devicePixelRatio"]!.Type, Is.EqualTo(JTokenType.Null));
        });
    }

    [Test]
    public void TestCanSerializeParametersWithUserContexts()
    {
        SetViewportCommandParameters properties = new()
        {
            UserContextIds = new List<string>() { "myUserContextId" },
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.That(serialized, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(serialized.ContainsKey("userContexts"));
            Assert.That(serialized["userContexts"]!.Type, Is.EqualTo(JTokenType.Array));
            JArray? userContextsArray = serialized["userContexts"]!.Value<JArray>();
            Assert.That(userContextsArray, Is.Not.Null);
            Assert.That(userContextsArray, Has.Count.EqualTo(1));
            Assert.That(userContextsArray![0].Type, Is.EqualTo(JTokenType.String));
            Assert.That(userContextsArray![0].Value<string>(), Is.EqualTo("myUserContextId"));
        });
    }
}
