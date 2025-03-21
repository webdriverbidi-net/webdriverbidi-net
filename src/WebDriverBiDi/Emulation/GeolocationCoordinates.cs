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
    private double latitude;
    private double longitude;
    private double? accuracy;
    private double? altitude;
    private double? altitudeAccuracy;
    private double? heading;
    private double? speed;

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
        this.longitude = longitude;
        this.latitude = latitude;
    }

    /// <summary>
    /// Gets or sets the longitude of the geographic position in degrees.
    /// Positive values are north of the equator; negative values are south of
    /// the equator.
    /// </summary>
    [JsonPropertyName("latitude")]
    public double Latitude { get => this.latitude; set => this.latitude = value; }

    /// <summary>
    /// Gets or sets the latitude of the geographic position in degrees.
    /// Positive values are east of the prime meridian; negative values are west
    /// of the prime meridian.
    /// </summary>
    [JsonPropertyName("longitude")]
    public double Longitude { get => this.longitude; set => this.longitude = value; }

    /// <summary>
    /// Gets or sets the accuracy of the geographic position to a 95% confidence level. If omitted, is interpreted as 1.0.
    /// </summary>
    [JsonPropertyName("accuracy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Accuracy { get => this.accuracy; set => this.accuracy = value; }

    /// <summary>
    /// Gets or sets the altitude of the geographic position, in meters.
    /// </summary>
    [JsonPropertyName("altitude")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Altitude { get => this.altitude; set => this.altitude = value; }

    /// <summary>
    /// Gets or sets the accuracy of the altitude of the geographic position to a 95% confidence level.
    /// </summary>
    [JsonPropertyName("altitudeAccuracy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AltitudeAccuracy { get => this.altitudeAccuracy; set => this.altitudeAccuracy = value; }

    /// <summary>
    /// Gets or sets the heading of the movement of the geographic position, in degrees. If the device is stationary, should be set to double.NaN.
    /// </summary>
    [JsonPropertyName("heading")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
    public double? Heading { get => this.heading; set => this.heading = value; }

    /// <summary>
    /// Gets or sets the speed of the movement of the geographic position, in meters per second. If the device is stationary, should be set to 0.
    /// </summary>
    [JsonPropertyName("speed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Speed { get => this.speed; set => this.speed = value; }
}
