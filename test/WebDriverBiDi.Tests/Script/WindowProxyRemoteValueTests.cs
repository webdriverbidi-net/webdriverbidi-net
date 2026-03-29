namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class WindowProxyRemoteValueTests
{
    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Window));
        Assert.That(result.Value.Context, Is.EqualTo("myContextid"));
        Assert.That(result.Handle, Is.Null);
        Assert.That(result.InternalId, Is.Null);    
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Window));
        Assert.That(result.Value.Context, Is.EqualTo("myContextid"));
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
    }


    [Test]
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
                        "type": "window",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "value": {
                          "context": "myContextid"
                        }
                      }
                      """;
        
        WindowProxyRemoteValue? result = JsonSerializer.Deserialize<WindowProxyRemoteValue>(json);

        Assert.That(result, Is.Not.Null);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
    public void TestDeserializingWindowProxyRemoteValueWithMissingValueThrows()
    {
        string json = """
                      {
                        "type": "window"
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializingWindowProxyRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "window",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<WindowProxyRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToRemoteObjectReference(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        WindowProxyRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}