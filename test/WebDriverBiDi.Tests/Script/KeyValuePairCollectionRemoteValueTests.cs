namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class KeyValuePairCollectionRemoteValueTests
{
    [Test]
    public void TestCanDeserializeKeyValuePairCollectionRemoteValue()
    {
        string json = """
                      {
                        "type": "map",
                        "value": []
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a KeyValuePairCollectionRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Map));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Empty);
        Assert.That(result.Handle, Is.Null);
        Assert.That(result.InternalId, Is.Null);    
    }

    [Test]
    public void TestCanDeserializeKeyValuePairCollectionRemoteValueWithValueContainingStringKeys()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            "keyValue",
                            {
                              "type": "number",
                              "value": 42
                            }
                          ]
                        ]
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Map));
        Assert.That(result.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanDeserializeKeyValuePairCollectionRemoteValueWithValueContainingObjectKeys()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "string",
                              "value": "keyValue"
                            },
                            {
                              "type": "number",
                              "value": 42
                            }
                          ]
                        ]
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Map));
        Assert.That(result.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanDeserializeKeyValuePairCollectionRemoteValueWithHandle()
    {
        string json = """
                      {
                        "type": "map",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "value": []
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a KeyValuePairCollectionRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Map));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Empty);
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
    }

    [Test]
    public void TestCanDeserializeKeyValuePairCollectionRemoteValueWithNoValue()
    {
        string json = """
                      {
                        "type": "map",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a KeyValuePairCollectionRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Map));
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
    }

    [Test]
    public void TestCanConvertMapWithStringKeysToLocalValue()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            "keyValue",
                            {
                              "type": "number",
                              "value": 42
                            }
                          ]
                        ]
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("map"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Assert.That(argumentLocalValue.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanConvertMapWithObjectKeysToLocalValue()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "string",
                              "value": "keyValue"
                            },
                            {
                              "type": "number",
                              "value": 42
                            }
                          ]
                        ]
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("map"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Assert.That(argumentLocalValue.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanConvertObjectToLocalValue()
    {
        string json = """
                      {
                        "type": "object",
                        "value": [
                          [
                            {
                              "type": "string",
                              "value": "keyValue"
                            },
                            {
                              "type": "number",
                              "value": 42
                            }
                          ]
                        ]
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("object"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Assert.That(argumentLocalValue.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanConvertToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "map",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;
        
        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
    public void TestDeserializingKeyValuePairCollectionRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": {}
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingCollectionRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-list-like-value",
                        "value": []
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<CollectionRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestConvertingToLocalValueWithNullValueThrows()
    {
        string json = """
                      {
                        "type": "map"
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToLocalValue(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": []
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToRemoteObjectReference(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "map",
                        "value": []
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        KeyValuePairCollectionRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
