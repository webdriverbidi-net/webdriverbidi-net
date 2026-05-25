namespace WebDriverBiDi.Script;

using System.Text.Json;

public class ObjectReferenceRemoteValueTests
{
    [Fact]
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
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Symbol, result.Type);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
    }

    [Fact]
    public void TestCanDeserializeObjectReferenceRemoteValueWithMissingHandle()
    {
        string json = """
                      {
                        "type": "symbol",
                        "internalId": "myInternalId"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Symbol, result.Type);
        Assert.Null(result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
    }

    [Fact]
    public void TestCanDeserializeObjectReferenceRemoteValueWithMissingInternalId()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Symbol, result.Type);
        Assert.Equal("myHandle", result.Handle);
        Assert.Null(result.InternalId);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        RemoteObjectReference remoteObjectReference = (RemoteObjectReference)localValue;
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
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

        Assert.NotNull(result);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidHandleTypeThrows()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidInternalIdTypeThrows()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle",
                        "internalId": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-symbol-value",
                        "handle": "myHandle"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
    public void TestConvertingToLocalValueWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "symbol"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToLocalValue());
    }

    [Fact]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "symbol"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToRemoteObjectReference());
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "symbol",
                        "handle": "myHandle"
                      }
                      """;

        ObjectReferenceRemoteValue? result = JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json);
        Assert.NotNull(result);
        ObjectReferenceRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
