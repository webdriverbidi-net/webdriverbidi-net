namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[TestFixture]
public class LocalValueTests
{
    [Test]
    public void TestCanSerializeUndefined()
    {
        LocalValue value = LocalValue.Undefined;
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(1)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("undefined"));
    }

    [Test]
    public void TestCanSerializeNull()
    {
        LocalValue value = LocalValue.Null;
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(1)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("null"));
    }

    [Test]
    public void TestCanSerializeString()
    {
        LocalValue value = LocalValue.String("hello");
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("string"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("hello"));
    }

    [Test]
    public void TestCanSerializeInteger()
    {
        LocalValue value = LocalValue.Number(123);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Integer));
        Assert.That(parsed["value"]!.Value<long>(), Is.EqualTo(123));
    }

    [Test]
    public void TestCanSerializeLong()
    {
        LocalValue value = LocalValue.Number(123L);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Integer));
        Assert.That(parsed["value"]!.Value<long>(), Is.EqualTo(123));
    }

    [Test]
    public void TestCanSerializeDouble()
    {
        LocalValue value = LocalValue.Number(3.14);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Float));
        Assert.That(parsed["value"]!.Value<double>(), Is.EqualTo(3.14));
    }

    [Test]
    public void TestCanSerializeNaN()
    {
        LocalValue value = LocalValue.NaN;
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("NaN"));
    }

    [Test]
    public void TestCanSerializeInfinity()
    {
        LocalValue value = LocalValue.Infinity;
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("Infinity"));
    }

    [Test]
    public void TestCanSerializeNegativeInfinity()
    {
        LocalValue value = LocalValue.NegativeInfinity;
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("-Infinity"));
    }

    [Test]
    public void TestCanSerializeNegativeZero()
    {
        LocalValue value = LocalValue.NegativeZero;
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("-0"));
    }

    [Test]
    public void TestCanSerializeZero()
    {
        LocalValue value = LocalValue.Number(0.0);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Float));
        Assert.That(parsed["value"]!.Value<double>(), Is.EqualTo(0.0));
    }

    [Test]
    public void TestCanSerializeBoolean()
    {
        LocalValue value = LocalValue.Boolean(true);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("boolean"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(parsed["value"]!.Value<bool>(), Is.EqualTo(true));
    }

    [Test]
    public void TestCanSerializeBigInt()
    {
        LocalValue value = LocalValue.BigInt(123);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("bigint"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo("123"));
    }

    [Test]
    public void TestCanSerializeDate()
    {
        DateTime actualValue = DateTime.Now;
        string expectedValue = actualValue.ToString("YYYY-MM-ddTHH:mm:ss.fffzzz");
        LocalValue value = LocalValue.Date(actualValue);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("date"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Value<string>(), Is.EqualTo(expectedValue));
    }

    [Test]
    public void TestCanSerializeRegularExpression()
    {
        LocalValue value = LocalValue.RegExp("pattern.*");
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("regexp"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
        JObject? valueObject = parsed["value"] as JObject;
        Assert.That(valueObject!.Count, Is.EqualTo(1));
        Assert.That(valueObject.ContainsKey("pattern"));
        Assert.That(valueObject["pattern"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(valueObject["pattern"]!.Value<string>(), Is.EqualTo("pattern.*"));
    }

    [Test]
    public void TestCanSerializeRegularExpressionWithFlags()
    {
        LocalValue value = LocalValue.RegExp("pattern.*", "gi");
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("regexp"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));
        JObject? valueObject = parsed["value"] as JObject;
        Assert.That(valueObject!.Count, Is.EqualTo(2));
        Assert.That(valueObject.ContainsKey("pattern"));
        Assert.That(valueObject["pattern"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(valueObject["pattern"]!.Value<string>(), Is.EqualTo("pattern.*"));
        Assert.That(valueObject.ContainsKey("flags"));
        Assert.That(valueObject["flags"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(valueObject["flags"]!.Value<string>(), Is.EqualTo("gi"));
    }

    [Test]
    public void TestCanSerializeArray()
    {
        List<LocalValue> arrayList = new List<LocalValue>()
        {
            LocalValue.String("hello"),
            LocalValue.Number(123),
            LocalValue.Null,
            LocalValue.Boolean(true)
        };
        LocalValue value = LocalValue.Array(arrayList);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("array"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        JArray? valueObject = parsed["value"] as JArray;
        Assert.That(valueObject!.Count, Is.EqualTo(4));

        Assert.That(valueObject[0].Type, Is.EqualTo(JTokenType.Object));
        JObject? itemObject = valueObject[0] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(2));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("string"));
        Assert.That(itemObject.ContainsKey("value"));
        Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(itemObject["value"]!.Value<string>(), Is.EqualTo("hello"));

        Assert.That(valueObject[1].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[1] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(2));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(itemObject.ContainsKey("value"));
        Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Integer));
        Assert.That(itemObject["value"]!.Value<long>(), Is.EqualTo(123));

        Assert.That(valueObject[2].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[2] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(1));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("null"));

        Assert.That(valueObject[3].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[3] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(2));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("boolean"));
        Assert.That(itemObject.ContainsKey("value"));
        Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(itemObject["value"]!.Value<bool>(), Is.EqualTo(true));
    }

    [Test]
    public void TestCanSerializeSet()
    {
        List<LocalValue> arrayList = new List<LocalValue>()
        {
            LocalValue.String("hello"),
            LocalValue.Number(123),
            LocalValue.Null,
            LocalValue.Boolean(true)
        };
        LocalValue value = LocalValue.Set(arrayList);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("set"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Array));
        JArray? valueObject = parsed["value"] as JArray;
        Assert.That(valueObject!.Count, Is.EqualTo(4));

        Assert.That(valueObject[0].Type, Is.EqualTo(JTokenType.Object));
        JObject? itemObject = valueObject[0] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(2));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("string"));
        Assert.That(itemObject.ContainsKey("value"));
        Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.String));
        Assert.That(itemObject["value"]!.Value<string>(), Is.EqualTo("hello"));

        Assert.That(valueObject[1].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[1] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(2));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("number"));
        Assert.That(itemObject.ContainsKey("value"));
        Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Integer));
        Assert.That(itemObject["value"]!.Value<long>(), Is.EqualTo(123));

        Assert.That(valueObject[2].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[2] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(1));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("null"));

        Assert.That(valueObject[3].Type, Is.EqualTo(JTokenType.Object));
        itemObject = valueObject[3] as JObject;
        Assert.That(itemObject!.Count, Is.EqualTo(2));
        Assert.That(itemObject.ContainsKey("type"));
        Assert.That(itemObject["type"]!.Value<string>(), Is.EqualTo("boolean"));
        Assert.That(itemObject.ContainsKey("value"));
        Assert.That(itemObject["value"]!.Type, Is.EqualTo(JTokenType.Boolean));
        Assert.That(itemObject["value"]!.Value<bool>(), Is.EqualTo(true));
    }

    [Test]
    public void TestCanSerializeMap()
    {
        Dictionary<string, LocalValue> dictionary = new Dictionary<string, LocalValue>()
        {
            { "string", LocalValue.String("hello") },
            { "number", LocalValue.Number(123) },
            { "null", LocalValue.Null },
            { "boolean", LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Map(dictionary);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("map"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JObject? valueObject = parsed["value"] as JObject;
        Assert.That(valueObject!.Count, Is.EqualTo(4));
        Assert.That(valueObject.ContainsKey("string"));
        Assert.That(valueObject["string"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(valueObject.ContainsKey("number"));
        Assert.That(valueObject["number"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(valueObject.ContainsKey("null"));
        Assert.That(valueObject["null"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(valueObject.ContainsKey("boolean"));
        Assert.That(valueObject["boolean"]!.Type, Is.EqualTo(JTokenType.Object));
    }

    [Test]
    public void TestCanSerializeObject()
    {
        Dictionary<string, LocalValue> dictionary = new Dictionary<string, LocalValue>()
        {
            { "string", LocalValue.String("hello") },
            { "number", LocalValue.Number(123) },
            { "null", LocalValue.Null },
            { "boolean", LocalValue.Boolean(true) }
        };

        LocalValue value = LocalValue.Object(dictionary);
        string json = JsonConvert.SerializeObject(value);
        var parsed = JObject.Parse(json);
        Assert.That(parsed.Count, Is.EqualTo(2)); 
        Assert.That(parsed.ContainsKey("type"));
        Assert.That(parsed["type"]!.Value<string>(), Is.EqualTo("object"));
        Assert.That(parsed.ContainsKey("value"));
        Assert.That(parsed["value"]!.Type, Is.EqualTo(JTokenType.Object));

        // N.B., We are not validating the content of each object in the map
        // values; it should be sufficient that the serialization of individual
        // values works.
        JObject? valueObject = parsed["value"] as JObject;
        Assert.That(valueObject!.Count, Is.EqualTo(4));
        Assert.That(valueObject.ContainsKey("string"));
        Assert.That(valueObject["string"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(valueObject.ContainsKey("number"));
        Assert.That(valueObject["number"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(valueObject.ContainsKey("null"));
        Assert.That(valueObject["null"]!.Type, Is.EqualTo(JTokenType.Object));
        Assert.That(valueObject.ContainsKey("boolean"));
        Assert.That(valueObject["boolean"]!.Type, Is.EqualTo(JTokenType.Object));
    }

    [Test]
    public void TestCanSerializeMapWithLocalValueAsKeys()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<LocalValue, LocalValue> dictionary = new Dictionary<LocalValue, LocalValue>()
        {
            { LocalValue.String("string"), LocalValue.String("hello") },
            { LocalValue.String("number"), LocalValue.Number(123) },
            { LocalValue.Number(1), LocalValue.Null },
            { LocalValue.NaN, LocalValue.Boolean(true) }
        };
        LocalValue value = LocalValue.Map(dictionary);
        string json = JsonConvert.SerializeObject(value);
    }

    [Test]
    public void TestCanSerializeObjectWithLocalValueAsKeys()
    {
        // N.B., this is an edge case, and we are doing no further validation
        // than that the JSON serializer will properly serialize the object.
        Dictionary<LocalValue, LocalValue> dictionary = new Dictionary<LocalValue, LocalValue>()
        {
            { LocalValue.String("string"), LocalValue.String("hello") },
            { LocalValue.String("number"), LocalValue.Number(123) },
            { LocalValue.Number(1), LocalValue.Null },
            { LocalValue.NaN, LocalValue.Boolean(true) }
        };
        LocalValue value = LocalValue.Object(dictionary);
        string json = JsonConvert.SerializeObject(value);
    }
}