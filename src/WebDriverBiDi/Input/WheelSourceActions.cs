// <copyright file="WheelSourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// Represents actions with a wheel input device.
/// </summary>
public class WheelSourceActions : SourceActions
{
    private readonly List<IWheelSourceAction> actions = new();

    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonPropertyName("type")]
    public override string Type => "wheel";

    /// <summary>
    /// Gets the list of actions for this input device.
    /// </summary>
    [JsonPropertyName("actions")]
    public List<IWheelSourceAction> Actions => this.actions;
}
