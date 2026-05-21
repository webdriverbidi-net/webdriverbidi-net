namespace WebDriverBiDi.Script;

using System.Text.Json;
using Newtonsoft.Json.Linq;

public class LocalValueTests
{
    [Fact]
    public void TestCanSerializeUndefined()
    {
        LocalValue value = LocalValue.Undefined;
        Assert.IsType<LocalArgumentValue>(value);
        Assert.Null(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);

        Assert.Single(parsed);
        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("undefined", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeNull()
    {
        LocalValue value = LocalValue.Null;
        Assert.Null(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Single(parsed);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("null", type.Value<string>());
    }

    [Fact]
    public void TestCanSerializeString()
    {
        LocalValue value = LocalValue.String("hello");
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("string", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.String, parsedValue.Type);
        Assert.Equal("hello", parsedValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeInteger()
    {
        LocalValue value = LocalValue.Number(123);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Integer, parsedValue.Type);
        Assert.Equal(123L, parsedValue.Value<long>());
    }

    [Fact]
    public void TestCanSerializeLong()
    {
        LocalValue value = LocalValue.Number(123L);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Integer, parsedValue.Type);
        Assert.Equal(123L, parsedValue.Value<long>());
    }

    [Fact]
    public void TestCanSerializeDecimal()
    {
        LocalValue value = LocalValue.Number(123.23m);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Float, parsedValue.Type);
        Assert.Equal(123.23, parsedValue.Value<double>());
    }

    [Fact]
    public void TestCanSerializeDouble()
    {
        LocalValue value = LocalValue.Number(3.14);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Float, parsedValue.Type);
        Assert.Equal(3.14, parsedValue.Value<double>());
    }

    [Fact]
    public void TestCanSerializeNaN()
    {
        LocalValue value = LocalValue.NaN;
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.String, parsedValue.Type);
        Assert.Equal("NaN", parsedValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeInfinity()
    {
        LocalValue value = LocalValue.Infinity;
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.String, parsedValue.Type);
        Assert.Equal("Infinity", parsedValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeNegativeInfinity()
    {
        LocalValue value = LocalValue.NegativeInfinity;
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.String, parsedValue.Type);
        Assert.Equal("-Infinity", parsedValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeNegativeZero()
    {
        LocalValue value = LocalValue.NegativeZero;
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.String, parsedValue.Type);
        Assert.Equal("-0", parsedValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeZero()
    {
        LocalValue value = LocalValue.Number(0.0);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("number", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Integer, parsedValue.Type);
        Assert.Equal(0.0, parsedValue.Value<double>());
    }

    [Fact]
    public void TestCanSerializeBoolean()
    {
        LocalValue value = LocalValue.Boolean(true);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("boolean", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Boolean, parsedValue.Type);
        Assert.True(parsedValue.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeBigInt()
    {
        LocalValue value = LocalValue.BigInt(123);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("bigint", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.String, parsedValue.Type);
        Assert.Equal("123", parsedValue.Value<string>());
    }

    [Fact]
    public void TestCanSerializeDate()
    {
        DateTime actualValue = DateTime.Now;
        string expectedValue = actualValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        LocalValue value = LocalValue.Date(actualValue);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("date", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Date, parsedValue.Type);
        Assert.Equal(expectedValue, parsedValue.Value<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }

    [Fact]
    public void TestCanSerializeRegularExpression()
    {
        LocalValue value = LocalValue.RegExp("pattern.*");
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("regexp", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Object, parsedValue.Type);

        JObject? valueObject = parsed["value"] as JObject;
        Assert.NotNull(valueObject);
        Assert.Single(valueObject);
        Assert.True(valueObject.ContainsKey("pattern"));
        JToken? pattern = valueObject["pattern"];
        Assert.NotNull(pattern);
        Assert.Equal(JTokenType.String, pattern.Type);
        Assert.Equal("pattern.*", pattern.Value<string>());
    }

    [Fact]
    public void TestCanSerializeRegularExpressionWithFlags()
    {
        LocalValue value = LocalValue.RegExp("pattern.*", "gi");
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("regexp", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Object, parsedValue.Type);

        JObject? valueObject = parsed["value"] as JObject;
        Assert.NotNull(valueObject);
        Assert.Equal(2, valueObject.Count);
        Assert.True(valueObject.ContainsKey("pattern"));
        JToken? pattern = valueObject["pattern"];
        Assert.NotNull(pattern);
        Assert.Equal(JTokenType.String, pattern.Type);
        Assert.Equal("pattern.*", pattern.Value<string>());

        Assert.True(valueObject.ContainsKey("flags"));
        JToken? flags = valueObject["flags"];
        Assert.NotNull(flags);
        Assert.Equal(JTokenType.String, flags.Type);
        Assert.Equal("gi", flags.Value<string>());
    }

    [Fact]
    public void TestCanSerializeArray()
    {
        List<LocalValue> arrayList =
        [
            LocalValue.String("hello"),
            LocalValue.Number(123),
            LocalValue.Null,
            LocalValue.Boolean(true)
        ];
        LocalValue value = LocalValue.Array(arrayList);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("array", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        JArray? valueObject = parsed["value"] as JArray;
        Assert.NotNull(valueObject);
        Assert.Equal(4, valueObject.Count);

        Assert.Equal(JTokenType.Object, valueObject[0].Type);

        JObject? itemObject = valueObject[0] as JObject;
        Assert.NotNull(itemObject);
        Assert.Equal(2, itemObject.Count);

        Assert.True(itemObject.ContainsKey("type"));
        JToken? itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("string", itemType.Value<string>());

        Assert.True(itemObject.ContainsKey("value"));
        JToken? itemValue = itemObject["value"];
        Assert.NotNull(itemValue);
        Assert.Equal(JTokenType.String, itemValue.Type);
        Assert.Equal("hello", itemValue.Value<string>());

        Assert.Equal(JTokenType.Object, valueObject[1].Type);

        itemObject = valueObject[1] as JObject;
        Assert.NotNull(itemObject);
        Assert.Equal(2, itemObject.Count);

        Assert.True(itemObject.ContainsKey("type"));
        itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("number", itemType.Value<string>());

        Assert.True(itemObject.ContainsKey("value"));
        itemValue = itemObject["value"];
        Assert.NotNull(itemValue);
        Assert.Equal(JTokenType.Integer, itemValue.Type);
        Assert.Equal(123L, itemValue.Value<long>());

        Assert.Equal(JTokenType.Object, valueObject[2].Type);

        itemObject = valueObject[2] as JObject;
        Assert.NotNull(itemObject);
        Assert.Single(itemObject);

        Assert.True(itemObject.ContainsKey("type"));
        itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("null", itemType.Value<string>());

        Assert.Equal(JTokenType.Object, valueObject[3].Type);

        itemObject = valueObject[3] as JObject;
        Assert.NotNull(itemObject);
        Assert.Equal(2, itemObject.Count);

        Assert.True(itemObject.ContainsKey("type"));
        itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("boolean", itemType.Value<string>());

        Assert.True(itemObject.ContainsKey("value"));
        itemValue = itemObject["value"];
        Assert.NotNull(itemValue);
        Assert.Equal(JTokenType.Boolean, itemValue.Type);
        Assert.True(itemValue.Value<bool>());
    }

    [Fact]
    public void TestCanSerializeSet()
    {
        List<LocalValue> arrayList =
        [
            LocalValue.String("hello"),
            LocalValue.Number(123),
            LocalValue.Null,
            LocalValue.Boolean(true)
        ];
        LocalValue value = LocalValue.Set(arrayList);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("set", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        JArray? valueObject = parsed["value"] as JArray;
        Assert.NotNull(valueObject);
        Assert.Equal(4, valueObject.Count);

        Assert.Equal(JTokenType.Object, valueObject[0].Type);
        JObject? itemObject = valueObject[0] as JObject;
        Assert.NotNull(itemObject);
        Assert.Equal(2, itemObject.Count);

        Assert.True(itemObject.ContainsKey("type"));
        JToken? itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("string", itemType.Value<string>());

        Assert.True(itemObject.ContainsKey("value"));
        JToken? itemValue = itemObject["value"];
        Assert.NotNull(itemValue);
        Assert.Equal(JTokenType.String, itemValue.Type);
        Assert.Equal("hello", itemValue.Value<string>());

        Assert.Equal(JTokenType.Object, valueObject[1].Type);
        itemObject = valueObject[1] as JObject;
        Assert.NotNull(itemObject);
        Assert.Equal(2, itemObject.Count);

        Assert.True(itemObject.ContainsKey("type"));
        itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("number", itemType.Value<string>());

        Assert.True(itemObject.ContainsKey("value"));
        itemValue = itemObject["value"];
        Assert.NotNull(itemValue);
        Assert.Equal(JTokenType.Integer, itemValue.Type);
        Assert.Equal(123L, itemValue.Value<long>());

        Assert.Equal(JTokenType.Object, valueObject[2].Type);
        itemObject = valueObject[2] as JObject;
        Assert.NotNull(itemObject);
        Assert.Single(itemObject);

        Assert.True(itemObject.ContainsKey("type"));
        itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("null", itemType.Value<string>());

        Assert.Equal(JTokenType.Object, valueObject[3].Type);
        itemObject = valueObject[3] as JObject;
        Assert.NotNull(itemObject);
        Assert.Equal(2, itemObject.Count);

        Assert.True(itemObject.ContainsKey("type"));
        itemType = itemObject["type"];
        Assert.NotNull(itemType);
        Assert.Equal("boolean", itemType.Value<string>());

        Assert.True(itemObject.ContainsKey("value"));
        itemValue = itemObject["value"];
        Assert.NotNull(itemValue);
        Assert.Equal(JTokenType.Boolean, itemValue.Type);
        Assert.True(itemValue.Value<bool>());
    }

    [Fact]
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
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("map", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.NotNull(valueArray);
        Assert.Equal(dictionary.Count, valueArray.Count);
        List<string> foundStrings = [];
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.Equal(JTokenType.Array, valueArray[i].Type);
            JArray? itemArray = valueArray[i] as JArray;
            Assert.NotNull(itemArray);

            Assert.Equal(2, itemArray.Count);
            Assert.Equal(JTokenType.String, itemArray[0].Type);

            foundStrings.Add(itemArray[0].Value<string>() ?? "");
            Assert.Equal(JTokenType.Object, itemArray[1].Type);
        }

        Assert.Equivalent(dictionary.Keys, foundStrings);
    }

    [Fact]
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
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("object", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.NotNull(valueArray);
        Assert.Equal(dictionary.Count, valueArray.Count);
        List<string> foundStrings = [];
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.Equal(JTokenType.Array, valueArray[i].Type);
            JArray? itemArray = valueArray[i] as JArray;
            Assert.NotNull(itemArray);

            Assert.Equal(2, itemArray.Count);
            Assert.Equal(JTokenType.String, itemArray[0].Type);

            foundStrings.Add(itemArray[0].Value<string>() ?? "");
            Assert.Equal(JTokenType.Object, itemArray[1].Type);
        }

        Assert.Equivalent(dictionary.Keys, foundStrings);
    }

    [Fact]
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
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("map", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.NotNull(valueArray);
        Assert.Equal(dictionary.Count, valueArray.Count);
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.Equal(JTokenType.Array, valueArray[i].Type);
            JArray? itemArray = valueArray[i] as JArray;
            Assert.NotNull(itemArray);
            Assert.Equal(2, itemArray.Count);

            Assert.Equal(JTokenType.Object, itemArray[0].Type);
            Assert.Equal(JTokenType.Object, itemArray[1].Type);
        }
    }

    [Fact]
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
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("object", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.NotNull(valueArray);
        Assert.Equal(dictionary.Count, valueArray.Count);
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.Equal(JTokenType.Array, valueArray[i].Type);
            JArray? itemArray = valueArray[i] as JArray;
            Assert.NotNull(itemArray);
            Assert.Equal(2, itemArray.Count);

            Assert.Equal(JTokenType.Object, itemArray[0].Type);
            Assert.Equal(JTokenType.Object, itemArray[1].Type);
        }
    }

    [Fact]
    public void TestCanSerializeMapWithMixedValuesAsKeys()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<object, LocalValue> dictionary = new()
        {
            { "string", LocalValue.String("hello") },
            { LocalValue.String("number"), LocalValue.Number(123) },
            { LocalValue.Number(1), LocalValue.Null },
            { LocalValue.NaN, LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Map(dictionary);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("map", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.NotNull(valueArray);
        Assert.Equal(dictionary.Count, valueArray.Count);
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.Equal(JTokenType.Array, valueArray[i].Type);
            JArray? itemArray = valueArray[i] as JArray;
            Assert.NotNull(itemArray);
            Assert.Equal(2, itemArray.Count);

            Assert.True(itemArray[0].Type == JTokenType.Object || itemArray[0].Type == JTokenType.String);
            Assert.Equal(JTokenType.Object, itemArray[1].Type);
        }
    }

    [Fact]
    public void TestCreatingMapWithInvalidKeyTypeThrows()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<object, LocalValue> dictionary = new()
        {
            { 1, LocalValue.String("hello") },
        };

        Assert.ThrowsAny<WebDriverBiDiException>(() => LocalValue.Map(dictionary));
    }

    [Fact]
    public void TestSerializingMapWithInvalidKeyTypeThrows()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<object, LocalValue> dictionary = [];
        LocalValue value = LocalValue.Map(dictionary);
        dictionary[1] = LocalValue.String("hello");
        Assert.NotNull(((LocalArgumentValue)value).Value);
        Assert.ThrowsAny<WebDriverBiDiException>(() => JsonSerializer.Serialize(value));
    }

    [Fact]
    public void TestCanSerializeObjectWithMixedValuesAsKeys()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<object, LocalValue> dictionary = new()
        {
            { "string", LocalValue.String("hello") },
            { LocalValue.String("number"), LocalValue.Number(123) },
            { LocalValue.Number(1), LocalValue.Null },
            { LocalValue.NaN, LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Object(dictionary);
        Assert.NotNull(((LocalArgumentValue)value).Value);
        string json = JsonSerializer.Serialize(value);
        JObject parsed = JObject.Parse(json);
        Assert.Equal(2, parsed.Count);

        Assert.True(parsed.ContainsKey("type"));
        JToken? type = parsed["type"];
        Assert.NotNull(type);
        Assert.Equal("object", type.Value<string>());

        Assert.True(parsed.ContainsKey("value"));
        JToken? parsedValue = parsed["value"];
        Assert.NotNull(parsedValue);
        Assert.Equal(JTokenType.Array, parsedValue.Type);

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JArray? valueArray = parsed["value"] as JArray;
        Assert.NotNull(valueArray);
        Assert.Equal(dictionary.Count, valueArray.Count);
        for (int i = 0; i < dictionary.Count; i++)
        {
            Assert.Equal(JTokenType.Array, valueArray[i].Type);
            JArray? itemArray = valueArray[i] as JArray;
            Assert.NotNull(itemArray);
            Assert.Equal(2, itemArray.Count);

            Assert.True(itemArray[0].Type == JTokenType.Object || itemArray[0].Type == JTokenType.String);
            Assert.Equal(JTokenType.Object, itemArray[1].Type);
        }
    }

    [Fact]
    public void TestCreatingObjectWithInvalidKeyTypeThrows()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<object, LocalValue> dictionary = new()
        {
            { 1, LocalValue.String("hello") },
        };

        Assert.ThrowsAny<WebDriverBiDiException>(() => LocalValue.Object(dictionary));
    }

    [Fact]
    public void TestSerializingObjectWithInvalidKeyTypeThrows()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<object, LocalValue> dictionary = [];
        LocalValue value = LocalValue.Object(dictionary);
        dictionary[1] = LocalValue.String("hello");
        Assert.NotNull(((LocalArgumentValue)value).Value);
        Assert.ThrowsAny<WebDriverBiDiException>(() => JsonSerializer.Serialize(value));
    }

    [Fact]
    public void TestCopySemantics()
    {
        LocalValue value = LocalValue.Undefined;
        Assert.Null(((LocalArgumentValue)value).Value);
        LocalValue copy = value with { };
        Assert.Equal(value, copy);
    }
}
