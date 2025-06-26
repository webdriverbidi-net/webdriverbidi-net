// <copyright file="SetScreenOrientationOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setScreenOrientationOverride command.
/// </summary>
public class SetScreenOrientationOverrideCommandParameters : CommandParameters<EmptyResult>
{
    private ScreenOrientation? screenOrientation;
    private List<string>? contexts;
    private List<string>? userContexts;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetScreenOrientationOverrideCommandParameters"/> class.
    /// </summary>
    public SetScreenOrientationOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setScreenOrientationOverride";

    /// <summary>
    /// Gets or sets the emulated screen orientation settings for the browser.
    /// When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("screenOrientation")]
    [JsonInclude]
    public ScreenOrientation? ScreenOrientation { get => this.screenOrientation; set => this.screenOrientation = value; }

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
