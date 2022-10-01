namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[TestFixture]
public class NodePropertiesTests
{
    [Test]
    public void TestCanDeserialize()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0 }";
        NodeProperties? nodeProperties = JsonConvert.DeserializeObject<NodeProperties>(json);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        Assert.That(nodeProperties.LocalName, Is.Null);
        Assert.That(nodeProperties.NamespaceUri, Is.Null);
        Assert.That(nodeProperties.Attributes, Is.Null);
        Assert.That(nodeProperties.Children, Is.Null);
        Assert.That(nodeProperties.ShadowRoot, Is.Null);
    }

    [Test]
    public void TestDeserializeWithMissingNodeTypeThrows()
    {
        string json = @"{ ""nodeValue"": """", ""childNodeCount"": 0 }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidNodeTypeThrows()
    {
        string json = @"{ ""nodeType"": {}, ""nodeValue"": """", ""childNodeCount"": 0 }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithMissingNodeValueThrows()
    {
        string json = @"{ ""nodeType"": 1, ""childNodeCount"": 0 }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidNodeValueTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": {}}, ""childNodeCount"": 0 }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithMissingChildNodeCountThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """" }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidChildNodeCountTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalLocalName()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""localName"": ""myLocalName"" }";
        NodeProperties? nodeProperties = JsonConvert.DeserializeObject<NodeProperties>(json);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        Assert.That(nodeProperties.LocalName, Is.EqualTo("myLocalName"));
        Assert.That(nodeProperties.NamespaceUri, Is.Null);
        Assert.That(nodeProperties.Attributes, Is.Null);
        Assert.That(nodeProperties.Children, Is.Null);
        Assert.That(nodeProperties.ShadowRoot, Is.Null);
    }

    [Test]
    public void TestDeserializeWithInvalidLocalNameTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""localName"": {}} }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalNamespaceUri()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""namespaceURI"": ""myNamespace"" }";
        NodeProperties? nodeProperties = JsonConvert.DeserializeObject<NodeProperties>(json);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        Assert.That(nodeProperties.LocalName, Is.Null);
        Assert.That(nodeProperties.NamespaceUri, Is.EqualTo("myNamespace"));
        Assert.That(nodeProperties.Attributes, Is.Null);
        Assert.That(nodeProperties.Children, Is.Null);
        Assert.That(nodeProperties.ShadowRoot, Is.Null);
    }

    [Test]
    public void TestDeserializeWithInvalidNamespaceUriTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""namespaceURI"": {}} }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalAttributes()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""attributes"": { ""attributeName"": ""attributeValue"" } }";
        NodeProperties? nodeProperties = JsonConvert.DeserializeObject<NodeProperties>(json);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        Assert.That(nodeProperties.LocalName, Is.Null);
        Assert.That(nodeProperties.NamespaceUri, Is.Null);
        Assert.That(nodeProperties.Attributes, Is.Not.Null);
        Assert.That(nodeProperties.Attributes!.Count, Is.EqualTo(1));
        Assert.That(nodeProperties.Attributes.ContainsKey("attributeName"));
        Assert.That(nodeProperties.Attributes["attributeName"], Is.EqualTo("attributeValue"));
        Assert.That(nodeProperties.Children, Is.Null);
        Assert.That(nodeProperties.ShadowRoot, Is.Null);
    }

    [Test]
    public void TestDeserializeWithInvalidAttributesTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""attributes"": [] }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidAttributeNameTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""attributes"": { {}: ""attributeValue"" } }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestDeserializeWithInvalidAttributeValueTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""attributes"": { ""attrbuteName"": [] } }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalChildren()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""children"": [ { ""type"": ""node"", ""value"": { ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0 } } ] }";
        NodeProperties? nodeProperties = JsonConvert.DeserializeObject<NodeProperties>(json);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        Assert.That(nodeProperties.LocalName, Is.Null);
        Assert.That(nodeProperties.NamespaceUri, Is.Null);
        Assert.That(nodeProperties.Attributes, Is.Null);
        Assert.That(nodeProperties.Children, Is.Not.Null);
        Assert.That(nodeProperties.Children!.Count, Is.EqualTo(1));
        Assert.That(nodeProperties.Children[0].HasValue, Is.True);
        Assert.That(nodeProperties.Children[0].Value, Is.TypeOf<NodeProperties>());
        Assert.That(nodeProperties.ShadowRoot, Is.Null);
    }

    [Test]
    public void TestCanDeserializeWithOptionalEmptyChildren()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""children"": [] }";
        NodeProperties? nodeProperties = JsonConvert.DeserializeObject<NodeProperties>(json);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        Assert.That(nodeProperties.LocalName, Is.Null);
        Assert.That(nodeProperties.NamespaceUri, Is.Null);
        Assert.That(nodeProperties.Attributes, Is.Null);
        Assert.That(nodeProperties.Children, Is.Not.Null);
        Assert.That(nodeProperties.Children!.Count, Is.EqualTo(0));
        Assert.That(nodeProperties.ShadowRoot, Is.Null);
    }

    [Test]
    public void TestDeserializeWithInvalidChildrenTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""children"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonSerializationException>());
    }

    [Test]
    public void TestDeserializeWithInvalidChildrenElementTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""children"": [ ""invalid"" ] }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }

    [Test]
    public void TestCanDeserializeWithOptionalShadowRoot()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""shadowRoot"": { ""type"": ""node"", ""value"": { ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0 } } }";
        NodeProperties? nodeProperties = JsonConvert.DeserializeObject<NodeProperties>(json);
        Assert.That(nodeProperties, Is.Not.Null);
        Assert.That(nodeProperties!.NodeType, Is.EqualTo(1));
        Assert.That(nodeProperties.NodeValue, Is.EqualTo(string.Empty));
        Assert.That(nodeProperties.ChildNodeCount, Is.EqualTo(0));
        Assert.That(nodeProperties.LocalName, Is.Null);
        Assert.That(nodeProperties.NamespaceUri, Is.Null);
        Assert.That(nodeProperties.Attributes, Is.Null);
        Assert.That(nodeProperties.Children, Is.Null);
        Assert.That(nodeProperties.ShadowRoot, Is.Not.Null);
        Assert.That(nodeProperties.ShadowRoot!.HasValue, Is.True);
        Assert.That(nodeProperties.ShadowRoot.Value, Is.TypeOf<NodeProperties>());
    }

    [Test]
    public void TestDeserializeWithInvalidShadowRootTypeThrows()
    {
        string json = @"{ ""nodeType"": 1, ""nodeValue"": """", ""childNodeCount"": 0, ""shadowRoot"": ""invalid"" }";
        Assert.That(() => JsonConvert.DeserializeObject<NodeProperties>(json), Throws.InstanceOf<JsonReaderException>());
    }
}