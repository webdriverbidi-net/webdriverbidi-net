// <copyright file="RemoteObjectReference.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a remote reference to an existing ECMAScript object in the browser.
/// </summary>
public record RemoteObjectReference : RemoteReference
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteObjectReference"/> class.
    /// </summary>
    /// <param name="handle">The handle of the remote object.</param>
    public RemoteObjectReference(string handle)
        : base(handle, null)
    {
    }

    /// <summary>
    /// Gets or sets the handle of the remote object.
    /// </summary>
    [JsonIgnore]
    public string Handle { get => this.InternalHandle!; set => this.InternalHandle = value; }

    /// <summary>
    /// Gets or sets the shard ID of the remote object.
    /// </summary>
    [JsonIgnore]
    public string? SharedId { get => this.InternalSharedId; set => this.InternalSharedId = value; }
}
