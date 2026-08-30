// <copyright file="SharedReferenceInfo.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Provides information about a received reference to a remote object identified by a shared ID, such as a
/// node. Use <see cref="ToSharedReference"/> to obtain a mutable <see cref="SharedReference"/> that can be
/// passed as an argument to subsequent commands.
/// </summary>
public record SharedReferenceInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SharedReferenceInfo"/> class.
    /// </summary>
    [JsonConstructor]
    internal SharedReferenceInfo()
    {
    }

    /// <summary>
    /// Gets the shared ID of the remote object.
    /// </summary>
    [JsonPropertyName("sharedId")]
    [JsonRequired]
    [JsonInclude]
    public string SharedId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the handle of the remote object, if any.
    /// </summary>
    [JsonPropertyName("handle")]
    [JsonInclude]
    public string? Handle { get; internal set; }

    /// <summary>
    /// Converts this received reference to a mutable <see cref="SharedReference"/> for use as an argument
    /// to subsequent commands.
    /// </summary>
    /// <returns>A <see cref="SharedReference"/> representing this reference.</returns>
    public SharedReference ToSharedReference() => new(this.SharedId) { Handle = this.Handle };
}
