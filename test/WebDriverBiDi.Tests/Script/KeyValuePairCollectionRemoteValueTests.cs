namespace WebDriverBiDi.Script;

using System.Text.Json;

public class KeyValuePairCollectionRemoteValueTests
{
    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Map, result.Type);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        Assert.Null(result.Handle);
        Assert.Null(result.InternalId);
    }

    [Fact]
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

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Map, result.Type);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
    }

    [Fact]
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

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Map, result.Type);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Map, result.Type);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
    }

    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Map, result.Type);
        Assert.Null(result.Value);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
    }

    [Fact]
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

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("map", argumentLocalValue.Type);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentLocalValue.Value);
        Assert.Single((Dictionary<object, LocalValue>)argumentLocalValue.Value);
    }

    [Fact]
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

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("map", argumentLocalValue.Type);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentLocalValue.Value);
        Assert.Single((Dictionary<object, LocalValue>)argumentLocalValue.Value);
    }

    [Fact]
    public void TestConvertingMapWithStructurallyIdenticalObjectKeysToLocalValuePreservesDistinctEntries()
    {
        // A JavaScript Map may contain two distinct object keys that serialize
        // identically (for example, two Date keys holding the same instant).
        // RemoteValueDictionary preserves them as distinct entries via reference
        // equality, and converting to a LocalValue must not collapse them, even
        // though the converted keys are structurally equal records.
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            "stringKey",
                            {
                              "type": "number",
                              "value": 1
                            }
                          ],
                          [
                            {
                              "type": "date",
                              "value": "1970-01-01T00:00:00.000Z"
                            },
                            {
                              "type": "number",
                              "value": 2
                            }
                          ],
                          [
                            {
                              "type": "date",
                              "value": "1970-01-01T00:00:00.000Z"
                            },
                            {
                              "type": "number",
                              "value": 3
                            }
                          ]
                        ]
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("map", argumentLocalValue.Type);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentLocalValue.Value);
        Dictionary<object, LocalValue> convertedDictionary = (Dictionary<object, LocalValue>)argumentLocalValue.Value;
        Assert.Equal(3, convertedDictionary.Count);

        // String keys are compared by value.
        Assert.True(convertedDictionary.ContainsKey("stringKey"));

        // The two date keys are structurally equal records, yet remain distinct entries.
        List<LocalValue> dateKeys = convertedDictionary.Keys.OfType<LocalValue>().ToList();
        Assert.Equal(2, dateKeys.Count);
        Assert.Equal(dateKeys[0], dateKeys[1]);
        Assert.NotSame(dateKeys[0], dateKeys[1]);

        // Each entry is retrievable by its own key instance and carries its own value.
        LocalArgumentValue firstDateKeyedValue = (LocalArgumentValue)convertedDictionary[dateKeys[0]];
        LocalArgumentValue secondDateKeyedValue = (LocalArgumentValue)convertedDictionary[dateKeys[1]];
        Assert.NotEqual(firstDateKeyedValue.Value, secondDateKeyedValue.Value);
    }

    [Fact]
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

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("object", argumentLocalValue.Type);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentLocalValue.Value);
        Assert.Single((Dictionary<object, LocalValue>)argumentLocalValue.Value);
    }

    [Fact]
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

        Assert.NotNull(result);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
    public void TestDeserializingKeyValuePairCollectionRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": {}
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json));
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
                        "type": "map"
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToLocalValue());
    }

    [Fact]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": []
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToRemoteObjectReference());
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "map",
                        "value": []
                      }
                      """;

        KeyValuePairCollectionRemoteValue? result = JsonSerializer.Deserialize<KeyValuePairCollectionRemoteValue>(json);
        Assert.NotNull(result);
        KeyValuePairCollectionRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
