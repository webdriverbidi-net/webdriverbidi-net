// <copyright file="PermissionGrant.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Permissions;

/// <summary>
/// The state of a permission for an origin, such as "geolocation" granted to "https://example.com".
/// </summary>
/// <param name="Descriptor">The permission.</param>
/// <param name="State">Whether the permission is granted, denied, or prompted for.</param>
/// <param name="Origin">The origin to which the state applies.</param>
public sealed record PermissionGrant(PermissionDescriptor Descriptor, PermissionState State, string Origin)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionGrant"/> class for a permission named by its name alone.
    /// </summary>
    /// <param name="name">The name of the permission, such as "geolocation".</param>
    /// <param name="state">Whether the permission is granted, denied, or prompted for.</param>
    /// <param name="origin">The origin to which the state applies.</param>
    public PermissionGrant(string name, PermissionState state, string origin)
        : this(new PermissionDescriptor(name), state, origin)
    {
    }
}
