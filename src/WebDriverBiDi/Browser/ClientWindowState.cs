// <copyright file="ClientWindowState.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browser;

using System.Text.Json.Serialization;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The enumerated values of client window states.
/// </summary>
[JsonConverter(typeof(EnumValueJsonConverter<ClientWindowState>))]
public enum ClientWindowState
{
    /// <summary>
    /// The client window state is normal.
    /// </summary>
    Normal,

    /// <summary>
    /// The client window state is minimized, usually meaning that the window
    /// is represented by an icon, and having zero width and height.
    /// </summary>
    Minimized,

    /// <summary>
    /// The client window state is maximized, usually meaning that the window
    /// is sized to take up the full width and height of the system's current
    /// display, but retaining so-called "chrome" elements, like a menu bar,
    /// toolbar, and so on.
    /// </summary>
    Maximized,

    /// <summary>
    /// The client window state is full screen, usually meaning that the window's
    /// content is sized to take up the full width and height of the system's current
    /// display, and not displaying so-called "chrome" elements, like a menu bar,
    /// toolbar, and so on.
    /// </summary>
    Fullscreen,
}
