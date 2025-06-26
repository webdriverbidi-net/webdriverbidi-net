// <copyright file="SetLocaleOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setScreenOrientationOverride command.
/// </summary>
public class SetLocaleOverrideCommandParameters : CommandParameters<EmptyResult>
{
    private string? locale;
    private List<string>? contexts;
    private List<string>? userContexts;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLocaleOverrideCommandParameters"/> class.
    /// </summary>
    public SetLocaleOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setLocaleOverride";

    /// <summary>
    /// Gets or sets the emulated locale for the browser. The value should be a valid structurally correct
    /// locale tag (e.g., "en-US", "pt-BR", etc.). When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("locale")]
    [JsonInclude]
    public string? Locale { get => this.locale; set => this.locale = value; }

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
