// <copyright file="RemoteReference.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a remote reference.
/// </summary>
public record RemoteReference : ArgumentValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteReference"/> class.
    /// </summary>
    /// <param name="handle">The handle of the remote object.</param>
    /// <param name="sharedId">The shared ID of the remote object.</param>
    protected RemoteReference(string? handle, string? sharedId)
    {
        this.InternalHandle = handle;
        this.InternalSharedId = sharedId;
    }

    /// <summary>
    /// Gets the dictionary of additional data about the remote reference.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; } = [];

    /// <summary>
    /// Gets or sets the internally accessible handle of the remote reference.
    /// </summary>
    [JsonPropertyName("handle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    protected internal string? InternalHandle { get; set; }

    /// <summary>
    /// Gets or sets the internally accessible shared ID of the remote reference.
    /// </summary>
    [JsonPropertyName("sharedId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonInclude]
    protected internal string? InternalSharedId { get; set; }
}
