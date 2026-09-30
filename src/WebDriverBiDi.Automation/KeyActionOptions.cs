// <copyright file="KeyActionOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// Options for pressing a key on an element.
/// </summary>
public sealed class KeyActionOptions
{
    /// <summary>
    /// Gets the modifier keys held down while the key is pressed.
    /// </summary>
    public KeyModifiers Modifiers { get; init; }

    /// <summary>
    /// Gets the time the action may wait for the element, or <see langword="null"/> for
    /// <see cref="AutomationOptions.ActionTimeout"/>.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}
