// <copyright file="TracingSamples.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// Code snippets for docs/articles/tracing.md

namespace Dramaturge.Docs.Code;

using static Dramaturge.Assertions;

/// <summary>
/// Snippets for the tracing guide. Compiled at build time to prevent API drift.
/// </summary>
public static class TracingSamples
{
    /// <summary>
    /// Recording a browser's trace.
    /// </summary>
    /// <param name="browser">A browser.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RecordTrace(Browser browser)
    {
        #region RecordTrace
        TraceRecordingOptions options = new() { Title = "Checkout", Snapshots = true, Screenshots = true, Sources = true };
        await using (TraceRecording trace = await browser.RecordTraceAsync("traces/checkout.zip", options))
        {
            Page page = await browser.NewPageAsync();
            await page.NavigateAsync("https://example.com/checkout");
            await page.GetByRole("button", "Place order").ClickAsync();
            await Expect(page.GetByRole("heading", "Thank you")).ToBeVisibleAsync();
        }
        #endregion
    }

    /// <summary>
    /// Stopping a trace explicitly.
    /// </summary>
    /// <param name="browser">A browser.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task StopTrace(Browser browser)
    {
        #region StopTrace
        TraceRecording trace = await browser.RecordTraceAsync("trace.zip");
        try
        {
            Page page = await browser.NewPageAsync();
            await page.NavigateAsync("https://example.com/");
        }
        finally
        {
            string path = await trace.StopAsync();
            Console.WriteLine($"Open {path} at https://trace.playwright.dev");
        }
        #endregion
    }
}
