// <copyright file="NodeProperties.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing information about a Node object.
/// </summary>
public record NodeProperties
{
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
    public uint NodeType { get; internal set; } = 0;

    /// <summary>
    /// Gets the count of the child nodes.
    /// </summary>
    [JsonPropertyName("childNodeCount")]
    [JsonRequired]
    [JsonInclude]
    public uint ChildNodeCount { get; internal set; } = 0;

    /// <summary>
    /// Gets the value of the node.
    /// </summary>
    [JsonPropertyName("nodeValue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? NodeValue { get; internal set; }

    /// <summary>
    /// Gets the local name of the node.
    /// </summary>
    [JsonPropertyName("localName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? LocalName { get; internal set; }

    /// <summary>
    /// Gets the namespace URI of the node.
    /// </summary>
    [JsonPropertyName("namespaceURI")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public string? NamespaceUri { get; internal set; }

    /// <summary>
    /// Gets a read-only list of the children of the node.
    /// </summary>
    [JsonIgnore]
    public IList<RemoteValue>? Children
    {
        get
        {
            if (this.SerializableChildren is null)
            {
                return null;
            }

            return this.SerializableChildren.AsReadOnly();
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
            if (this.SerializableAttributes is null)
            {
                return null;
            }

            field ??= new NodeAttributes(this.SerializableAttributes);
            return field;
        }
    }

    /// <summary>
    /// Gets the mode of the shadow root, if one is present.
    /// </summary>
    [JsonPropertyName("mode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public ShadowRootMode? Mode { get; internal set; }

    /// <summary>
    /// Gets the RemoteValue representing the shadow root of this node, if available.
    /// </summary>
    [JsonPropertyName("shadowRoot")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    public RemoteValue? ShadowRoot { get; internal set; }

    /// <summary>
    /// Gets or sets the list of child nodes for serialization purposes.
    /// </summary>
    [JsonPropertyName("children")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal List<RemoteValue>? SerializableChildren { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of attributes of this node for serialization purposes.
    /// </summary>
    [JsonPropertyName("attributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    internal Dictionary<string, string>? SerializableAttributes { get; set; }
}
