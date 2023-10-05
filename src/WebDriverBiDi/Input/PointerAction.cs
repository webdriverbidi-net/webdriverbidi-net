// <copyright file="PointerAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

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
                throw new WebDriverBiDiException("Twist value must be between 0 and 359");
            }

            this.twist = value;
        }
    }

    /// <summary>
    /// Gets or sets the altitude angle (angle from the horizontal) of the pointer device.
    /// Must be between 0 and 1.5707963267948966 (pi / 2). If omitted, defaults to 0.0.
    /// </summary>
    [JsonProperty("altitudeAngle", NullValueHandling = NullValueHandling.Ignore)]
    public double? AltitudeAngle
    {
        get
        {
            return this.altitudeAngle;
        }

        set
        {
            if (value < 0 || value > Math.PI / 2)
            {
                throw new WebDriverBiDiException("AltitudeAngle value must be between 0 and 1.5707963267948966 (pi / 2) inclusive");
            }

            this.altitudeAngle = value;
        }
    }

    /// <summary>
    /// Gets or sets the azimuth angle (angle from "north," or a line directly up from the point of contact)
    /// of the pointer device. Must be between 0 and 6.283185307179586 (2 *pi) If omitted, defaults to 0.0.
    /// </summary>
    [JsonProperty("azimuthAngle", NullValueHandling = NullValueHandling.Ignore)]
    public double? AzimuthAngle
    {
        get
        {
            return this.azimuthAngle;
        }

        set
        {
            if (value < 0 || value > Math.PI * 2)
            {
                throw new WebDriverBiDiException("AzimuthAngle value must be between 0 and 6.283185307179586 (2 * pi) inclusive");
            }

            this.azimuthAngle = value;
        }
    }
}
