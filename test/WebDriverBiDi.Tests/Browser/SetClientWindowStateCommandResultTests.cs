namespace WebDriverBiDi.Browser;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class SetClientWindowStateCommandResultTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        SetClientWindowStateCommandResult? result = JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.IsActive, Is.True);
            Assert.That(result.X, Is.EqualTo(100));
            Assert.That(result.Y, Is.EqualTo(200));
            Assert.That(result.Width, Is.EqualTo(300));
            Assert.That(result.Height, Is.EqualTo(400));
        });
    }

    [Test]
    public void TestDeserializingWithMissingWindowIdThrows()
    {
        string json = @"{ ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidWindowIdDataTypeThrows()
    {
        string json = @"{ ""clientWindow"": 1234, ""active"": ""invalid"", ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithMaximizedState()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""maximized"", ""x"": 0, ""y"": 0, ""width"": 1280, ""height"": 1024 }";
        SetClientWindowStateCommandResult? result = JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.State, Is.EqualTo(ClientWindowState.Maximized));
            Assert.That(result.IsActive, Is.True);
            Assert.That(result.X, Is.EqualTo(0));
            Assert.That(result.Y, Is.EqualTo(0));
            Assert.That(result.Width, Is.EqualTo(1280));
            Assert.That(result.Height, Is.EqualTo(1024));
        });
    }

    [Test]
    public void TestCanDeserializeWithMinimizedState()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""minimized"", ""x"": 0, ""y"": 0, ""width"": 0, ""height"": 0 }";
        SetClientWindowStateCommandResult? result = JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.State, Is.EqualTo(ClientWindowState.Minimized));
            Assert.That(result.IsActive, Is.True);
            Assert.That(result.X, Is.EqualTo(0));
            Assert.That(result.Y, Is.EqualTo(0));
            Assert.That(result.Width, Is.EqualTo(0));
            Assert.That(result.Height, Is.EqualTo(0));
        });
    }

    [Test]
    public void TestCanDeserializeWithFullscreenState()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""fullscreen"", ""x"": 0, ""y"": 0, ""width"": 1280, ""height"": 1024 }";
        SetClientWindowStateCommandResult? result = JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.State, Is.EqualTo(ClientWindowState.Fullscreen));
            Assert.That(result.IsActive, Is.True);
            Assert.That(result.X, Is.EqualTo(0));
            Assert.That(result.Y, Is.EqualTo(0));
            Assert.That(result.Width, Is.EqualTo(1280));
            Assert.That(result.Height, Is.EqualTo(1024));
        });
    }

    [Test]
    public void TestDeserializingWithMissingStateThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidStateValueThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""invalid"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializingWithInvalidStateDataTypeThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": 123, ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestCanDeserializeWithInactive()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": false, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        SetClientWindowStateCommandResult? result = JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions);
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.ClientWindowId, Is.EqualTo("myClientWindow"));
            Assert.That(result.State, Is.EqualTo(ClientWindowState.Normal));
            Assert.That(result.IsActive, Is.False);
            Assert.That(result.X, Is.EqualTo(100));
            Assert.That(result.Y, Is.EqualTo(200));
            Assert.That(result.Width, Is.EqualTo(300));
            Assert.That(result.Height, Is.EqualTo(400));
        });
    }

    [Test]
    public void TestDeserializingWithMissingActiveThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidActiveValueThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": ""invalid"", ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingXThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidXValueThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": -1, ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidXDataTypeThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": ""invalid"", ""y"": 200, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingYThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidYValueThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": -1, ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidYDataTypeThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": ""invalid"", ""width"": 300, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingWidthThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidWidthValueThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": -1, ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidWidthDataTypeThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": ""invalid"", ""height"": 400 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingHeightThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidHeightValueThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": -1 }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidHeightDataTypeThrows()
    {
        string json = @"{ ""clientWindow"": ""myClientWindow"", ""active"": true, ""state"": ""normal"", ""x"": 100, ""y"": 200, ""width"": 300, ""height"": ""invalid"" }";
        Assert.That(() => JsonSerializer.Deserialize<SetClientWindowStateCommandResult>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
