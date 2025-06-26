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
    private ScreenOrientationNatural naturalOrientation;
    private ScreenOrientationType emulatedOrientationType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenOrientation"/> class.
    /// </summary>
    /// <param name="naturalOrientation">The natural screen orientation of the emulated display.</param>
    /// <param name="emulatedOrientationType">The current orientation of the emulated display.</param>
    public ScreenOrientation(ScreenOrientationNatural naturalOrientation, ScreenOrientationType emulatedOrientationType)
    {
        this.naturalOrientation = naturalOrientation;
        this.emulatedOrientationType = emulatedOrientationType;
    }

    /// <summary>
    /// Gets or sets the natural screen orientation of the emulated display.
    /// </summary>
    [JsonPropertyName("natural")]
    public ScreenOrientationNatural NaturalScreenOrientation { get => this.naturalOrientation; set => this.naturalOrientation = value; }

    /// <summary>
    /// Gets or sets the current orientation of the emulated display.
    /// </summary>
    [JsonPropertyName("type")]
    public ScreenOrientationType ScreenOrientationType { get => this.emulatedOrientationType; set => this.emulatedOrientationType = value; }
}
