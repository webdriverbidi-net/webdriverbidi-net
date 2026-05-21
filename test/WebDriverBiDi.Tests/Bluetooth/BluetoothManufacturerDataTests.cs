namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class BluetoothManufacturerDataTests
{
    [Fact]
    public void TestCanSerialize()
    {
        BluetoothManufacturerData properties = new(123, "myData");
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("key"));
        JToken? key = serialized["key"];
        Assert.NotNull(key);
        Assert.Equal(JTokenType.Integer, key.Type);
        Assert.Equal(123u, key.Value<uint>());

        Assert.True(serialized.ContainsKey("data"));
        JToken? data = serialized["data"];
        Assert.NotNull(data);
        Assert.Equal(JTokenType.String, data.Type);
        Assert.Equal("myData", data.Value<string>());
    }

    [Fact]
    public void TestCanUpdatePropertiesAfterInstantiation()
    {
        BluetoothManufacturerData properties = new(123, "myData")
        {
            Key = 456,
            Data = "myUpdatedData"
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(2, serialized.Count);

        Assert.True(serialized.ContainsKey("key"));
        JToken? key = serialized["key"];
        Assert.NotNull(key);
        Assert.Equal(JTokenType.Integer, key.Type);
        Assert.Equal(456u, key.Value<uint>());

        Assert.True(serialized.ContainsKey("data"));
        JToken? data = serialized["data"];
        Assert.NotNull(data);
        Assert.Equal(JTokenType.String, data.Type);
        Assert.Equal("myUpdatedData", data.Value<string>());
    }
}
