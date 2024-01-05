// <copyright file="NodeProperties.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing information about a Node object.
/// </summary>
public class NodeProperties
{
    private uint nodeType = 0;
    private uint childNodeCount = 0;
    private string? nodeValue;
    private string? localName;
    private string? namespaceUri;
    private List<RemoteValue>? children;
    private NodeAttributes? attributes;
    private Dictionary<string, string>? attributesDictionary;
    private ShadowRootMode? mode;
    private RemoteValue? shadowRoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeProperties"/> class.
    /// </summary>
    [JsonConstructor]
    internal NodeProperties()
    {
    }

    /// <summary>
    /// Gets the type of node.
    /// </summary>
    [JsonPropertyName("nodeType")]
    [JsonRequired]
    [JsonInclude]
    public uint NodeType { get => this.nodeType; private set => this.nodeType = value; }

    /// <summary>
    /// Gets the count of the child nodes.
    /// </summary>
    [JsonPropertyName("childNodeCount")]
    [JsonRequired]
    [JsonInclude]
    public uint ChildNodeCount { get => this.childNodeCount; private set => this.childNodeCount = value; }

    /// <summary>
    /// Gets the value of the node.
    /// </summary>
    [JsonPropertyName("nodeValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? NodeValue { get => this.nodeValue; private set => this.nodeValue = value; }

    /// <summary>
    /// Gets the local name of the node.
    /// </summary>
    [JsonPropertyName("localName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? LocalName { get => this.localName; private set => this.localName = value; }

    /// <summary>
    /// Gets the namespace URI of the node.
    /// </summary>
    [JsonPropertyName("namespaceURI")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? NamespaceUri { get => this.namespaceUri; private set => this.namespaceUri = value; }

    /// <summary>
    /// Gets a read-only list of the children of the node.
    /// </summary>
    [JsonIgnore]
    public IList<RemoteValue>? Children
    {
        get
        {
            if (this.children is null)
            {
                return null;
            }

            return this.children.AsReadOnly();
        }
    }

    /// <summary>
    /// Gets a read-only dictionary of the attributes of this node.
    /// </summary>
    [JsonIgnore]
    public NodeAttributes? Attributes
    {
        get
        {
            if (this.attributesDictionary is null)
            {
                return null;
            }

            this.attributes ??= new NodeAttributes(this.attributesDictionary);

            return this.attributes;
        }
    }

    /// <summary>
    /// Gets the mode of the shadow root, if one is present.
    /// </summary>
    [JsonPropertyName("mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public ShadowRootMode? Mode { get => this.mode; private set => this.mode = value; }

    /// <summary>
    /// Gets the RemoteValue representing the shadow root of this node, if available.
    /// </summary>
    [JsonPropertyName("shadowRoot")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public RemoteValue? ShadowRoot { get => this.shadowRoot; private set => this.shadowRoot = value; }

    /// <summary>
    /// Gets or sets the list of child nodes for serialization purposes.
    /// </summary>
    [JsonPropertyName("children")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<RemoteValue>? SerializableChildren { get => this.children; set => this.children = value; }

    /// <summary>
    /// Gets or sets the dictionary of attributes of this node for serialization purposes.
    /// </summary>
    [JsonPropertyName("attributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal Dictionary<string, string>? SerializableAttributes { get => this.attributesDictionary; set => this.attributesDictionary = value; }
}
