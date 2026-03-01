namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;
using System.Text.Json.Serialization;

[TestFixture]
public class FixedDoubleJsonConverterTests
{
    [Test]
    public void TestReadIntegerJsonNumberReturnsDouble()
    {
        string json = """{ "value": 42 }""";
        TestWrapper? result = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value, Is.EqualTo(42.0));
    }

    [Test]
    public void TestReadDecimalJsonNumberReturnsDouble()
    {
        string json = """{ "value": 3.14 }""";
        TestWrapper? result = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value, Is.EqualTo(3.14));
    }

    [Test]
    public void TestWriteIntegerDoubleOutputsWithDecimal()
    {
        TestWrapper wrapper = new() { Value = 42.0 };
        string json = JsonSerializer.Serialize(wrapper);
        Assert.That(json, Does.Contain("42.0"));
    }

    [Test]
    public void TestWriteDecimalDoubleOutputsCorrectly()
    {
        TestWrapper wrapper = new() { Value = 3.14 };
        string json = JsonSerializer.Serialize(wrapper);
        Assert.That(json, Does.Contain("3.14"));
    }

    [Test]
    public void TestWriteVeryPreciseDoublePreservesPrecision()
    {
        double preciseValue = 1.23456789012345;
        TestWrapper wrapper = new() { Value = preciseValue };
        string json = JsonSerializer.Serialize(wrapper);
        Assert.That(json, Does.Contain("1.23456789012345"));
        TestWrapper? deserialized = JsonSerializer.Deserialize<TestWrapper>(json);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.Value, Is.EqualTo(preciseValue));
    }

    private class TestWrapper
    {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(FixedDoubleJsonConverter))]
        public double Value { get; set; }
    }
}
