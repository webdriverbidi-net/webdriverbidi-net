// <copyright file="BrowserKind.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Browsers;

/// <summary>
/// Specifies the browser to launch for WebDriver BiDi automation.
/// </summary>
public enum BrowserKind
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
    /// Microsoft Edge browser, which is never downloaded: the installed Edge of the release channel is launched directly
    /// or through msedgedriver, which is downloaded in the matching version, or on a remote grid.
    /// </summary>
    Edge,

    /// <summary>
    /// Apple Safari browser, on macOS only. The installed Safari or Safari Technology Preview is launched through
    /// safaridriver, or on a remote grid.
    /// </summary>
    Safari,
}
