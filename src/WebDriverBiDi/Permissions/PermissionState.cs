// <copyright file="PermissionState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Permissions;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Values used for the granting or revoking of permissions.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<PermissionState>))]
public enum PermissionState
{
    /// <summary>
    /// The permission should be granted to the user.
    /// </summary>
    Granted,

    /// <summary>
    /// The permission should be denied to the user.
    /// </summary>
    Denied,

    /// <summary>
    /// The user should be prompted for permission access.
    /// </summary>
    Prompt,
}
