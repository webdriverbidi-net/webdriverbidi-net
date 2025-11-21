// <copyright file="PointerAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// Base class for pointer actions.
/// </summary>
public class PointerAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointerAction"/> class.
    /// </summary>
    protected PointerAction()
    {
    }

    /// <summary>
    /// Gets or sets the width of the pointer in pixels. If omitted, defaults to 1.
    /// </summary>
    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the pointer in pixels. If omitted, defaults to 1.
    /// </summary>
    [JsonPropertyName("height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Height { get; set; }

    /// <summary>
    /// Gets or sets the pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    [JsonPropertyName("pressure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double? Pressure { get; set; }

    /// <summary>
    /// Gets or sets the tangential pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    [JsonPropertyName("tangentialPressure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double? TangentialPressure { get; set; }

    /// <summary>
    /// Gets or sets the twist of the pointer in degrees, between 0 and 359, on the surface. If omitted, defaults to 0.
    /// </summary>
    [JsonPropertyName("twist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ulong? Twist
    {
        get;
        set
        {
            if (value > 359)
            {
                throw new WebDriverBiDiException("Twist value must be between 0 and 359");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the altitude angle (angle from the horizontal) of the pointer device.
    /// Must be between 0 and 1.5707963267948966 (pi / 2). If omitted, defaults to 0.0.
    /// </summary>
    [JsonPropertyName("altitudeAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double? AltitudeAngle
    {
        get;
        set
        {
            if (value < 0 || value > Math.PI / 2)
            {
                throw new WebDriverBiDiException("AltitudeAngle value must be between 0 and 1.5707963267948966 (pi / 2) inclusive");
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the azimuth angle (angle from "north," or a line directly up from the point of contact)
    /// of the pointer device. Must be between 0 and 6.283185307179586 (2 *pi) If omitted, defaults to 0.0.
    /// </summary>
    [JsonPropertyName("azimuthAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    public double? AzimuthAngle
    {
        get;
        set
        {
            if (value < 0 || value > Math.PI * 2)
            {
                throw new WebDriverBiDiException("AzimuthAngle value must be between 0 and 6.283185307179586 (2 * pi) inclusive");
            }

            field = value;
        }
    }
}
