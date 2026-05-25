namespace WebDriverBiDi.Script;

using System.Text.Json;

public class DateRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeDateRemoteValue()
    {
        DateTime expectedDate = new(2020, 7, 19, 23, 47, 19, 856, DateTimeKind.Utc);
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Date, result.Type);
        Assert.Equal(expectedDate, result.Value);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        DateTime expectedDate = new(2020, 7, 19, 23, 47, 19, 856, DateTimeKind.Utc);
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.Equal("date", argumentLocalValue.Type);
        Assert.IsType<DateTime>(argumentLocalValue.Value);
        Assert.Equal(expectedDate, argumentLocalValue.Value);
    }

    [Fact]
    public void TestCanConvertToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);

        Assert.NotNull(result);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
    public void TestCanUseImplicitConversionToDateTime()
    {
        DateTime expectedDate = new(2020, 7, 19, 23, 47, 19, 856, DateTimeKind.Utc);
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);

        Assert.NotNull(result);
        DateTime dateTimeValue = result;
        Assert.Equal(expectedDate, dateTimeValue);
    }

    [Fact]
    public void TestDeserializingDateRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "date"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingDateRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingDateRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<DateRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidHandleTypeThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z",
                        "handle": 1234,
                        "internalId": "myInternalId"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidInternalIdTypeThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z",
                        "handle": "myHandle",
                        "internalId": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToRemoteObjectReference());
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);
        Assert.NotNull(result);
        DateRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
