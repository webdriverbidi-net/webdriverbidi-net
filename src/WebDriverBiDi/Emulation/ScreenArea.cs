// <copyright file="ScreenArea.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Emulation;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the setting the emulated area of a screen.
/// </summary>
public class ScreenArea
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenArea"/> class.
    /// </summary>
    [JsonConstructor]
    public ScreenArea()
    {
    }

    /// <summary>
    /// Gets or sets the height of the screen.
    /// </summary>
    [JsonPropertyName("height")]
    public ulong Height { get; set; } = 0;

    /// <summary>
    /// Gets or sets the width of the screen.
    /// </summary>
    [JsonPropertyName("width")]
    public ulong Width { get; set; } = 0;
}
