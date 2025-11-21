// <copyright file="SetGeolocationOverrideErrorCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setGeolocationOverride command.
/// </summary>
public class SetGeolocationOverrideErrorCommandParameters : SetGeolocationOverrideCommandParameters
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetGeolocationOverrideErrorCommandParameters"/> class.
    /// </summary>
    public SetGeolocationOverrideErrorCommandParameters()
        : base()
    {
    }

    /// <summary>
    /// Gets or sets the error to return when emulating the geolocation.
    /// </summary>
    [JsonPropertyName("error")]
    [JsonInclude]
    public GeolocationPositionError Error { get; set; } = new();
}
