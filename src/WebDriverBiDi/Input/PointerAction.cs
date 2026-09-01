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
    /// <remarks>
    /// Valid values for this property range from 0.0 to 1.0, inclusive. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("pressure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    [SpecRange(0.0, 1.0)]
    public double? Pressure { get; set; }

    /// <summary>
    /// Gets or sets the tangential pressure of the pointer on the surface. If omitted, defaults to 0.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from -1.0 to 1.0, inclusive. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("tangentialPressure")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    [SpecRange(-1.0, 1.0)]
    public double? TangentialPressure { get; set; }

    /// <summary>
    /// Gets or sets the twist of the pointer in degrees on the surface. If omitted, defaults to 0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from 0 to 359, inclusive. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("twist")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, 359.0)]
    public ulong? Twist { get; set; }

    /// <summary>
    /// Gets or sets the altitude angle (angle from the horizontal) of the pointer device. If omitted,
    /// defaults to 0.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from 0.0 to 1.5707963267948966 (pi / 2), inclusive. This
    /// property does not validate its value; a value outside this range is sent as-is, and a
    /// conforming remote end rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("altitudeAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    [SpecRange(0.0, 1.5707963267948966)]
    public double? AltitudeAngle { get; set; }

    /// <summary>
    /// Gets or sets the azimuth angle (angle from "north," or a line directly up from the point of contact)
    /// of the pointer device. If omitted, defaults to 0.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from 0.0 to 6.283185307179586 (2 * pi), inclusive. This
    /// property does not validate its value; a value outside this range is sent as-is, and a
    /// conforming remote end rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("azimuthAngle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(FixedDoubleJsonConverter))]
    [SpecRange(0.0, 6.283185307179586)]
    public double? AzimuthAngle { get; set; }
}
