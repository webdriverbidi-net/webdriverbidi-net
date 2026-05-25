namespace WebDriverBiDi.Script;

using System.Text.Json;

public class WindowProxyRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeWindowProxyRemoteValue()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;

        WindowProxyRemoteValue? result = JsonSerializer.Deserialize<WindowProxyRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Window, result.Type);
        Assert.Equal("myContextid", result.Value.Context);
        Assert.Null(result.Handle);
        Assert.Null(result.InternalId);
    }

    [Fact]
    public void TestCanDeserializeWindowProxyRemoteValueWithHandle()
    {
        string json = """
                      {
                        "type": "window",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;

        WindowProxyRemoteValue? result = JsonSerializer.Deserialize<WindowProxyRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Window, result.Type);
        Assert.Equal("myContextid", result.Value.Context);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "window",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;

        WindowProxyRemoteValue? result = JsonSerializer.Deserialize<WindowProxyRemoteValue>(json);

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
                        "type": "window",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;

        WindowProxyRemoteValue? result = JsonSerializer.Deserialize<WindowProxyRemoteValue>(json);

        Assert.NotNull(result);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
    public void TestDeserializingWindowProxyRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "window"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingWindowProxyRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingWindowProxyRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-window-value",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingWindowProxyRemoteValueWithInvalidHandleTypeThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContextid"
                        },
                        "handle": 1234,
                        "internalId": "myInternalId"
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingWindowProxyRemoteValueWithInvalidInternalIdTypeThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContextid"
                        },
                        "handle": "myHandle",
                        "internalId": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json));
    }

    [Fact]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;

        WindowProxyRemoteValue? result = JsonSerializer.Deserialize<WindowProxyRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToRemoteObjectReference());
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "window",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;

        WindowProxyRemoteValue? result = JsonSerializer.Deserialize<WindowProxyRemoteValue>(json);
        Assert.NotNull(result);
        WindowProxyRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
