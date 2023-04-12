// <copyright file="PointerActionProperties.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

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
    public ulong? Width { get => width; set => width = value; }

    /// <summary>
    /// Gets or sets the height of the pointer in pixels. If omitted, defaults to 1.
    /// </summary>
    public ulong? Height { get => height; set => height = value; }

    /// <summary>
    /// Gets or sets the pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    public double? Pressure { get => pressure; set => pressure = value; }

    /// <summary>
    /// Gets or sets the tangential pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    public double? TangentialPressure { get => tangentialPressure; set => tangentialPressure = value; }

    /// <summary>
    /// Gets or sets the twist of the pointer in degrees, between 0 and 359, on the surface. If omitted, defaults to 0.
    /// </summary>
    public ulong? Twist { get => twist; set => twist = value; }

    /// <summary>
    /// Gets or sets the altitude angle (angle from the horizontal) of the pointer device. If omitted, defaults to 0.0.
    /// </summary>
    public double? AltitudeAngle { get => altitudeAngle; set => altitudeAngle = value; }

    /// <summary>
    /// Gets or sets the azimuth angle (angle from "north," or a line directly up from the point of contact)
    /// of the pointer device. If omitted, defaults to 0.0.
    /// </summary>
    public double? AzimuthAngle { get => azimuthAngle; set => azimuthAngle = value; }

    /// <summary>
    /// Gets or sets the angle, in degrees, of the pointer device from left to right from the vertical.
    /// Must be between -90 and 90; if omitted, defaults to 0.
    /// </summary>
    public long? TiltX { get => tiltX; set => tiltX = value; }

    /// <summary>
    /// Gets or sets the angle, in degrees, of the pointer device away from the user from the vertical.
    /// Must be between -90 and 90; if omitted, defaults to 0.
    /// </summary>
    public long? TiltY { get => tiltY; set => tiltY = value; }
}