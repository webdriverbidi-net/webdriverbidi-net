namespace WebDriverBiDi.Script;

using System.Text.Json;

public class NodeRemoteValueTests
{
    [Fact]
    public void TestCanDeserializeNodeRemoteValue()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Node, result.Type);
        Assert.NotNull(result.Value);
        Assert.Equal(1u, result.Value.NodeType);
        Assert.Equal(0u, result.Value.ChildNodeCount);
        Assert.Null(result.Handle);
        Assert.Null(result.InternalId);
        Assert.Null(result.SharedId);
    }

    [Fact]
    public void TestCanDeserializeNodeRemoteValueWithValue()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Node, result.Type);
        Assert.NotNull(result.Value);
        Assert.Equal(1u, result.Value.NodeType);
        Assert.Equal(0u, result.Value.ChildNodeCount);
        Assert.Null(result.Handle);
        Assert.Null(result.InternalId);
        Assert.Null(result.SharedId);
    }

    [Fact]
    public void TestCanDeserializeNodeRemoteValueWithHandle()
    {
        string json = """
                      {
                        "type": "node",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a NodeRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Node, result.Type);
        Assert.NotNull(result.Value);
        Assert.Equal(1u, result.Value.NodeType);
        Assert.Equal(0u, result.Value.ChildNodeCount);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
        Assert.Null(result.SharedId);
    }

    [Fact]
    public void TestCanDeserializeNodeRemoteValueWithSharedId()
    {
        string json = """
                      {
                        "type": "node",
                        "handle": "myHandle",
                        "internalId": "myInternalId",
                        "sharedId": "mySharedId"
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        // Note: Testing of the multiple types (array, set, htmlcollection, nodelist)
        // that can be deserialized into a NodeRemoteValue is tested in
        // RemoteValueTests. Likewise, validation of collection members is also tested
        // there.
        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Node, result.Type);
        Assert.Null(result.Value);
        Assert.Equal("myHandle", result.Handle);
        Assert.Equal("myInternalId", result.InternalId);
        Assert.Equal("mySharedId", result.SharedId);
    }

    [Fact]
    public void TestCanGetNodeProperties()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Equal(RemoteValueType.Node, result.Type);
        NodeProperties nodeProperties = result.GetNodeProperties();
        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
    }

    [Fact]
    public void TestGettingNodePropertiesWithNullValueThrows()
    {
        string json = """
                      {
                        "type": "node"
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);
        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.GetNodeProperties());
    }

    [Fact]
    public void TestCanConvertToLocalValue()
    {
        string json = """
                      {
                        "type": "node",
                        "sharedId": "mySharedId",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        Assert.NotNull(result);
        LocalValue localValue = result.ToLocalValue();
        SharedReference sharedReference = (SharedReference)localValue;
        Assert.Equal("mySharedId", sharedReference.SharedId);
        Assert.Null(sharedReference.Handle);
    }

    [Fact]
    public void TestCanConvertToRemoteObjectReference()
    {
        string json = """
                      {
                        "type": "node",
                        "handle": "myHandle",
                        "internalId": "myInternalId"
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        Assert.NotNull(result);
        Assert.Null(result.Value);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.Equal("myHandle", remoteObjectReference.Handle);
        Assert.Null(remoteObjectReference.SharedId);
    }

    [Fact]
    public void TestDeserializingNodeRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingNodeRemoteValueWithInvalidTypeValueThrows()
    {
        string json = """
                      {
                        "type": "not-node-value",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeRemoteValue>(json));
    }

    [Fact]
    public void TestDeserializingObjectReferenceRemoteValueWithInvalidHandleTypeThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        },
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
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        },
                        "handle": "myHandle",
                        "internalId": 2133
                      }
                      """;

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json));
    }

    [Fact]
    public void TestConvertingToLocalValueWithoutSharedIdThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToLocalValue());
    }

    [Fact]
    public void TestConvertingToRemoteObjectReferenceWithoutHandleThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);

        Assert.NotNull(result);
        Assert.ThrowsAny<WebDriverBiDiException>(() => result.ToRemoteObjectReference());
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "type": "node",
                        "value": {
                          "nodeType": 1,
                          "childNodeCount": 0
                        }
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);
        Assert.NotNull(result);
        NodeRemoteValue copy = result with { };
        Assert.Equal(result, copy);
        Assert.NotSame(result, copy);
    }
}
