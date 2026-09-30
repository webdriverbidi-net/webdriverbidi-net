// <copyright file="FillOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

/// <summary>
/// Options for filling or clearing an editable element.
/// </summary>
public sealed class FillOptions
{
    /// <summary>
    /// Gets a value indicating whether to act without waiting for the element to be visible, stable, enabled,
    /// editable, and uncovered.
    /// </summary>
    public bool Force { get; init; }

    /// <summary>
    /// Gets the time the action may wait for the element, or <see langword="null"/> for
    /// <see cref="AutomationOptions.ActionTimeout"/>.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}
