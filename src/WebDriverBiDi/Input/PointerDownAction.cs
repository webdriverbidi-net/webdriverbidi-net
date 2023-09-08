// <copyright file="PointerDownAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using Newtonsoft.Json;

/// <summary>
/// An action to send a pointer down on a pointer device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PointerDownAction : PointerAction, IPointerSourceAction
{
    private readonly string actionType = "pointerDown";
    private long button;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointerDownAction"/> class.
    /// </summary>
    /// <param name="button">The button used for the pointer down.</param>
    public PointerDownAction(long button)
        : base()
    {
        this.button = button;
    }

    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonProperty("type")]
    public string Type => this.actionType;

    /// <summary>
    /// Gets or sets the button to be pressed down.
    /// </summary>
    [JsonProperty("button")]
    public long Button { get => this.button; set => this.button = value; }
}