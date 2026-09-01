// <copyright file="GeolocationCoordinates.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// A data class representing a set of geolocation coordinates.
/// </summary>
public class GeolocationCoordinates
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeolocationCoordinates"/> class.
    /// </summary>
    /// <param name="longitude">
    /// The longitude of the geographic position in degrees. Positive values are east of the prime
    /// meridian; negative values are west of the prime meridian.
    /// </param>
    /// <param name="latitude">
    /// The latitude of the geographic position in degrees. Positive values are north of the equator;
    /// negative values are south of the equator.
    /// </param>
    public GeolocationCoordinates(double longitude, double latitude)
    {
        this.Longitude = longitude;
        this.Latitude = latitude;
    }

    /// <summary>
    /// Gets or sets the latitude of the geographic position in degrees.
    /// Positive values are north of the equator; negative values are south of
    /// the equator.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from -90.0 to 90.0, inclusive. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("latitude")]
    [SpecRange(-90.0, 90.0)]
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude of the geographic position in degrees.
    /// Positive values are east of the prime meridian; negative values are west
    /// of the prime meridian.
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from -180.0 to 180.0, inclusive. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("longitude")]
    [SpecRange(-180.0, 180.0)]
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets the accuracy of the geographic position to a 95% confidence level. If omitted, is interpreted as 1.0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("accuracy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity)]
    public double? Accuracy { get; set; }

    /// <summary>
    /// Gets or sets the altitude of the geographic position, in meters.
    /// </summary>
    [JsonPropertyName("altitude")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Altitude { get; set; }

    /// <summary>
    /// Gets or sets the accuracy of the altitude of the geographic position to a 95% confidence level.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("altitudeAccuracy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity)]
    public double? AltitudeAccuracy { get; set; }

    /// <summary>
    /// Gets or sets the heading of the movement of the geographic position, in degrees. If the device is stationary, leave this property unset (<see langword="null"/>).
    /// </summary>
    /// <remarks>
    /// Valid values for this property range from 0.0 to 360.0, inclusive. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("heading")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, 360.0)]
    public double? Heading { get; set; }

    /// <summary>
    /// Gets or sets the speed of the movement of the geographic position, in meters per second. If the device is stationary, should be set to 0.
    /// </summary>
    /// <remarks>
    /// Valid values for this property are greater than or equal to 0.0. This property does not
    /// validate its value; a value outside this range is sent as-is, and a conforming remote end
    /// rejects it when the command is executed.
    /// </remarks>
    [JsonPropertyName("speed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [SpecRange(0.0, double.PositiveInfinity)]
    public double? Speed { get; set; }
}
