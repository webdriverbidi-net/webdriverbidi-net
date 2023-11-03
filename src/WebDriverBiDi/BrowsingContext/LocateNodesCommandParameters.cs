// <copyright file="LocateNodesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;
using WebDriverBiDi.Script;

/// <summary>
/// Provides parameters for the browsingContext.locateNodes command.
/// </summary>
public class LocateNodesCommandParameters : CommandParameters<LocateNodesCommandResult>
{
    private readonly List<SharedReference> contextNodes = new();
    private string browsingContextId;
    private Locator locator;
    private ulong? maxNodeCount;
    private ResultOwnership? resultOwnership;
    private string? sandbox;
    private SerializationOptions? serializationOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocateNodesCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context in which to locate nodes.</param>
    /// <param name="locator">The locator used to locate nodes.</param>
    public LocateNodesCommandParameters(string browsingContextId, Locator locator)
    {
        this.browsingContextId = browsingContextId;
        this.locator = locator;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "browsingContext.locateNodes";

    /// <summary>
    /// Gets or sets the ID of the browsing context in which to locate nodes.
    /// </summary>
    [JsonPropertyName("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    /// <summary>
    /// Gets or sets the locator used to locate nodes.
    /// </summary>
    [JsonPropertyName("locator")]
    [JsonRequired]
    public Locator Locator { get => this.locator; set => this.locator = value; }

    /// <summary>
    /// Gets or sets the maximum number of nodes to be returned by the command.
    /// When omitted or <see langword="null"/>, the command returns all located nodes.
    /// </summary>
    [JsonPropertyName("maxNodeCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? MaxNodeCount { get => this.maxNodeCount; set => this.maxNodeCount = value; }

    /// <summary>
    /// Gets or sets the ownership model to use for references to located nodes.
    /// </summary>
    [JsonPropertyName("resultOwnership")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ResultOwnership? ResultOwnership { get => this.resultOwnership; set => this.resultOwnership = value; }

    /// <summary>
    /// Gets or sets the sandbox into which references to located nodes will be placed, if any.
    /// </summary>
    [JsonPropertyName("sandbox")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Sandbox { get => this.sandbox; set => this.sandbox = value; }

    /// <summary>
    /// Gets or sets the serialization options for serializing located node references.
    /// </summary>
    [JsonPropertyName("serializationOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SerializationOptions? SerializationOptions { get => this.serializationOptions; set => this.serializationOptions = value; }

    /// <summary>
    /// Gets the list of context nodes within which to locate child nodes.
    /// If empty, nodes will be located from the top-level document.
    /// </summary>
    [JsonIgnore]
    public List<SharedReference> ContextNodes => this.contextNodes;

    /// <summary>
    /// Gets the list of context nodes for serialization purposes.
    /// </summary>
    [JsonPropertyName("contextNodes")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal IList<SharedReference>? SerializableContextNodes
    {
        get
        {
            if (this.contextNodes.Count == 0)
            {
                return null;
            }

            return this.contextNodes.AsReadOnly();
        }
    }
}
