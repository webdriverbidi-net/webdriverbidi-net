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
    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> class.
    /// </summary>
    [JsonConstructor]
    public Viewport()
        : this(false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Viewport"/> class.
    /// </summary>
    /// <param name="useDefaultViewport">A value indicating whether the viewport should be reset to its default settings.</param>
    private Viewport(bool useDefaultViewport)
    {
        this.IsResetViewport = useDefaultViewport;
    }

    /// <summary>
    /// Gets a <see cref="Viewport"/> object that indicates the viewport should be reset to the default.
    /// </summary>
    public static Viewport ResetToDefaultViewport => new(true);

    /// <summary>
    /// Gets or sets the height of the viewport.
    /// </summary>
    [JsonPropertyName("height")]
    public ulong Height { get; set; } = 0;

    /// <summary>
    /// Gets or sets the width of the viewport.
    /// </summary>
    [JsonPropertyName("width")]
    public ulong Width { get; set; } = 0;

    /// <summary>
    /// Gets a value indicating whether to reset the viewport to its default size.
    /// </summary>
    [JsonIgnore]
    internal bool IsResetViewport { get; }
}
