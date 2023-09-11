// <copyright file="Viewport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

using System.Text.Json.Serialization;

/// <summary>
/// Provides parameters for the browsingContext.create command.
/// </summary>
public class Viewport
{
    private ulong height = 0;
    private ulong width = 0;

    /// <summary>
    /// Gets or sets the height of the viewport.
    /// </summary>
    [JsonPropertyName("height")]
    public ulong Height { get => this.height; set => this.height = value; }

    /// <summary>
    /// Gets or sets the width of the viewport.
    /// </summary>
    [JsonPropertyName("width")]
    public ulong Width { get => this.width; set => this.width = value; }
}
