namespace WebDriverBiDi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class LocalValueTests
{
    [Test]
    public void TestCanSerializeUndefined()
    {
        LocalValue value = LocalValue.Undefined;
        Assert.That(value.Value, Is.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Has.Count.EqualTo(1));
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("undefined"));
        });
    }

    [Test]
    public void TestCanSerializeNull()
    {
        LocalValue value = LocalValue.Null;
        Assert.That(value.Value, Is.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("null"));
        });
    }

    [Test]
    public void TestCanSerializeString()
    {
        LocalValue value = LocalValue.String("hello");
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("hello"));
        });
    }

    [Test]
    public void TestCanSerializeInteger()
    {
        LocalValue value = LocalValue.Number(123);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(parsed["value"]!.Value<long>(), Is.EqualTo(123));
        });
    }

    [Test]
    public void TestCanSerializeLong()
    {
        LocalValue value = LocalValue.Number(123L);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(parsed["value"]!.Value<long>(), Is.EqualTo(123));
        });
    }

    [Test]
    public void TestCanSerializeDouble()
    {
        LocalValue value = LocalValue.Number(3.14);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(parsed["value"]!.Value<double>(), Is.EqualTo(3.14));
        });
    }

    [Test]
    public void TestCanSerializeNaN()
    {
        LocalValue value = LocalValue.NaN;
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("NaN"));
        });
    }

    [Test]
    public void TestCanSerializeInfinity()
    {
        LocalValue value = LocalValue.Infinity;
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("Infinity"));
        });
    }

    [Test]
    public void TestCanSerializeNegativeInfinity()
    {
        LocalValue value = LocalValue.NegativeInfinity;
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("-Infinity"));
        });
    }

    [Test]
    public void TestCanSerializeNegativeZero()
    {
        LocalValue value = LocalValue.NegativeZero;
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("-0"));
        });
    }

    [Test]
    public void TestCanSerializeZero()
    {
        LocalValue value = LocalValue.Number(0.0);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Float));
            Assert.That(parsed["value"]!.Value<double>(), Is.EqualTo(0.0));
        });
    }

    [Test]
    public void TestCanSerializeBoolean()
    {
        LocalValue value = LocalValue.Boolean(true);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("boolean"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(parsed["value"]!.Value<bool>(), Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanSerializeBigInt()
    {
        LocalValue value = LocalValue.BigInt(123);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("bigint"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("123"));
        });
    }

    [Test]
    public void TestCanSerializeDate()
    {
        DateTime actualValue = DateTime.Now;
        string expectedValue = actualValue.ToString("YYYY-MM-ddTHH:mm:ss.fffZ");
        LocalValue value = LocalValue.Date(actualValue);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("date"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo(expectedValue));
        });
    }

    [Test]
    public void TestCanSerializeRegularExpression()
    {
        LocalValue value = LocalValue.RegExp("pattern.*");
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("regexp"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? valueObject = parsed["value"] as JObject;
        Assert.Multiple(() =>
        {
            Assert.That(valueObject!, Has.Count.EqualTo(1));
            Assert.That(valueObject!, Contains.Key("pattern"));
            Assert.That(valueObject!["pattern"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["pattern"]!.Value<string>(), Is.EqualTo("pattern.*"));
        });
    }

    [Test]
    public void TestCanSerializeRegularExpressionWithFlags()
    {
        LocalValue value = LocalValue.RegExp("pattern.*", "gi");
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("regexp"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? valueObject = parsed["value"] as JObject;
        Assert.Multiple(() =>
        {
            Assert.That(valueObject!, Has.Count.EqualTo(2));
            Assert.That(valueObject!, Contains.Key("pattern"));
            Assert.That(valueObject!["pattern"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["pattern"]!.Value<string>(), Is.EqualTo("pattern.*"));
            Assert.That(valueObject, Contains.Key("flags"));
            Assert.That(valueObject["flags"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(valueObject["flags"]!.Value<string>(), Is.EqualTo("gi"));
        });
    }

    [Test]
    public void TestCanSerializeArray()
    {
        List<LocalValue> arrayList = new()
        {
            LocalValue.String("hello"),
            LocalValue.Number(123),
            LocalValue.Null,
            LocalValue.Boolean(true)
        };
        LocalValue value = LocalValue.Array(arrayList);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("array"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        });
        JArray? valueObject = parsed["value"] as JArray;
        Assert.That(valueObject, Is.Not.Null);
        Assert.That(valueObject, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(valueObject![0].Type, Is.EqualTo(JTokenType.Object));
        });
        JObject? itemObject = valueObject![0] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(itemObject, Contains.Key("value"));
            Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(itemObject["value"]!.Value<string>(), Is.EqualTo("hello"));

            Assert.That(valueObject[1].Type, Is.EqualTo(JTokenType.Object));
        });
        itemObject = valueObject[1] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(itemObject, Contains.Key("value"));
            Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(itemObject["value"]!.Value<long>(), Is.EqualTo(123));

            Assert.That(valueObject[2].Type, Is.EqualTo(JTokenType.Object));
        });
        itemObject = valueObject[2] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("null"));

            Assert.That(valueObject[3].Type, Is.EqualTo(JTokenType.Object));
        });
        itemObject = valueObject[3] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("boolean"));
            Assert.That(itemObject, Contains.Key("value"));
            Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(itemObject["value"]!.Value<bool>(), Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanSerializeSet()
    {
        List<LocalValue> arrayList = new()
        {
            LocalValue.String("hello"),
            LocalValue.Number(123),
            LocalValue.Null,
            LocalValue.Boolean(true)
        };
        LocalValue value = LocalValue.Set(arrayList);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("set"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        });
        JArray? valueObject = parsed["value"] as JArray;
        Assert.That(valueObject, Is.Not.Null);
        Assert.That(valueObject, Has.Count.EqualTo(4));

        Assert.That(valueObject![0].Type, Is.EqualTo(JTokenType.Object));
        JObject? itemObject = valueObject[0] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("string"));
            Assert.That(itemObject, Contains.Key("value"));
            Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.String));
            Assert.That(itemObject["value"]!.Value<string>(), Is.EqualTo("hello"));
        });

        Assert.That(valueObject[1].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[1] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("number"));
            Assert.That(itemObject, Contains.Key("value"));
            Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Integer));
            Assert.That(itemObject["value"]!.Value<long>(), Is.EqualTo(123));
        });

        Assert.That(valueObject[2].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[2] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("null"));
       });

        Assert.That(valueObject[3].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[3] as JObject;
        Assert.That(itemObject, Is.Not.Null);
        Assert.That(itemObject, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(itemObject!, Contains.Key("type"));
            Assert.That(itemObject!["type"]!.Value<string>(), Is.EqualTo("boolean"));
            Assert.That(itemObject, Contains.Key("value"));
            Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Boolean));
            Assert.That(itemObject["value"]!.Value<bool>(), Is.EqualTo(true));
        });
    }

    [Test]
    public void TestCanSerializeMap()
    {
        Dictionary<string, LocalValue> dictionary = new()
        {
            { "string", LocalValue.String("hello") },
            { "number", LocalValue.Number(123) },
            { "null", LocalValue.Null },
            { "boolean", LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Map(dictionary);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("map"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        });

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.That(valueArray, Is.Not.Null);
        Assert.That(valueArray, Has.Count.EqualTo(dictionary.Count));
        List<string> foundStrings = new();
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.That(valueArray![i].Type, Is.EqualTo(JTokenType.Array));
            JArray? itemArray = valueArray[i] as JArray;
            Assert.That(itemArray, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(itemArray, Has.Count.EqualTo(2));
                Assert.That(itemArray![0].Type, Is.EqualTo(JTokenType.String));
            });
            foundStrings.Add(itemArray![0].Value<string>() ?? "");
            Assert.That(itemArray[1].Type, Is.EqualTo(JTokenType.Object));
        }

        Assert.That(foundStrings, Is.EquivalentTo(dictionary.Keys));
    }

    [Test]
    public void TestCanSerializeObject()
    {
        Dictionary<string, LocalValue> dictionary = new()
        {
            { "string", LocalValue.String("hello") },
            { "number", LocalValue.Number(123) },
            { "null", LocalValue.Null },
            { "boolean", LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Object(dictionary);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("object"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        });

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.That(valueArray, Is.Not.Null);
        Assert.That(valueArray, Has.Count.EqualTo(dictionary.Count));
        List<string> foundStrings = new();
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.That(valueArray![i].Type, Is.EqualTo(JTokenType.Array));
            JArray? itemArray = valueArray[i] as JArray;
            Assert.That(itemArray, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(itemArray!, Has.Count.EqualTo(2));
                Assert.That(itemArray![0].Type, Is.EqualTo(JTokenType.String));
            });
            foundStrings.Add(itemArray![0].Value<string>() ?? "");
            Assert.That(itemArray[1].Type, Is.EqualTo(JTokenType.Object));
        }

        Assert.That(foundStrings, Is.EquivalentTo(dictionary.Keys));
    }

    [Test]
    public void TestCanSerializeMapWithLocalValueAsKeys()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<LocalValue, LocalValue> dictionary = new()
        {
            { LocalValue.String("string"), LocalValue.String("hello") },
            { LocalValue.String("number"), LocalValue.Number(123) },
            { LocalValue.Number(1), LocalValue.Null },
            { LocalValue.NaN, LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Map(dictionary);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("map"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        });

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.That(valueArray, Is.Not.Null);
        Assert.That(valueArray, Has.Count.EqualTo(dictionary.Count));
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.That(valueArray![i].Type, Is.EqualTo(JTokenType.Array));
            JArray? itemArray = valueArray[i] as JArray;
            Assert.That(itemArray, Is.Not.Null);
            Assert.That(itemArray, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(itemArray![0].Type, Is.EqualTo(JTokenType.Object));
                Assert.That(itemArray[1].Type, Is.EqualTo(JTokenType.Object));
            });
        }
    }

    [Test]
    public void TestCanSerializeObjectWithLocalValueAsKeys()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<LocalValue, LocalValue> dictionary = new()
        {
            { LocalValue.String("string"), LocalValue.String("hello") },
            { LocalValue.String("number"), LocalValue.Number(123) },
            { LocalValue.Number(1), LocalValue.Null },
            { LocalValue.NaN, LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Object(dictionary);
        Assert.That(value.Value, Is.Not.Null);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(parsed, Contains.Key("type"));
            Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("object"));
            Assert.That(parsed, Contains.Key("value"));
            Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        });

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.That(valueArray, Is.Not.Null);
        Assert.That(valueArray, Has.Count.EqualTo(dictionary.Count));
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.That(valueArray![i].Type, Is.EqualTo(JTokenType.Array));
            JArray? itemArray = valueArray[i] as JArray;
            Assert.That(itemArray, Is.Not.Null);
            Assert.That(itemArray, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(itemArray![0].Type, Is.EqualTo(JTokenType.Object));
                Assert.That(itemArray[1].Type, Is.EqualTo(JTokenType.Object));
            });
        }
    }
}