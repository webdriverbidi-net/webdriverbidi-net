// <copyright file="InputSourceKind.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Client;

/// <summary>
/// Enumerated values for the kinds of input sources available.
/// </summary>
public enum InputSourceKind
{
    /// <summary>
    /// Represents a null input source.
    /// </summary>
    None,

    /// <summary>
    /// Represents a key-based input source, primarily for entering text.
    /// </summary>
    Key,

    /// <summary>
    /// Represents a pointer-based input source, such as a mouse, pen, or stylus.
    /// </summary>
    Pointer,

    /// <summary>
    /// Represents a wheel input source.
    /// </summary>
    Wheel,
}
