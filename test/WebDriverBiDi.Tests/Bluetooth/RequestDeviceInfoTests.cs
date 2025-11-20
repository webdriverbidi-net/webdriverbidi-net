namespace WebDriverBiDi.Bluetooth;

using System.Runtime;
using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class RequestDeviceInfoTests
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
                        "id": "myDeviceId",
                        "name": "myDeviceName"
                      }
                      """;
        RequestDeviceInfo? info = JsonSerializer.Deserialize<RequestDeviceInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(info.DeviceId, Is.EqualTo("myDeviceId"));
            Assert.That(info.DeviceName, Is.EqualTo("myDeviceName"));
        });
    }

    [Test]
    public void TestCanDeserializeWithNullName()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                        "name": null
                      }
                      """;
        RequestDeviceInfo? info = JsonSerializer.Deserialize<RequestDeviceInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(info.DeviceId, Is.EqualTo("myDeviceId"));
            Assert.That(info.DeviceName, Is.Null);
        });
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                        "name": "myDeviceName"
                      }
                      """;
        RequestDeviceInfo? info = JsonSerializer.Deserialize<RequestDeviceInfo>(json, deserializationOptions);
        Assert.That(info, Is.Not.Null);
        RequestDeviceInfo copy = info with { };
        Assert.That(copy, Is.EqualTo(info));
    }

    [Test]
    public void TestDeserializingWithMissingDeviceIdThrows()
    {
        string json = """
                      {
                        "name": "myDeviceName"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDeviceIdTypeThrows()
    {
        string json = """
                      {
                        "id": 123,
                        "name": "myDeviceName"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithMissingDeviceNameThrows()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWithInvalidDeviceNameTypeThrows()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                        "name": 123
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
