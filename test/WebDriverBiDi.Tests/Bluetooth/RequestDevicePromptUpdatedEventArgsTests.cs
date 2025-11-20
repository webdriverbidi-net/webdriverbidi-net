namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using NUnit.Framework.Internal.Execution;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class RequestDevicePromptUpdatedEventArgsTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Prompt, Is.EqualTo("myPromptId"));
            Assert.That(eventArgs.Devices, Has.Count.EqualTo(0));
        });
    }

    [Test]
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
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Prompt, Is.EqualTo("myPromptId"));
            Assert.That(eventArgs.Devices, Has.Count.EqualTo(1));
            Assert.That(eventArgs.Devices[0].DeviceId, Is.EqualTo("myDeviceId"));
            Assert.That(eventArgs.Devices[0].DeviceName, Is.EqualTo("myDeviceName"));
        });
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        RequestDevicePromptUpdatedEventArgs? eventArgs = JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        RequestDevicePromptUpdatedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "prompt": "myPromptId",
                        "devices": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingPromptThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "devices": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidPromptTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": {},
                        "devices": []
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingDevicesThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDevicesTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "prompt": "myPromptId",
                        "devices": "someDevice"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDevicePromptUpdatedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
