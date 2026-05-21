namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class CharacteristicPropertiesTests
{
    [Fact]
    public void TestCanSerialize()
    {
        CharacteristicProperties properties = new();
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Empty(serialized);
    }

    [Fact]
    public void TestCanSerializeWithAllValuesTrue()
    {
        CharacteristicProperties properties = new()
        {
            IsBroadcast = true,
            IsRead = true,
            IsWriteWithoutResponse = true,
            IsWrite = true,
            IsNotify = true,
            IsIndicate = true,
            IsAuthenticatedSignedWrites = true,
            IsExtendedProperties = true,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(8, serialized.Count);

        Assert.True(serialized.ContainsKey("broadcast"));
        JToken? broadcast = serialized["broadcast"];
        Assert.NotNull(broadcast);
        Assert.Equal(JTokenType.Boolean, broadcast.Type);
        Assert.True(broadcast.Value<bool>());

        Assert.True(serialized.ContainsKey("read"));
        JToken? read = serialized["read"];
        Assert.NotNull(read);
        Assert.Equal(JTokenType.Boolean, read.Type);
        Assert.True(read.Value<bool>());

        Assert.True(serialized.ContainsKey("writeWithoutResponse"));
        JToken? writeWithoutResponse = serialized["read"];
        Assert.NotNull(writeWithoutResponse);
        Assert.Equal(JTokenType.Boolean, writeWithoutResponse.Type);
        Assert.True(writeWithoutResponse.Value<bool>());

        Assert.True(serialized.ContainsKey("write"));
        JToken? write = serialized["write"];
        Assert.NotNull(write);
        Assert.Equal(JTokenType.Boolean, write.Type);
        Assert.True(write.Value<bool>());

        Assert.True(serialized.ContainsKey("notify"));
        JToken? notify = serialized["notify"];
        Assert.NotNull(notify);
        Assert.Equal(JTokenType.Boolean, notify.Type);
        Assert.True(notify.Value<bool>());

        Assert.True(serialized.ContainsKey("indicate"));
        JToken? indicate = serialized["indicate"];
        Assert.NotNull(indicate);
        Assert.Equal(JTokenType.Boolean, indicate.Type);
        Assert.True(indicate.Value<bool>());

        Assert.True(serialized.ContainsKey("authenticatedSignedWrites"));
        JToken? authenticatedSignedWrites = serialized["authenticatedSignedWrites"];
        Assert.NotNull(authenticatedSignedWrites);
        Assert.Equal(JTokenType.Boolean, authenticatedSignedWrites.Type);
        Assert.True(authenticatedSignedWrites.Value<bool>());

        Assert.True(serialized.ContainsKey("extendedProperties"));
        JToken? extendedProperties = serialized["extendedProperties"];
        Assert.NotNull(extendedProperties);
        Assert.Equal(JTokenType.Boolean, extendedProperties.Type);
        Assert.True(extendedProperties.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeWithAllValuesFalse()
    {
        CharacteristicProperties properties = new()
        {
            IsBroadcast = false,
            IsRead = false,
            IsWriteWithoutResponse = false,
            IsWrite = false,
            IsNotify = false,
            IsIndicate = false,
            IsAuthenticatedSignedWrites = false,
            IsExtendedProperties = false,
        };
        string json = JsonSerializer.Serialize(properties);
        JObject serialized = JObject.Parse(json);
        Assert.Equal(8, serialized.Count);

        Assert.True(serialized.ContainsKey("broadcast"));
        JToken? broadcast = serialized["broadcast"];
        Assert.NotNull(broadcast);
        Assert.Equal(JTokenType.Boolean, broadcast.Type);
        Assert.False(broadcast.Value<bool>());

        Assert.True(serialized.ContainsKey("read"));
        JToken? read = serialized["read"];
        Assert.NotNull(read);
        Assert.Equal(JTokenType.Boolean, read.Type);
        Assert.False(read.Value<bool>());

        Assert.True(serialized.ContainsKey("writeWithoutResponse"));
        JToken? writeWithoutResponse = serialized["read"];
        Assert.NotNull(writeWithoutResponse);
        Assert.Equal(JTokenType.Boolean, writeWithoutResponse.Type);
        Assert.False(writeWithoutResponse.Value<bool>());

        Assert.True(serialized.ContainsKey("write"));
        JToken? write = serialized["write"];
        Assert.NotNull(write);
        Assert.Equal(JTokenType.Boolean, write.Type);
        Assert.False(write.Value<bool>());

        Assert.True(serialized.ContainsKey("notify"));
        JToken? notify = serialized["notify"];
        Assert.NotNull(notify);
        Assert.Equal(JTokenType.Boolean, notify.Type);
        Assert.False(notify.Value<bool>());

        Assert.True(serialized.ContainsKey("indicate"));
        JToken? indicate = serialized["indicate"];
        Assert.NotNull(indicate);
        Assert.Equal(JTokenType.Boolean, indicate.Type);
        Assert.False(indicate.Value<bool>());

        Assert.True(serialized.ContainsKey("authenticatedSignedWrites"));
        JToken? authenticatedSignedWrites = serialized["authenticatedSignedWrites"];
        Assert.NotNull(authenticatedSignedWrites);
        Assert.Equal(JTokenType.Boolean, authenticatedSignedWrites.Type);
        Assert.False(authenticatedSignedWrites.Value<bool>());

        Assert.True(serialized.ContainsKey("extendedProperties"));
        JToken? extendedProperties = serialized["extendedProperties"];
        Assert.NotNull(extendedProperties);
        Assert.Equal(JTokenType.Boolean, extendedProperties.Type);
        Assert.False(extendedProperties.Value<bool>());
    }
}
