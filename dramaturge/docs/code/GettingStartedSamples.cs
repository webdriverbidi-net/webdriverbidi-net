// <copyright file="GettingStartedSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/getting-started.md

namespace Dramaturge.Docs.Code;

using Dramaturge.Browsers;
using WebDriverBiDi;
using static Dramaturge.Assertions;

/// <summary>
/// Snippets for the getting started guide. Compiled at build time to prevent API drift.
/// </summary>
public static class GettingStartedSamples
{
    /// <summary>
    /// A first program.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task FirstProgram()
    {
        #region FirstProgram
        await using BrowserGroup group = await BrowserGroup.LaunchAsync(BrowserLauncher.Configure(BrowserKind.Chrome));
        Page page = await group.DefaultBrowser.NewPageAsync();
        await page.NavigateAsync("https://example.com");

        ElementLocator heading = page.GetByRole("heading");
        Console.WriteLine(await heading.TextContentAsync());

        await page.GetByRole("link").ClickAsync();
        await Expect(page).Not.ToHaveUrlAsync("https://example.com/");
        #endregion
    }

    /// <summary>
    /// The objects a group holds.
    /// </summary>
    /// <param name="group">A launched group.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ObjectModel(BrowserGroup group)
    {
        #region ObjectModel
        // A browser of its own: cookies, storage, and cache that no other browser in the group shares.
        Browser browser = await group.CreateBrowserAsync();
        Page page = await browser.NewPageAsync();

        // Frames are found from the elements that hold them.
        Frame frame = await page.Locate(new WebDriverBiDi.BrowsingContext.CssLocator("iframe#editor")).ContentFrameAsync();
        await frame.GetByRole("textbox").FillAsync("Hello");

        // Closing the browser closes its pages and discards its cookies and storage.
        await browser.CloseAsync();
        #endregion
    }

    /// <summary>
    /// What happens when an element never becomes ready.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Failures(Page page)
    {
        #region Failures
        try
        {
            await page.GetByRole("button", "Save").ClickAsync();
        }
        catch (WebDriverBiDiTimeoutException ex)
        {
            // Names the locator and what the last check saw, such as "the element was disabled".
            Console.WriteLine(ex.Message);
        }
        catch (AmbiguousElementException ex)
        {
            // More than one element matched; a locator for an action must match exactly one.
            Console.WriteLine(ex.Message);
        }
        #endregion
    }

    /// <summary>
    /// Adding Dramaturge to a driver that is already connected.
    /// </summary>
    /// <param name="driver">A connected driver with its session started.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ConnectToDriver(BiDiDriver driver)
    {
        #region ConnectToDriver
        // Disposing the group removes what it added, and leaves the driver, its session, and the browser running.
        await using BrowserGroup group = await BrowserGroup.ConnectAsync(driver);
        Page page = await group.DefaultBrowser.NewPageAsync();
        #endregion
    }
}
