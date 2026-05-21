namespace WebDriverBiDi.Script;

using System.Text.Json;

public class CollectionRemoteValueTests
{
    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Array, result.Type);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        Assert.Null(result.Handle);
        Assert.Null(result.InternalId);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Array, result.Type);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Null(result.Handle);
        Assert.Null(result.InternalId);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Array, result.Type);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Array, result.Type);
        Assert.Null(result.Value);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
    }

    [Fact]
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

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("array", argumentLocalValue.Type);
        Assert.IsType<IEnumerable<LocalValue>>(argumentLocalValue.Value, exactMatch: false);
        Assert.Single((IEnumerable<LocalValue>)argumentLocalValue.Value);
    }

    [Fact]
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

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("set", argumentLocalValue.Type);
        Assert.IsType<IEnumerable<LocalValue>>(argumentLocalValue.Value, exactMatch: false);
        Assert.Single((IEnumerable<LocalValue>)argumentLocalValue.Value);
    }

    [Fact]
    public void TestCanConvertHtmlCollectionToLocalValue()
    {
        string json = """
                      {
                        "type": "htmlcollection",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("array", argumentLocalValue.Type);
        Assert.IsType<IEnumerable<LocalValue>>(argumentLocalValue.Value, exactMatch: false);
        Assert.Empty((IEnumerable<LocalValue>)argumentLocalValue.Value);
    }

    [Fact]
    public void TestCanConvertNodeListToLocalValue()
    {
        string json = """
                      {
                        "type": "nodelist",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("array", argumentLocalValue.Type);
        Assert.IsType<IEnumerable<LocalValue>>(argumentLocalValue.Value, exactMatch: false);
        Assert.Empty((IEnumerable<LocalValue>)argumentLocalValue.Value);
    }

    [Fact]
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

        Assert.NotNull(result);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
    public void TestDeserializingCollectionRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CollectionRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingCollectionRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-list-like-value",
                        "value": []
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<CollectionRemoteValue>(json));
    }

    [Fact]
    public void TestConvertingToLocalValueWithNullValueThrows()
    {
        string json = """
                      {
                        "type": "array"
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToLocalValue());
    }

    [Fact]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToRemoteObjectReference());
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "array",
                        "value": []
                      }
                      """;

        CollectionRemoteValue? result = JsonSerializer.Deserialize<CollectionRemoteValue>(json);
        Assert.NotNull(result);
        CollectionRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
