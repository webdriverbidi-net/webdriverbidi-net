namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json;

[TestFixture]
public class ITypeSafeRemoteValueTests
{
    [Test]
    public void TestCanDeserializeStringRemoteValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<StringRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue, Is.InstanceOf<ValueHoldingRemoteValue<string>>());
        Assert.That(stringValue.Value, Is.EqualTo("myValue"));
        Assert.That(stringValue.ValueObject, Is.EqualTo("myValue"));
    }

    [Test]
    public void TestCanConvertStringRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<StringRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(stringValue, Is.Not.Null);
        LocalValue localValue = stringValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("string"));
        Assert.That(argumentValue.Value, Is.EqualTo("myValue"));
    }

    [Test]
    public void TestCanDeserializeDoubleRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<NumberRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? doubleValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(doubleValue, Is.Not.Null);
        Assert.That(doubleValue, Is.InstanceOf<ValueHoldingRemoteValue<double>>());
        Assert.That(doubleValue.Value, Is.EqualTo(3.14));
    }

    [Test]
    public void TestStringRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<StringRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(stringValue, Is.Not.Null);
        StringRemoteValue copy = stringValue with { };
        Assert.That(copy, Is.EqualTo(stringValue));
    }

    [Test]
    public void TestCanConvertDoubleRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<NumberRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? doubleValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(doubleValue, Is.Not.Null);
        LocalValue localValue = doubleValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("number"));
        Assert.That(argumentValue.Value, Is.EqualTo(3.14));
    }

    [Test]
    public void TestDoubleRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<NumberRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? doubleValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(doubleValue, Is.Not.Null);
        NumberRemoteValue copy = doubleValue with { };
        Assert.That(copy, Is.EqualTo(doubleValue));
    }

    [Test]
    public void TestCanDeserializeLongRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 123
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<NumberRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(longValue, Is.Not.Null);
        Assert.That(longValue.Value, Is.EqualTo(123));
    }

    [Test]
    public void TestCanConvertLongRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 123
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<NumberRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(longValue, Is.Not.Null);
        LocalValue localValue = longValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("number"));
        Assert.That(argumentValue.Value, Is.EqualTo(123));
    }

    [Test]
    public void TestLongRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 123
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<NumberRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(longValue, Is.Not.Null);
        NumberRemoteValue copy = longValue with { };
        Assert.That(copy, Is.EqualTo(longValue));
    }

    [Test]
    public void TestCanDeserializeBooleanRemoteValue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<BooleanRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(booleanValue, Is.Not.Null);
        Assert.That(booleanValue.Value, Is.True);
    }

    [Test]
    public void TestCanConvertBooleanRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<BooleanRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(booleanValue, Is.Not.Null);
        LocalValue localValue = booleanValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("boolean"));
        Assert.That(argumentValue.Value, Is.EqualTo(true));
    }

    [Test]
    public void TestBooleanRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": true
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<BooleanRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(booleanValue, Is.Not.Null);
        BooleanRemoteValue copy = booleanValue with { };
        Assert.That(copy, Is.EqualTo(booleanValue));
    }

    [Test]
    public void TestCanDeserializeBigIntRemoteValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<BigIntegerRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out BigIntegerRemoteValue? bigIntegerValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(bigIntegerValue, Is.Not.Null);
        Assert.That(bigIntegerValue.Value, Is.EqualTo(new BigInteger(123)));
    }

    [Test]
    public void TestCanConvertBigIntRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<BigIntegerRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out BigIntegerRemoteValue? bigIntegerValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(bigIntegerValue, Is.Not.Null);
        LocalValue localValue = bigIntegerValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("bigint"));
        Assert.That(argumentValue.Value, Is.EqualTo(new BigInteger(123)));
    }

    [Test]
    public void TestBigIntRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<BigIntegerRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out BigIntegerRemoteValue? bigIntegerValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(bigIntegerValue, Is.Not.Null);
        BigIntegerRemoteValue copy = bigIntegerValue with { };
        Assert.That(copy, Is.EqualTo(bigIntegerValue));
    }

    [Test]
    public void TestCanDeserializeDateRemoteValue()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:26.056Z"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<DateRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out DateRemoteValue? dateValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(dateValue, Is.Not.Null);
        Assert.That(dateValue.Value, Is.EqualTo(DateTime.Parse("2020-07-19T23:47:26.056Z").ToUniversalTime()));
    }

    [Test]
    public void TestCanConvertDateRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:26.056Z"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<DateRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out DateRemoteValue? dateValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(dateValue, Is.Not.Null);
        LocalValue localValue = dateValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("date"));
        Assert.That(argumentValue.Value, Is.EqualTo(DateTime.Parse("2020-07-19T23:47:26.056Z").ToUniversalTime()));
    }

    [Test]
    public void TestDateRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:26.056Z"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.InstanceOf<DateRemoteValue>());
        bool conversionResult = remoteValue.TryConvertTo(out DateRemoteValue? dateValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(dateValue, Is.Not.Null);
        DateRemoteValue copy = dateValue with { };
        Assert.That(copy, Is.EqualTo(dateValue));
    }

    [Test]
    public void TestCanDeserializeRegularExpressionRemoteValue()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern", "gi");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.That(primitiveValue.Value, Is.Not.Null);
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out RegExpRemoteValue? regexRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(regexRemoteValue, Is.Not.Null);
        Assert.That(regexRemoteValue.Value, Is.EqualTo(expectedRegexValue));
    }

    [Test]
    public void TestCanConvertRegularExpressionRemoteValueToLocalValue()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern", "gi");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.That(primitiveValue.Value, Is.Not.Null);
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out RegExpRemoteValue? regexRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(regexRemoteValue, Is.Not.Null);
        LocalValue localValue = regexRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("regexp"));
        Assert.That(argumentValue.Value, Is.EqualTo(expectedRegexValue));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out RegExpRemoteValue? regexRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(regexRemoteValue, Is.Not.Null);
        RegExpRemoteValue copy = regexRemoteValue with { };
        Assert.That(copy, Is.EqualTo(regexRemoteValue));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out NodeRemoteValue? nodeRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(nodeRemoteValue, Is.Not.Null);
        NodeProperties nodeProperties = nodeRemoteValue.Value;
        Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out NodeRemoteValue? nodeRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(nodeRemoteValue, Is.Not.Null);
        LocalValue localValue = nodeRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<SharedReference>());
        SharedReference sharedReference = (SharedReference)localValue;
        Assert.That(sharedReference.SharedId, Is.EqualTo("mySharedId"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out NodeRemoteValue? nodeRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(nodeRemoteValue, Is.Not.Null);
        NodeRemoteValue copy = nodeRemoteValue with { };
        Assert.That(copy, Is.EqualTo(nodeRemoteValue));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(listRemoteValue, Is.Not.Null);
        RemoteValueList arrayValue = listRemoteValue.Value;
        Assert.That(arrayValue, Is.Not.Null);
        Assert.That(arrayValue, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            conversionResult = arrayValue[0].TryConvertTo(out StringRemoteValue? stringValue);
            Assert.That(conversionResult, Is.True);
            Assert.That(stringValue!.Value, Is.EqualTo("stringValue"));
            conversionResult = arrayValue[1].TryConvertTo(out NumberRemoteValue? longValue);
            Assert.That(conversionResult, Is.True);
            Assert.That(longValue!.Value, Is.EqualTo(123));
            conversionResult = arrayValue[2].TryConvertTo(out BooleanRemoteValue? booleanValue);
            Assert.That(conversionResult, Is.True);
            Assert.That(booleanValue!.Value, Is.EqualTo(true));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(listRemoteValue, Is.Not.Null);
        LocalValue localValue = listRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("array"));
        Assert.That(argumentValue.Value, Is.InstanceOf<List<LocalValue>>());
        List<LocalValue> localValueList = (List<LocalValue>)argumentValue.Value;
        Assert.That(localValueList, Has.Count.EqualTo(3));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(listRemoteValue, Is.Not.Null);
        RemoteValueList arrayValue = listRemoteValue.Value;
        Assert.That(arrayValue, Is.Not.Null);
        Assert.That(arrayValue, Has.Count.EqualTo(3));

        conversionResult = arrayValue[0].TryConvertTo(out StringRemoteValue? stringValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));

        conversionResult = arrayValue[1].TryConvertTo(out NumberRemoteValue? longValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(longValue, Is.Not.Null);
        Assert.That(longValue.Value, Is.EqualTo(123));

        conversionResult = arrayValue[2].TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(booleanValue, Is.Not.Null);
        Assert.That(booleanValue.Value, Is.EqualTo(true));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(listRemoteValue, Is.Not.Null);
        LocalValue localValue = listRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("set"));
        Assert.That(argumentValue.Value, Is.InstanceOf<List<LocalValue>>());
        List<LocalValue> localValueList = (List<LocalValue>)argumentValue.Value;
        Assert.That(localValueList, Has.Count.EqualTo(3));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out CollectionRemoteValue? listRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(listRemoteValue, Is.Not.Null);
        CollectionRemoteValue copy = listRemoteValue with { };
        Assert.That(copy, Is.EqualTo(listRemoteValue));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;

        RemoteValue stringPropertyValue = dictionaryValue["stringProperty"];
        conversionResult = stringPropertyValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));

        RemoteValue longPropertyValue = dictionaryValue["numberProperty"];
        conversionResult = longPropertyValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(longValue, Is.Not.Null);
        Assert.That(longValue.Value, Is.EqualTo(123));

        RemoteValue booleanPropertyValue = dictionaryValue["booleanProperty"];
        conversionResult = booleanPropertyValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(booleanValue, Is.Not.Null);
        Assert.That(booleanValue.Value, Is.EqualTo(true));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(1));
        object keyValueObject = dictionaryValue.ElementAt(0).Key;
        Assert.That(keyValueObject, Is.InstanceOf<ObjectReferenceRemoteValue>());
        ObjectReferenceRemoteValue key = (ObjectReferenceRemoteValue)keyValueObject;
        Assert.That(key.Type, Is.EqualTo(RemoteValueType.Symbol));
        Assert.That(key.Handle, Is.EqualTo("mySymbol"));
        RemoteValue value = dictionaryValue[key];
        Assert.That(value, Is.InstanceOf<StringRemoteValue>());
        StringRemoteValue stringValue = (StringRemoteValue)value;
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;

        RemoteValue stringPropertyValue = dictionaryValue["stringProperty"];
        conversionResult = stringPropertyValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));

        object keyValueObject = dictionaryValue.ElementAt(1).Key;
        Assert.That(keyValueObject, Is.InstanceOf<ObjectReferenceRemoteValue>());
        ObjectReferenceRemoteValue key = (ObjectReferenceRemoteValue)keyValueObject;
        Assert.That(key.Type, Is.EqualTo(RemoteValueType.Symbol));
        Assert.That(key.Handle, Is.EqualTo("mySymbol"));
        RemoteValue value = dictionaryValue[key];
        Assert.That(value, Is.InstanceOf<StringRemoteValue>());
        StringRemoteValue symbolStringValue = (StringRemoteValue)value;
        Assert.That(symbolStringValue.Value, Is.EqualTo("symbolValue"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("map"));
        Assert.That(argumentValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(3));
        Assert.That(dictionaryValue, Contains.Key("stringProperty"));
        Assert.That(dictionaryValue["stringProperty"], Is.InstanceOf<LocalArgumentValue>());
        Assert.That(((LocalArgumentValue)dictionaryValue["stringProperty"]).Type, Is.EqualTo("string"));
        Assert.That(((LocalArgumentValue)dictionaryValue["stringProperty"]).Value, Is.EqualTo("stringValue"));
        Assert.That(dictionaryValue, Contains.Key("numberProperty"));
        Assert.That(((LocalArgumentValue)dictionaryValue["numberProperty"]).Type, Is.EqualTo("number"));
        Assert.That(((LocalArgumentValue)dictionaryValue["numberProperty"]).Value, Is.EqualTo(123));
        Assert.That(dictionaryValue, Contains.Key("booleanProperty"));
        Assert.That(((LocalArgumentValue)dictionaryValue["booleanProperty"]).Type, Is.EqualTo("boolean"));
        Assert.That(((LocalArgumentValue)dictionaryValue["booleanProperty"]).Value, Is.EqualTo(true));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("map"));
        Assert.That(argumentValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(1));
        object key = dictionaryValue.ElementAt(0).Key;
        Assert.That(key, Is.InstanceOf<RemoteObjectReference>());
        RemoteObjectReference keyReference = (RemoteObjectReference)key;
        Assert.That(keyReference.Handle, Is.EqualTo("mySymbol"));
        LocalValue value = dictionaryValue[key];
        Assert.That(value, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue stringValue = (LocalArgumentValue)value;
        Assert.That(stringValue.Type, Is.EqualTo("string"));
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("map"));
        Assert.That(argumentValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(2));
        object key1 = dictionaryValue.ElementAt(0).Key;
        Assert.That(key1, Is.InstanceOf<string>());
        Assert.That((string)key1, Is.EqualTo("stringProperty"));
        LocalValue value1 = dictionaryValue[key1];
        Assert.That(value1, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue stringValue = (LocalArgumentValue)value1;
        Assert.That(stringValue.Type, Is.EqualTo("string"));
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));
        object key2 = dictionaryValue.ElementAt(1).Key;
        Assert.That(key2, Is.InstanceOf<RemoteObjectReference>());
        RemoteObjectReference keyReference = (RemoteObjectReference)key2;
        Assert.That(keyReference.Handle, Is.EqualTo("mySymbol"));
        LocalValue value2 = dictionaryValue[key2];
        Assert.That(value2, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue symbolValue = (LocalArgumentValue)value2;
        Assert.That(symbolValue.Type, Is.EqualTo("string"));
        Assert.That(symbolValue.Value, Is.EqualTo("symbolValue"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;

        RemoteValue stringPropertyValue = dictionaryValue["stringProperty"];
        conversionResult = stringPropertyValue.TryConvertTo(out StringRemoteValue? stringValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));

        RemoteValue longPropertyValue = dictionaryValue["numberProperty"];
        conversionResult = longPropertyValue.TryConvertTo(out NumberRemoteValue? longValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(longValue, Is.Not.Null);
        Assert.That(longValue.Value, Is.EqualTo(123));

        RemoteValue booleanPropertyValue = dictionaryValue["booleanProperty"];
        conversionResult = booleanPropertyValue.TryConvertTo(out BooleanRemoteValue? booleanValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(booleanValue, Is.Not.Null);
        Assert.That(booleanValue.Value, Is.EqualTo(true));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        RemoteValueDictionary dictionaryValue = mapRemoteValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(1));
        object keyValueObject = dictionaryValue.ElementAt(0).Key;
        Assert.That(keyValueObject, Is.InstanceOf<ObjectReferenceRemoteValue>());
        ObjectReferenceRemoteValue key = (ObjectReferenceRemoteValue)keyValueObject;
        Assert.That(key.Type, Is.EqualTo(RemoteValueType.Symbol));
        Assert.That(key.Handle, Is.EqualTo("mySymbol"));
        RemoteValue value = dictionaryValue[key];
        Assert.That(value, Is.InstanceOf<StringRemoteValue>());
        StringRemoteValue stringValue = (StringRemoteValue)value;
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("object"));
        Assert.That(argumentValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(3));
        Assert.That(dictionaryValue, Contains.Key("stringProperty"));
        Assert.That(dictionaryValue["stringProperty"], Is.InstanceOf<LocalArgumentValue>());
        Assert.That(((LocalArgumentValue)dictionaryValue["stringProperty"]).Type, Is.EqualTo("string"));
        Assert.That(((LocalArgumentValue)dictionaryValue["stringProperty"]).Value, Is.EqualTo("stringValue"));
        Assert.That(dictionaryValue, Contains.Key("numberProperty"));
        Assert.That(((LocalArgumentValue)dictionaryValue["numberProperty"]).Type, Is.EqualTo("number"));
        Assert.That(((LocalArgumentValue)dictionaryValue["numberProperty"]).Value, Is.EqualTo(123));
        Assert.That(dictionaryValue, Contains.Key("booleanProperty"));
        Assert.That(((LocalArgumentValue)dictionaryValue["booleanProperty"]).Type, Is.EqualTo("boolean"));
        Assert.That(((LocalArgumentValue)dictionaryValue["booleanProperty"]).Value, Is.EqualTo(true));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("object"));
        Assert.That(argumentValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(1));
        object key = dictionaryValue.ElementAt(0).Key;
        Assert.That(key, Is.InstanceOf<RemoteObjectReference>());
        RemoteObjectReference keyReference = (RemoteObjectReference)key;
        Assert.That(keyReference.Handle, Is.EqualTo("mySymbol"));
        LocalValue value = dictionaryValue[key];
        Assert.That(value, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue stringValue = (LocalArgumentValue)value;
        Assert.That(stringValue.Type, Is.EqualTo("string"));
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        LocalValue localValue = mapRemoteValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("object"));
        Assert.That(argumentValue.Value, Is.InstanceOf<Dictionary<object, LocalValue>>());
        Dictionary<object, LocalValue> dictionaryValue = (Dictionary<object, LocalValue>)argumentValue.Value;
        Assert.That(dictionaryValue, Has.Count.EqualTo(2));
        object key1 = dictionaryValue.ElementAt(0).Key;
        Assert.That(key1, Is.InstanceOf<string>());
        Assert.That((string)key1, Is.EqualTo("stringProperty"));
        LocalValue value1 = dictionaryValue[key1];
        Assert.That(value1, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue stringValue = (LocalArgumentValue)value1;
        Assert.That(stringValue.Type, Is.EqualTo("string"));
        Assert.That(stringValue.Value, Is.EqualTo("stringValue"));
        object key2 = dictionaryValue.ElementAt(1).Key;
        Assert.That(key2, Is.InstanceOf<RemoteObjectReference>());
        RemoteObjectReference keyReference = (RemoteObjectReference)key2;
        Assert.That(keyReference.Handle, Is.EqualTo("mySymbol"));
        LocalValue value2 = dictionaryValue[key2];
        Assert.That(value2, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue symbolValue = (LocalArgumentValue)value2;
        Assert.That(symbolValue.Type, Is.EqualTo("string"));
        Assert.That(symbolValue.Value, Is.EqualTo("symbolValue"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out KeyValuePairCollectionRemoteValue? mapRemoteValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(mapRemoteValue, Is.Not.Null);
        KeyValuePairCollectionRemoteValue copy = mapRemoteValue with { };
        Assert.That(copy, Is.EqualTo(mapRemoteValue));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out WindowProxyRemoteValue? windowProxyValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(windowProxyValue, Is.Not.Null);
        Assert.That(windowProxyValue.Value.Context, Is.EqualTo("myContext"));
        Assert.That(windowProxyValue.Handle, Is.EqualTo("myHandle"));
        Assert.That(windowProxyValue.InternalId, Is.EqualTo("123"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out WindowProxyRemoteValue? windowProxyValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(windowProxyValue, Is.Not.Null);
        LocalValue localValue = windowProxyValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<RemoteObjectReference>());
        RemoteObjectReference argumentValue = (RemoteObjectReference)localValue;
        Assert.That(argumentValue.Handle, Is.EqualTo("myHandle"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out WindowProxyRemoteValue? windowProxyValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(windowProxyValue, Is.Not.Null);
        WindowProxyRemoteValue copy = windowProxyValue with { };
        Assert.That(copy, Is.EqualTo(windowProxyValue));
    }

    [Test]
    public void TestCanDeserializeNullRemoteValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out NullRemoteValue? nullValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(nullValue, Is.Not.Null);
    }

    [Test]
    public void TestCanConvertNullRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out NullRemoteValue? nullValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(nullValue, Is.Not.Null);
        LocalValue localValue = nullValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("null"));
    }

    [Test]
    public void TestNullRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out NullRemoteValue? nullValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(nullValue, Is.Not.Null);
        NullRemoteValue copy = nullValue with { };
        Assert.That(copy, Is.EqualTo(nullValue));
    }

    [Test]
    public void TestCanDeserializeUndefinedRemoteValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out UndefinedRemoteValue? undefinedValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(undefinedValue, Is.Not.Null);
    }

    [Test]
    public void TestCanConvertUndefinedRemoteValueToLocalValue()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out UndefinedRemoteValue? undefinedValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(undefinedValue, Is.Not.Null);
        LocalValue localValue = undefinedValue.ToLocalValue();
        Assert.That(localValue, Is.InstanceOf<LocalArgumentValue>());
        LocalArgumentValue argumentValue = (LocalArgumentValue)localValue;
        Assert.That(argumentValue.Type, Is.EqualTo("undefined"));
    }

    [Test]
    public void TestUndefinedRemoteValueCopySemantics()
    {
        string json = """
                      {
                        "type": "undefined"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        bool conversionResult = remoteValue.TryConvertTo(out UndefinedRemoteValue? undefinedValue);
        Assert.That(conversionResult, Is.True);
        Assert.That(undefinedValue, Is.Not.Null);
        UndefinedRemoteValue copy = undefinedValue with { };
        Assert.That(copy, Is.EqualTo(undefinedValue));
    }
}
