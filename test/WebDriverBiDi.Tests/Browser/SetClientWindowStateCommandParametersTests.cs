namespace WebDriverBiDi.Browser;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class SetClientWindowStateCommandParametersTests
{
    [Fact]
    public void TestCommandName()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId");
        Assert.Equal("browser.setClientWindowState", properties.MethodName);
    }

    [Fact]
    public void TestCanSerializeParameters()
    {
        SetClientWindowStateCommandParameters properties = new("myWindowId");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("clientWindow"));
        JToken? clientWindow = serialized["clientWindow"];
        Assert.NotNull(clientWindow);
        Assert.Equal(JTokenType.String, clientWindow.Type);
        Assert.Equal("myWindowId", clientWindow.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("normal", state.Value<string>());
    }

    [Fact]
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
        Assert.Equal(6, serialized.Count);

        Assert.True(serialized.ContainsKey("clientWindow"));
        JToken? clientWindow = serialized["clientWindow"];
        Assert.NotNull(clientWindow);
        Assert.Equal(JTokenType.String, clientWindow.Type);
        Assert.Equal("myWindowId", clientWindow.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("normal", state.Value<string>());

        Assert.True(serialized.ContainsKey("x"));
        JToken? x = serialized["x"];
        Assert.NotNull(x);
        Assert.Equal(JTokenType.Integer, x.Type);
        Assert.Equal(100UL, x.Value<ulong>());

        Assert.True(serialized.ContainsKey("y"));
        JToken? y = serialized["y"];
        Assert.NotNull(y);
        Assert.Equal(JTokenType.Integer, y.Type);
        Assert.Equal(200UL, y.Value<ulong>());

        Assert.True(serialized.ContainsKey("width"));
        JToken? width = serialized["width"];
        Assert.NotNull(width);
        Assert.Equal(JTokenType.Integer, width.Type);
        Assert.Equal(300UL, width.Value<ulong>());

        Assert.True(serialized.ContainsKey("height"));
        JToken? height = serialized["height"];
        Assert.NotNull(height);
        Assert.Equal(JTokenType.Integer, height.Type);
        Assert.Equal(400UL, height.Value<ulong>());
    }

    [Fact]
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
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("clientWindow"));
        JToken? clientWindow = serialized["clientWindow"];
        Assert.NotNull(clientWindow);
        Assert.Equal(JTokenType.String, clientWindow.Type);
        Assert.Equal("myWindowId", clientWindow.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("maximized", state.Value<string>());
    }

    [Fact]
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
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("clientWindow"));
        JToken? clientWindow = serialized["clientWindow"];
        Assert.NotNull(clientWindow);
        Assert.Equal(JTokenType.String, clientWindow.Type);
        Assert.Equal("myWindowId", clientWindow.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("minimized", state.Value<string>());
    }

    [Fact]
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
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("clientWindow"));
        JToken? clientWindow = serialized["clientWindow"];
        Assert.NotNull(clientWindow);
        Assert.Equal(JTokenType.String, clientWindow.Type);
        Assert.Equal("myWindowId", clientWindow.Value<string>());

        Assert.True(serialized.ContainsKey("state"));
        JToken? state = serialized["state"];
        Assert.NotNull(state);
        Assert.Equal(JTokenType.String, state.Type);
        Assert.Equal("fullscreen", state.Value<string>());
    }
}
