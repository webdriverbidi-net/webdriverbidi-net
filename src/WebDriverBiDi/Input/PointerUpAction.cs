// <copyright file="PointerUpAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// An action to send a pointer up on a pointer device.
/// </summary>
public class PointerUpAction : IPointerSourceAction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PointerUpAction"/> class.
    /// </summary>
    /// <param name="button">The button used for the pointer down.</param>
    public PointerUpAction(long button)
        : base()
    {
        this.Button = button;
    }

    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "pointerUp";

    /// <summary>
    /// Gets or sets the button to be pressed down.
    /// </summary>
    [JsonPropertyName("button")]
    public long Button { get; set; }
}
