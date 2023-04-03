// <copyright file="PauseAction.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;

/// <summary>
/// An action to pause the execution of a device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PauseAction : INoneSourceAction, IKeySourceAction, IPointerSourceAction, IWheelSourceAction
{
    private readonly string actionType = "pause";
    private long? duration;

    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonProperty("type")]
    public string Type => this.actionType;

    /// <summary>
    /// Gets or sets the duration of the pause.
    /// </summary>
    [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
    public long? Duration { get => this.duration; set => this.duration = value; }
}