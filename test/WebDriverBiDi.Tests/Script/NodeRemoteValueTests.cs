namespace WebDriverBiDi.Script;

using System.Text.Json;

[TestFixture]
public class NodeRemoteValueTests
{
    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Node));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.NodeType, Is.EqualTo(1));
        Assert.That(result.Value.ChildNodeCount, Is.Zero);
        Assert.That(result.Handle, Is.Null);
        Assert.That(result.InternalId, Is.Null);    
        Assert.That(result.SharedId, Is.Null);
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Node));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.NodeType, Is.EqualTo(1));
        Assert.That(result.Value.ChildNodeCount, Is.Zero);
        Assert.That(result.Handle, Is.Null);
        Assert.That(result.InternalId, Is.Null);
        Assert.That(result.SharedId, Is.Null);
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Node));
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value.NodeType, Is.EqualTo(1));
        Assert.That(result.Value.ChildNodeCount, Is.Zero);
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
        Assert.That(result.SharedId, Is.Null);
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Node));
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Handle, Is.EqualTo("myHandle"));
        Assert.That(result.InternalId, Is.EqualTo("myInternalId"));
        Assert.That(result.SharedId, Is.EqualTo("mySharedId"));
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Type, Is.EqualTo(RemoteValueType.Node));
        NodeProperties nodeProperties = result.GetNodeProperties();
        Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.ChildNodeCount, Is.Zero);
    }

    [Test]
    public void TestGettingNodePropertiesWithNullValueThrows()
    {
        string json = """
                      {
                        "type": "node"
                      }
                      """;

        NodeRemoteValue? result = JsonSerializer.Deserialize<NodeRemoteValue>(json);
        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.GetNodeProperties(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        LocalValue localValue = result.ToLocalValue();
        SharedReference sharedReference = (SharedReference)localValue;
        Assert.That(sharedReference.SharedId, Is.EqualTo("mySharedId"));
        Assert.That(sharedReference.Handle, Is.Null);
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.Null);
        RemoteObjectReference remoteObjectReference = result.ToRemoteObjectReference();
        Assert.That(remoteObjectReference.Handle, Is.EqualTo("myHandle"));
        Assert.That(remoteObjectReference.SharedId, Is.Null);
    }

    [Test]
    public void TestDeserializingNodeRemoteValueWithInvalidValueTypeThrows()
    {
        string json = """
                      {
                        "type": "node",
                        "value": 2133
                      }
                      """;

        Assert.That(() => JsonSerializer.Deserialize<NodeRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<NodeRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(() => JsonSerializer.Deserialize<ObjectReferenceRemoteValue>(json), Throws.InstanceOf<JsonException>());
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToLocalValue(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
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

        Assert.That(result, Is.Not.Null);
        Assert.That(() => result.ToRemoteObjectReference(), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
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
        Assert.That(result, Is.Not.Null);
        NodeRemoteValue copy = result with { };
        Assert.That(copy, Is.EqualTo(result));
        Assert.That(copy, Is.Not.SameAs(result));
    }
}