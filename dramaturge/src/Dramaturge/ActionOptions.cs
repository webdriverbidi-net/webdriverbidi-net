// <copyright file="ActionOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Options for an action on an element that waits for the element to be ready, such as filling it.
/// </summary>
public class ActionOptions
{
    /// <summary>
    /// Gets a value indicating whether to act without waiting for the element to be ready for the action, such as
    /// visible, enabled, and uncovered.
    /// </summary>
    public bool Force { get; init; }

    /// <summary>
    /// Gets the time the action may wait for the element, or <see langword="null"/> for
    /// <see cref="DramaturgeOptions.ActionTimeout"/>.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}
