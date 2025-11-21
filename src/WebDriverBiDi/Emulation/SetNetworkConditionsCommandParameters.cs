// <copyright file="SetNetworkConditionsCommandParameters.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the emulation.setNetworkConditions command.
/// </summary>
public class SetNetworkConditionsCommandParameters : CommandParameters<SetNetworkConditionsCommandResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetNetworkConditionsCommandParameters"/> class.
    /// </summary>
    public SetNetworkConditionsCommandParameters()
    {
    }

    /// <summary>
    /// Gets the method name of the command.
    /// </summary>
    [JsonIgnore]
    public override string MethodName => "emulation.setNetworkConditions";

    /// <summary>
    /// Gets or sets the network conditions to emulate. When <see langword="null"/>, clears the emulated network conditions.
    /// </summary>
    [JsonPropertyName("networkConditions")]
    [JsonInclude]
    public NetworkConditions? NetworkConditions { get; set; }

    /// <summary>
    /// Gets or sets the browsing contexts for which to set the emulated network conditions.
    /// </summary>
    [JsonPropertyName("contexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Contexts { get; set; }

    /// <summary>
    /// Gets or sets the user contexts for which to set the emulated network conditions.
    /// </summary>
    [JsonPropertyName("userContexts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? UserContexts { get; set; }
}
