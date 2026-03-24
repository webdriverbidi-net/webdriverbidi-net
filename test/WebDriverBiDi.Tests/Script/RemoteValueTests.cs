namespace WebDriverBiDi.Script;

using System.Numerics;
using System.Text.Json;

[TestFixture]
public class RemoteValueTests
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(remoteValue, Is.InstanceOf<StringRemoteValue>());
            Assert.That(((StringRemoteValue)remoteValue).Value, Is.EqualTo("myValue"));
        }
    }

    [Test]
    public void TestDeserializingInvalidStringRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "string",
                        "value": 7
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'value' property for string must be a non-null string"));
    }

    [Test]
    public void TestCanDeserializeIntegerRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 1
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Number));
            Assert.That(remoteValue, Is.InstanceOf<LongRemoteValue>());
            Assert.That(((LongRemoteValue)remoteValue).Value, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestCanDeserializeFloatRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Number));
            Assert.That(remoteValue, Is.InstanceOf<DoubleRemoteValue>());
            Assert.That(((DoubleRemoteValue)remoteValue).Value, Is.EqualTo(3.14));
        }
    }

    [Test]
    public void TestCanDeserializeNaNRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "NaN"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Number));
            Assert.That(remoteValue, Is.InstanceOf<DoubleRemoteValue>());
            Assert.That(((DoubleRemoteValue)remoteValue).Value, Is.EqualTo(double.NaN));
        }
    }

    [Test]
    public void TestCanDeserializeNegativeZeroRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "-0"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Number));
            Assert.That(remoteValue, Is.InstanceOf<DoubleRemoteValue>());
            Assert.That(((DoubleRemoteValue)remoteValue).Value, Is.EqualTo(double.NegativeZero));
            Assert.That(double.IsNegative(((DoubleRemoteValue)remoteValue).Value), Is.True);
        }
    }

    [Test]
    public void TestCanDeserializeInfinityRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "Infinity"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Number));
            Assert.That(remoteValue, Is.InstanceOf<DoubleRemoteValue>());
            Assert.That(((DoubleRemoteValue)remoteValue).Value, Is.EqualTo(double.PositiveInfinity));
        }
    }

    [Test]
    public void TestCanDeserializeNegativeInfinityRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "-Infinity"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Number));
            Assert.That(remoteValue, Is.InstanceOf<DoubleRemoteValue>());
            Assert.That(((DoubleRemoteValue)remoteValue).Value, Is.EqualTo(double.NegativeInfinity));
        }
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        RemoteValue copy = remoteValue with { };
        Assert.That(copy, Is.EqualTo(remoteValue));
    }

    [Test]
    public void TestDeserializingInvalidSpecialNumericRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "number",
                        "value": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("invalid value 'invalid' for 'value' property of number"));
    }

    [Test]
    public void TestDeserializingInvalidNumericRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "number",
                        "value": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("invalid type Boolean for 'value' property of number"));
        json = """
               {
                 "type": "number",
                 "value": false
               }
               """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("invalid type Boolean for 'value' property of number"));
        json = """
               {
                 "type": "number",
                 "value": null
               }
               """;
        json = @"{ ""type"": ""number"", ""value"": null }";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("invalid type Null for 'value' property of number"));
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Boolean));
            Assert.That(remoteValue, Is.InstanceOf<BooleanRemoteValue>());
            Assert.That(((BooleanRemoteValue)remoteValue).Value, Is.EqualTo(true));
        }
    }

    [Test]
    public void TestDeserializingInvalidBooleanRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "boolean",
                        "value": "hello"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'value' property for boolean must be a boolean value"));
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.BigInt));
            Assert.That(remoteValue, Is.InstanceOf<BigIntegerRemoteValue>());
            Assert.That(((BigIntegerRemoteValue)remoteValue).Value, Is.EqualTo(new BigInteger(123)));
        }
    }

    [Test]
    public void TestDeserializingInvalidBigIntRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("bigint must have a non-null 'value' property whose value is a string"));
    }

    [Test]
    public void TestDeserializingInvalidBigIntValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "bigint",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("cannot parse invalid value 'some value' for bigint"));
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Date));
            Assert.That(remoteValue, Is.InstanceOf<DateRemoteValue>());
            Assert.That(((DateRemoteValue)remoteValue).Value, Is.EqualTo(new DateTime(2020, 07, 19, 23, 47, 26, 56, DateTimeKind.Utc)));
        }
    }

    [Test]
    public void TestDeserializingInvalidDateRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": true
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("date must have a non-null 'value' property whose value is a string"));
    }

    [Test]
    public void TestDeserializingInvalidDateValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("cannot parse invalid value 'some value' for date"));
    }

    [Test]
    public void TestDeserializingRegularExpressionRemoteValue()
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.RegExp));
            Assert.That(remoteValue, Is.InstanceOf<RegExpRemoteValue>());
            RegExpRemoteValue regExpRemoteValue = (RegExpRemoteValue)remoteValue;
            Assert.That(regExpRemoteValue.Handle, Is.Null);
            Assert.That(regExpRemoteValue.InternalId, Is.Null);
            Assert.That(regExpRemoteValue.Value, Is.EqualTo(expectedRegexValue));
        }
    }

    [Test]
    public void TestDeserializingRegularExpressionWithNullFlagsRemoteValue()
    {
        LocalValue regexLocalValue = LocalValue.RegExp("myPattern");
        LocalArgumentValue primitiveValue = (LocalArgumentValue)regexLocalValue;
        Assert.That(primitiveValue.Value, Is.Not.Null);
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.RegExp));
            Assert.That(remoteValue, Is.InstanceOf<RegExpRemoteValue>());
            RegExpRemoteValue regExpRemoteValue = (RegExpRemoteValue)remoteValue;
            Assert.That(regExpRemoteValue.Handle, Is.Null);
            Assert.That(regExpRemoteValue.InternalId, Is.Null);
            Assert.That(regExpRemoteValue.Value, Is.EqualTo(expectedRegexValue));
        }
    }

    [Test]
    public void TestDeserializingInvalidRegularExpressionValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "regexp",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("regexp must have a non-null 'value' property whose value is an object"));
    }

    [Test]
    public void TestDeserializingNodeRemoteValue()
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Node));
            Assert.That(remoteValue, Is.InstanceOf<NodeRemoteValue>());
            NodeRemoteValue nodeRemoteValue = remoteValue.ConvertTo<NodeRemoteValue>();
            Assert.That(nodeRemoteValue.Handle, Is.Null);
            Assert.That(nodeRemoteValue.InternalId, Is.Null);
            Assert.That(nodeRemoteValue.SharedId, Is.Null);
            NodeProperties nodeProperties = nodeRemoteValue.Value;
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestDeserializingNodeRemoteValueWithSharedId()
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Node));
            Assert.That(remoteValue, Is.InstanceOf<NodeRemoteValue>());
            NodeRemoteValue nodeRemoteValue = remoteValue.ConvertTo<NodeRemoteValue>();
            Assert.That(nodeRemoteValue.Handle, Is.Null);
            Assert.That(nodeRemoteValue.InternalId, Is.Null);
            Assert.That(nodeRemoteValue.SharedId, Is.EqualTo("mySharedId"));
            NodeProperties nodeProperties = nodeRemoteValue.Value;
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        }
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("RemoteValue 'sharedId' property, when present, must be a string"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Array));
            Assert.That(remoteValue, Is.InstanceOf<CollectionRemoteValue>());
            CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
            Assert.That(listRemoteValue.Handle, Is.Null);
            Assert.That(listRemoteValue.InternalId, Is.Null);
            RemoteValueList arrayValue = listRemoteValue.Value;
            Assert.That(arrayValue, Is.Not.Null);
            Assert.That(arrayValue, Has.Count.EqualTo(3));
            Assert.That(arrayValue[0], Is.InstanceOf<StringRemoteValue>());
            Assert.That(((StringRemoteValue)arrayValue[0]).Value, Is.EqualTo("stringValue"));
            Assert.That(arrayValue[1], Is.InstanceOf<LongRemoteValue>());
            Assert.That(((LongRemoteValue)arrayValue[1]).Value, Is.EqualTo(123));
            Assert.That(arrayValue[2], Is.InstanceOf<BooleanRemoteValue>());
            Assert.That(((BooleanRemoteValue)arrayValue[2]).Value, Is.EqualTo(true));
        }
    }

    [Test]
    public void TestDeserializingInvalidNodeValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("node must have a non-null 'value' property whose value is an object"));
    }

    [Test]
    public void TestDeserializingArrayRemoteValue()
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Array));
            Assert.That(remoteValue, Is.InstanceOf<CollectionRemoteValue>());
            CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
            Assert.That(listRemoteValue.Handle, Is.Null);
            Assert.That(listRemoteValue.InternalId, Is.Null);
            RemoteValueList arrayValue = listRemoteValue.Value;
            Assert.That(arrayValue, Is.Not.Null);
            Assert.That(arrayValue, Has.Count.EqualTo(3));
            Assert.That(arrayValue[0], Is.InstanceOf<StringRemoteValue>());
            Assert.That(((StringRemoteValue)arrayValue[0]).Value, Is.EqualTo("stringValue"));
            Assert.That(arrayValue[1], Is.InstanceOf<LongRemoteValue>());
            Assert.That(((LongRemoteValue)arrayValue[1]).Value, Is.EqualTo(123));
            Assert.That(arrayValue[2], Is.InstanceOf<BooleanRemoteValue>());
            Assert.That(((BooleanRemoteValue)arrayValue[2]).Value, Is.EqualTo(true));
        }
    }

    [Test]
    public void TestDeserializingInvalidArrayValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "array",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("array must have a non-null 'value' property whose value is an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("each element for list must be an object"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Set));
            CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
            Assert.That(listRemoteValue.Handle, Is.Null);
            Assert.That(listRemoteValue.InternalId, Is.Null);
            RemoteValueList setValue = listRemoteValue.Value;
            Assert.That(setValue, Is.Not.Null);
            Assert.That(setValue, Has.Count.EqualTo(3));
            Assert.That(setValue[0], Is.InstanceOf<StringRemoteValue>());
            Assert.That(((StringRemoteValue)setValue[0]).Value, Is.EqualTo("stringValue"));
            Assert.That(setValue[1], Is.InstanceOf<LongRemoteValue>());
            Assert.That(((LongRemoteValue)setValue[1]).Value, Is.EqualTo(123));
            Assert.That(setValue[2], Is.InstanceOf<BooleanRemoteValue>());
            Assert.That(((BooleanRemoteValue)setValue[2]).Value, Is.EqualTo(true));
        }
    }

    [Test]
    public void TestDeserializingInvalidSetValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "set",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("set must have a non-null 'value' property whose value is an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("each element for list must be an object"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.NodeList));
            CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
            Assert.That(listRemoteValue.Handle, Is.Null);
            Assert.That(listRemoteValue.InternalId, Is.Null);
            RemoteValueList nodeListValue = listRemoteValue.Value;
            Assert.That(nodeListValue, Is.Not.Null);
            Assert.That(nodeListValue, Has.Count.EqualTo(1));
            Assert.That(nodeListValue[0], Is.InstanceOf<NodeRemoteValue>());
            NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)nodeListValue[0];
            Assert.That(nodeRemoteValue.Value.NodeType, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestDeserializingInvalidNodeListValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "nodelist",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("nodelist must have a non-null 'value' property whose value is an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("each element for list must be an object"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.HtmlCollection));
            CollectionRemoteValue listRemoteValue = (CollectionRemoteValue)remoteValue;
            Assert.That(listRemoteValue.Handle, Is.Null);
            Assert.That(listRemoteValue.InternalId, Is.Null);
            RemoteValueList htmlCollectionValue = listRemoteValue.Value;
            Assert.That(htmlCollectionValue, Is.Not.Null);
            Assert.That(htmlCollectionValue, Has.Count.EqualTo(1));
            Assert.That(htmlCollectionValue[0], Is.InstanceOf<NodeRemoteValue>());
            NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)htmlCollectionValue[0];
            Assert.That(nodeRemoteValue.Value.NodeType, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestDeserializingInvalidHtmlCollectionValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "htmlcollection",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("htmlcollection must have a non-null 'value' property whose value is an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("each element for list must be an object"));
    }

    [Test]
    public void TestDeserializingMapRemoteValue()
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            Assert.That(remoteValue, Is.InstanceOf<KeyValuePairCollectionRemoteValue>());
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(3));
            Assert.That(dictionaryValue, Contains.Key("stringProperty"));
            Assert.That(dictionaryValue["stringProperty"], Is.InstanceOf<StringRemoteValue>());
            Assert.That(((StringRemoteValue)dictionaryValue["stringProperty"]).Value, Is.EqualTo("stringValue"));
            Assert.That(dictionaryValue, Contains.Key("numberProperty"));
            Assert.That(dictionaryValue["numberProperty"], Is.InstanceOf<LongRemoteValue>());
            Assert.That(((LongRemoteValue)dictionaryValue["numberProperty"]).Value, Is.EqualTo(123));
            Assert.That(dictionaryValue, Contains.Key("booleanProperty"));
            Assert.That(dictionaryValue["booleanProperty"], Is.InstanceOf<BooleanRemoteValue>());
            Assert.That(((BooleanRemoteValue)dictionaryValue["booleanProperty"]).Value, Is.EqualTo(true));
        }
    }

    [Test]
    public void TestDeserializingMapRemoteValueWithStringRemoteValueKey()
    {
        string json = """
                      {
                        "type": "map",
                        "value": [
                          [
                            {
                              "type": "string",
                              "value": "stringProperty"
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<StringRemoteValue>());
            Assert.That(((StringRemoteValue)dictionaryItem.Key).Value, Is.EqualTo("stringProperty"));
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<LongRemoteValue>());
            Assert.That(((LongRemoteValue)dictionaryItem.Key).Value, Is.EqualTo(2));
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<BooleanRemoteValue>());
            Assert.That(((BooleanRemoteValue)dictionaryItem.Key).Value, Is.True);
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<BigIntegerRemoteValue>());
            Assert.That(((BigIntegerRemoteValue)dictionaryItem.Key).Value, Is.EqualTo(new BigInteger(1234)));
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<NullRemoteValue>());
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<UndefinedRemoteValue>());
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<DateRemoteValue>());
            DateRemoteValue dateRemoteValue = (DateRemoteValue)dictionaryItem.Key;
            Assert.That(dateRemoteValue.Value, Is.EqualTo(new DateTime(2020, 07, 19, 23, 47, 26, 56, DateTimeKind.Utc)));
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)dictionaryItem.Key;
            Assert.That(objectReferenceRemoteValue.Handle, Is.EqualTo("myHandle"));
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Map));
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(1));
            KeyValuePair<object, RemoteValue> dictionaryItem = dictionaryValue.ElementAt(0);
            Assert.That(dictionaryItem.Key, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)dictionaryItem.Key;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.EqualTo("123"));
            Assert.That(dictionaryItem.Value, Is.InstanceOf<StringRemoteValue>());
            StringRemoteValue stringRemoteValue = (StringRemoteValue)dictionaryItem.Value;
            Assert.That(stringRemoteValue.Type, Is.EqualTo(RemoteValueType.String));
            Assert.That(stringRemoteValue.Value, Is.EqualTo("stringValue"));
        }
    }

    [Test]
    public void TestDeserializingInvalidMapValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "map",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("map must have a non-null 'value' property whose value is an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("map must have a non-null 'value' property whose value is an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("RemoteValue array element for dictionary must be an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("RemoteValue array element for dictionary must be an array"));
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("RemoteValue array element for dictionary must be an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must have a first element (key) that is either a string or an object"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must have a second element (value) that is an object"));
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Object));
            Assert.That(remoteValue, Is.InstanceOf<KeyValuePairCollectionRemoteValue>());
            KeyValuePairCollectionRemoteValue keyValuePairRemoteValue = (KeyValuePairCollectionRemoteValue)remoteValue;
            Assert.That(keyValuePairRemoteValue.Handle, Is.Null);
            Assert.That(keyValuePairRemoteValue.InternalId, Is.Null);
            RemoteValueDictionary dictionaryValue = keyValuePairRemoteValue.Value;
            Assert.That(dictionaryValue, Has.Count.EqualTo(3));
            Assert.That(dictionaryValue, Contains.Key("stringProperty"));
            Assert.That(dictionaryValue["stringProperty"], Is.InstanceOf<StringRemoteValue>());
            Assert.That(((StringRemoteValue)dictionaryValue["stringProperty"]).Value, Is.EqualTo("stringValue"));
            Assert.That(dictionaryValue, Contains.Key("numberProperty"));
            Assert.That(dictionaryValue["numberProperty"], Is.InstanceOf<LongRemoteValue>());
            Assert.That(((LongRemoteValue)dictionaryValue["numberProperty"]).Value, Is.EqualTo(123));
            Assert.That(dictionaryValue, Contains.Key("booleanProperty"));
            Assert.That(dictionaryValue["booleanProperty"], Is.InstanceOf<BooleanRemoteValue>());
            Assert.That(((BooleanRemoteValue)dictionaryValue["booleanProperty"]).Value, Is.EqualTo(true));
        }
    }

    [Test]
    public void TestDeserializingInvalidObjectValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "object",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("object must have a non-null 'value' property whose value is an array"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("object must have a non-null 'value' property whose value is an array"));
    }

    [Test]
    public void TestDeserializingSymbolRemoteValue()
    {
        string json = """
                      {
                        "type": "symbol"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Symbol));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingFunctionRemoteValue()
    {
        string json = """
                      {
                        "type": "function"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Function));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingWeakMapRemoteValue()
    {
        string json = """
                      {
                        "type": "weakmap"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.WeakMap));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingWeakSetRemoteValue()
    {
        string json = """
                      {
                        "type": "weakset"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.WeakSet));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingGeneratorRemoteValue()
    {
        string json = """
                      {
                        "type": "generator"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Generator));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingErrorRemoteValue()
    {
        string json = """
                      {
                        "type": "error"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Error));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingProxyRemoteValue()
    {
        string json = """
                      {
                        "type": "proxy"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Proxy));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingPromiseRemoteValue()
    {
        string json = """
                      {
                        "type": "promise"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Promise));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingTypedArrayRemoteValue()
    {
        string json = """
                      {
                        "type": "typedarray"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.TypedArray));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
    public void TestDeserializingArrayBufferRemoteValue()
    {
        string json = """
                      {
                        "type": "arraybuffer"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.ArrayBuffer));
            Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
            ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
            Assert.That(objectReferenceRemoteValue.Handle, Is.Null);
            Assert.That(objectReferenceRemoteValue.InternalId, Is.Null);
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Window));
            Assert.That(remoteValue, Is.InstanceOf<WindowProxyRemoteValue>());
            WindowProxyRemoteValue windowProxyRemoteValue = (WindowProxyRemoteValue)remoteValue;
            Assert.That(windowProxyRemoteValue.Handle, Is.Null);
            Assert.That(windowProxyRemoteValue.InternalId, Is.Null);
            Assert.That(windowProxyRemoteValue.Value.Context, Is.EqualTo("myContext"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(remoteValue.Type, Is.EqualTo(RemoteValueType.Window));
            Assert.That(remoteValue, Is.InstanceOf<WindowProxyRemoteValue>());
            WindowProxyRemoteValue windowProxyRemoteValue = (WindowProxyRemoteValue)remoteValue;
            Assert.That(windowProxyRemoteValue.Handle, Is.Not.Null);
            Assert.That(windowProxyRemoteValue.Handle, Is.EqualTo("myHandle"));
            Assert.That(windowProxyRemoteValue.InternalId, Is.Not.Null);
            Assert.That(windowProxyRemoteValue.InternalId, Is.EqualTo("123"));
            Assert.That(windowProxyRemoteValue.Value.Context, Is.EqualTo("myContext"));
        }
    }

    [Test]
    public void TestDeserializingInvalidWindowValueRemoteValueThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": "some value"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("window must have a non-null 'value' property whose value is an object"));
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingRemoteValueWithMissingTypeThrows()
    {
        string json = """
                      {
                        "value": "myValue"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must contain a 'type' property"));
    }

    [Test]
    public void TestDeserializingRemoteValueWithInvalidTypeThrows()
    {
        string json = """
                      {
                        "type": 3,
                        "value": "myValue"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("type property must be a string"));
    }

    [Test]
    public void TestDeserializingRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "invalid",
                        "value": "myValue"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'type' property value 'invalid' is not a valid RemoteValue type"));
    }

    [Test]
    public void TestDeserializingRemoteValueWithEmptyStringTypeThrows()
    {
        string json = """
                      {
                        "type": "",
                        "value": "myValue"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("non-empty 'type' property that is a string"));
    }

    [Test]
    public void TestValueAsWithIncorrectType()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 1
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(() => remoteValue.ConvertTo<StringRemoteValue>().Value, Throws.InstanceOf<WebDriverBiDiException>().With.Message.EqualTo($"RemoteValue of type '{remoteValue.Type}' cannot be converted to type 'StringRemoteValue'"));
        }
    }

    [Test]
    public void TestNullRemoteValueAsValueType()
    {
        string json = """
                      {
                        "type": "null"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue, Is.InstanceOf<NullRemoteValue>());
    }

    [Test]
    public void TestDeserializingNonObjectThrows()
    {
        string json = @"[ ""invalid remote value"" ]";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("RemoteValue JSON must be an object"));
    }

    [Test]
    public void TestCannotSerialize()
    {
        string json = """
                      {
                        "type": "string",
                        "value": "myValue"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(() => JsonSerializer.Serialize(remoteValue!), Throws.InstanceOf<NotImplementedException>());
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        RemoteReference reference = objectReferenceRemoteValue.ToRemoteReference();
        Assert.That(reference, Is.InstanceOf<RemoteObjectReference>());
    }

    [Test]
    public void TestConvertToRemoteReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "symbol",
                        "internalId": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue, Is.InstanceOf<ObjectReferenceRemoteValue>());
        ObjectReferenceRemoteValue objectReferenceRemoteValue = (ObjectReferenceRemoteValue)remoteValue;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(() => objectReferenceRemoteValue.ToRemoteReference(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("must have a valid handle"));
        }
    }

    [Test]
    public void TestConvertNodeRemoteValueToRemoteReference()
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
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue, Is.InstanceOf<NodeRemoteValue>());
        NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)remoteValue;
        RemoteReference reference = nodeRemoteValue.ToRemoteReference();
        Assert.That(reference, Is.InstanceOf<SharedReference>());
    }

    [Test]
    public void TestConvertNodeRemoteValueWithoutSharedIdToRemoteReferenceThrows()
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
        Assert.That(remoteValue, Is.InstanceOf<NodeRemoteValue>());
        NodeRemoteValue nodeRemoteValue = (NodeRemoteValue)remoteValue;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(() => nodeRemoteValue.ToRemoteReference(), Throws.InstanceOf<WebDriverBiDiException>().With.Message.Contains("must have a valid shared ID"));
        }
    }

    [Test]
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
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue, Is.InstanceOf<NodeRemoteValue>());
        SharedReference reference = ((NodeRemoteValue)remoteValue).ToSharedReference();
        Assert.That(reference, Is.InstanceOf<SharedReference>());
    }

    [Test]
    public void TestConvertNonNodeRemoteValueToSharedReferenceThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "handle": "myHandle",
                        "internalId": "123"
                      }
                      """;
        RemoteValue? remoteValue = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(remoteValue, Is.Not.Null);
        Assert.That(remoteValue.TryConvertTo(out NodeRemoteValue? _), Is.False);
    }
}
