// <copyright file="PressSequentiallyOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Options for typing text into an element one key at a time.
/// </summary>
public sealed class PressSequentiallyOptions
{
    /// <summary>
    /// Gets the time the browser waits between one key and the next. Defaults to no wait.
    /// </summary>
    public TimeSpan Delay { get; init; }

    /// <summary>
    /// Gets the time the action may wait for the element, or <see langword="null"/> for
    /// <see cref="AutomationOptions.ActionTimeout"/>.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}
