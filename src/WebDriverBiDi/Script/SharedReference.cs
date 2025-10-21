// <copyright file="SharedReference.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a remote reference to an object in the browser containing a shared ID, such as a node.
/// </summary>
public record SharedReference : RemoteReference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SharedReference"/> class.
    /// </summary>
    /// <param name="sharedId">The shared ID of the remote object.</param>
    public SharedReference(string sharedId)
        : base(null, sharedId)
    {
    }

    /// <summary>
    /// Gets or sets the shared ID of the remote object.
    /// </summary>
    [JsonIgnore]
    public string SharedId { get => this.InternalSharedId!; set => this.InternalSharedId = value; }

    /// <summary>
    /// Gets or sets the handle of the remote object.
    /// </summary>
    [JsonIgnore]
    public string? Handle { get => this.InternalHandle!; set => this.InternalHandle = value; }
}
