namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class ScanRecordTests
{
    [Fact]
    public void TestCanSerialize()
    {
        ScanRecord properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeWithOptionalData()
    {
        ScanRecord properties = new()
        {
            Name = "myName",
            Appearance = 123,
            ManufacturerData = [new BluetoothManufacturerData(456, "myManufacturerData")],
            UUIDs = ["my-service-uuid"]
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(4, serialized.Count);

        // BluetoothManufacturerData serialization is tested in its own set of tests,
        // so its serialized structure need not be fully verified here.
        Assert.True(serialized.ContainsKey("name"));
        JToken? name = serialized["name"];
        Assert.NotNull(name);
        Assert.Equal(JTokenType.String, name.Type);
        Assert.Equal("myName", name.Value<string>());

        Assert.True(serialized.ContainsKey("uuids"));
        JToken? uuids = serialized["uuids"];
        Assert.NotNull(uuids);
        Assert.Equal(JTokenType.Array, uuids.Type);
        JToken uuid = Assert.Single(uuids);

        Assert.NotNull(uuid);
        Assert.Equal(JTokenType.String, uuid.Type);
        Assert.Equal("my-service-uuid", uuid.Value<string>());

        Assert.True(serialized.ContainsKey("appearance"));
        JToken? appearance = serialized["appearance"];
        Assert.NotNull(appearance);
        Assert.Equal(JTokenType.Integer, appearance.Type);
        Assert.Equal(123u, appearance.Value<uint>());

        Assert.True(serialized.ContainsKey("manufacturerData"));
        JToken? manufacturerData = serialized["manufacturerData"];
        Assert.NotNull(manufacturerData);
        Assert.Equal(JTokenType.Array, manufacturerData.Type);
        JToken manufacturerDataArray = Assert.Single(manufacturerData);
        Assert.Equal(JTokenType.Object, manufacturerDataArray.Type);
    }
}
