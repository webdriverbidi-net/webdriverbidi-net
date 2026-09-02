namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class RequestDevicePromptUpdatedEventArgsTests
{
    private readonly JsonSerializerOptions options = new()
    {
        RespectNullableAnnotations = true,
    };

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
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myPromptId", eventArgs.PromptId);
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
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myPromptId", eventArgs.PromptId);
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
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options);
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullContextThrows()
    {
        string json = """
                      {
                        "context": null,
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullPromptThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": null,
                        "devices": []
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
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
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
    }

    [Fact]
    public void TestDeserializingWithNullDevicesThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": null
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, this.options));
    }
}
