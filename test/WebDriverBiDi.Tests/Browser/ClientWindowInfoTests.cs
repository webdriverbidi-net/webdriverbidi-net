namespace WebDriverBiDi.Browser;

using System.Text.Json;

public class ClientWindowsInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        ClientWindowInfo? result = JsonSerializer.Deserialize<ClientWindowInfo>(json);
        Assert.NotNull(result);

        Assert.Equal("myClientWindow", result.ClientWindowId);
        Assert.Equal(ClientWindowState.Normal, result.State);
        Assert.True(result.IsActive);
        Assert.Equal(100u, result.X);
        Assert.Equal(200u, result.Y);
        Assert.Equal(300u, result.Width);
        Assert.Equal(400u, result.Height);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        ClientWindowInfo? result = JsonSerializer.Deserialize<ClientWindowInfo>(json);
        Assert.NotNull(result);
        ClientWindowInfo? copy = result with { };
        Assert.Equal(result, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingWindowIdThrows()
    {
        string json = """
                      {
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400 
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidWindowIdDataTypeThrows()
    {
        string json = """
                      {
                        "clientWindow": 1234,
                        "active": "invalid",
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestCanDeserializeWithMaximizedState()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "maximized",
                        "x": 0,
                        "y": 0,
                        "width": 1280,
                        "height": 1024
                      }
                      """;
        ClientWindowInfo? result = JsonSerializer.Deserialize<ClientWindowInfo>(json);
        Assert.NotNull(result);

        Assert.Equal("myClientWindow", result.ClientWindowId);
        Assert.Equal(ClientWindowState.Maximized, result.State);
        Assert.True(result.IsActive);
        Assert.Equal(0u, result.X);
        Assert.Equal(0u, result.Y);
        Assert.Equal(1280u, result.Width);
        Assert.Equal(1024u, result.Height);
    }

    [Fact]
    public void TestCanDeserializeWithMinimizedState()
    {
        string json = """
                      { 
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "minimized",
                        "x": 0,
                        "y": 0,
                        "width": 0,
                        "height": 0
                      }
                      """;
        ClientWindowInfo? result = JsonSerializer.Deserialize<ClientWindowInfo>(json);
        Assert.NotNull(result);

        Assert.Equal("myClientWindow", result.ClientWindowId);
        Assert.Equal(ClientWindowState.Minimized, result.State);
        Assert.True(result.IsActive);
        Assert.Equal(0u, result.X);
        Assert.Equal(0u, result.Y);
        Assert.Equal(0u, result.Width);
        Assert.Equal(0u, result.Height);
    }

    [Fact]
    public void TestCanDeserializeWithFullscreenState()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "fullscreen",
                        "x": 0,
                        "y": 0,
                        "width": 1280,
                        "height": 1024
                      }
                      """;
        ClientWindowInfo? result = JsonSerializer.Deserialize<ClientWindowInfo>(json);
        Assert.NotNull(result);

        Assert.Equal("myClientWindow", result.ClientWindowId);
        Assert.Equal(ClientWindowState.Fullscreen, result.State);
        Assert.True(result.IsActive);
        Assert.Equal(0u, result.X);
        Assert.Equal(0u, result.Y);
        Assert.Equal(1280u, result.Width);
        Assert.Equal(1024u, result.Height);
    }

    [Fact]
    public void TestDeserializingWithMissingStateThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidStateValueThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "invalid",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidStateDataTypeThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": 123,
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestCanDeserializeWithInactive()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": false,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        ClientWindowInfo? result = JsonSerializer.Deserialize<ClientWindowInfo>(json);
        Assert.NotNull(result);

        Assert.Equal("myClientWindow", result.ClientWindowId);
        Assert.Equal(ClientWindowState.Normal, result.State);
        Assert.False(result.IsActive);
        Assert.Equal(100u, result.X);
        Assert.Equal(200u, result.Y);
        Assert.Equal(300u, result.Width);
        Assert.Equal(400u, result.Height);
    }

    [Fact]
    public void TestDeserializingWithMissingActiveThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidActiveValueThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": "invalid",
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingXThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidXValueThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": -1,
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidXDataTypeThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": "invalid",
                        "y": 200,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingYThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidYValueThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": -1,
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidYDataTypeThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": "invalid",
                        "width": 300,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingWidthThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true, "state":
                        "normal",
                        "x": 100,
                        "y": 200,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidWidthValueThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": -1,
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidWidthDataTypeThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": "invalid",
                        "height": 400
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingHeightThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidHeightValueThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": -1
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidHeightDataTypeThrows()
    {
        string json = """
                      {
                        "clientWindow": "myClientWindow",
                        "active": true,
                        "state": "normal",
                        "x": 100,
                        "y": 200,
                        "width": 300,
                        "height": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ClientWindowInfo>(json));
    }
}
