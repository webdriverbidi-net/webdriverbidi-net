namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;

[TestFixture]
public class NumberJsonConverterTests
{
    [Test]
    public void TestDeserializingValidFloatingPointNumber()
    {
        string json = "3.14159";
        double? result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.That(result, Is.EqualTo(3.14159));
    }

    [Test]
    public void TestDeserializingInteger()
    {
        string json = "12345";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.That(result, Is.EqualTo(12345));
    }

    [Test]
    public void TestDeserializingInfinity()
    {
        string json = "\"Infinity\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.That(result, Is.EqualTo(double.PositiveInfinity));
    }

    [Test]
    public void TestDeserializingNegativeInfinity()
    {
        string json = "\"-Infinity\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.That(result, Is.EqualTo(double.NegativeInfinity));
    }

    [Test]
    public void TestDeserializingNaN()
    {
        string json = "\"NaN\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.That(result, Is.EqualTo(double.NaN));
    }

    [Test]
    public void TestDeserializingNegativeZero()
    {
        string json = "\"-0\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.That(result, Is.EqualTo(-0.0));
    }

    [Test]
    public void TestDeserializingInvalidStringThrows()
    {
        string json = "\"not-a-number\"";
        Assert.That(() => JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.EqualTo($"Invalid value 'not-a-number' for 'value' property of number"));
    }

    [Test]
    public void TestDeserializingInvalidDataTypeThrows()
    {
        string json = "false";
        Assert.That(() => JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } }), Throws.InstanceOf<JsonException>().With.Message.Contains($"Unexpected token parsing number."));
    }

    [Test]
    public void TestSerializationThrows()
    {
        double value = 3.14159;
        Assert.That(() => JsonSerializer.Serialize(value, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } }), Throws.InstanceOf<NotSupportedException>());
    }
}
