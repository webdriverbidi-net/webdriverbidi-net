// <copyright file="SkillSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Compiled counterparts for the code blocks in skills/dramaturge, which coding agents read as plain files, so their
// samples cannot be region references. Each block names the region it mirrors in a
// '<!-- readme-csharp: path#Region -->' marker, and docs/tools/validate-doc-regions.sh compares the two.

namespace Dramaturge.Docs.Code.Skill;

using System.Text.RegularExpressions;
using Dramaturge;
using Dramaturge.Browsers;
using static Dramaturge.Assertions;

/// <summary>
/// Snippets for the Dramaturge agent skill. Compiled at build time to prevent API drift.
/// </summary>
public static class SkillSamples
{
    /// <summary>
    /// Launching, and a browser of its own for one test.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Launch()
    {
        #region Launch
        DramaturgeOptions options = new() { ActionTimeout = TimeSpan.FromSeconds(10) };
        await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Chrome).WithHeadlessOption(), options);

        // A browser of its own: cookies, storage, and cache no other browser in the group shares.
        Browser browser = await group.CreateBrowserAsync();
        Page page = await browser.NewPageAsync();
        await page.NavigateAsync("https://example.com/login");
        #endregion
    }

    /// <summary>
    /// Locators, in order of preference.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Locators(Page page)
    {
        #region Locators
        await page.GetByRole("button", "Sign in").ClickAsync();
        await page.GetByLabel("Email address").FillAsync("ada@example.com");
        await page.GetByPlaceholder("Search").FillAsync("lovelace");
        await page.GetByTestId("checkout").ClickAsync();

        // Narrow by chaining and filtering rather than by position or a long CSS path.
        ElementLocator row = page.GetByRole("row").Filter(hasText: "Ada");
        await row.GetByRole("button", "Delete").ClickAsync();
        #endregion
    }

    /// <summary>
    /// Asserting with retries.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Assertions(Page page)
    {
        #region Assertions
        await page.GetByRole("button", "Save").ClickAsync();

        // Each is checked again until it holds, or fails after five seconds saying what it last saw.
        await Expect(page.GetByRole("status")).ToHaveTextAsync("Saved");
        await Expect(page.GetByRole("progressbar")).Not.ToBeVisibleAsync();
        await Expect(page).ToHaveUrlAsync(new Regex("/orders/\\d+$"));
        await Expect(page.GetByRole("listitem")).ToHaveCountAsync(3);
        #endregion
    }

    /// <summary>
    /// Answering a request with a route.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Routes(Page page)
    {
        #region Routes
        // Give a body or a header: Chrome sends a status-only response on to the network instead.
        RouteRegistration route = await page.RouteAsync(
            "https://api.example.com/v1/user",
            r => r.FulfillAsync(200, """{"name":"Ada"}""", new Dictionary<string, string>() { ["Content-Type"] = "application/json" }));
        await page.NavigateAsync("https://example.com/profile");
        await route.RemoveAsync();
        #endregion
    }

    /// <summary>
    /// Waiting for what a click starts.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Waits(Page page)
    {
        #region Waits
        // A click that navigates does not wait for the new page; wrap it when the next step needs that page.
        await page.RunAndWaitForNavigationAsync(() => page.GetByRole("link", "Checkout").ClickAsync());

        Download download = await page.RunAndWaitForDownloadAsync(() => page.GetByRole("link", "Export").ClickAsync());
        DownloadOutcome outcome = await download.WaitForEndAsync();
        #endregion
    }
}
