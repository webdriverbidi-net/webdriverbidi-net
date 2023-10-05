// <copyright file="KeyDownAction.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Input;

using Newtonsoft.Json;

/// <summary>
/// An action to send a key down on a keyboard device.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class KeyDownAction : IKeySourceAction
{
    private readonly string actionType = "keyDown";
    private string value = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyDownAction"/> class.
    /// </summary>
    /// <param name="value">The text of keys to send for key down.</param>
    public KeyDownAction(string value)
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
    [JsonProperty("type")]
    public string Type => this.actionType;

    /// <summary>
    /// Gets or sets the value of the key down action.
    /// </summary>
    [JsonProperty("value")]
    public string Value { get => this.value; set => this.value = value; }
}
