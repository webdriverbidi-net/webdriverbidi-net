namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

public class FixedDoubleJsonConverterTests
{
    [Fact]
    public void TestReadIntegerJsonNumberReturnsDouble()
    {
        string json = """{ "value": 42 }""";
        TestWrapper? result = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.NotNull(result);
        Assert.Equal(42.0, result.Value);
    }

    [Fact]
    public void TestReadDecimalJsonNumberReturnsDouble()
    {
        string json = """{ "value": 3.14 }""";
        TestWrapper? result = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.NotNull(result);
        Assert.Equal(3.14, result.Value);
    }

    [Fact]
    public void TestWriteIntegerDoubleOutputsWithDecimal()
    {
        TestWrapper wrapper = new() { Value = 42.0 };
        string json = JsonSerializer.Serialize(wrapper);
        Assert.Contains("42.0", json);
    }

    [Fact]
    public void TestWriteDecimalDoubleOutputsCorrectly()
    {
        TestWrapper wrapper = new() { Value = 3.14 };
        string json = JsonSerializer.Serialize(wrapper);
        Assert.Contains("3.14", json);
    }

    [Fact]
    public void TestWriteVeryPreciseDoublePreservesPrecision()
    {
        double preciseValue = 1.23456789012345;
        TestWrapper wrapper = new() { Value = preciseValue };
        string json = JsonSerializer.Serialize(wrapper);
        Assert.Contains("1.23456789012345", json);
        TestWrapper? deserialized = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(preciseValue, deserialized.Value);
    }

    private class TestWrapper
    {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(FixedDoubleJsonConverter))]
        public double Value { get; set; }
    }
}
