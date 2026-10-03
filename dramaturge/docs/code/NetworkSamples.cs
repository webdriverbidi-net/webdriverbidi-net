// <copyright file="NetworkSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/network.md

namespace Dramaturge.Docs.Code;

using System.Text.RegularExpressions;
using WebDriverBiDi.Network;

/// <summary>
/// Snippets for the network guide. Compiled at build time to prevent API drift.
/// </summary>
public static class NetworkSamples
{
    /// <summary>
    /// Answering a request with a response.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Fulfill(Page page)
    {
        #region Fulfill
        RouteRegistration route = await page.RouteAsync(
            "https://api.example.com/v1/user",
            r => r.FulfillAsync(200, """{"name":"Ada"}""", new Dictionary<string, string>() { ["Content-Type"] = "application/json" }));

        await page.NavigateAsync("https://example.com/profile");
        await route.RemoveAsync();
        #endregion
    }

    /// <summary>
    /// Changing, aborting, and passing on requests.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ContinueAndAbort(Page page)
    {
        #region ContinueAndAbort
        // Blocks images.
        await page.RouteAsync(request => request.Url.EndsWith(".png", StringComparison.Ordinal), r => r.AbortAsync());

        // Sends API requests to a staging server, with an extra header.
        await page.RouteAsync(new Regex("^https://api\\.example\\.com/"), r => r.ContinueAsync(new RouteOverrides()
        {
            Url = r.Request.Url.Replace("api.example.com", "staging-api.example.com"),
            Headers = new Dictionary<string, string>() { ["X-Test-Run"] = "nightly" },
        }));

        // Decides nothing for GET requests, which pass to the next matching route, or continue unchanged.
        await page.RouteAsync(new Regex("/orders"), async r =>
        {
            if (r.Request.Method == "POST")
            {
                await r.FulfillAsync(201, """{"id":42}""");
            }
        });
        #endregion
    }

    /// <summary>
    /// Stopping only the requests a pattern matches.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Filter(Page page)
    {
        #region Filter
        UrlPatternPattern apiOnly = new() { HostName = "api.example.com" };
        await page.RouteAsync(new Regex("/v1/"), r => r.AbortAsync(), apiOnly);
        #endregion
    }

    /// <summary>
    /// A route for every page of a browser.
    /// </summary>
    /// <param name="browser">A browser.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task BrowserRoutes(Browser browser)
    {
        #region BrowserRoutes
        // Answers the browser's requests to the API, from any of its pages, popups included, and its workers.
        await browser.RouteAsync(
            new Regex("^https://api\\.example\\.com/v1/flags"),
            r => r.FulfillAsync(200, """{"newCheckout":true}""", new Dictionary<string, string>() { ["Content-Type"] = "application/json" }),
            new UrlPatternPattern() { HostName = "api.example.com" });
        #endregion
    }

    /// <summary>
    /// Recording a page's traffic to a HAR file.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RecordHar(Page page)
    {
        #region RecordHar
        // Disposing the recording writes the file.
        await using (HarRecording recording = await page.RecordHarAsync(
            "checkout.har",
            new HarRecordingOptions() { Include = request => request.Url.StartsWith("https://api.example.com/", StringComparison.Ordinal) }))
        {
            await page.NavigateAsync("https://example.com/checkout");
            await page.GetByRole("button", "Place order").ClickAsync();
        }
        #endregion
    }

    /// <summary>
    /// Replaying a HAR file.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task ReplayHar(Page page)
    {
        #region ReplayHar
        // The API's requests are answered from the file; one it has no entry for fails.
        await page.RouteFromHarAsync("checkout.har", new Regex("^https://api\\.example\\.com/"));

        // A request the file has no entry for goes to the next route, or to the network.
        await page.RouteFromHarAsync("assets.har", new HarRouteOptions() { NotFound = HarNotFound.Fallback });
        #endregion
    }

    /// <summary>
    /// Waiting for a request or a response.
    /// </summary>
    /// <param name="page">A page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WaitForRequests(Page page)
    {
        #region WaitForRequests
        BeforeRequestSentEventArgs sent = await page.RunAndWaitForRequestAsync(
            () => page.GetByRole("button", "Save").ClickAsync(),
            request => request.Method == "PUT");

        ResponseCompletedEventArgs done = await page.RunAndWaitForResponseAsync(
            () => page.GetByRole("button", "Refresh").ClickAsync(),
            new Regex("/api/items"));
        Console.WriteLine(done.Response.Status);
        #endregion
    }

    /// <summary>
    /// Cookies.
    /// </summary>
    /// <param name="browser">A browser.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Cookies(Browser browser)
    {
        #region Cookies
        await browser.AddCookiesAsync([new BrowserCookie("session", "abc123", "example.com") { Secure = true, HttpOnly = true }]);
        IReadOnlyList<BrowserCookie> cookies = await browser.GetCookiesAsync(domain: "example.com");
        await browser.ClearCookiesAsync(name: "session");
        #endregion
    }
}
