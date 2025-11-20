namespace WebDriverBiDi.Script;

using System.Text.Json;
using WebDriverBiDi.JsonConverters;

[TestFixture]
public class NodePropertiesTests
{
    private JsonSerializerOptions deserializationOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
    };

    [Test]
    public void TestCanDeserialize()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Null);
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
    public void TestCopySemantics()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        NodeProperties copy = nodeProperties with { };
        Assert.That(copy, Is.EqualTo(nodeProperties));
    }

    [Test]
    public void TestDeserializeWithMissingNodeTypeThrows()
    {
        string json = """
                      {
                        "childNodeCount": 0
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidNodeTypeThrows()
    {
        string json = """
                      {
                        "nodeType": {},
                        "childNodeCount": 0
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithMissingChildNodeCountThrows()
    {
        string json = """
                      {
                        "nodeType": 1
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidChildNodeCountTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalNodeValue()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "nodeValue": "myNodeValue"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.EqualTo("myNodeValue"));
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Null);
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithInvalidNodeValueTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "nodeValue": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalLocalName()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "localName": "myLocalName"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.EqualTo("myLocalName"));
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Null);
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithInvalidLocalNameTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "localName": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalNamespaceUri()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "namespaceURI": "myNamespace"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.EqualTo("myNamespace"));
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Null);
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithInvalidNamespaceUriTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "namespaceURI": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Not.Null);
            Assert.That(nodeProperties.Attributes, Has.Count.EqualTo(1));
            Assert.That(nodeProperties.Attributes, Contains.Key("attributeName"));
            Assert.That(nodeProperties.Attributes!["attributeName"], Is.EqualTo("attributeValue"));
            Assert.That(nodeProperties.Children, Is.Null);
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
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
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Not.Null);
            Assert.That(nodeProperties.Children, Has.Count.EqualTo(1));
            Assert.That(nodeProperties.Children![0].HasValue, Is.True);
            Assert.That(nodeProperties.Children[0].Value, Is.TypeOf<NodeProperties>());
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
    public void TestCanDeserializeWithOptionalEmptyChildren()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "children": []
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Not.Null);
            Assert.That(nodeProperties.Children!, Is.Empty);
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithInvalidChildrenTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "children": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestDeserializeWithInvalidChildrenElementTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "children": [ "invalid" ]
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalModeValue()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "mode": "open"
                      }
                      """;
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.Mode, Is.EqualTo(ShadowRootMode.Open));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Null);
            Assert.That(nodeProperties.ShadowRoot, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithInvalidModeValueTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "mode": {}
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
    public void TestDeserializeWithInvalidModeValueThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "mode": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<WebDriverBiDiException>());
    }

    [Test]
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
        NodeProperties? nodeProperties = JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(nodeProperties.NodeType, Is.EqualTo(1));
            Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
            Assert.That(nodeProperties.NodeValue, Is.Null);
            Assert.That(nodeProperties.LocalName, Is.Null);
            Assert.That(nodeProperties.NamespaceUri, Is.Null);
            Assert.That(nodeProperties.Attributes, Is.Null);
            Assert.That(nodeProperties.Children, Is.Null);
            Assert.That(nodeProperties.ShadowRoot, Is.Not.Null);
            Assert.That(nodeProperties.ShadowRoot!.HasValue, Is.True);
            Assert.That(nodeProperties.ShadowRoot.Value, Is.TypeOf<NodeProperties>());
            Assert.That(nodeProperties.Mode, Is.Null);
        });
    }

    [Test]
    public void TestDeserializeWithInvalidShadowRootTypeThrows()
    {
        string json = """
                      {
                        "nodeType": 1,
                        "childNodeCount": 0,
                        "shadowRoot": "invalid"
                      }
                      """;
        Assert.That(() => JsonSerializer.Deserialize<NodeProperties>(json, deserializationOptions), Throws.InstanceOf<JsonException>());
    }
}
