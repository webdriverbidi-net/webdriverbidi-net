// <copyright file="GeolocationPositionError.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// A data class representing an error in retrieving a geolocation.
/// </summary>
public class GeolocationPositionError
{
    /// <summary>
    /// Gets the type of the error.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "positionUnavailable";
}
