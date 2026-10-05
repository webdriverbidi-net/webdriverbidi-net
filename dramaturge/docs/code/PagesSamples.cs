// <copyright file="PagesSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/pages-and-frames.md

namespace Dramaturge.Docs.Code;

using System.Text.RegularExpressions;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Script;
using WebDriverBiDi.Session;

/// <summary>
/// Snippets for the pages and frames guide. Compiled at build time to prevent API drift.
/// </summary>
public static class PagesSamples
{
    /// <summary>
    /// Navigating and waiting for navigation.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Navigation(Page page)
    {
        #region Navigation
        await page.NavigateAsync("https://example.com/shop");
        await page.NavigateAsync("https://example.com/feed", ReadinessState.Interactive);
        await page.GoBackAsync();
        await page.ReloadAsync();

        // Waits for the navigation the click causes, whatever its URL.
        string url = await page.RunAndWaitForNavigationAsync(() => page.GetByRole("link", "Checkout").ClickAsync());

        // Waits until the URL matches, including a change made with history.pushState.
        await page.WaitForUrlAsync(new Regex("/checkout/payment$"));
        #endregion
    }

    /// <summary>
    /// Running scripts in a page.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Scripts(Page page)
    {
        #region Scripts
        int width = await page.EvaluateAsync<int>("() => window.innerWidth");
        string[] links = await page.EvaluateAsync<string[]>("() => [...document.links].map(link => link.href)");
        Dictionary<string, object?> settings = await page.EvaluateAsync<Dictionary<string, object?>>(
            "(key) => JSON.parse(localStorage.getItem(key))",
            [LocalValue.String("settings")]);

        // An element is passed to the function first.
        string tag = await page.GetByRole("main").EvaluateAsync<string>("(element) => element.tagName");

        // Calls a function until it returns a truthy value.
        await page.WaitForFunctionAsync("() => window.appReady === true");
        #endregion
    }

    /// <summary>
    /// Scripts that run before a page's own.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InitScripts(Page page)
    {
        #region InitScripts
        InitScript script = await page.AddInitScriptAsync("() => { Math.random = () => 0.5; }");
        await page.ReloadAsync();
        await script.RemoveAsync();
        #endregion
    }

    /// <summary>
    /// Console messages and errors.
    /// </summary>
    /// <param name="page">A page.</param>
    public static void ConsoleAndErrors(Page page)
    {
        #region ConsoleAndErrors
        page.OnConsoleMessage.AddObserver(e => Console.WriteLine($"[{e.Method}] {e.Text}"));
        page.OnPageError.AddObserver(e => Console.WriteLine($"Uncaught: {e.Message}"));
        #endregion
    }

    /// <summary>
    /// Answering dialogs.
    /// </summary>
    /// <param name="group">A launched group.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Dialogs(BrowserGroup group)
    {
        #region Dialogs
        // Leaves dialogs open, so that they can be answered here.
        Browser browser = await group.CreateBrowserAsync(new BrowserOptions()
        {
            UnhandledPromptBehavior = new UserPromptHandler() { Default = UserPromptHandlerType.Ignore },
        });
        Page page = await browser.NewPageAsync();

        page.OnDialog.AddObserver(async e =>
        {
            if (e.Dialog.Type == UserPromptType.Prompt)
            {
                await e.Dialog.AcceptAsync("Ada");
            }
            else
            {
                await e.Dialog.DismissAsync();
            }
        });
        #endregion
    }

    /// <summary>
    /// Popups.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Popups(Page page)
    {
        #region Popups
        Page help = await page.RunAndWaitForPopupAsync(() => page.GetByRole("link", "Open help").ClickAsync());
        await help.WaitForLoadStateAsync();
        #endregion
    }

    /// <summary>
    /// Downloads.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Downloads(Page page)
    {
        #region Downloads
        await page.Browser.AllowDownloadsAsync("/tmp/downloads");

        Download download = await page.RunAndWaitForDownloadAsync(() => page.GetByRole("link", "Export CSV").ClickAsync());
        DownloadOutcome outcome = await download.WaitForEndAsync();
        Console.WriteLine($"{download.SuggestedFileName}: {outcome.Status} at {outcome.FilePath}");
        #endregion
    }

    /// <summary>
    /// Screenshots, PDFs, and the viewport.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Capture(Page page)
    {
        #region Capture
        byte[] screen = await page.ScreenshotAsync();
        byte[] whole = await page.ScreenshotAsync(new PageScreenshotOptions() { FullPage = true });
        byte[] chart = await page.GetByRole("img", "Sales chart").ScreenshotAsync();
        byte[] pdf = await page.PdfAsync(new PdfOptions() { Orientation = PrintOrientation.Landscape });

        await page.SetViewportSizeAsync(375, 812);
        await page.ResetViewportSizeAsync();
        #endregion
    }

    /// <summary>
    /// Video.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Video(Page page)
    {
        #region Video
        await using VideoRecording video = await page.RecordVideoAsync("videos/checkout.webm", new VideoRecordingOptions() { Width = 1280, Height = 720 });

        await page.NavigateAsync("https://example.com/checkout");
        await page.GetByRole("button", "Place order").ClickAsync();
        string path = await video.StopAsync();
        #endregion
    }
}
