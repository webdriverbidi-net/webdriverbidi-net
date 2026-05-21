namespace WebDriverBiDi.Script;

using System.Text.Json;

public class NodePropertiesTests
{
    [Fact]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.Null(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);
        NodeProperties copy = nodeProperties with { };

        Assert.Equal(nodeProperties, copy);
        Assert.NotSame(nodeProperties, copy);
    }

    [Fact]
    public void TestDeserializeWithMissingNodeTypeThrows()
    {
        string json = """
                      {
                        "childNodeCount": 0
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidNodeTypeThrows()
    {
        string json = """
                      {
                        "nodeType": {},
                        "childNodeCount": 0
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithMissingChildNodeCountThrows()
    {
        string json = """
                      {
                        "nodeType": 1
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidChildNodeCountTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalNodeValue()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "nodeValue": "myNodeValue"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Equal("myNodeValue", nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.Null(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestDeserializeWithInvalidNodeValueTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "nodeValue": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalLocalName()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "localName": "myLocalName"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Equal("myLocalName", nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.Null(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestDeserializeWithInvalidLocalNameTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "localName": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalNamespaceUri()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "namespaceURI": "myNamespace"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Equal("myNamespace", nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.Null(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestDeserializeWithInvalidNamespaceUriTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "namespaceURI": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalAttributes()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "attributes": {
                          "attributeName": "attributeValue"
                        }
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.NotNull(nodeProperties.Attributes);
        Assert.Single(nodeProperties.Attributes);
        Assert.True(nodeProperties.Attributes.ContainsKey("attributeName"));
        Assert.Equal("attributeValue", nodeProperties.Attributes["attributeName"]);
        Assert.Null(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestAttributesPropertyCachesValue()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "attributes": {
                          "attributeName": "attributeValue"
                        }
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        // Access Attributes property twice to verify caching behavior
        NodeAttributes? firstAccess = nodeProperties.Attributes;
        NodeAttributes? secondAccess = nodeProperties.Attributes;

        Assert.NotNull(firstAccess);
        Assert.Same(firstAccess, secondAccess);
    }

    [Fact]
    public void TestDeserializeWithInvalidAttributesTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "attributes": {
                          "attributeName": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidAttributeNameTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "attributes": {
                          true: "attributeValue"
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidAttributeValueTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "attributes": {
                          "attributeName": []
                        }
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalChildren()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "children": [
                          {
                            "type": "node",
                            "value": {
                              "nodeType": 1,
                              "nodeValue": "",
                              "childNodeCount": 0
                            }
                          }
                        ]
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.NotNull(nodeProperties.Children);
        Assert.Single(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestCanDeserializeWithOptionalEmptyChildren()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "children": []
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.NotNull(nodeProperties.Children);
        Assert.Empty(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestDeserializeWithInvalidChildrenTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "children": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidChildrenElementTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "children": [ "invalid" ]
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalModeValue()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "mode": "open"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Equal(ShadowRootMode.Open, nodeProperties.Mode);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.Null(nodeProperties.Children);
        Assert.Null(nodeProperties.ShadowRoot);
    }

    [Fact]
    public void TestDeserializeWithInvalidModeValueTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "mode": {}
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestDeserializeWithInvalidModeValueThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "mode": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }

    [Fact]
    public void TestCanDeserializeWithOptionalShadowRoot()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "shadowRoot": {
                          "type": "node",
                          "value": {
                            "nodeType": 1,
                            "nodeValue": "",
                            "childNodeCount": 0
                          }
                        }
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json);
        Assert.NotNull(nodeProperties);

        Assert.Equal(1u, nodeProperties.NodeType);
        Assert.Equal(0u, nodeProperties.ChildNodeCount);
        Assert.Null(nodeProperties.NodeValue);
        Assert.Null(nodeProperties.LocalName);
        Assert.Null(nodeProperties.NamespaceUri);
        Assert.Null(nodeProperties.Attributes);
        Assert.Null(nodeProperties.Children);
        Assert.NotNull(nodeProperties.ShadowRoot);
        Assert.Null(nodeProperties.Mode);
    }

    [Fact]
    public void TestDeserializeWithInvalidShadowRootTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "shadowRoot": "invalid"
                      }
                      """;
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<NodeProperties>(json));
    }
}
