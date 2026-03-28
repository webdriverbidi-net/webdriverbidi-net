namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class DateRemoteValueTests
{
    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Date));
        Assert.That(result.Value, Is.EqualTo(expectedDate));
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        LocalArgumentValue argumentLocalValue = (LocalArgumentValue)localValue;
        Assert.That(argumentLocalValue.Type, Is.EqualTo("date"));
        Assert.That(argumentLocalValue.Value, Is.InstanceOf<DateTime>());
        Assert.That(argumentLocalValue.Value, Is.EqualTo(expectedDate));
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        DateTime dateTimeValue = result;
        Assert.That(dateTimeValue, Is.EqualTo(expectedDate));
    }

    [Test]
    public void TestDeserializingDateRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "date"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<DateRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingDateRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<DateRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingDateRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<DateRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToRemoteObjectReference(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "date",
                        "value": "2020-07-19T23:47:19.856Z"
                      }
                      """;

        DateRemoteValue? result = JsonSerializer.Deserialize<DateRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        DateRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
