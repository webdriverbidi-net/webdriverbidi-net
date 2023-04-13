// <copyright file="PointerActionProperties.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

/// <summary>
/// Represents the optional properties of a pointer action.
/// </summary>
public class PointerActionProperties
{
    private ulong? width;
    private ulong? height;
    private double? pressure;
    private double? tangentialPressure;
    private ulong? twist;
    private double? altitudeAngle;
    private double? azimuthAngle;
    private long? tiltX;
    private long? tiltY;

    /// <summary>
    /// Gets or sets the width of the pointer in pixels. If omitted, defaults to 1.
    /// </summary>
    public ulong? Width { get => this.width; set => this.width = value; }

    /// <summary>
    /// Gets or sets the height of the pointer in pixels. If omitted, defaults to 1.
    /// </summary>
    public ulong? Height { get => this.height; set => this.height = value; }

    /// <summary>
    /// Gets or sets the pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    public double? Pressure { get => this.pressure; set => this.pressure = value; }

    /// <summary>
    /// Gets or sets the tangential pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    public double? TangentialPressure { get => this.tangentialPressure; set => this.tangentialPressure = value; }

    /// <summary>
    /// Gets or sets the twist of the pointer in degrees, between 0 and 359, on the surface. If omitted, defaults to 0.
    /// </summary>
    public ulong? Twist { get => this.twist; set => this.twist = value; }

    /// <summary>
    /// Gets or sets the altitude angle (angle from the horizontal) of the pointer device. If omitted, defaults to 0.0.
    /// </summary>
    public double? AltitudeAngle { get => this.altitudeAngle; set => this.altitudeAngle = value; }

    /// <summary>
    /// Gets or sets the azimuth angle (angle from "north," or a line directly up from the point of contact)
    /// of the pointer device. If omitted, defaults to 0.0.
    /// </summary>
    public double? AzimuthAngle { get => this.azimuthAngle; set => this.azimuthAngle = value; }

    /// <summary>
    /// Gets or sets the angle, in degrees, of the pointer device from left to right from the vertical.
    /// Must be between -90 and 90; if omitted, defaults to 0.
    /// </summary>
    public long? TiltX { get => this.tiltX; set => this.tiltX = value; }

    /// <summary>
    /// Gets or sets the angle, in degrees, of the pointer device away from the user from the vertical.
    /// Must be between -90 and 90; if omitted, defaults to 0.
    /// </summary>
    public long? TiltY { get => this.tiltY; set => this.tiltY = value; }
}