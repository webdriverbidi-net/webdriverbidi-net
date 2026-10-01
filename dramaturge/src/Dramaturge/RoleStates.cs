// <copyright file="RoleStates.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// ARIA states an element found by role must have, for <see cref="Frame.GetByRole"/>. A state left
/// <see langword="null"/> is not checked; a state that does not apply to an element, such as checked for a link,
/// does not match.
/// </summary>
public sealed class RoleStates
{
    /// <summary>
    /// Gets the checked state: of a native checkbox or radio button, or aria-checked for a role that supports it.
    /// </summary>
    public ToggleState? Checked { get; init; }

    /// <summary>
    /// Gets the pressed state of a toggle button, from aria-pressed.
    /// </summary>
    public ToggleState? Pressed { get; init; }

    /// <summary>
    /// Gets the expanded state: whether a details element is open, or aria-expanded for a role that supports it.
    /// </summary>
    public bool? Expanded { get; init; }

    /// <summary>
    /// Gets the selected state: of a native option, or aria-selected for a role that supports it.
    /// </summary>
    public bool? Selected { get; init; }

    /// <summary>
    /// Gets the level: of a native h1 to h6 heading, or aria-level for a role that supports it.
    /// </summary>
    public int? Level { get; init; }

    /// <summary>
    /// Gets the disabled state: a natively disabled control, or aria-disabled on it or an ancestor.
    /// </summary>
    public bool? Disabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether no state is set.
    /// </summary>
    internal bool IsEmpty => this.Checked is null && this.Pressed is null && this.Expanded is null && this.Selected is null && this.Level is null && this.Disabled is null;
}
