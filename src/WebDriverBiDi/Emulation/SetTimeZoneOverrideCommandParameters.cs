// <copyright file="SetTimeZoneOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setTimezoneOverride command.
/// </summary>
public class SetTimeZoneOverrideCommandParameters : CommandParameters<SetTimeZoneOverrideCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetTimeZoneOverrideCommandParameters"/> class.
    /// </summary>
    public SetTimeZoneOverrideCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setTimezoneOverride";

    /// <summary>
    /// Gets or sets the emulated time zone for the browser. The value should be a valid structurally correct
    /// named time zone identifier (e.g., "America/New_York", "Europe/London", "Asia/Tokyo", "Asia/Kolkata" etc.),
    /// or a UTC offset time zone identifier (e.g., "-5:00", "+5:30", etc.). When <see langword="null"/>,
    /// clears the override.
    /// </summary>
    [JsonPropertyName("timezone")]
    [JsonInclude]
    public string? TimeZone { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the time zone override.
    /// </summary>
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the time zone override.
    /// </summary>
    /// <remarks>
    /// This property is nullable to distinguish between omitting the property from the JSON payload (null)
    /// and sending an empty array (empty list). When null, the property is not included in the command;
    /// when an empty list, an empty array is sent to the remote end.
    /// </remarks>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
