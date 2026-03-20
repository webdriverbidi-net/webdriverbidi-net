namespace WebDriverBiDi.JsonConverters;

using System.Numerics;
using System.Text.Json;
using WebDriverBiDi.Script;

[TestFixture]
public class RemoteValueJsonConverterTests
{
    [Test]
    public void TestDeserializeStringType()
    {
        string json = """{"type": "string", "value": "hello"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("string"));
        Assert.That(result.HasValue);
        Assert.That(result.ValueAs<string>(), Is.EqualTo("hello"));
    }

    [Test]
    public void TestDeserializeBooleanTrue()
    {
        string json = """{"type": "boolean", "value": true}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("boolean"));
        Assert.That(result.ValueAs<bool>(), Is.True);
    }

    [Test]
    public void TestDeserializeBooleanFalse()
    {
        string json = """{"type": "boolean", "value": false}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("boolean"));
        Assert.That(result.ValueAs<bool>(), Is.False);
    }

    [Test]
    public void TestDeserializeNumberInteger()
    {
        string json = """{"type": "number", "value": 42}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("number"));
        Assert.That(result.Value, Is.InstanceOf<long>());
        Assert.That(result.ValueAs<long>(), Is.EqualTo(42));
    }

    [Test]
    public void TestDeserializeNumberDecimal()
    {
        string json = """{"type": "number", "value": 3.14}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("number"));
        Assert.That(result.ValueAs<double>(), Is.EqualTo(3.14).Within(0.001));
    }

    [Test]
    public void TestDeserializeNumberInfinity()
    {
        string json = """{"type": "number", "value": "Infinity"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("number"));
        Assert.That(result.ValueAs<double>(), Is.EqualTo(double.PositiveInfinity));
    }

    [Test]
    public void TestDeserializeNumberNegativeInfinity()
    {
        string json = """{"type": "number", "value": "-Infinity"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("number"));
        Assert.That(result.ValueAs<double>(), Is.EqualTo(double.NegativeInfinity));
    }

    [Test]
    public void TestDeserializeNumberNaN()
    {
        string json = """{"type": "number", "value": "NaN"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("number"));
        Assert.That(double.IsNaN(result.ValueAs<double>()), Is.True);
    }

    [Test]
    public void TestDeserializeNumberNegativeZero()
    {
        string json = """{"type": "number", "value": "-0"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("number"));
        Assert.That(result.Value, Is.InstanceOf<double>());
        Assert.That(result.ValueAs<double>(), Is.EqualTo(-0.0));
        Assert.That(double.IsNegative(result.ValueAs<double>()), Is.True);
    }

    [Test]
    public void TestDeserializeInvalidNumberStringThrowsJsonException()
    {
        string json = """{"type": "number", "value": "invalid"}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("invalid value"));
    }

    [Test]
    public void TestDeserializeNumberWithInvalidJsonTypeThrowsJsonException()
    {
        string json = """{"type": "number", "value": true}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("invalid type"));
    }

    [Test]
    public void TestDeserializeBigint()
    {
        string json = """{"type": "bigint", "value": "12345678901234567890"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("bigint"));
        Assert.That(result.ValueAs<BigInteger>(), Is.EqualTo(BigInteger.Parse("12345678901234567890")));
    }

    [Test]
    public void TestDeserializeInvalidBigintThrowsJsonException()
    {
        string json = """{"type": "bigint", "value": "not-a-number"}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("cannot parse"));
    }

    [Test]
    public void TestDeserializeNullType()
    {
        string json = """{"type": "null"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("null"));
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void TestDeserializeUndefinedType()
    {
        string json = """{"type": "undefined"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("undefined"));
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void TestDeserializeDate()
    {
        string json = """{"type": "date", "value": "2024-01-15T12:00:00Z"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("date"));
        Assert.That(result.Value, Is.InstanceOf<DateTime>());
        Assert.That(result.ValueAs<DateTime>().Year, Is.EqualTo(2024));
        Assert.That(result.ValueAs<DateTime>().Month, Is.EqualTo(1));
    }

    [Test]
    public void TestDeserializeInvalidDateThrowsJsonException()
    {
        string json = """{"type": "date", "value": "not-a-date"}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("cannot parse"));
    }

    [Test]
    public void TestDeserializeArrayWithNestedRemoteValues()
    {
        string json = """{"type": "array", "value": [{"type": "string", "value": "a"}, {"type": "number", "value": 1}]}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("array"));
        Assert.That(result.Value, Is.InstanceOf<RemoteValueList>());
        RemoteValueList? list = result.ValueAs<RemoteValueList>();
        Assert.That(list, Has.Count.EqualTo(2));
        Assert.That(list[0].ValueAs<string>(), Is.EqualTo("a"));
        Assert.That(list[1].ValueAs<long>(), Is.EqualTo(1));
    }

    [Test]
    public void TestDeserializeMapWithStringKeys()
    {
        string json = """{"type": "map", "value": [["key1", {"type": "string", "value": "val1"}]]}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("map"));
        Assert.That(result.Value, Is.InstanceOf<RemoteValueDictionary>());
        RemoteValueDictionary? dict = result.ValueAs<RemoteValueDictionary>();
        Assert.That(dict, Contains.Key("key1"));
        Assert.That(dict!["key1"].ValueAs<string>(), Is.EqualTo("val1"));
    }

    [Test]
    public void TestDeserializeMapWithObjectKeys()
    {
        string json = """{"type": "map", "value": [[{"type": "string", "value": "objKey"}, {"type": "string", "value": "val1"}]]}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("map"));
        Assert.That(result.Value, Is.InstanceOf<RemoteValueDictionary>());
        RemoteValueDictionary? dict = result.ValueAs<RemoteValueDictionary>();
        Assert.That(dict, Contains.Key("objKey"));
        Assert.That(dict["objKey"].ValueAs<string>(), Is.EqualTo("val1"));
    }

    [Test]
    public void TestDeserializeWithHandleProperty()
    {
        string json = """{"type": "string", "value": "hello", "handle": "handle-123"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Handle, Is.EqualTo("handle-123"));
    }

    [Test]
    public void TestDeserializeWithInternalIdProperty()
    {
        string json = """{"type": "string", "value": "hello", "internalId": "internal-456"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.InternalId, Is.EqualTo("internal-456"));
    }

    [Test]
    public void TestDeserializeNodeWithSharedId()
    {
        string json = """{"type": "node", "value": {"nodeType": 1, "childNodeCount": 0}, "sharedId": "shared-789"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Type, Is.EqualTo("node"));
        Assert.That(result.SharedId, Is.EqualTo("shared-789"));
        Assert.That(result.Value, Is.InstanceOf<NodeProperties>());
    }

    [Test]
    public void TestDeserializeWithMissingTypeThrowsJsonException()
    {
        string json = """{"value": "hello"}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("'type' property"));
    }

    [Test]
    public void TestDeserializeWithNonStringTypeThrowsJsonException()
    {
        string json = """{"type": 123, "value": "hello"}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("type property must be a string"));
    }

    [Test]
    public void TestDeserializeWithEmptyTypeThrowsJsonException()
    {
        string json = """{"type": "", "value": "hello"}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("non-empty"));
    }

    [Test]
    public void TestDeserializeWithInvalidTypeThrowsJsonException()
    {
        string json = """{"type": "invalid-type", "value": "hello"}""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("not a valid RemoteValue type"));
    }

    [Test]
    public void TestDeserializeNonObjectRootThrowsJsonException()
    {
        string json = """["invalid"]""";
        Assert.That(() => JsonSerializer.Deserialize<RemoteValue>(json), Throws.InstanceOf<JsonException>().With.Message.Contains("must be an object"));
    }

    [Test]
    public void TestWriteThrowsNotImplementedException()
    {
        string json = """{"type": "string", "value": "hello"}""";
        RemoteValue? result = JsonSerializer.Deserialize<RemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(() => JsonSerializer.Serialize(result!), Throws.InstanceOf<NotImplementedException>());
    }
}
