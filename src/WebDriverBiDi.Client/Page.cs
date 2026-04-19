// <copyright file="Page.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Elements;

/// <summary>
/// Provides a high-level abstraction over a browsing context for navigating and interacting with web pages.
/// </summary>
public class Page
{
    private readonly BiDiDriver driver;
    private readonly string browsingContextId;
    private readonly ElementStateInspector inspector;
    private readonly ElementLocatorSettings locatorSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="Page"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="browsingContextId">The ID of the browsing context this page wraps.</param>
    public Page(BiDiDriver driver, string browsingContextId)
        : this(driver, browsingContextId, new ElementLocatorSettings())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Page"/> class with custom locator settings.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used for executing commands.</param>
    /// <param name="browsingContextId">The ID of the browsing context this page wraps.</param>
    /// <param name="locatorSettings">The <see cref="ElementLocatorSettings"/> to apply to element locators created by this page.</param>
    public Page(BiDiDriver driver, string browsingContextId, ElementLocatorSettings locatorSettings)
    {
        this.driver = driver;
        this.browsingContextId = browsingContextId;
        this.locatorSettings = locatorSettings;
        this.inspector = new ElementStateInspector(driver);
    }

    /// <summary>
    /// Gets the ID of the browsing context this page wraps.
    /// </summary>
    public string BrowsingContextId => this.browsingContextId;

    /// <summary>
    /// Navigates the page to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    /// <param name="wait">The readiness state to wait for after navigation. Defaults to <see cref="ReadinessState.Complete"/>.</param>
    /// <param name="timeout">Optional timeout override. If null, uses the driver's default command timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the URL navigated to after any redirects.</returns>
    public async Task<string> NavigateAsync(string url, ReadinessState wait = ReadinessState.Complete, TimeSpan? timeout = null)
    {
        NavigateCommandParameters parameters = new(this.browsingContextId, url)
        {
            Wait = wait,
        };
        NavigateCommandResult result = await this.driver.BrowsingContext.NavigateAsync(parameters, timeout).ConfigureAwait(false);
        return result.Url;
    }

    /// <summary>
    /// Captures a screenshot of the page.
    /// </summary>
    /// <param name="fullPage">
    /// When <see langword="true"/>, captures the full document including content outside the viewport.
    /// When <see langword="false"/> (default), captures only the visible viewport.
    /// </param>
    /// <param name="timeout">Optional timeout override. If null, uses the driver's default command timeout.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the screenshot as a PNG byte array.</returns>
    public async Task<byte[]> GetScreenshotAsync(bool fullPage = false, TimeSpan? timeout = null)
    {
        CaptureScreenshotCommandParameters parameters = new(this.browsingContextId)
        {
            Origin = fullPage ? ScreenshotOrigin.Document : ScreenshotOrigin.Viewport,
        };
        CaptureScreenshotCommandResult result = await this.driver.BrowsingContext.CaptureScreenshotAsync(parameters, timeout).ConfigureAwait(false);
        return Convert.FromBase64String(result.Data);
    }

    /// <summary>
    /// Creates an <see cref="ElementLocator"/> for finding elements in this page using the specified locator strategy.
    /// </summary>
    /// <param name="locator">The <see cref="Locator"/> strategy to use to find elements.</param>
    /// <returns>An <see cref="ElementLocator"/> that can be used to interact with the matched elements.</returns>
    public ElementLocator LocateElement(Locator locator)
    {
        return new ElementLocator(this.driver, this.browsingContextId, locator, null, this.inspector, this.locatorSettings, null);
    }
}
