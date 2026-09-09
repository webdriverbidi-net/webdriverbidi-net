namespace WebDriverBiDi.Script;

using System.Text.Json;

public class NumberRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeNumberRemoteValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Number, result.Type);
        Assert.Equal(3.14159, result.Value);
    }

    [Fact]
    public void TestCanDeserializeNumberRemoteValueWithInteger()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Number, result.Type);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("number", argumentLocalValue.Type);
        Assert.IsType<double>(argumentLocalValue.Value);
        Assert.Equal(3.14159, argumentLocalValue.Value);
    }

    [Fact]
    public void TestCanConvertToLong()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(42, result.ToLong());
    }

    [Fact]
    public void TestCanConvertToInt()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(42, result.ToInt());
    }

    [Fact]
    public void TestCanUseImplicitConversionToInt()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        int intValue = result;
        Assert.Equal(42, intValue);
    }

    [Fact]
    public void TestCanUseImplicitConversionToLong()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        long longValue = result;
        Assert.Equal(42, longValue);
    }

    [Theory]
    [InlineData(2.7, 2)]
    [InlineData(-2.7, -2)]
    [InlineData(2.5, 2)]
    [InlineData(3.5, 3)]
    public void TestConvertToLongTruncatesTowardZero(double value, long expected)
    {
        string json = $$"""
                      {
                        "type": "number",
                        "value": {{value}}
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result.ToLong());
    }

    [Theory]
    [InlineData(2.7, 2)]
    [InlineData(-2.7, -2)]
    [InlineData(2.5, 2)]
    [InlineData(3.5, 3)]
    public void TestConvertToIntTruncatesTowardZero(double value, int expected)
    {
        string json = $$"""
                      {
                        "type": "number",
                        "value": {{value}}
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result.ToInt());
    }

    /// <summary>
    /// Pins the conversions for finite values that lie outside the target range. These take the same
    /// branches as the infinities, so they add no coverage — they pin the contract. They are also the
    /// inputs whose behaviour was platform-defined before the saturation fix, so an unguarded cast
    /// returning an unspecified value would look like a passing build without them.
    /// </summary>
    /// <param name="value">The value as it appears in the protocol payload.</param>
    /// <param name="expected">The expected result of the conversion.</param>
    [Theory]
    [InlineData(3000000000, int.MaxValue)]
    [InlineData(-3000000000, int.MinValue)]
    [InlineData(1e300, int.MaxValue)]
    [InlineData(-1e300, int.MinValue)]
    public void TestConvertToIntSaturatesForFiniteOutOfRangeValues(double value, int expected)
    {
        string json = $$"""
                      {
                        "type": "number",
                        "value": {{value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}}
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result.ToInt());
    }

    /// <summary>
    /// Pins the same contract for <see cref="NumberRemoteValue.ToLong"/>.
    /// </summary>
    /// <param name="value">The value as it appears in the protocol payload.</param>
    /// <param name="expected">The expected result of the conversion.</param>
    [Theory]
    [InlineData(1e300, long.MaxValue)]
    [InlineData(-1e300, long.MinValue)]
    public void TestConvertToLongSaturatesForFiniteOutOfRangeValues(double value, long expected)
    {
        string json = $$"""
                      {
                        "type": "number",
                        "value": {{value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)}}
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result.ToLong());
    }

    /// <summary>
    /// Pins the boundary values that must <em>not</em> saturate.
    /// </summary>
    /// <remarks>
    /// <c>long.MaxValue</c> is not exactly representable as a <see cref="double"/>: the nearest double is
    /// 9223372036854775808, one greater. 9223372036854774784 is the largest double strictly below
    /// <c>long.MaxValue</c>, so it must convert to itself rather than saturating. This is the row that
    /// fails if the guard is ever "simplified" to a comparison against a different constant, or if the
    /// comparison operator is loosened — a change that would otherwise look harmless and silently clamp
    /// every large value to <c>long.MaxValue</c>. <c>int.MaxValue</c> <em>is</em> exactly representable,
    /// so the equivalent int boundary saturates to the same number either way and is included to record
    /// that the two types differ here for a reason.
    /// </remarks>
    [Fact]
    public void TestConvertDoesNotSaturateAtTheRepresentableBoundary()
    {
        NumberRemoteValue? largestDoubleBelowLongMaxValue = JsonSerializer.Deserialize<NumberRemoteValue>(
            """
            {
              "type": "number",
              "value": 9223372036854774784
            }
            """);
        Assert.NotNull(largestDoubleBelowLongMaxValue);
        Assert.Equal(9223372036854774784L, largestDoubleBelowLongMaxValue.ToLong());

        NumberRemoteValue? justBelowIntMaxValue = JsonSerializer.Deserialize<NumberRemoteValue>(
            """
            {
              "type": "number",
              "value": 2147483646
            }
            """);
        Assert.NotNull(justBelowIntMaxValue);
        Assert.Equal(2147483646, justBelowIntMaxValue.ToInt());
    }

    [Theory]
    [InlineData("NaN", 0L)]
    [InlineData("Infinity", long.MaxValue)]
    [InlineData("-Infinity", long.MinValue)]
    public void TestConvertToLongSaturatesForSpecialValues(string specialValue, long expected)
    {
        string json = $$"""
                      {
                        "type": "number",
                        "value": "{{specialValue}}"
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result.ToLong());
    }

    [Theory]
    [InlineData("NaN", 0)]
    [InlineData("Infinity", int.MaxValue)]
    [InlineData("-Infinity", int.MinValue)]
    public void TestConvertToIntSaturatesForSpecialValues(string specialValue, int expected)
    {
        string json = $$"""
                      {
                        "type": "number",
                        "value": "{{specialValue}}"
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result.ToInt());
    }

    [Fact]
    public void TestCanUseImplicitConversionToDouble()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 42.0
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);

        Assert.NotNull(result);
        double doubleValue = result;
        Assert.Equal(42, doubleValue);
    }

    [Fact]
    public void TestDeserializingNumberRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "number"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NumberRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingNumberRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "number",
                        "value": {}
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NumberRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingNumberRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-number",
                        "value": 3.14159
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NumberRemoteValue>(json));
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "number",
                        "value": 3.14159
                      }
                      """;

        NumberRemoteValue? result = JsonSerializer.Deserialize<NumberRemoteValue>(json);
        Assert.NotNull(result);
        NumberRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
