// <copyright file="SetGeolocationOverrideCoordinatesCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setGeolocationOverride command.
/// </summary>
public class SetGeolocationOverrideCoordinatesCommandParameters : SetGeolocationOverrideCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetGeolocationOverrideCoordinatesCommandParameters"/> class.
    /// </summary>
    public SetGeolocationOverrideCoordinatesCommandParameters()
        : base()
    {
    }

    /// <summary>
    /// Gets or sets the coordinates to which to override the geolocation. When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("coordinates")]
    [JsonInclude]
    public GeolocationCoordinates? Coordinates { get; set; }
}
