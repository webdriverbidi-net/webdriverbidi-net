// <copyright file="PointerActionOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// Options for a pointer action on an element, such as hovering over it.
/// </summary>
public class PointerActionOptions
{
    /// <summary>
    /// Gets where on the element the pointer goes, relative to the center of its visible part, or
    /// <see langword="null"/> for the point the element's actionability check chooses, which is the center unless
    /// something covers it.
    /// </summary>
    public PointerOffset? Offset { get; init; }

    /// <summary>
    /// Gets the modifier keys held down during the action.
    /// </summary>
    public KeyModifiers Modifiers { get; init; }

    /// <summary>
    /// Gets a value indicating whether to act without waiting for the element to be visible, stable, enabled, and
    /// uncovered. The element is still scrolled into view.
    /// </summary>
    public bool Force { get; init; }

    /// <summary>
    /// Gets the time the action may wait for the element, or <see langword="null"/> for
    /// <see cref="AutomationOptions.ActionTimeout"/>.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}
