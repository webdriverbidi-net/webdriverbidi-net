namespace WebDriverBiDi.JsonConverters;

using System.Text.Json;

public class ExpandedYearDateTimeJsonConverterTests
{
    private static readonly JsonSerializerOptions ConverterOptions = new() { Converters = { new ExpandedYearDateTimeJsonConverter() } };

    [Fact]
    public void TestDeserializingStandardDate()
    {
        DateTime expected = new(2020, 7, 19, 23, 47, 19, 856, DateTimeKind.Utc);
        DateTime result = JsonSerializer.Deserialize<DateTime>("\"2020-07-19T23:47:19.856Z\"", ConverterOptions);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TestDeserializingInRangeExpandedYearDate()
    {
        DateTime expected = new(2020, 7, 19, 23, 47, 19, 856, DateTimeKind.Utc);
        DateTime result = JsonSerializer.Deserialize<DateTime>("\"+002020-07-19T23:47:19.856Z\"", ConverterOptions);
        Assert.Equal(expected, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void TestDeserializingMaximumJavaScriptDateClampsToMaxValue()
    {
        DateTime result = JsonSerializer.Deserialize<DateTime>("\"+275760-09-13T00:00:00.000Z\"", ConverterOptions);
        Assert.Equal(DateTime.MaxValue, result);
    }

    [Fact]
    public void TestDeserializingMinimumJavaScriptDateClampsToMinValue()
    {
        DateTime result = JsonSerializer.Deserialize<DateTime>("\"-271821-04-20T00:00:00.000Z\"", ConverterOptions);
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void TestDeserializingExpandedYearZeroClampsToMinValue()
    {
        DateTime result = JsonSerializer.Deserialize<DateTime>("\"+000000-01-01T00:00:00.000Z\"", ConverterOptions);
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void TestDeserializingPlainYearZeroClampsToMinValue()
    {
        DateTime result = JsonSerializer.Deserialize<DateTime>("\"0000-01-01T00:00:00.000Z\"", ConverterOptions);
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void TestDeserializingPlainYearZeroLeapDayClampsToMinValue()
    {
        // Year zero is a leap year in the proleptic Gregorian calendar, so
        // February 29 of year zero is a real Date.prototype.toISOString() output.
        DateTime result = JsonSerializer.Deserialize<DateTime>("\"0000-02-29T00:00:00.000Z\"", ConverterOptions);
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void TestDeserializingNonStringTokenThrows()
    {
        Assert.Contains("JSON serialization of date value should be a string, but was True", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("true", ConverterOptions)).Message);
    }

    [Fact]
    public void TestDeserializingArbitraryStringThrows()
    {
        Assert.Contains("Cannot parse invalid value 'some value' for date", Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"some value\"", ConverterOptions)).Message);
    }

    [Fact]
    public void TestDeserializingSignedStringTooShortForExpandedYearThrows()
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"+123456\"", ConverterOptions));
    }

    [Fact]
    public void TestDeserializingExpandedYearWithNonDigitAboveNineThrows()
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"+27576a-01-01T00:00:00.000Z\"", ConverterOptions));
    }

    [Fact]
    public void TestDeserializingExpandedYearWithNonDigitBelowZeroThrows()
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"+12345--01-01T00:00:00.000Z\"", ConverterOptions));
    }

    [Fact]
    public void TestDeserializingInRangeExpandedYearWithInvalidRemainderThrows()
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"+002020-99-19T23:47:19.856Z\"", ConverterOptions));
    }

    [Fact]
    public void TestDeserializingOutOfRangeExpandedYearWithInvalidRemainderThrows()
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"+010000-13-01T00:00:00.000Z\"", ConverterOptions));
    }

    [Fact]
    public void TestDeserializingPlainYearZeroWithInvalidRemainderThrows()
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateTime>("\"0000-99-01T00:00:00.000Z\"", ConverterOptions));
    }

    [Fact]
    public void TestSerializationThrows()
    {
        DateTime value = new(2020, 7, 19, 23, 47, 19, 856, DateTimeKind.Utc);
        Assert.ThrowsAny<NotSupportedException>(() => JsonSerializer.Serialize(value, ConverterOptions));
    }
}
