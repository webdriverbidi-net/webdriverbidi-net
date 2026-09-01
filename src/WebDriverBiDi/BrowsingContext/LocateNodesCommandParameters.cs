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
    /// <summary>
    /// Initializes a new instance of the <see cref="LocateNodesCommandParameters"/> class.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context in which to locate nodes.</param>
    /// <param name="locator">The locator used to locate nodes.</param>
    public LocateNodesCommandParameters(string browsingContextId, Locator locator)
    {
        this.BrowsingContextId = browsingContextId;
        this.Locator = locator;
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
    public string BrowsingContextId { get; set; }

    /// <summary>
    /// Gets or sets the locator used to locate nodes.
    /// </summary>
    [JsonPropertyName("locator")]
    public Locator Locator { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of nodes to be returned by the command.
    /// When omitted or <see langword="null"/>, the command returns all located nodes.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 1. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("maxNodeCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(1.0, double.PositiveInfinity)]
    public ulong? MaxNodeCount { get; set; }

    /// <summary>
    /// Gets or sets the serialization options for serializing located node references.
    /// </summary>
    [JsonPropertyName("serializationOptions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SerializationOptions? SerializationOptions { get; set; }

    /// <summary>
    /// Gets the list of nodes within which to locate child nodes.
    /// If empty, nodes will be located from the top-level document.
    /// </summary>
    [JsonIgnore]
    public List<SharedReference> StartNodes { get; } = [];

    /// <summary>
    /// Gets the list of context nodes for serialization purposes.
    /// </summary>
    /// <remarks>
    /// The serializable value is null when the corresponding list is empty, so the property is omitted
    /// from the JSON payload entirely; an empty array is never sent. When the list has entries, they are
    /// sent to the remote end.
    /// </remarks>
    [JsonPropertyName("startNodes")]
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    internal IList<SharedReference>? SerializableStartNodes
    {
        get
        {
            if (this.StartNodes.Count == 0)
            {
                return null;
            }

            return this.StartNodes.AsReadOnly();
        }
    }
}
