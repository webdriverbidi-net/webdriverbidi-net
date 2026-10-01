// <copyright file="ToggleState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// The state of a control that can be on, off, or partly on, such as a checkbox or a toggle button.
/// </summary>
public enum ToggleState
{
    /// <summary>
    /// The control is off: unchecked, or not pressed.
    /// </summary>
    Off,

    /// <summary>
    /// The control is on: checked, or pressed.
    /// </summary>
    On,

    /// <summary>
    /// The control is partly on, such as an indeterminate checkbox, or aria-checked or aria-pressed set to "mixed".
    /// </summary>
    Mixed,
}
