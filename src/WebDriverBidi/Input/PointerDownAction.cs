// <copyright file="PointerDownAction.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Input;

using Newtonsoft.Json;

/// <summary>
/// An action to send a pointer down on a pointer device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class PointerDownAction : PointerAction, IPointerSourceAction
{
    private readonly string actionType = "pointerDown";
    private long button = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="PointerDownAction"/> class.
    /// </summary>
    public PointerDownAction()
        : base()
    {
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