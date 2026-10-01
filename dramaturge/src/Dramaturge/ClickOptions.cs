// <copyright file="ClickOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

using WebDriverBiDi.Input;

/// <summary>
/// Options for clicking an element.
/// </summary>
public sealed class ClickOptions : PointerActionOptions
{
    private readonly int clickCount = 1;

    /// <summary>
    /// Gets the mouse button pressed. Defaults to the left button.
    /// </summary>
    public PointerButton Button { get; init; } = PointerButton.Left;

    /// <summary>
    /// Gets the number of times the button is pressed and released. Defaults to 1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value less than 1.</exception>
    public int ClickCount
    {
        get => this.clickCount;
        init => this.clickCount = value >= 1 ? value : throw new ArgumentOutOfRangeException(nameof(this.ClickCount), value, "The click count must be at least 1.");
    }
}
