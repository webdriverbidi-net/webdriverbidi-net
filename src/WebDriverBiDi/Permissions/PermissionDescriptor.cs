// <copyright file="PermissionDescriptor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Permissions;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.activate command.
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
    [JsonRequired]
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
