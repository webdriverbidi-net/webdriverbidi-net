// <copyright file="SetPermissionCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Permissions;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.activate command.
/// </summary>
public class SetPermissionCommandParameters : CommandParameters<SetPermissionCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetPermissionCommandParameters"/> class.
    /// </summary>
    /// <param name="permissionName">The name of the permission to set.</param>
    /// <param name="state">the state of the permission to set.</param>
    /// <param name="origin">The origin, usually a URL, for which the permission will be set.</param>
    public SetPermissionCommandParameters(string permissionName, PermissionState state, string origin)
        : this(new PermissionDescriptor(permissionName), state, origin)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SetPermissionCommandParameters"/> class.
    /// </summary>
    /// <param name="descriptor">The descriptor of the permission to set.</param>
    /// <param name="state">the state of the permission to set.</param>
    /// <param name="origin">The origin, usually a URL, for which the permission will be set.</param>
    public SetPermissionCommandParameters(PermissionDescriptor descriptor, PermissionState state, string origin)
    {
        this.Descriptor = descriptor;
        this.State = state;
        this.Origin = origin;
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "permissions.setPermission";

    /// <summary>
    /// Gets or sets the descriptor of the permission to set.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("descriptor")]
    public PermissionDescriptor Descriptor { get; set; }

    /// <summary>
    /// Gets or sets the state of the permission to set.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("state")]
    public PermissionState State { get; set; }

    /// <summary>
    /// Gets or sets the origin, usually a URL, for which to set the permission.
    /// </summary>
    [JsonRequired]
    [JsonPropertyName("origin")]
    public string Origin { get; set; }

    /// <summary>
    /// Gets or sets the embedded origin of the permission.
    /// </summary>
    [JsonPropertyName("embeddedOrigin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? EmbeddedOrigin { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user context for which to set the permission.
    /// </summary>
    [JsonPropertyName("userContext")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserContextId { get; set; }
}
