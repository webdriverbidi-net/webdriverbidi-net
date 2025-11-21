// <copyright file="ScreenOrientation.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setScreenOrientationOverride command.
/// </summary>
public class ScreenOrientation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenOrientation"/> class.
    /// </summary>
    /// <param name="naturalOrientation">The natural screen orientation of the emulated display.</param>
    /// <param name="emulatedOrientationType">The current orientation of the emulated display.</param>
    public ScreenOrientation(ScreenOrientationNatural naturalOrientation, ScreenOrientationType emulatedOrientationType)
    {
        this.NaturalScreenOrientation = naturalOrientation;
        this.ScreenOrientationType = emulatedOrientationType;
    }

    /// <summary>
    /// Gets or sets the natural screen orientation of the emulated display.
    /// </summary>
    [JsonPropertyName("natural")]
    public ScreenOrientationNatural NaturalScreenOrientation { get; set; }

    /// <summary>
    /// Gets or sets the current orientation of the emulated display.
    /// </summary>
    [JsonPropertyName("type")]
    public ScreenOrientationType ScreenOrientationType { get; set; }
}
