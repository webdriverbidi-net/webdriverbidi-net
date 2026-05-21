namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;

public class NumberJsonConverterTests
{
    [Fact]
    public void TestDeserializingValidFloatingPointNumber()
    {
        string json = "3.14159";
        double? result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.Equal(3.14159, result);
    }

    [Fact]
    public void TestDeserializingInteger()
    {
        string json = "12345";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.Equal(12345, result);
    }

    [Fact]
    public void TestDeserializingInfinity()
    {
        string json = "\"Infinity\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.Equal(double.PositiveInfinity, result);
    }

    [Fact]
    public void TestDeserializingNegativeInfinity()
    {
        string json = "\"-Infinity\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.Equal(double.NegativeInfinity, result);
    }

    [Fact]
    public void TestDeserializingNaN()
    {
        string json = "\"NaN\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.Equal(double.NaN, result);
    }

    [Fact]
    public void TestDeserializingNegativeZero()
    {
        string json = "\"-0\"";
        double result = JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } });
        Assert.Equal(-0.0, result);
    }

    [Fact]
    public void TestDeserializingInvalidStringThrows()
    {
        string json = "\"not-a-number\"";
        Assert.Equal($"Invalid value 'not-a-number' for 'value' property of number", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } })).Message);
    }

    [Fact]
    public void TestDeserializingInvalidDataTypeThrows()
    {
        string json = "false";
        Assert.Contains($"Unexpected token parsing number.", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<double>(json, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } })).Message);
    }

    [Fact]
    public void TestSerializationThrows()
    {
        double value = 3.14159;
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Serialize(value, new JsonSerializerOptions { Converters = { new NumberJsonConverter() } }));
    }
}
