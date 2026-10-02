// <copyright file="DramaturgeReadmeSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

// Compiled counterparts for the code blocks in README.md and src/Dramaturge/README.md, which their readers see
// without DocFX, so their samples cannot be region references. Each fence names the region it mirrors in a
// '<!-- readme-csharp: path#Region -->' marker, and docs/tools/validate-doc-regions.sh compares the two.

namespace Dramaturge.Docs.Code;

using System.Text.RegularExpressions;
using Dramaturge;
using Dramaturge.Browsers;
using static Dramaturge.Assertions;

/// <summary>
/// Snippets for the Dramaturge READMEs. Compiled at build time to prevent API drift.
/// </summary>
public static class DramaturgeReadmeSamples
{
    /// <summary>
    /// The quick start: signing in on a page.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task QuickStart()
    {
        #region QuickStart
        // Downloads Chrome for Testing on first use. Disposing the group closes the browser.
        await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Chrome).WithHeadlessOption());
        Page page = await group.DefaultBrowser.NewPageAsync();
        await page.NavigateAsync("https://example.com/login");

        // Each action waits until its element is ready: visible, stable, enabled, and not covered.
        await page.GetByLabel("User name").FillAsync("ada");
        await page.GetByLabel("Password").FillAsync("correct horse battery staple");
        await page.GetByRole("button", "Sign in").ClickAsync();

        // Each expectation is checked again until it holds, or fails after five seconds saying what it last saw.
        await Expect(page).ToHaveUrlAsync(new Regex("/dashboard$"));
        await Expect(page.GetByRole("heading", "Welcome, Ada")).ToBeVisibleAsync();
        #endregion
    }
}
