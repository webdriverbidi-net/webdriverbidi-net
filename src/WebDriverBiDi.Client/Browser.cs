// <copyright file="Browser.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Elements;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Session;

/// <summary>
/// Provides a high-level abstraction for launching a browser, initializing a WebDriver BiDi session,
/// and accessing a <see cref="Page"/> object for the initial top-level browsing context.
/// </summary>
public class Browser : IAsyncDisposable
{
    private readonly BrowserLauncher launcher;
    private readonly BiDiDriver driver;
    private readonly ElementLocatorSettings locatorSettings;
    private Page? page;
    private bool disposed = false;

    private Browser(BrowserLauncher launcher, BiDiDriver driver, ElementLocatorSettings locatorSettings)
    {
        this.launcher = launcher;
        this.driver = driver;
        this.locatorSettings = locatorSettings;
    }

    /// <summary>
    /// Gets the <see cref="BiDiDriver"/> instance used to communicate with the browser.
    /// </summary>
    public BiDiDriver Driver => this.driver;

    /// <summary>
    /// Launches the specified browser, initializes a WebDriver BiDi session, and returns a
    /// <see cref="Browser"/> instance representing the running browser.
    /// </summary>
    /// <param name="browserKind">The browser to launch.</param>
    /// <param name="headless">Whether to run the browser in headless mode. Default is false.</param>
    /// <param name="locatorSettings">Optional <see cref="ElementLocatorSettings"/> to apply to element locators. If null, default settings are used.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the launched <see cref="Browser"/> instance.</returns>
    public static Task<Browser> LaunchAsync(BrowserKind browserKind, bool headless = false, ElementLocatorSettings? locatorSettings = null)
    {
        BrowserLauncher launcher = BrowserLauncher.Create(browserKind, headless);
        return LaunchAsync(launcher, locatorSettings);
    }

    /// <summary>
    /// Launches a browser using the specified <see cref="BrowserLauncherBuilder"/> configuration,
    /// initializes a WebDriver BiDi session, and returns a <see cref="Browser"/> instance
    /// representing the running browser.
    /// </summary>
    /// <param name="launcherBuilder">The configured launcher builder to use.</param>
    /// <param name="locatorSettings">Optional <see cref="ElementLocatorSettings"/> to apply to element locators. If null, default settings are used.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the launched <see cref="Browser"/> instance.</returns>
    public static async Task<Browser> LaunchAsync(BrowserLauncherBuilder launcherBuilder, ElementLocatorSettings? locatorSettings = null)
    {
        BrowserLauncher launcher = launcherBuilder.Build();
        return await LaunchAsync(launcher, locatorSettings).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the <see cref="Page"/> object for the initial top-level browsing context.
    /// </summary>
    /// <returns>The <see cref="Page"/> object for the initial top-level browsing context.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no top-level browsing context is available.</exception>
    public Page GetPage()
    {
        if (this.page is null)
        {
            throw new InvalidOperationException("No page is available. The browser may not have been launched successfully.");
        }

        return this.page;
    }

    /// <summary>
    /// Asynchronously releases all resources used by this browser instance, including stopping
    /// the WebDriver BiDi session and quitting the browser process.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        try
        {
            if (this.launcher.IsBrowserCloseAllowed)
            {
                await this.driver.Browser.CloseAsync(new WebDriverBiDi.Browser.CloseCommandParameters()).ConfigureAwait(false);
            }
        }
        catch
        {
            // Suppress: closing via BiDi is best-effort; the launcher will terminate the process.
        }

        try
        {
            await this.driver.StopAsync().ConfigureAwait(false);
        }
        catch
        {
            // Suppress exceptions from driver stop during disposal.
        }

        await this.launcher.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private static async Task<Browser> LaunchAsync(BrowserLauncher launcher, ElementLocatorSettings? locatorSettings)
    {
        ElementLocatorSettings settings = locatorSettings ?? new ElementLocatorSettings();
        BiDiDriver driver = new();

        try
        {
            await launcher.StartAsync().ConfigureAwait(false);
            await launcher.LaunchBrowserAsync().ConfigureAwait(false);
            await driver.StartAsync(launcher.WebSocketUrl).ConfigureAwait(false);

            if (!launcher.IsBiDiSessionInitialized)
            {
                await driver.Session.NewSessionAsync(new NewCommandParameters()).ConfigureAwait(false);
            }

            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters() { MaxDepth = 1 }).ConfigureAwait(false);
            if (tree.ContextTree.Count == 0)
            {
                throw new WebDriverBiDiException("No top-level browsing context found after launching browser.");
            }

            string contextId = tree.ContextTree[0].BrowsingContextId;
            Browser browser = new(launcher, driver, settings)
            {
                page = new Page(driver, contextId, settings),
            };

            return browser;
        }
        catch
        {
            await driver.DisposeAsync().ConfigureAwait(false);
            await launcher.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
