// <copyright file="PointerSourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Represents actions with a pointer input device.
/// </summary>
public class PointerSourceActions : SourceActions
{
    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => "pointer";

    /// <summary>
    /// Gets or sets the parameters for the pointer device.
    /// </summary>
    [JsonPropertyName("parameters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PointerParameters? Parameters { get; set; }

    /// <summary>
    /// Gets the list of actions for this input device.
    /// </summary>
    [JsonPropertyName("actions")]
    public List<IPointerSourceAction> Actions { get; } = [];
}
