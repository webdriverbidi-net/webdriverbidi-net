// <copyright file="PermissionDescriptor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Permissions;

using System.Text.Json.Serialization;

/// <summary>
/// Represents a descriptor for a browser permission.
/// </summary>
public class PermissionDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionDescriptor"/> class.
    /// </summary>
    /// <param name="name">The name of the permission.</param>
    public PermissionDescriptor(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Gets or sets the name of the permission.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets the dictionary containing additional descriptor members to send for the permission.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Permissions specification converts the descriptor to the WebIDL descriptor type of
    /// the named permission, and for several permissions that type defines members beyond
    /// <c>name</c> — for example, the <c>midi</c> permission's <c>sysex</c> member, or the
    /// <c>camera</c> permission's <c>panTiltZoom</c> member. Entries added to this dictionary
    /// are serialized as additional members of the descriptor object.
    /// </para>
    /// <para>
    /// An entry may not reuse the name of a property this type already serializes; doing so would write that name
    /// twice in the same JSON object, and a JSON object with a duplicate name has no defined meaning. Sending a
    /// command that contains such an entry throws <see cref="WebDriverBiDiSerializationException"/> rather than
    /// emitting the ambiguous payload. Set the typed property instead.
    /// </para>
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, object?> AdditionalData { get; } = [];
}
