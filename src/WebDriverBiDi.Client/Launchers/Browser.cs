// <copyright file="Browser.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Launchers;

/// <summary>
/// Specifies the browser to launch for WebDriver BiDi automation.
/// </summary>
public enum Browser
{
    /// <summary>
    /// Google Chrome browser. Fully supported with direct launch, driver-based launch, and remote grid connections.
    /// </summary>
    Chrome,

    /// <summary>
    /// Mozilla Firefox browser. Fully supported with direct launch, driver-based launch, and remote grid connections.
    /// </summary>
    Firefox,

    /// <summary>
    /// Microsoft Edge browser. Not yet implemented - will throw <see cref="NotImplementedException"/> when used.
    /// </summary>
    /// <remarks>
    /// While Edge is Chromium-based, it has different distribution channels and download mechanisms that require
    /// separate implementation. Support is planned for a future release.
    /// </remarks>
    Edge,

    /// <summary>
    /// Apple Safari browser. Not yet implemented - will throw <see cref="NotImplementedException"/> when used.
    /// </summary>
    /// <remarks>
    /// Safari has limited WebDriver BiDi support and is macOS-only. Support is planned for a future release
    /// pending maturity of Safari's BiDi implementation.
    /// </remarks>
    Safari,
}
