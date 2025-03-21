// <copyright file="SetGeolocationOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setGeolocationOverride command.
/// </summary>
public class SetGeolocationOverrideCommandParameters : CommandParameters<EmptyResult>
{
    private GeolocationCoordinates? coordinates;
    private List<string>? contexts;
    private List<string>? userContexts;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetGeolocationOverrideCommandParameters"/> class.
    /// </summary>
    public SetGeolocationOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setGeolocationOverride";

    /// <summary>
    /// Gets or sets the coordinates to which to override the geolocation. When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("coordinates")]
    [JsonInclude]
    public GeolocationCoordinates? Coordinates { get => this.coordinates; set => this.coordinates = value; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the geolocation override.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get => this.contexts; set => this.contexts = value; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the geolocation override.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get => this.userContexts; set => this.userContexts = value; }
}
