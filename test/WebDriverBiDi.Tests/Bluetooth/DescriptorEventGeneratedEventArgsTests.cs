namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class DescriptorEventGeneratedEventArgsTests
{
    [Fact]
    public void TestCanDeserializeWithReadType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        DescriptorEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(DescriptorEventGeneratedType.Read, eventArgs.Type);
        Assert.Null(eventArgs.Data);
    }

    [Fact]
    public void TestCanDeserializeWithWriteType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "write"
                      }
                      """;
        DescriptorEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(DescriptorEventGeneratedType.Write, eventArgs.Type);
        Assert.Null(eventArgs.Data);
    }

    [Fact]
    public void TestCanDeserializeWithEmptyData()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read",
                        "data": []
                      }
                      """;
        DescriptorEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(DescriptorEventGeneratedType.Read, eventArgs.Type);
        Assert.NotNull(eventArgs.Data);
        Assert.Empty(eventArgs.Data);
    }

    [Fact]
    public void TestCanDeserializeWithData()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read",
                        "data": [123, 456]
                      }
                      """;
        DescriptorEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(DescriptorEventGeneratedType.Read, eventArgs.Type);
        Assert.NotNull(eventArgs.Data);
        Assert.Equal(2, eventArgs.Data.Count);
        Assert.Equal(123u, eventArgs.Data[0]);
        Assert.Equal(456u, eventArgs.Data[1]);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        DescriptorEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);
        DescriptorEventGeneratedEventArgs copy = eventArgs with { };
        Assert.Equal(eventArgs, copy);
    }

    [Fact]
    public void TestDeserializingWithMissingContextThrows()
    {
        string json = """
                      {
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidContextTypeThrows()
    {
        string json = """
                      {
                        "context": {},
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingAddressThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidAddressTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": {},
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingServiceUuidThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidServiceUuidTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": {},
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingCharacteristicUuidThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                     """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidCharacteristicUuidTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": {},
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingDescriptorUuidThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "type": "read"
                      }
                     """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidDescriptorUuidTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": {},
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidTypeDataTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DescriptorEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestCanDeserializeWithInvalidDataType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read",
                        "data": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestCanDeserializeWithInvalidDataElementsType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "descriptorUuid": "myDescriptorUuid",
                        "type": "read",
                        "data": ["123", false]
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
    }
}
