// <copyright file="RemoteReference.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Script;

using System.Text.Json.Serialization;

/// <summary>
/// Object containing a remote reference.
/// </summary>
[JsonDerivedType(typeof(RemoteObjectReference))]
[JsonDerivedType(typeof(SharedReference))]
public record RemoteReference : LocalValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteReference"/> class.
    /// </summary>
    protected RemoteReference()
    {
    }

    /// <summary>
    /// Gets the dictionary of additional data about the remote reference.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; } = [];
}
