namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetViewportCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetViewportCommandParameters properties = new();
        Assert.Equal("browsingContext.setViewport", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetViewportCommandParameters properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeParametersWithBrowsingContext()
    {
        SetViewportCommandParameters properties = new()
        {
            BrowsingContextId = "myContext",
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("context"));
        JToken? context = serialized["context"];
        Assert.NotNull(context);
        Assert.Equal(JTokenType.String, context.Type);
        Assert.Equal("myContext", context.Value<string>());
    }

    [Fact]
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
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("viewport"));
        JToken? viewportToken = serialized["viewport"];
        Assert.NotNull(viewportToken);
        Assert.Equal(JTokenType.Object, viewportToken.Type);
        JObject? viewportObject = viewportToken.Value<JObject>();
        Assert.NotNull(viewportObject);
        Assert.Equal(2, viewportObject.Count);

        Assert.True(viewportObject.ContainsKey("width"));
        JToken? viewportWidth = viewportObject["width"];
        Assert.NotNull(viewportWidth);
        Assert.Equal(JTokenType.Integer, viewportWidth.Type);
        Assert.Equal(1280UL, viewportWidth.Value<ulong>());

        Assert.True(viewportObject.ContainsKey("height"));
        JToken? viewportHeight = viewportObject["height"];
        Assert.NotNull(viewportHeight);
        Assert.Equal(JTokenType.Integer, viewportHeight.Type);
        Assert.Equal(1024UL, viewportHeight.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersWithDefaultViewport()
    {
        SetViewportCommandParameters properties = new()
        {
            Viewport = new Viewport()
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("viewport"));
        JToken? viewportToken = serialized["viewport"];
        Assert.NotNull(viewportToken);
        Assert.Equal(JTokenType.Object, viewportToken.Type);
        JObject? viewportObject = viewportToken.Value<JObject>();
        Assert.NotNull(viewportObject);
        Assert.Equal(2, viewportObject.Count);

        Assert.True(viewportObject.ContainsKey("width"));
        JToken? viewportWidth = viewportObject["width"];
        Assert.NotNull(viewportWidth);
        Assert.Equal(JTokenType.Integer, viewportWidth.Type);
        Assert.Equal(0U, viewportWidth.Value<ulong>());

        Assert.True(viewportObject.ContainsKey("height"));
        JToken? viewportHeight = viewportObject["height"];
        Assert.NotNull(viewportHeight);
        Assert.Equal(JTokenType.Integer, viewportHeight.Type);
        Assert.Equal(0U, viewportHeight.Value<ulong>());
    }

    [Fact]
    public void TestCanSerializeParametersWithViewportReset()
    {
        SetViewportCommandParameters properties = new()
        {
            Viewport = SetViewportCommandParameters.ResetToDefaultViewport,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("viewport"));
        JToken? viewport = serialized["viewport"];
        Assert.NotNull(viewport);
        Assert.Equal(JTokenType.Null, viewport.Type);
    }

    [Fact]
    public void TestCanSerializeParametersWithDevicePixelRatio()
    {
        SetViewportCommandParameters properties = new()
        {
            DevicePixelRatio = 1.5
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("devicePixelRatio"));
        JToken? devicePixelRatio = serialized["devicePixelRatio"];
        Assert.NotNull(devicePixelRatio);
        Assert.Equal(JTokenType.Float, devicePixelRatio.Type);
        Assert.Equal(1.5, devicePixelRatio.Value<double>());
    }

    [Fact]
    public void TestCanSerializeParametersWithResetDevicePixelRatio()
    {
        SetViewportCommandParameters properties = new()
        {
            DevicePixelRatio = SetViewportCommandParameters.ResetToDefaultDevicePixelRatio,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("devicePixelRatio"));
        JToken? devicePixelRatio = serialized["devicePixelRatio"];
        Assert.NotNull(devicePixelRatio);
        Assert.Equal(JTokenType.Null, devicePixelRatio.Type);
    }

    [Fact]
    public void TestCanSerializeParametersWithUserContexts()
    {
        SetViewportCommandParameters properties = new()
        {
            UserContexts = ["myUserContextId"],
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Single(serialized);

        Assert.True(serialized.ContainsKey("userContexts"));
        JToken? userContextsToken = serialized["userContexts"];
        Assert.NotNull(userContextsToken);
        Assert.Equal(JTokenType.Array, userContextsToken.Type);
        JArray? userContextsArray = userContextsToken.Value<JArray>();
        Assert.NotNull(userContextsArray);
        Assert.Single(userContextsArray);
        Assert.Equal(JTokenType.String, userContextsArray[0].Type);
        Assert.Equal("myUserContextId", userContextsArray[0].Value<string>());
    }
}
