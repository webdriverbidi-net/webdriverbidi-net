// <copyright file="ClickNavigationBehavior.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Elements;

/// <summary>
/// Enumerated values for the navigation behavior expected when clicking an element.
/// </summary>
public enum ClickNavigationBehavior
{
    /// <summary>
    /// Expect no navigation, omit waiting for navigation after a click.
    /// </summary>
    None,

    /// <summary>
    /// Expect a navigation on click, wait for that navigation to begin.
    /// </summary>
    WaitForNavigationStart,

    /// <summary>
    /// Wait for the resulting page to fire the DOMContentLoaded event.
    /// </summary>
    WaitForDomContentLoadedEvent,

    /// <summary>
    /// Wait for the resulting page to fire the load event.
    /// </summary>
    WaitForLoadEvent,
}
