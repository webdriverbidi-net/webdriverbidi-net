namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json;

public class ITypeSafeRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeStringRemoteValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.IsType<StringRemoteValue>(remoteValue);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        Assert.IsType<ValueHoldingRemoteValue<string>>(stringValue, exactMatch: false);
        Assert.Equal("myValue", stringValue.Value);
        Assert.Equal("myValue", stringValue.ValueObject);
    }

    [Fact]
    public void TestCanConvertStringRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.IsType<StringRemoteValue>(remoteValue);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        LocalValue localValue = stringValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue, exactMatch: false);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("string", argumentValue.Type);
        Assert.Equal("myValue", argumentValue.Value);
    }

    [Fact]
    public void TestCanDeserializeDoubleRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? doubleValue);
        Assert.True(conversionResult);
        Assert.NotNull(doubleValue);
        Assert.IsType<ValueHoldingRemoteValue<double>>(doubleValue, exactMatch: false);
        Assert.Equal(3.14, doubleValue.Value);
    }

    [Fact]
    public void TestStringRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<StringRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        StringRemoteValue copy = stringValue with { };
        Assert.Equal(stringValue, copy);
    }

    [Fact]
    public void TestCanConvertDoubleRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? doubleValue);
        Assert.True(conversionResult);
        Assert.NotNull(doubleValue);
        LocalValue localValue = doubleValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("number", argumentValue.Type);
        Assert.Equal(3.14, argumentValue.Value);
    }

    [Fact]
    public void TestDoubleRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? doubleValue);
        Assert.True(conversionResult);
        Assert.NotNull(doubleValue);
        NumberRemoteValue copy = doubleValue with { };
        Assert.Equal(doubleValue, copy);
    }

    [Fact]
    public void TestCanDeserializeLongRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 123
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.True(conversionResult);
        Assert.NotNull(longValue);
        Assert.Equal(123, longValue.Value);
    }

    [Fact]
    public void TestCanConvertLongRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 123
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.True(conversionResult);
        Assert.NotNull(longValue);
        LocalValue localValue = longValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("number", argumentValue.Type);
        Assert.Equal(123.0, argumentValue.Value);
    }

    [Fact]
    public void TestLongRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 123
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<NumberRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.True(conversionResult);
        Assert.NotNull(longValue);
        NumberRemoteValue copy = longValue with { };
        Assert.Equal(longValue, copy);
    }

    [Fact]
    public void TestCanDeserializeBooleanRemoteValue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<BooleanRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.True(conversionResult);
        Assert.NotNull(booleanValue);
        Assert.True(booleanValue.Value);
    }

    [Fact]
    public void TestCanConvertBooleanRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<BooleanRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.True(conversionResult);
        Assert.NotNull(booleanValue);
        LocalValue localValue = booleanValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("boolean", argumentValue.Type);
        Assert.Equal(true, argumentValue.Value);
    }

    [Fact]
    public void TestBooleanRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<BooleanRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.True(conversionResult);
        Assert.NotNull(booleanValue);
        BooleanRemoteValue copy = booleanValue with { };
        Assert.Equal(booleanValue, copy);
    }

    [Fact]
    public void TestCanDeserializeBigIntRemoteValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<BigIntegerRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out BigIntegerRemoteValue? bigIntegerValue);
        Assert.True(conversionResult);
        Assert.NotNull(bigIntegerValue);
        Assert.Equal(new BigInteger(123), bigIntegerValue.Value);
    }

    [Fact]
    public void TestCanConvertBigIntRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<BigIntegerRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out BigIntegerRemoteValue? bigIntegerValue);
        Assert.True(conversionResult);
        Assert.NotNull(bigIntegerValue);
        LocalValue localValue = bigIntegerValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("bigint", argumentValue.Type);
        Assert.Equal(new BigInteger(123), argumentValue.Value);
    }

    [Fact]
    public void TestBigIntRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<BigIntegerRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out BigIntegerRemoteValue? bigIntegerValue);
        Assert.True(conversionResult);
        Assert.NotNull(bigIntegerValue);
        BigIntegerRemoteValue copy = bigIntegerValue with { };
        Assert.Equal(bigIntegerValue, copy);
    }

    [Fact]
    public void TestCanDeserializeDateRemoteValue()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:26.056Z"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<DateRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out DateRemoteValue? dateValue);
        Assert.True(conversionResult);
        Assert.NotNull(dateValue);
        Assert.Equal(DateTime.Parse("2020-07-19T23:47:26.056Z").ToUniversalTime(), dateValue.Value);
    }

    [Fact]
    public void TestCanConvertDateRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:26.056Z"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<DateRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out DateRemoteValue? dateValue);
        Assert.True(conversionResult);
        Assert.NotNull(dateValue);
        LocalValue localValue = dateValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("date", argumentValue.Type);
        Assert.Equal(DateTime.Parse("2020-07-19T23:47:26.056Z").ToUniversalTime(), argumentValue.Value);
    }

    [Fact]
    public void TestDateRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:26.056Z"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        Assert.IsType<DateRemoteValue>(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out DateRemoteValue? dateValue);
        Assert.True(conversionResult);
        Assert.NotNull(dateValue);
        DateRemoteValue copy = dateValue with { };
        Assert.Equal(dateValue, copy);
    }

    [Fact]
    public void TestCanDeserializeRegularExpressionRemoteValue()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern", "gi");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.NotNull(primitiveValue.Value);
        RegularExpressionValue expectedRegexValue = (RegularExpressionValue)primitiveValue.Value;

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
        bool conversionResult = remoteValue.TryConvertTo(out RegExpRemoteValue? regexRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(regexRemoteValue);
        Assert.Equal(expectedRegexValue, regexRemoteValue.Value);
    }

    [Fact]
    public void TestCanConvertRegularExpressionRemoteValueToLocalValue()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern", "gi");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.NotNull(primitiveValue.Value);
        RegularExpressionValue expectedRegexValue = (RegularExpressionValue)primitiveValue.Value;

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
        bool conversionResult = remoteValue.TryConvertTo(out RegExpRemoteValue? regexRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(regexRemoteValue);
        LocalValue localValue = regexRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("regexp", argumentValue.Type);
        Assert.Equal(expectedRegexValue, argumentValue.Value);
    }

    [Fact]
    public void TestRegularExpressionRemoteValueCopySemantics()
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
        bool conversionResult = remoteValue.TryConvertTo(out RegExpRemoteValue? regexRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(regexRemoteValue);
        RegExpRemoteValue copy = regexRemoteValue with { };
        Assert.Equal(regexRemoteValue, copy);
    }

    [Fact]
    public void TestCanDeserializeNodeRemoteValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out NodeRemoteValue? nodeRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(nodeRemoteValue);
        Assert.NotNull(nodeRemoteValue.Value);
        NodeProperties nodeProperties = nodeRemoteValue.Value;
        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(string.Empty, nodeProperties.NodeValue);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
    }

    [Fact]
    public void TestCanConvertNodeRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "nodeValue": "",
                          "childNodeCount": 0
                        },
                        "sharedId": "mySharedId"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NodeRemoteValue? nodeRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(nodeRemoteValue);
        LocalValue localValue = nodeRemoteValue.ToLocalValue();
        Assert.IsType<SharedReference>(localValue);
        SharedReference sharedReference = (SharedReference)localValue;
        Assert.Equal("mySharedId", sharedReference.SharedId);
    }

    [Fact]
    public void TestNodeRemoteValueCopySemantics()
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
        bool conversionResult = remoteValue.TryConvertTo(out NodeRemoteValue? nodeRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(nodeRemoteValue);
        NodeRemoteValue copy = nodeRemoteValue with { };
        Assert.Equal(nodeRemoteValue, copy);
    }

    [Fact]
    public void TestCanDeserializeArrayRemoteValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(listRemoteValue);
        Assert.NotNull(listRemoteValue.Value);
        RemoteValueList arrayValue = listRemoteValue.Value;
        Assert.NotNull(arrayValue);
        Assert.Equal(3, arrayValue.Count);

        conversionResult = arrayValue[0].TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        Assert.Equal("stringValue", stringValue.Value);

        conversionResult = arrayValue[1].TryConvertTo(out NumberRemoteValue? longValue);
        Assert.True(conversionResult);
        Assert.NotNull(longValue);
        Assert.Equal(123, longValue.Value);

        conversionResult = arrayValue[2].TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.True(conversionResult);
        Assert.NotNull(booleanValue);
        Assert.True(booleanValue.Value);
    }

    [Fact]
    public void TestCanConvertArrayRemoteValueToLocalValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(listRemoteValue);
        LocalValue localValue = listRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("array", argumentValue.Type);
        Assert.IsType<List<LocalValue>>(argumentValue.Value);
        Assert.NotNull(argumentValue.Value);
        List<LocalValue> localValueList = (List<LocalValue>)argumentValue.Value;
        Assert.Equal(3, localValueList.Count);
    }

    [Fact]
    public void TestCanDeserializeSetRemoteValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(listRemoteValue);
        Assert.NotNull(listRemoteValue.Value);
        RemoteValueList arrayValue = listRemoteValue.Value;
        Assert.NotNull(arrayValue);
        Assert.Equal(3, arrayValue.Count);

        conversionResult = arrayValue[0].TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        Assert.Equal("stringValue", stringValue.Value);

        conversionResult = arrayValue[1].TryConvertTo(out NumberRemoteValue? longValue);
        Assert.True(conversionResult);
        Assert.NotNull(longValue);
        Assert.Equal(123, longValue.Value);

        conversionResult = arrayValue[2].TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.True(conversionResult);
        Assert.NotNull(booleanValue);
        Assert.True(booleanValue.Value);
    }

    [Fact]
    public void TestCanConvertSetRemoteValueToLocalValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(listRemoteValue);
        LocalValue localValue = listRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("set", argumentValue.Type);
        Assert.IsType<List<LocalValue>>(argumentValue.Value);
        Assert.NotNull(argumentValue.Value);
        List<LocalValue> localValueList = (List<LocalValue>)argumentValue.Value;
        Assert.Equal(3, localValueList.Count);
    }

    [Fact]
    public void TestListRemoteValueCopySemantics()
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
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(listRemoteValue);
        CollectionRemoteValue copy = listRemoteValue with { };
        Assert.Equal(listRemoteValue, copy);
    }

    [Fact]
    public void TestCanDeserializeMapRemoteValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        Assert.NotNull(mapRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;

        RemoteValue stringPropertyValue = dictionaryValue["stringProperty"];
        conversionResult = stringPropertyValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        Assert.Equal("stringValue", stringValue.Value);

        RemoteValue longPropertyValue = dictionaryValue["numberProperty"];
        conversionResult = longPropertyValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.True(conversionResult);
        Assert.NotNull(longValue);
        Assert.Equal(123, longValue.Value);

        RemoteValue booleanPropertyValue = dictionaryValue["booleanProperty"];
        conversionResult = booleanPropertyValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.True(conversionResult);
        Assert.NotNull(booleanValue);
        Assert.True(booleanValue.Value);
    }

    [Fact]
    public void TestCanDeserializeMapRemoteValueWithObjectKeys()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "symbol",
                              "handle": "mySymbol"
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        Assert.NotNull(mapRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;
        Assert.Single(dictionaryValue);
        object keyValueObject = dictionaryValue.ElementAt(0).Key;
        Assert.IsType<ObjectReferenceRemoteValue>(keyValueObject);
        ObjectReferenceRemoteValue key = (ObjectReferenceRemoteValue)keyValueObject;
        Assert.Equal(RemoteValueType.Symbol, key.Type);
        Assert.Equal("mySymbol", key.Handle);
        RemoteValue value = dictionaryValue[key];
        Assert.IsType<StringRemoteValue>(value);
        StringRemoteValue stringValue = (StringRemoteValue)value;
        Assert.Equal("stringValue", stringValue.Value);
    }

    [Fact]
    public void TestCanDeserializeMapRemoteValueWithMixedKeys()
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
                            {
                              "type": "symbol",
                              "handle": "mySymbol"
                            },
                            {
                              "type": "string",
                              "value": "symbolValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        Assert.NotNull(mapRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;

        RemoteValue stringPropertyValue = dictionaryValue["stringProperty"];
        conversionResult = stringPropertyValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        Assert.Equal("stringValue", stringValue.Value);

        object keyValueObject = dictionaryValue.ElementAt(1).Key;
        Assert.IsType<ObjectReferenceRemoteValue>(keyValueObject);
        ObjectReferenceRemoteValue key = (ObjectReferenceRemoteValue)keyValueObject;
        Assert.Equal(RemoteValueType.Symbol, key.Type);
        Assert.Equal("mySymbol", key.Handle);
        RemoteValue value = dictionaryValue[key];
        Assert.IsType<StringRemoteValue>(value);
        StringRemoteValue symbolStringValue = (StringRemoteValue)value;
        Assert.Equal("symbolValue", symbolStringValue.Value);
    }

    [Fact]
    public void TestCanConvertMapRemoteValueToLocalValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("map", argumentValue.Type);
        Assert.NotNull(argumentValue.Value);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentValue.Value);
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.Equal(3, dictionaryValue.Count);
        Assert.True(dictionaryValue.ContainsKey("stringProperty"));
        Assert.IsType<LocalArgumentValue>(dictionaryValue["stringProperty"]);
        Assert.Equal("string", ((LocalArgumentValue)dictionaryValue["stringProperty"]).Type);
        Assert.Equal("stringValue", ((LocalArgumentValue)dictionaryValue["stringProperty"]).Value);
        Assert.True(dictionaryValue.ContainsKey("numberProperty"));
        Assert.Equal("number", ((LocalArgumentValue)dictionaryValue["numberProperty"]).Type);
        Assert.Equal(123.0, ((LocalArgumentValue)dictionaryValue["numberProperty"]).Value);
        Assert.True(dictionaryValue.ContainsKey("booleanProperty"));
        Assert.Equal("boolean", ((LocalArgumentValue)dictionaryValue["booleanProperty"]).Type);
        Assert.Equal(true, ((LocalArgumentValue)dictionaryValue["booleanProperty"]).Value);
    }

    [Fact]
    public void TestCanConvertMapRemoteValueWithObjectKeysToLocalValue()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "symbol",
                              "handle": "mySymbol"
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("map", argumentValue.Type);
        Assert.NotNull(argumentValue.Value);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentValue.Value);
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.Single(dictionaryValue);
        object key = dictionaryValue.ElementAt(0).Key;
        Assert.IsType<RemoteObjectReference>(key);
        RemoteObjectReference keyReference = (RemoteObjectReference)key;
        Assert.Equal("mySymbol", keyReference.Handle);
        LocalValue value = dictionaryValue[key];
        Assert.IsType<LocalArgumentValue>(value);
        LocalArgumentValue stringValue = (LocalArgumentValue)value;
        Assert.Equal("string", stringValue.Type);
        Assert.Equal("stringValue", stringValue.Value);
    }

    [Fact]
    public void TestCanConvertMapRemoteValueWithMixedKeysToLocalValue()
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
                            {
                              "type": "symbol",
                              "handle": "mySymbol"
                            },
                            {
                              "type": "string",
                              "value": "symbolValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("map", argumentValue.Type);
        Assert.NotNull(argumentValue.Value);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentValue.Value);
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.Equal(2, dictionaryValue.Count);
        object key1 = dictionaryValue.ElementAt(0).Key;
        Assert.IsType<string>(key1);
        Assert.Equal("stringProperty", (string)key1);
        LocalValue value1 = dictionaryValue[key1];
        Assert.IsType<LocalArgumentValue>(value1);
        LocalArgumentValue stringValue = (LocalArgumentValue)value1;
        Assert.Equal("string", stringValue.Type);
        Assert.Equal("stringValue", stringValue.Value);
        object key2 = dictionaryValue.ElementAt(1).Key;
        Assert.IsType<RemoteObjectReference>(key2);
        RemoteObjectReference keyReference = (RemoteObjectReference)key2;
        Assert.Equal("mySymbol", keyReference.Handle);
        LocalValue value2 = dictionaryValue[key2];
        Assert.IsType<LocalArgumentValue>(value2);
        LocalArgumentValue symbolValue = (LocalArgumentValue)value2;
        Assert.Equal("string", symbolValue.Type);
        Assert.Equal("symbolValue", symbolValue.Value);
    }

    [Fact]
    public void TestCanDeserializeObjectRemoteValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        Assert.NotNull(mapRemoteValue.Value);
        Assert.NotNull(mapRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;

        RemoteValue stringPropertyValue = dictionaryValue["stringProperty"];
        conversionResult = stringPropertyValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.True(conversionResult);
        Assert.NotNull(stringValue);
        Assert.Equal("stringValue", stringValue.Value);

        RemoteValue longPropertyValue = dictionaryValue["numberProperty"];
        conversionResult = longPropertyValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.True(conversionResult);
        Assert.NotNull(longValue);
        Assert.Equal(123, longValue.Value);

        RemoteValue booleanPropertyValue = dictionaryValue["booleanProperty"];
        conversionResult = booleanPropertyValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.True(conversionResult);
        Assert.NotNull(booleanValue);
        Assert.True(booleanValue.Value);
    }

    [Fact]
    public void TestCanDeserializeObjectRemoteValueWithObjectKeys()
    {
        string json = """
                      {
                        "type": "object",
                        "value": [
                          [
                            {
                              "type": "symbol",
                              "handle": "mySymbol"
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        Assert.NotNull(mapRemoteValue.Value);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;
        Assert.Single(dictionaryValue);
        object keyValueObject = dictionaryValue.ElementAt(0).Key;
        Assert.IsType<ObjectReferenceRemoteValue>(keyValueObject);
        ObjectReferenceRemoteValue key = (ObjectReferenceRemoteValue)keyValueObject;
        Assert.Equal(RemoteValueType.Symbol, key.Type);
        Assert.Equal("mySymbol", key.Handle);
        RemoteValue value = dictionaryValue[key];
        Assert.IsType<StringRemoteValue>(value);
        StringRemoteValue stringValue = (StringRemoteValue)value;
        Assert.Equal("stringValue", stringValue.Value);
    }

    [Fact]
    public void TestCanConvertObjectRemoteValueToLocalValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("object", argumentValue.Type);
        Assert.NotNull(argumentValue.Value);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentValue.Value);
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.Equal(3, dictionaryValue.Count);
        Assert.True(dictionaryValue.ContainsKey("stringProperty"));
        Assert.IsType<LocalArgumentValue>(dictionaryValue["stringProperty"]);
        Assert.Equal("string", ((LocalArgumentValue)dictionaryValue["stringProperty"]).Type);
        Assert.Equal("stringValue", ((LocalArgumentValue)dictionaryValue["stringProperty"]).Value);
        Assert.True(dictionaryValue.ContainsKey("numberProperty"));
        Assert.Equal("number", ((LocalArgumentValue)dictionaryValue["numberProperty"]).Type);
        Assert.Equal(123.0, ((LocalArgumentValue)dictionaryValue["numberProperty"]).Value);
        Assert.True(dictionaryValue.ContainsKey("booleanProperty"));
        Assert.Equal("boolean", ((LocalArgumentValue)dictionaryValue["booleanProperty"]).Type);
        Assert.Equal(true, ((LocalArgumentValue)dictionaryValue["booleanProperty"]).Value);
    }

    [Fact]
    public void TestCanConvertObjectRemoteValueWithObjectKeysToLocalValue()
    {
        string json = """
                      {
                        "type": "object",
                        "value": [
                          [
                            {
                              "type": "symbol",
                              "handle": "mySymbol"
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("object", argumentValue.Type);
        Assert.NotNull(argumentValue.Value);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentValue.Value);
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.Single(dictionaryValue);
        object key = dictionaryValue.ElementAt(0).Key;
        Assert.IsType<RemoteObjectReference>(key);
        RemoteObjectReference keyReference = (RemoteObjectReference)key;
        Assert.Equal("mySymbol", keyReference.Handle);
        LocalValue value = dictionaryValue[key];
        Assert.IsType<LocalArgumentValue>(value);
        LocalArgumentValue stringValue = (LocalArgumentValue)value;
        Assert.Equal("string", stringValue.Type);
        Assert.Equal("stringValue", stringValue.Value);
    }

    [Fact]
    public void TestCanConvertObjectRemoteValueWithMixedKeysToLocalValue()
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
                            {
                              "type": "symbol",
                              "handle": "mySymbol"
                            },
                            {
                              "type": "string",
                              "value": "symbolValue"
                            }
                          ]
                        ]
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("object", argumentValue.Type);
        Assert.NotNull(argumentValue.Value);
        Assert.IsType<Dictionary<object, LocalValue>>(argumentValue.Value);
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.Equal(2, dictionaryValue.Count);
        object key1 = dictionaryValue.ElementAt(0).Key;
        Assert.IsType<string>(key1);
        Assert.Equal("stringProperty", (string)key1);
        LocalValue value1 = dictionaryValue[key1];
        Assert.IsType<LocalArgumentValue>(value1);
        LocalArgumentValue stringValue = (LocalArgumentValue)value1;
        Assert.Equal("string", stringValue.Type);
        Assert.Equal("stringValue", stringValue.Value);
        object key2 = dictionaryValue.ElementAt(1).Key;
        Assert.IsType<RemoteObjectReference>(key2);
        RemoteObjectReference keyReference = (RemoteObjectReference)key2;
        Assert.Equal("mySymbol", keyReference.Handle);
        LocalValue value2 = dictionaryValue[key2];
        Assert.IsType<LocalArgumentValue>(value2);
        LocalArgumentValue symbolValue = (LocalArgumentValue)value2;
        Assert.Equal("string", symbolValue.Type);
        Assert.Equal("symbolValue", symbolValue.Value);
    }

    [Fact]
    public void TestKeyValuePairRemoteValueCopySemantics()
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
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.True(conversionResult);
        Assert.NotNull(mapRemoteValue);
        KeyValuePairCollectionRemoteValue copy = mapRemoteValue with { };
        Assert.Equal(mapRemoteValue, copy);
    }

    [Fact]
    public void TestCanDeserializeWindowProxyRemoteValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out WindowProxyRemoteValue? windowProxyValue);
        Assert.True(conversionResult);
        Assert.NotNull(windowProxyValue);
        Assert.Equal("myContext", windowProxyValue.Value.Context);
        Assert.Equal("myHandle", windowProxyValue.Handle);
        Assert.Equal("123", windowProxyValue.InternalId);
    }

    [Fact]
    public void TestCanConvertWindowProxyRemoteValueToLocalValue()
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
        bool conversionResult = remoteValue.TryConvertTo(out WindowProxyRemoteValue? windowProxyValue);
        Assert.True(conversionResult);
        Assert.NotNull(windowProxyValue);
        LocalValue localValue = windowProxyValue.ToLocalValue();
        Assert.IsType<RemoteObjectReference>(localValue);
        RemoteObjectReference argumentValue = (RemoteObjectReference)localValue;
        Assert.Equal("myHandle", argumentValue.Handle);
    }

    [Fact]
    public void TestWindowProxyRemoteValueCopySemantics()
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
        bool conversionResult = remoteValue.TryConvertTo(out WindowProxyRemoteValue? windowProxyValue);
        Assert.True(conversionResult);
        Assert.NotNull(windowProxyValue);
        WindowProxyRemoteValue copy = windowProxyValue with { };
        Assert.Equal(windowProxyValue, copy);
    }

    [Fact]
    public void TestCanDeserializeNullRemoteValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NullRemoteValue? nullValue);
        Assert.True(conversionResult);
        Assert.NotNull(nullValue);
    }

    [Fact]
    public void TestCanConvertNullRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NullRemoteValue? nullValue);
        Assert.True(conversionResult);
        Assert.NotNull(nullValue);
        LocalValue localValue = nullValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("null", argumentValue.Type);
    }

    [Fact]
    public void TestNullRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out NullRemoteValue? nullValue);
        Assert.True(conversionResult);
        Assert.NotNull(nullValue);
        NullRemoteValue copy = nullValue with { };
        Assert.Equal(nullValue, copy);
    }

    [Fact]
    public void TestCanDeserializeUndefinedRemoteValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out UndefinedRemoteValue? undefinedValue);
        Assert.True(conversionResult);
        Assert.NotNull(undefinedValue);
    }

    [Fact]
    public void TestCanConvertUndefinedRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out UndefinedRemoteValue? undefinedValue);
        Assert.True(conversionResult);
        Assert.NotNull(undefinedValue);
        LocalValue localValue = undefinedValue.ToLocalValue();
        Assert.IsType<LocalArgumentValue>(localValue);
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.Equal("undefined", argumentValue.Type);
    }

    [Fact]
    public void TestUndefinedRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.NotNull(remoteValue);
        bool conversionResult = remoteValue.TryConvertTo(out UndefinedRemoteValue? undefinedValue);
        Assert.True(conversionResult);
        Assert.NotNull(undefinedValue);
        UndefinedRemoteValue copy = undefinedValue with { };
        Assert.Equal(undefinedValue, copy);
    }
}
