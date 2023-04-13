// <copyright file="PointerButton.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

/// <summary>
/// Specifies the button used during a pointer down or up action.
/// </summary>
public enum PointerButton
{
    /// <summary>
    /// The button used is the primary button.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The button used is the middle button or mouse wheel.
    /// </summary>
    Middle = 1,

    /// <summary>
    /// The button used is the secondary button.
    /// </summary>
    Right = 2,

    /// <summary>
    /// The X1 button used for navigating back.
    /// </summary>
    Back = 3,

    /// <summary>
    /// The X2 button used for navigating forward.
    /// </summary>
    Forward = 4,
}
