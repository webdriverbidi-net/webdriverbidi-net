// <copyright file="ConfigurationSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/configuration.md

namespace Dramaturge.Docs.Code;

using Dramaturge.Browsers;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Permissions;
using static Dramaturge.Assertions;

/// <summary>
/// Snippets for the configuration guide. Compiled at build time to prevent API drift.
/// </summary>
public static class ConfigurationSamples
{
    /// <summary>
    /// Options for a group.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GroupOptions()
    {
        #region GroupOptions
        DramaturgeOptions options = new()
        {
            ActionTimeout = TimeSpan.FromSeconds(10),
            ExpectTimeout = TimeSpan.FromSeconds(10),
            TestIdAttribute = "data-test",
            PierceShadowRoots = true,
        };

        await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Firefox), options);
        #endregion
    }

    /// <summary>
    /// Options for a browser.
    /// </summary>
    /// <param name="group">A launched group.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PerBrowserOptions(BrowserGroup group)
    {
        #region BrowserOptions
        BrowserOptions options = new()
        {
            Viewport = new Viewport() { Width = 1280, Height = 720 },
            Locale = "fr-FR",
            TimeZone = "Europe/Paris",
            Geolocation = new GeolocationCoordinates(48.8566, 2.3522),
            Permissions = { new PermissionGrant("geolocation", PermissionState.Granted, "https://example.com") },
        };

        Browser browser = await group.CreateBrowserAsync(options);
        #endregion
    }

    /// <summary>
    /// Timeouts for one call.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task PerCallTimeouts(Page page)
    {
        #region PerCallTimeouts
        await page.NavigateAsync("https://example.com/report", timeout: TimeSpan.FromMinutes(2));
        await page.GetByRole("button", "Generate").ClickAsync(new ClickOptions() { Timeout = TimeSpan.FromMinutes(1) });
        await Expect(page.GetByText("Report ready")).ToBeVisibleAsync(TimeSpan.FromMinutes(1));
        #endregion
    }
}
