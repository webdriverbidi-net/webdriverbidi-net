// <copyright file="WheelSourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using Newtonsoft.Json;

/// <summary>
/// Represents actions with a wheel input device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WheelSourceActions : SourceActions
{
    private readonly List<IWheelSourceAction> actions = new();

    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonProperty("type")]
    public override string Type => "wheel";

    /// <summary>
    /// Gets the list of actions for this input device.
    /// </summary>
    [JsonProperty("actions")]
    public List<IWheelSourceAction> Actions => this.actions;
}