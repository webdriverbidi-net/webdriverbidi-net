// <copyright file="NodeProperties.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Script;

using Newtonsoft.Json;

/// <summary>
/// Object containing information about a Node object.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
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
    [JsonProperty("nodeType", Required = Required.Always)]
    public uint NodeType { get => this.nodeType; internal set => this.nodeType = value; }

    /// <summary>
    /// Gets the count of the child nodes.
    /// </summary>
    [JsonProperty("childNodeCount", Required = Required.Always)]
    public uint ChildNodeCount { get => this.childNodeCount; internal set => this.childNodeCount = value; }

    /// <summary>
    /// Gets the value of the node.
    /// </summary>
    [JsonProperty("nodeValue", NullValueHandling = NullValueHandling.Ignore)]
    public string? NodeValue { get => this.nodeValue; internal set => this.nodeValue = value; }

    /// <summary>
    /// Gets the local name of the node.
    /// </summary>
    [JsonProperty("localName", NullValueHandling = NullValueHandling.Ignore)]
    public string? LocalName { get => this.localName; internal set => this.localName = value; }

    /// <summary>
    /// Gets the namespace URI of the node.
    /// </summary>
    [JsonProperty("namespaceURI", NullValueHandling = NullValueHandling.Ignore)]
    public string? NamespaceUri { get => this.namespaceUri; internal set => this.namespaceUri = value; }

    /// <summary>
    /// Gets a read-only list of the children of the node.
    /// </summary>
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
    /// Gets the RemoteValue representing the shadow root of this node, if available.
    /// </summary>
    [JsonProperty("shadowRoot", NullValueHandling = NullValueHandling.Ignore)]
    public RemoteValue? ShadowRoot { get => this.shadowRoot; internal set => this.shadowRoot = value; }

    /// <summary>
    /// Gets or sets the list of child nodes for serialization purposes.
    /// </summary>
    [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
    internal List<RemoteValue>? SerializableChildren { get => this.children; set => this.children = value; }

    /// <summary>
    /// Gets or sets the dictionary of attributes of this node for serialization purposes.
    /// </summary>
    [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
    internal Dictionary<string, string>? SerializableAttributes { get => this.attributesDictionary; set => this.attributesDictionary = value; }
}