namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class CollectionRemoteValueTests
{
    [Test]
    public void TestCanDeserializeCollectionRemoteValue()
    {
        string json = """
                      {
                        "type": "array",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a CollectionRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Array));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Empty);
        Assert.That(result.Handle, Is.Null);
        Assert.That(result.InternalId, Is.Null);    
    }

    [Test]
    public void TestCanDeserializeCollectionRemoteValueWithValues()
    {
        string json = """
                      {
                        "type": "array",
                        "value": [
                          {
                            "type": "string",
                            "value": "test"
                          }
                        ]
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a CollectionRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Array));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Handle, Is.Null);
        Assert.That(result.InternalId, Is.Null);    
    }

    [Test]
    public void TestCanDeserializeCollectionRemoteValueWithHandle()
    {
        string json = """
                      {
                        "type": "array",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a CollectionRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Array));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value, Is.Empty);
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
    }

    [Test]
    public void TestCanDeserializeCollectionRemoteValueWithNoValue()
    {
        string json = """
                      {
                        "type": "array",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a CollectionRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Array));
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
    }

    [Test]
    public void TestCanConvertArrayToLocalValue()
    {
        string json = """
                      {
                        "type": "array",
                        "value": [
                          {
                            "type": "string",
                            "value": "test"
                          }
                        ]
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("array"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<IEnumerable<LocalValue>>());
        Assert.That(argumentLocalValue.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanConvertSetToLocalValue()
    {
        string json = """
                      {
                        "type": "set",
                        "value": [
                          {
                            "type": "string",
                            "value": "test"
                          }
                        ]
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("set"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<IEnumerable<LocalValue>>());
        Assert.That(argumentLocalValue.Value, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestCanConvertHtmlCollectionToLocalValue()
    {
        string json = """
                      {
                        "type": "htmlcollection",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("array"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<IEnumerable<LocalValue>>());
        Assert.That(argumentLocalValue.Value, Is.Empty);
    }

    [Test]
    public void TestCanConvertNodeListToLocalValue()
    {
        string json = """
                      {
                        "type": "nodelist",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("array"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<IEnumerable<LocalValue>>());
        Assert.That(argumentLocalValue.Value, Is.Empty);
    }

    [Test]
    public void TestCanConvertToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "array",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;
        
        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
    public void TestDeserializingCollectionRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<CollectionRemoteValue>(json), Throws.InstanceOf<JsonException>());
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
                        "type": "array"
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToLocalValue(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToRemoteObjectReference(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "array",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        CollectionRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
