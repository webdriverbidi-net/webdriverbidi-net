namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;

public class CharacteristicEventGeneratedEventArgsTests
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
                        "type": "read"
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(CharacteristicEventGeneratedType.Read, eventArgs.Type);
        Assert.Null(eventArgs.Data);
    }

    [Fact]
    public void TestCanDeserializeWithWriteWithoutResponseType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "type": "write-without-response"
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(CharacteristicEventGeneratedType.WriteWithoutResponse, eventArgs.Type);
        Assert.Null(eventArgs.Data);
    }

    [Fact]
    public void TestCanDeserializeWithWriteWithResponseType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "type": "write-with-response"
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(CharacteristicEventGeneratedType.WriteWithResponse, eventArgs.Type);
        Assert.Null(eventArgs.Data);
    }

    [Fact]
    public void TestCanDeserializeWithSubscribeResponseType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "type": "subscribe-to-notifications"
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(CharacteristicEventGeneratedType.SubscribeToNotifications, eventArgs.Type);
        Assert.Null(eventArgs.Data);
    }

    [Fact]
    public void TestCanDeserializeWithUnsubscribeType()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "type": "unsubscribe-from-notifications"
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(CharacteristicEventGeneratedType.UnsubscribeFromNotifications, eventArgs.Type);
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
                        "type": "read",
                        "data": []
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(CharacteristicEventGeneratedType.Read, eventArgs.Type);
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
                        "type": "read",
                        "data": [123, 456]
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);

        Assert.Equal("myContextId", eventArgs.BrowsingContextId);
        Assert.Equal("myAddress", eventArgs.Address);
        Assert.Equal("myServiceUuid", eventArgs.ServiceUuid);
        Assert.Equal("myCharacteristicUuid", eventArgs.CharacteristicUuid);
        Assert.Equal(CharacteristicEventGeneratedType.Read, eventArgs.Type);
        Assert.NotNull(eventArgs.Data);
        Assert.Equal(2, eventArgs.Data.Count);
        Assert.Equal<uint>(123U, eventArgs.Data[0]);
        Assert.Equal<uint>(456U, eventArgs.Data[1]);
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
                        "type": "read"
                      }
                      """;
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json);
        Assert.NotNull(eventArgs);
        CharacteristicEventGeneratedEventArgs copy = eventArgs with { };
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
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
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
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingAddressThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
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
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingServiceUuidThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "characteristicUuid": "myCharacteristicUuid",
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
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
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingCharacteristicUuidThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "type": "read"
                      }
                     """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
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
                        "type": "read"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
    }

    [Fact]
    public void TestDeserializingWithMissingTypeThrows()
    {
        string json = """
                      {
                        "context": "myContextId",
                        "address": "myAddress",
                        "serviceUuid": "myServiceUuid",
                        "characteristicUuid": "myCharacteristicUuid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
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
                        "type": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
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
                        "type": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
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
                        "type": "read",
                        "data": ["123", false]
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json));
    }
}
