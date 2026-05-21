namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class RequestDevicePromptUpdatedEventArgsTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myPromptId", eventArgs.Prompt);
        Assert.Empty(eventArgs.Devices);
    }

    [Fact]
    public void TestCanDeserializeWithDevices()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": [
                          {
                            "id": "myDeviceId",
                            "name": "myDeviceName"
                          }
                        ]
                      }
                      """;
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myPromptId", eventArgs.Prompt);
        Assert.Single(eventArgs.Devices);
        Assert.Equal("myDeviceId", eventArgs.Devices[0].DeviceId);
        Assert.Equal("myDeviceName", eventArgs.Devices[0].DeviceName);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json);
        Assert.NotNull(eventArgs);
        RequestDevicePromptUpdatedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingPromptThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "devices": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidPromptTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": {},
                        "devices": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingDevicesThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDevicesTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": "someDevice"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json));
    }
}
