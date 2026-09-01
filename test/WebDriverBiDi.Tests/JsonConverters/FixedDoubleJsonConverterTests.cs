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

    [Theory]
    [InlineData(Math.PI / 2)]
    [InlineData(2 * Math.PI)]
    [InlineData(0.30000000000000004)]
    [InlineData(1e-30)]
    [InlineData(double.Epsilon)]
    [InlineData(double.MaxValue)]
    [InlineData(-0.0)]
    public void TestWriteRoundTripsExactValue(double value)
    {
        TestWrapper wrapper = new() { Value = value };
        string json = JsonSerializer.Serialize(wrapper);
        TestWrapper? deserialized = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(value, deserialized.Value);
    }

    [Fact]
    public void TestWriteInclusiveBoundaryValueDoesNotExceedBoundary()
    {
        // Pointer altitude angles allow pi / 2 as an inclusive maximum; the serialized
        // text must not parse to a value beyond the boundary.
        double halfPi = Math.PI / 2;
        TestWrapper wrapper = new() { Value = halfPi };
        string json = JsonSerializer.Serialize(wrapper);
        TestWrapper? deserialized = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.NotNull(deserialized);
        Assert.True(deserialized.Value <= halfPi);
    }

    [Fact]
    public void TestWriteVerySmallDoubleDoesNotCollapseToZero()
    {
        TestWrapper wrapper = new() { Value = 1e-30 };
        string json = JsonSerializer.Serialize(wrapper);
        TestWrapper? deserialized = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.NotNull(deserialized);
        Assert.NotEqual(0.0, deserialized.Value);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void TestWriteNonFiniteDoubleThrowsJsonException(double value)
    {
        TestWrapper wrapper = new() { Value = value };
        JsonException exception = Assert.Throws<JsonException>(() => JsonSerializer.Serialize(wrapper));
        Assert.Contains("only finite numbers", exception.Message);
    }

    private class TestWrapper
    {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(FixedDoubleJsonConverter))]
        public double Value { get; set; }
    }
}
