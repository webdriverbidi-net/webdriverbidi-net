// <copyright file="KeyModifiers.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge;

/// <summary>
/// Modifier keys held down during a pointer action.
/// </summary>
[Flags]
public enum KeyModifiers
{
    /// <summary>
    /// No modifier keys.
    /// </summary>
    None = 0,

    /// <summary>
    /// The Alt (Option) key.
    /// </summary>
    Alt = 1,

    /// <summary>
    /// The Control key.
    /// </summary>
    Control = 2,

    /// <summary>
    /// The Meta (Command or Windows) key.
    /// </summary>
    Meta = 4,

    /// <summary>
    /// The Shift key.
    /// </summary>
    Shift = 8,
}
