// <copyright file="BrowserOptions.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Session;

/// <summary>
/// Settings for a browser created by <see cref="BrowserGroup.CreateBrowserAsync"/>, applied to its user context so
/// that they hold for every page it opens. A setting left <see langword="null"/> keeps the browser's default.
/// </summary>
public sealed class BrowserOptions
{
    /// <summary>
    /// Gets a value indicating whether the browser accepts untrusted and self-signed TLS certificates.
    /// </summary>
    public bool? AcceptInsecureCerts { get; init; }

    /// <summary>
    /// Gets the proxy through which the browser's pages connect.
    /// </summary>
    public ProxyConfiguration? Proxy { get; init; }

    /// <summary>
    /// Gets how the browser answers user prompts, such as alerts, that nothing else handles.
    /// </summary>
    public UserPromptHandler? UnhandledPromptBehavior { get; init; }

    /// <summary>
    /// Gets the size of the viewport of the browser's pages.
    /// </summary>
    public Viewport? Viewport { get; init; }

    /// <summary>
    /// Gets the ratio of device pixels to CSS pixels for the browser's pages.
    /// </summary>
    public double? DevicePixelRatio { get; init; }

    /// <summary>
    /// Gets the locale the browser's pages report, such as "de-DE".
    /// </summary>
    public string? Locale { get; init; }

    /// <summary>
    /// Gets the time zone the browser's pages use, such as "Europe/Berlin".
    /// </summary>
    public string? TimeZone { get; init; }

    /// <summary>
    /// Gets the user agent the browser's pages report.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Gets the CSS media features the browser's pages match, such as a preferred color scheme.
    /// </summary>
    public MediaFeatures? MediaFeatures { get; init; }

    /// <summary>
    /// Gets the position the browser's pages report.
    /// </summary>
    public GeolocationCoordinates? Geolocation { get; init; }

    /// <summary>
    /// Gets the permissions granted, denied, or left to prompt for, each for an origin.
    /// </summary>
    public IList<PermissionGrant> Permissions { get; } = [];
}
