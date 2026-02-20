// <copyright file="SetScrollbarTypeOverrideCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for setting the scroll bar type override for the browser.
/// </summary>
public class SetScrollbarTypeOverrideCommandParameters : CommandParameters<SetScrollbarTypeOverrideCommandResult>
{
    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setScrollbarTypeOverride";

    /// <summary>
    /// Gets or sets the type of scroll bar to be emulated. When <see langword="null"/>, clears the override.
    /// </summary>
    [JsonPropertyName("scrollbarType")]
    [JsonInclude]
    public ScrollbarType? ScrollbarType { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the scroll bar type override.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the scroll bar type override.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
