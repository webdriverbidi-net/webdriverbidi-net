// <copyright file="InteractionType.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

/// <summary>
/// Enumerated values for the types of interaction that can be performed on an element.
/// </summary>
public enum InteractionType
{
    /// <summary>
    /// Represents interacting with an element by clicking or tapping on it.
    /// </summary>
    Click,

    /// <summary>
    /// Represents interacting with an element by double-clicking or double-tapping on an element.
    /// </summary>
    DoubleClick,

    /// <summary>
    /// Represents interacting with an element by hovering over it with a pointer.
    /// </summary>
    Hover,

    /// <summary>
    /// Represents interacting with an element by dragging it in the browser.
    /// </summary>
    Drag,

    /// <summary>
    /// Represents interacting with an element by typing into it using a real or virtual keyboard.
    /// </summary>
    Type,

    /// <summary>
    /// Represents interacting with an element by clearing its value.
    /// </summary>
    Clear,
}