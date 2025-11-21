namespace WebDriverBiDi.Bluetooth;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class CharacteristicEventGeneratedEventArgsTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
            Assert.That(eventArgs.ServiceUuid, Is.EqualTo("myServiceUuid"));
            Assert.That(eventArgs.CharacteristicUuid, Is.EqualTo("myCharacteristicUuid"));
            Assert.That(eventArgs.Type, Is.EqualTo(CharacteristicEventGeneratedType.Read));
            Assert.That(eventArgs.Data, Is.Null);
        }
    }

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
            Assert.That(eventArgs.ServiceUuid, Is.EqualTo("myServiceUuid"));
            Assert.That(eventArgs.CharacteristicUuid, Is.EqualTo("myCharacteristicUuid"));
            Assert.That(eventArgs.Type, Is.EqualTo(CharacteristicEventGeneratedType.WriteWithoutResponse));
            Assert.That(eventArgs.Data, Is.Null);
        }
    }

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
            Assert.That(eventArgs.ServiceUuid, Is.EqualTo("myServiceUuid"));
            Assert.That(eventArgs.CharacteristicUuid, Is.EqualTo("myCharacteristicUuid"));
            Assert.That(eventArgs.Type, Is.EqualTo(CharacteristicEventGeneratedType.WriteWithResponse));
            Assert.That(eventArgs.Data, Is.Null);
        }
    }

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
            Assert.That(eventArgs.ServiceUuid, Is.EqualTo("myServiceUuid"));
            Assert.That(eventArgs.CharacteristicUuid, Is.EqualTo("myCharacteristicUuid"));
            Assert.That(eventArgs.Type, Is.EqualTo(CharacteristicEventGeneratedType.SubscribeToNotifications));
            Assert.That(eventArgs.Data, Is.Null);
        }
    }

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
            Assert.That(eventArgs.ServiceUuid, Is.EqualTo("myServiceUuid"));
            Assert.That(eventArgs.CharacteristicUuid, Is.EqualTo("myCharacteristicUuid"));
            Assert.That(eventArgs.Type, Is.EqualTo(CharacteristicEventGeneratedType.UnsubscribeFromNotifications));
            Assert.That(eventArgs.Data, Is.Null);
        }
    }

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
            Assert.That(eventArgs.ServiceUuid, Is.EqualTo("myServiceUuid"));
            Assert.That(eventArgs.CharacteristicUuid, Is.EqualTo("myCharacteristicUuid"));
            Assert.That(eventArgs.Type, Is.EqualTo(CharacteristicEventGeneratedType.Read));
            Assert.That(eventArgs.Data, Is.Not.Null);
            Assert.That(eventArgs.Data, Is.Empty);
        }
    }

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(eventArgs.BrowsingContextId, Is.EqualTo("myContextId"));
            Assert.That(eventArgs.Address, Is.EqualTo("myAddress"));
            Assert.That(eventArgs.ServiceUuid, Is.EqualTo("myServiceUuid"));
            Assert.That(eventArgs.CharacteristicUuid, Is.EqualTo("myCharacteristicUuid"));
            Assert.That(eventArgs.Type, Is.EqualTo(CharacteristicEventGeneratedType.Read));
            Assert.That(eventArgs.Data, Is.Not.Null);
            Assert.That(eventArgs.Data, Has.Count.EqualTo(2));
            Assert.That(eventArgs.Data![0], Is.EqualTo(123));
            Assert.That(eventArgs.Data![1], Is.EqualTo(456));
        }
    }

    [Test]
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
        CharacteristicEventGeneratedEventArgs? eventArgs = JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions);
        Assert.That(eventArgs, Is.Not.Null);
        CharacteristicEventGeneratedEventArgs copy = eventArgs with { };
        Assert.That(copy, Is.EqualTo(eventArgs));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<CharacteristicEventGeneratedEventArgs>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
