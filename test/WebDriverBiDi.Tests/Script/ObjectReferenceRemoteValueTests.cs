namespace WebDriverBiDi.Script;

using System.Text.Json;

public class ObjectReferenceRemoteValueTests
{
    [Test]
    public void TestCanDeserializeObjectReferenceRemoteValue()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        // Note: Testing of the multiple types (symbol, function, promise, etc.)
        // that can be deserialized into a ObjectReferenceremoteValue is tested in
        // RemoteValueTests.
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Symbol));
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
    }

    [Test]
    public void TestCanDeserializeObjectReferenceRemoteValueWithMissingHandle()
    {
        string json = """
                      {
                        "type": "symbol",
                        "internalId": "myInternalId"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Symbol));
        Assert.That(result.Handle, Is.Null);
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
    }

    [Test]
    public void TestCanDeserializeObjectReferenceRemoteValueWithMissingInternalId()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Symbol));
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.Null);
    }

    [Test]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        RemoteObjectReference remoteObjectReference = (RemoteObjectReference)localValue;
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
    public void TestCanConvertToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;
        
        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidHandleTypeThrows()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidInternalIdTypeThrows()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle",
                        "internalId": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-symbol-value",
                        "handle": "myHandle"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestConvertingToLocalValueWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "symbol"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToLocalValue(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "symbol"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToRemoteObjectReference(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        ObjectReferenceRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}
