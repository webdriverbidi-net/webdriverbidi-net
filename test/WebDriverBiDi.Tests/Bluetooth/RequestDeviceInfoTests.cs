namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class RequestDeviceInfoTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                        "name": "myDeviceName"
                      }
                      """;
        RequestDeviceInfo? info = JsonSerializer.Deserialize<RequestDeviceInfo>(json);
        Assert.NotNull(info);

        Assert.Equal("myDeviceId", info.DeviceId);
        Assert.Equal("myDeviceName", info.DeviceName);
    }

    [Fact]
    public void TestCanDeserializeWithNullName()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                        "name": null
                      }
                      """;
        RequestDeviceInfo? info = JsonSerializer.Deserialize<RequestDeviceInfo>(json);
        Assert.NotNull(info);

        Assert.Equal("myDeviceId", info.DeviceId);
        Assert.Null(info.DeviceName);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                        "name": "myDeviceName"
                      }
                      """;
        RequestDeviceInfo? info = JsonSerializer.Deserialize<RequestDeviceInfo>(json);
        Assert.NotNull(info);
        RequestDeviceInfo copy = info with { };
        Assert.Equal(info, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingDeviceIdThrows()
    {
        string json = """
                      {
                        "name": "myDeviceName"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDeviceIdTypeThrows()
    {
        string json = """
                      {
                        "id": 123,
                        "name": "myDeviceName"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingDeviceNameThrows()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDeviceNameTypeThrows()
    {
        string json = """
                      {
                        "id": "myDeviceId",
                        "name": 123
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RequestDeviceInfo>(json));
    }
}
