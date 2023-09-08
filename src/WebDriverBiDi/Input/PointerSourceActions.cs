// <copyright file="PointerSourceActions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using Newtonsoft.Json;

/// <summary>
/// Represents actions with a pointer input device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PointerSourceActions : SourceActions
{
    private readonly List<IPointerSourceAction> actions = new();
    private PointerParameters? parameters;

    /// <summary>
    /// Gets the type of the source actions.
    /// </summary>
    [JsonProperty("type")]
    public override string Type => "pointer";

    /// <summary>
    /// Gets or sets the parameters for the pointer device.
    /// </summary>
    [JsonProperty("parameters", NullValueHandling = NullValueHandling.Ignore)]
    public PointerParameters? Parameters { get => this.parameters; set => this.parameters = value; }

    /// <summary>
    /// Gets the list of actions for this input device.
    /// </summary>
    [JsonProperty("actions")]
    public List<IPointerSourceAction> Actions => this.actions;
}