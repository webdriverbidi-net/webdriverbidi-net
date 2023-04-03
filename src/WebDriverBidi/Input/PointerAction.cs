// <copyright file="PointerAction.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;

/// <summary>
/// Base class for pointer actions.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PointerAction
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
    /// Initializes a new instance of the <see cref="PointerAction"/> class.
    /// </summary>
    protected PointerAction()
    {
    }

    /// <summary>
    /// Gets or sets the width of the pointer in pixels. If omitted, defaults to 1.
    /// </summary>
    [JsonProperty("width", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? Width { get => this.width; set => this.width = value; }

    /// <summary>
    /// Gets or sets the height of the pointer in pixels. If omitted, defaults to 1.
    /// </summary>
    [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? Height { get => this.height; set => this.height = value; }

    /// <summary>
    /// Gets or sets the pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    [JsonProperty("pressure", NullValueHandling = NullValueHandling.Ignore)]
    public double? Pressure { get => this.pressure; set => this.pressure = value; }

    /// <summary>
    /// Gets or sets the tangential pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    [JsonProperty("tangentialPressure", NullValueHandling = NullValueHandling.Ignore)]
    public double? TangentialPressure { get => this.tangentialPressure; set => this.tangentialPressure = value; }

    /// <summary>
    /// Gets or sets the twist of the pointer in degrees, between 0 and 359, on the surface. If omitted, defaults to 0.
    /// </summary>
    [JsonProperty("twist", NullValueHandling = NullValueHandling.Ignore)]
    public ulong? Twist
    {
        get
        {
            return this.twist;
        }

        set
        {
            if (value > 359)
            {
                throw new WebDriverBidiException("Twist value must be between 0 and 359");
            }

            this.twist = value;
        }
    }

    /// <summary>
    /// Gets or sets the altitude angle (angle from the horizontal) of the pointer device. If omitted, defaults to 0.0.
    /// </summary>
    [JsonProperty("altitudeAngle", NullValueHandling = NullValueHandling.Ignore)]
    public double? AltitudeAngle { get => this.altitudeAngle; set => this.altitudeAngle = value; }

    /// <summary>
    /// Gets or sets the azimuth angle (angle from "north," or a line directly up from the point of contact)
    /// of the pointer device. If omitted, defaults to 0.0.
    /// </summary>
    [JsonProperty("azimuthAngle", NullValueHandling = NullValueHandling.Ignore)]
    public double? AzimuthAngle { get => this.azimuthAngle; set => this.azimuthAngle = value; }

    /// <summary>
    /// Gets or sets the angle, in degrees, of the pointer device from left to right from the vertical.
    /// Must be between -90 and 90; if omitted, defaults to 0.
    /// </summary>
    [JsonProperty("tiltX", NullValueHandling = NullValueHandling.Ignore)]
    public long? TiltX
    {
        get
        {
            return this.tiltX;
        }

        set
        {
            if (value < -90 || value > 90)
            {
                throw new WebDriverBidiException("TiltX value must be between -90 and 90 inclusive");
            }

            this.tiltX = value;
        }
    }

    /// <summary>
    /// Gets or sets the angle, in degrees, of the pointer device away from the user from the vertical.
    /// Must be between -90 and 90; if omitted, defaults to 0.
    /// </summary>
    [JsonProperty("tiltY", NullValueHandling = NullValueHandling.Ignore)]
    public long? TiltY
    {
        get
        {
            return this.tiltY;
        }

        set
        {
            if (value < -90 || value > 90)
            {
                throw new WebDriverBidiException("TiltY value must be between -90 and 90 inclusive");
            }

            this.tiltY = value;
        }
    }
}