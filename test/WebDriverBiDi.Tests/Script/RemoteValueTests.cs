namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json;

public class RemoteValueTests
{
    [Fact]
    public void TestDeserializingInvalidStringRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "string",
                        "value": 7
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestCanDeserializeNaNRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "NaN"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Number, remoteValue.Type);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        Assert.Equal(double.NaN, ((NumberRemoteValue)remoteValue).Value);
    }

    [Fact]
    public void TestCanDeserializeNegativeZeroRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "-0"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Number, remoteValue.Type);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        Assert.Equal(double.NegativeZero, ((NumberRemoteValue)remoteValue).Value);
        Assert.True(double.IsNegative(((NumberRemoteValue)remoteValue).Value));
    }

    [Fact]
    public void TestCanDeserializeInfinityRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "Infinity"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Number, remoteValue.Type);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        Assert.Equal(double.PositiveInfinity, ((NumberRemoteValue)remoteValue).Value);
    }

    [Fact]
    public void TestCanDeserializeNegativeInfinityRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "-Infinity"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Number, remoteValue.Type);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        Assert.Equal(double.NegativeInfinity, ((NumberRemoteValue)remoteValue).Value);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        RemoteValue copy = remoteValue with { };
        Assert.Equal(remoteValue, copy);
    }

    [Fact]
    public void TestDeserializingInvalidSpecialNumericRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "invalid"
                      }
                      """;
        Assert.Contains("Invalid value 'invalid' for 'value' property of number", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidNumericRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "number",
                        "value": true
                      }
                      """;
        Assert.Contains("Expected String or Number", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
        json = """
               {
                 "type": "number",
                 "value": false
               }
               """;
        Assert.Contains("Expected String or Number", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
        json = """
               {
                 "type": "number",
                 "value": null
               }
               """;
        json = @"{ ""type"": ""number"", ""value"": null }";
        Assert.Contains("Expected String or Number", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidBooleanRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": "hello"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidBigIntRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": true
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidBigIntValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("Cannot parse invalid value 'some value' for bigint", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidDateRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": true
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidDateValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRegularExpressionWithNullFlagsRemoteValue()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.NotNull(primitiveValue.Value);
        RegularExpressionValue expectedRegexValue = (RegularExpressionValue)primitiveValue.Value;

        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "myPattern"
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.RegExp, remoteValue.Type);
        Assert.IsType<RegExpRemoteValue>(remoteValue);
        RegExpRemoteValue regExpRemoteValue = (RegExpRemoteValue)remoteValue;
        Assert.Null(regExpRemoteValue.Handle);
        Assert.Null(regExpRemoteValue.InternalId);
        Assert.Equal(expectedRegexValue, regExpRemoteValue.Value);
    }

    [Fact]
    public void TestConvertingRegularExpressionRemoteValueToRemoteReference()
    {
        string json = """
                      {
                        "type": "regexp",
                        "handle": "myHandle",
                        "value": {
                          "pattern": "myPattern",
                          "flags": "gi"
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.RegExp, remoteValue.Type);
        Assert.IsType<RegExpRemoteValue>(remoteValue);
        RegExpRemoteValue regExpRemoteValue = (RegExpRemoteValue)remoteValue;
        Assert.Equal("myHandle", regExpRemoteValue.Handle);
        Assert.Null(regExpRemoteValue.InternalId);
        RemoteObjectReference remoteReference = regExpRemoteValue.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteReference.Handle);
    }

    [Fact]
    public void TestDeserializingInvalidRegularExpressionValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestConvertingRegularExpressionRemoteValueToRemoteReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": {
                          "pattern": "myPattern",
                          "flags": "gi"
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<RegExpRemoteValue>(remoteValue);
        RegExpRemoteValue regExpRemoteValue = (RegExpRemoteValue)remoteValue;
        Assert.Contains("must have a valid handle", Assert.ThrowsAny<WebDriverBiDiException>(() => regExpRemoteValue.ToRemoteObjectReference()).Message);
    }

    [Fact]
    public void TestDeserializingNodeRemoteValueWithInvalidSharedIdThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "sharedId": {},
                        "value": {
                          "nodeType": 1,
                          "nodeValue": "",
                          "childNodeCount": 0
                        }
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingSharedIdIsIgnoredForNotNodeRemoteValue()
    {
        string json = """
                      {
                        "type": "array",
                        "sharedId": "mySharedId",
                        "value": [
                          {
                            "type": "string",
                            "value": "stringValue"
                          },
                          {
                            "type": "number",
                            "value": 123
                          },
                          {
                            "type": "boolean",
                            "value": true
                          }
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Array, remoteValue.Type);
        Assert.IsType<CollectionRemoteValue>(remoteValue);
        CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
        Assert.Null(listRemoteValue.Handle);
        Assert.Null(listRemoteValue.InternalId);
        Assert.NotNull(listRemoteValue.Value);
        RemoteValueList arrayValue = listRemoteValue.Value;
        Assert.NotNull(arrayValue);
        Assert.Equal(3, arrayValue.Count);
        Assert.IsType<StringRemoteValue>(arrayValue[0]);
        Assert.Equal("stringValue", ((StringRemoteValue)arrayValue[0]).Value);
        Assert.IsType<NumberRemoteValue>(arrayValue[1]);
        Assert.Equal(123, ((NumberRemoteValue)arrayValue[1]).Value);
        Assert.IsType<BooleanRemoteValue>(arrayValue[2]);
        Assert.True(((BooleanRemoteValue)arrayValue[2]).Value);
    }

    [Fact]
    public void TestDeserializingInvalidNodeValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestConvertingCollectionRemoteValueToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "array",
                        "handle": "myHandle",
                        "value": [
                          {
                            "type": "string",
                            "value": "stringValue"
                          },
                          {
                            "type": "number",
                            "value": 123
                          },
                          {
                            "type": "boolean",
                            "value": true
                          }
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<CollectionRemoteValue>(remoteValue);
        CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
        RemoteObjectReference remoteReference = listRemoteValue.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteReference.Handle);
    }

    [Fact]
    public void TestConvertingCollectionRemoteValueToRemoteObjectReferenceWithMissingHandleThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": [
                          {
                            "type": "string",
                            "value": "stringValue"
                          },
                          {
                            "type": "number",
                            "value": 123
                          },
                          {
                            "type": "boolean",
                            "value": true
                          }
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<CollectionRemoteValue>(remoteValue);
        CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
        Assert.Contains("must have a valid handle", Assert.ThrowsAny<WebDriverBiDiException>(() => listRemoteValue.ToRemoteObjectReference()).Message);
    }

    [Fact]
    public void TestDeserializingInvalidArrayValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidArrayElementValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": [
                          "stringValue",
                          123,
                          true
                        ]
                      }
                      """;
        Assert.Contains("JSON for 'RemoteValue' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingSetRemoteValue()
    {
        string json = """
                      {
                        "type": "set",
                        "value": [
                          {
                            "type": "string",
                            "value": "stringValue"
                          },
                          {
                            "type": "number",
                            "value": 123
                          },
                          {
                            "type": "boolean",
                            "value": true
                          }
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Set, remoteValue.Type);
        CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
        Assert.Null(listRemoteValue.Handle);
        Assert.Null(listRemoteValue.InternalId);
        Assert.NotNull(listRemoteValue.Value);
        RemoteValueList setValue = listRemoteValue.Value;
        Assert.NotNull(setValue);
        Assert.Equal(3, setValue.Count);
        Assert.IsType<StringRemoteValue>(setValue[0]);
        Assert.Equal("stringValue", ((StringRemoteValue)setValue[0]).Value);
        Assert.IsType<NumberRemoteValue>(setValue[1]);
        Assert.Equal(123, ((NumberRemoteValue)setValue[1]).Value);
        Assert.IsType<BooleanRemoteValue>(setValue[2]);
        Assert.True(((BooleanRemoteValue)setValue[2]).Value);
    }

    [Fact]
    public void TestDeserializingInvalidSetValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "set",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidSetElementValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "set",
                        "value": [
                            "stringValue",
                            123,
                            true
                        ]
                      }
                      """;
        Assert.Contains("JSON for 'RemoteValue' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingNodeListRemoteValue()
    {
        string json = """
                      {
                        "type": "nodelist",
                        "value": [
                          {
                            "type": "node",
                            "value": {
                              "nodeType": 1,
                              "childNodeCount": 0
                            } 
                          }
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.NodeList, remoteValue.Type);
        CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
        Assert.Null(listRemoteValue.Handle);
        Assert.Null(listRemoteValue.InternalId);
        Assert.NotNull(listRemoteValue.Value);
        RemoteValueList nodeListValue = listRemoteValue.Value;
        Assert.NotNull(nodeListValue);
        Assert.Single(nodeListValue);
        Assert.IsType<NodeRemoteValue>(nodeListValue[0]);
        NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)nodeListValue[0];
        Assert.NotNull(nodeRemoteValue.Value);
        Assert.Equal(1u, nodeRemoteValue.Value.NodeType);
    }

    [Fact]
    public void TestDeserializingInvalidNodeListValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "nodelist",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidNodeListElementValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "nodelist",
                        "value": [
                          "stringValue",
                          123,
                          true
                        ]
                      }
                      """;
        Assert.Contains("JSON for 'RemoteValue' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingHtmlCollectionRemoteValue()
    {
        string json = """
                      {
                        "type": "htmlcollection",
                        "value": [
                          {
                            "type": "node",
                            "value": {
                              "nodeType": 1,
                              "childNodeCount": 0
                            } 
                          }
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.HtmlCollection, remoteValue.Type);
        CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
        Assert.Null(listRemoteValue.Handle);
        Assert.Null(listRemoteValue.InternalId);
        Assert.NotNull(listRemoteValue.Value);
        RemoteValueList htmlCollectionValue = listRemoteValue.Value;
        Assert.NotNull(htmlCollectionValue);
        Assert.Single(htmlCollectionValue);
        Assert.IsType<NodeRemoteValue>(htmlCollectionValue[0]);
        NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)htmlCollectionValue[0];
        Assert.NotNull(nodeRemoteValue.Value);
        Assert.Equal(1u, nodeRemoteValue.Value.NodeType);
    }

    [Fact]
    public void TestDeserializingInvalidHtmlCollectionValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "htmlcollection",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidHtmlCollectionElementValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "htmlcollection",
                        "value": [
                          "stringValue",
                          123,
                          true
                        ]
                      }
                      """;
        Assert.Contains("JSON for 'RemoteValue' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithIntegerRemoteValueKey()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "number",
                              "value": 2
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<NumberRemoteValue>(dictionaryItem.Key);
        Assert.Equal(2, ((NumberRemoteValue)dictionaryItem.Key).Value);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithBooleanRemoteValueKey()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "boolean",
                              "value": true
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<BooleanRemoteValue>(dictionaryItem.Key);
        Assert.True(((BooleanRemoteValue)dictionaryItem.Key).Value);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithBigintRemoteValueKey()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "bigint",
                              "value": "1234"
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<BigIntegerRemoteValue>(dictionaryItem.Key);
        Assert.Equal(new BigInteger(1234), ((BigIntegerRemoteValue)dictionaryItem.Key).Value);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithNullRemoteValueKey()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "null"
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<NullRemoteValue>(dictionaryItem.Key);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithUndefinedRemoteValueKey()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "undefined"
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<UndefinedRemoteValue>(dictionaryItem.Key);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithDateRemoteValueKey()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "date",
                              "value": "2020-07-19T23:47:26.056Z"
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<DateRemoteValue>(dictionaryItem.Key);
        DateRemoteValue dateRemoteValue = (DateRemoteValue)dictionaryItem.Key;
        Assert.Equal(new DateTime(2020, 07, 19, 23, 47, 26, 56, DateTimeKind.Utc), dateRemoteValue.Value);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithComplexRemoteValueKeyContainingHandle()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "symbol",
                              "handle": "myHandle"
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<ObjectReferenceRemoteValue>(dictionaryItem.Key);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)dictionaryItem.Key;
        Assert.Equal("myHandle", objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithComplexRemoteValueKeyContainingInternalId()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "symbol",
                              "internalId": "123"
                            },
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Map, remoteValue.Type);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Single(dictionaryValue);
        KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
        Assert.IsType<ObjectReferenceRemoteValue>(dictionaryItem.Key);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)dictionaryItem.Key;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Equal("123", objectReferenceRemoteValue.InternalId);
        Assert.IsType<StringRemoteValue>(dictionaryItem.Value);
        StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
        Assert.Equal(RemoteValueType.String, stringRemoteValue.Type);
        Assert.Equal("stringValue", stringRemoteValue.Value);
    }

    [Fact]
    public void TestConvertingMapRemoteValueToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "map",
                        "handle": "myHandle",
                        "value": [
                          [
                            "stringProperty",
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ],
                          [
                            "numberProperty",
                            {
                              "type": "number",
                              "value": 123
                            }
                          ],
                          [
                            "booleanProperty",
                            {
                              "type": "boolean",
                              "value": true
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<KeyValuePairCollectionRemoteValue>(remoteValue);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        RemoteObjectReference remoteObjectReference = keyValuePairRemoteValue.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
    }

    [Fact]
    public void TestDeserializingInvalidMapValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidMapElementValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": {
                          "stringProperty": "stringValue",
                          "numberProperty": 123,
                          "booleanProperty": true
                        }
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithoutArrayElementsThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          {
                            "stringProperty": "stringValue"
                          }
                        ]
                      }
                      """;
        Assert.Contains("RemoteValue array element for dictionary must be an array", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithInvalidArrayElementSizeThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            "stringProperty",
                            "stringValue",
                            "someOtherValue"
                          ]
                        ]
                      }
                      """;
        Assert.Contains("RemoteValue array element for dictionary must be an array", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
        json = """
               {
                 "type": "map",
                 "value": [
                   [
                     "stringProperty"
                   ]
                 ]
               }
               """;
        Assert.Contains("RemoteValue array element for dictionary must be an array", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithInvalidKeyTypeThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            123,
                            "stringValue"
                          ] 
                        ]
                      }
                      """;
        Assert.Contains("must have a first element (key) that is either a string or an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingMapRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            "stringValue",
                            "stringValue"
                          ] 
                        ]
                      }
                      """;
        Assert.Contains("must have a second element (value) that is an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestConvertingMapRemoteValueToRemoteObjectReferenceWithMissingHandleThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            "stringProperty",
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ],
                          [
                            "numberProperty",
                            {
                              "type": "number",
                              "value": 123
                            }
                          ],
                          [
                            "booleanProperty",
                            {
                              "type": "boolean",
                              "value": true
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<KeyValuePairCollectionRemoteValue>(remoteValue);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Contains("must have a valid handle", Assert.ThrowsAny<WebDriverBiDiException>(() => keyValuePairRemoteValue.ToRemoteObjectReference()).Message);
    }

    [Fact]
    public void TestDeserializingObjectRemoteValue()
    {
        string json = """
                      {
                        "type": "object",
                        "value": [
                          [
                            "stringProperty",
                            {
                              "type": "string",
                              "value": "stringValue"
                            }
                          ],
                          [
                            "numberProperty",
                            {
                              "type": "number",
                              "value": 123
                            }
                          ],
                          [
                            "booleanProperty",
                            {
                              "type": "boolean",
                              "value": true
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Object, remoteValue.Type);
        Assert.IsType<KeyValuePairCollectionRemoteValue>(remoteValue);
        KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
        Assert.Null(keyValuePairRemoteValue.Handle);
        Assert.Null(keyValuePairRemoteValue.InternalId);
        Assert.NotNull(keyValuePairRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
        Assert.Equal(3, dictionaryValue.Count);
        Assert.True(dictionaryValue.ContainsKey("stringProperty"));
        Assert.IsType<StringRemoteValue>(dictionaryValue["stringProperty"]);
        Assert.Equal("stringValue", ((StringRemoteValue)dictionaryValue["stringProperty"]).Value);
        Assert.True(dictionaryValue.ContainsKey("numberProperty"));
        Assert.IsType<NumberRemoteValue>(dictionaryValue["numberProperty"]);
        Assert.Equal(123, ((NumberRemoteValue)dictionaryValue["numberProperty"]).Value);
        Assert.True(dictionaryValue.ContainsKey("booleanProperty"));
        Assert.IsType<BooleanRemoteValue>(dictionaryValue["booleanProperty"]);
        Assert.True(((BooleanRemoteValue)dictionaryValue["booleanProperty"]).Value);
    }

    [Fact]
    public void TestDeserializingInvalidObjectValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "object",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingInvalidObjectElementValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "object",
                        "value": {
                          "stringProperty": "stringValue",
                          "numberProperty": 123,
                          "booleanProperty": true
                        }
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingSymbolRemoteValue()
    {
        string json = """
                      {
                        "type": "symbol"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Symbol, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingFunctionRemoteValue()
    {
        string json = """
                      {
                        "type": "function"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Function, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingWeakMapRemoteValue()
    {
        string json = """
                      {
                        "type": "weakmap"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.WeakMap, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingWeakSetRemoteValue()
    {
        string json = """
                      {
                        "type": "weakset"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.WeakSet, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingGeneratorRemoteValue()
    {
        string json = """
                      {
                        "type": "generator"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Generator, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingErrorRemoteValue()
    {
        string json = """
                      {
                        "type": "error"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Error, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingProxyRemoteValue()
    {
        string json = """
                      {
                        "type": "proxy"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Proxy, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingPromiseRemoteValue()
    {
        string json = """
                      {
                        "type": "promise"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Promise, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingTypedArrayRemoteValue()
    {
        string json = """
                      {
                        "type": "typedarray"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.TypedArray, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingArrayBufferRemoteValue()
    {
        string json = """
                      {
                        "type": "arraybuffer"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.ArrayBuffer, remoteValue.Type);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        Assert.Null(objectReferenceRemoteValue.Handle);
        Assert.Null(objectReferenceRemoteValue.InternalId);
    }

    [Fact]
    public void TestDeserializingWindowRemoteValue()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContext"
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Window, remoteValue.Type);
        Assert.IsType<WindowProxyRemoteValue>(remoteValue);
        WindowProxyRemoteValue windowProxyRemoteValue = (WindowProxyRemoteValue)remoteValue;
        Assert.Null(windowProxyRemoteValue.Handle);
        Assert.Null(windowProxyRemoteValue.InternalId);
        Assert.Equal("myContext", windowProxyRemoteValue.Value.Context);
    }

    [Fact]
    public void TestDeserializingWindowRemoteValueWithHandleAndInternalId()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContext"
                        },
                        "handle": "myHandle",
                        "internalId": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal(RemoteValueType.Window, remoteValue.Type);
        Assert.IsType<WindowProxyRemoteValue>(remoteValue);
        WindowProxyRemoteValue windowProxyRemoteValue = (WindowProxyRemoteValue)remoteValue;
        Assert.NotNull(windowProxyRemoteValue.Handle);
        Assert.Equal("myHandle", windowProxyRemoteValue.Handle);
        Assert.NotNull(windowProxyRemoteValue.InternalId);
        Assert.Equal("123", windowProxyRemoteValue.InternalId);
        Assert.Equal("myContext", windowProxyRemoteValue.Value.Context);
    }

    [Fact]
    public void TestDeserializingInvalidWindowValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": "some value"
                      }
                      """;
        Assert.Contains("JSON value could not be converted", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingWindowRemoteValueWithInvalidHandleTypeThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContext"
                        },
                        "handle": {},
                        "internalId": 123
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingWindowRemoteValueWithInvalidInternalIdTypeThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContext"
                        },
                        "handle": "myHandle",
                        "internalId": 123.45
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingRemoteValueWithMissingTypeThrows()
    {
        string json = """
                      {
                        "value": "myValue"
                      }
                      """;
        Assert.Contains("must contain a 'type' property", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRemoteValueWithInvalidTypeThrows()
    {
        string json = """
                      {
                        "type": 3,
                        "value": "myValue"
                      }
                      """;
        Assert.Contains("'type' property must be a string", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "invalid",
                        "value": "myValue"
                      }
                      """;
        Assert.Contains("value 'invalid' is not valid for enum type RemoteValueType", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestDeserializingRemoteValueWithEmptyStringTypeThrows()
    {
        string json = """
                      {
                        "type": "",
                        "value": "myValue"
                      }
                      """;
        Assert.Contains("value '' is not valid for enum type RemoteValueType", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestValueAsWithIncorrectType()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 1
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);

        Assert.Equal($"RemoteValue of type '{remoteValue.Type}' cannot be converted to type 'StringRemoteValue'", Assert.ThrowsAny<WebDriverBiDiException>(() => remoteValue.ConvertTo<StringRemoteValue>().Value).Message);
    }

    [Fact]
    public void TestNullRemoteValueAsValueType()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NullRemoteValue>(remoteValue);
    }

    [Fact]
    public void TestDeserializingNonObjectThrows()
    {
        string json = @"[ ""invalid remote value"" ]";
        Assert.Contains("JSON for 'RemoteValue' must be an object", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<RemoteValue>(json)).Message);
    }

    [Fact]
    public void TestConvertToRemoteReference()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle",
                        "internalId": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        RemoteReference reference = objectReferenceRemoteValue.ToRemoteObjectReference();
        Assert.IsType<RemoteObjectReference>(reference);
    }

    [Fact]
    public void TestConvertToRemoteReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "symbol",
                        "internalId": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<ObjectReferenceRemoteValue>(remoteValue);
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;

        Assert.Contains("must have a valid handle", Assert.ThrowsAny<WebDriverBiDiException>(() => objectReferenceRemoteValue.ToRemoteObjectReference()).Message);
    }

    [Fact]
    public void TestConvertNodeRemoteValueToRemoteReference()
    {
        string json = """
                      {
                        "type": "node",
                        "sharedId": "mySharedId",
                        "handle": "myHandle",
                        "value": {
                          "nodeType": 1,
                          "nodeValue": "",
                          "childNodeCount": 0
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NodeRemoteValue>(remoteValue);
        NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)remoteValue;
        RemoteObjectReference reference = nodeRemoteValue.ToRemoteObjectReference();
        Assert.Equal("myHandle", reference.Handle);
    }

    [Fact]
    public void TestConvertNodeRemoteValueWithoutHandleToRemoteReferenceThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "nodeValue": "",
                          "childNodeCount": 0
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NodeRemoteValue>(remoteValue);
        NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)remoteValue;

        Assert.Contains("must have a valid handle", Assert.ThrowsAny<WebDriverBiDiException>(() => nodeRemoteValue.ToRemoteObjectReference()).Message);
    }

    [Fact]
    public void TestConvertNodeRemoteValueToSharedReference()
    {
        string json = """
                      {
                        "type": "node",
                        "sharedId": "mySharedId",
                        "value": {
                          "nodeType": 1,
                          "nodeValue": "",
                          "childNodeCount": 0
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NodeRemoteValue>(remoteValue);
        SharedReference reference = ((NodeRemoteValue)remoteValue).ToSharedReference();
        Assert.IsType<SharedReference>(reference);
    }

    [Fact]
    public void TestConvertNodeRemoteValueToSharedReferenceWithoutSharedIdThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "nodeValue": "",
                          "childNodeCount": 0
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NodeRemoteValue>(remoteValue);
        NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)remoteValue;
        Assert.Contains("must have a valid shared ID", Assert.ThrowsAny<WebDriverBiDiException>(() => nodeRemoteValue.ToSharedReference()).Message);
    }

    [Fact]
    public void TestConvertNonNodeRemoteValueToSharedReferenceThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "handle": "myHandle",
                        "internalId": "123",
                        "value": {
                          "context": "myContext"
                        }
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.False(remoteValue.TryConvertTo(out NodeRemoteValue? _));
    }
}
