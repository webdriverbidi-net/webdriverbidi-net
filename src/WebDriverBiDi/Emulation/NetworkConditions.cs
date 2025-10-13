// <copyright file="NetworkConditions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// The abstract base class for network conditions to emulate.
/// </summary>
[JsonDerivedType(typeof(NetworkConditionsOffline))]
public abstract class NetworkConditions
{
    /// <summary>
    /// Gets the type of clip rectangle.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}
