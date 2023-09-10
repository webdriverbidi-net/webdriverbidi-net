// <copyright file="KeyUpAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using System.Text.Json.Serialization;

/// <summary>
/// An action to send a key up on a keyboard device.
/// </summary>
public class KeyUpAction : IKeySourceAction
{
    private readonly string actionType = "keyUp";
    private string value = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyUpAction"/> class.
    /// </summary>
    /// <param name="value">The text of keys to send for key up.</param>
    public KeyUpAction(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Action value cannot be null or the empty string", nameof(value));
        }

        this.value = value;
    }

    /// <summary>
    /// Gets the type of the action.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => this.actionType;

    /// <summary>
    /// Gets or sets the duration of the pause.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get => this.value; set => this.value = value; }
}