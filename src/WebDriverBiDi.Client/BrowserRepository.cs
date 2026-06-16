// <copyright file="BrowserRepository.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client;

using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Client.Elements;
using WebDriverBiDi.Client.Launchers;
using WebDriverBiDi.Session;

/// <summary>
/// Provides a high-level abstraction for launching a browser process, initializing a WebDriver BiDi session,
/// and managing <see cref="Browser"/> instances representing user contexts within that session.
/// </summary>
public class BrowserRepository : IAsyncDisposable
{
    private readonly BrowserLauncher launcher;
    private readonly BiDiDriver driver;
    private readonly ElementLocatorSettings locatorSettings;
    private readonly ElementStateInspector inspector;
    private readonly List<Browser> browsers = [];
    private string navigationEventSubscriptionId = string.Empty;
    private bool disposed = false;

    private BrowserRepository(BrowserLauncher launcher, BiDiDriver driver, ElementLocatorSettings locatorSettings)
    {
        this.launcher = launcher;
        this.driver = driver;
        this.locatorSettings = locatorSettings;
        this.inspector = new ElementStateInspector(driver);
    }

    /// <summary>
    /// Gets the <see cref="BiDiDriver"/> instance used to communicate with the browser.
    /// </summary>
    public BiDiDriver Driver => this.driver;

    /// <summary>
    /// Gets a read-only list of <see cref="Browser"/> instances representing the user contexts
    /// currently open in this browser session.
    /// </summary>
    public IReadOnlyList<Browser> Browsers => this.browsers.AsReadOnly();

    /// <summary>
    /// Gets the <see cref="ElementStateInspector"/> for contexts in this repository instance.
    /// </summary>
    internal ElementStateInspector ElementStateInspector => this.inspector;

    /// <summary>
    /// Launches the specified browser, initializes a WebDriver BiDi session, and returns a
    /// <see cref="BrowserRepository"/> instance representing the running browser.
    /// </summary>
    /// <param name="browserKind">The browser to launch.</param>
    /// <param name="headless">Whether to run the browser in headless mode. Default is false.</param>
    /// <param name="locatorSettings">Optional <see cref="ElementLocatorSettings"/> to apply to element locators. If null, default settings are used.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the launched <see cref="BrowserRepository"/> instance.</returns>
    public static Task<BrowserRepository> LaunchAsync(BrowserKind browserKind, bool headless = false, ElementLocatorSettings? locatorSettings = null)
    {
        BrowserLauncher launcher = BrowserLauncher.Create(browserKind, headless);
        return LaunchAsync(launcher, locatorSettings);
    }

    /// <summary>
    /// Launches a browser using the specified <see cref="BrowserLauncherBuilder"/> configuration,
    /// initializes a WebDriver BiDi session, and returns a <see cref="BrowserRepository"/> instance
    /// representing the running browser.
    /// </summary>
    /// <param name="launcherBuilder">The configured launcher builder to use.</param>
    /// <param name="locatorSettings">Optional <see cref="ElementLocatorSettings"/> to apply to element locators. If null, default settings are used.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the launched <see cref="BrowserRepository"/> instance.</returns>
    public static async Task<BrowserRepository> LaunchAsync(BrowserLauncherBuilder launcherBuilder, ElementLocatorSettings? locatorSettings = null)
    {
        BrowserLauncher launcher = launcherBuilder.Build();
        return await LaunchAsync(launcher, locatorSettings).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new user context in the browser and returns a <see cref="Browser"/> instance
    /// representing that user context.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the new <see cref="Browser"/> instance.</returns>
    public async Task<Browser> CreateBrowserAsync()
    {
        WebDriverBiDi.Browser.CreateUserContextCommandResult result = await this.driver.Browser.CreateUserContextAsync(new WebDriverBiDi.Browser.CreateUserContextCommandParameters()).ConfigureAwait(false);
        Browser browser = new(this.driver, result.UserContextId, this, this.locatorSettings);
        CreateCommandParameters createParameters = new(CreateType.Tab)
        {
            UserContextId = result.UserContextId,
        };
        CreateCommandResult browsingContextResult = await this.driver.BrowsingContext.CreateAsync(createParameters);
        browser.AddPage(new Page(this.driver, browsingContextResult.BrowsingContextId, this.inspector));
        this.browsers.Add(browser);
        return browser;
    }

    /// <summary>
    /// Asynchronously releases all resources used by this browser repository instance, including
    /// stopping the WebDriver BiDi session and quitting the browser process.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;

        Browser[] snapshot = [.. this.browsers];
        this.browsers.Clear();

        foreach (Browser browser in snapshot)
        {
            await browser.DisposeAsync().ConfigureAwait(false);
        }

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
            UnsubscribeByIdsCommandParameters unsubscribeParameters = new();
            unsubscribeParameters.SubscriptionIds.Add(this.navigationEventSubscriptionId);
            await this.driver.Session.UnsubscribeAsync(unsubscribeParameters).ConfigureAwait(false);
            await this.driver.StopAsync().ConfigureAwait(false);
        }
        catch
        {
            // Suppress exceptions from driver stop during disposal.
        }

        await this.launcher.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Removes a <see cref="Browser"/> instance from the list of managed browsers. Called by
    /// <see cref="Browser.DisposeAsync"/> when the user context is disposed independently.
    /// </summary>
    /// <param name="browser">The <see cref="Browser"/> to remove.</param>
    internal void RemoveBrowser(Browser browser)
    {
        this.browsers.Remove(browser);
    }

    private static async Task<BrowserRepository> LaunchAsync(BrowserLauncher launcher, ElementLocatorSettings? locatorSettings)
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

            BrowserRepository repository = new(launcher, driver, settings);

            // Initialize the ElementStateInspector, which registers a preload script.
            // The preload script will propagate to all pages loaded in any browsing
            // context managed by this driver, including created user contexts and
            // browsing contexts created by this BrowserRepository.
            await repository.inspector.AddInspectorAsync().ConfigureAwait(false);

            // Subscribe to all browsing context navigation events
            List<string> navigationEvents =
            [
                driver.BrowsingContext.OnNavigationStarted.EventName,
                driver.BrowsingContext.OnNavigationCommitted.EventName,
                driver.BrowsingContext.OnNavigationAborted.EventName,
                driver.BrowsingContext.OnNavigationFailed.EventName,
                driver.BrowsingContext.OnDomContentLoaded.EventName,
                driver.BrowsingContext.OnLoad.EventName,
            ];
            SubscribeCommandResult subscribeResult = await driver.Session.SubscribeAsync(new SubscribeCommandParameters(navigationEvents)).ConfigureAwait(false);
            string subscriptionId = subscribeResult.SubscriptionId;
            repository.navigationEventSubscriptionId = subscriptionId;

            GetTreeCommandParameters getTreeParameters = new()
            {
                MaxDepth = 1,
            };
            GetTreeCommandResult tree = await driver.BrowsingContext.GetTreeAsync(getTreeParameters).ConfigureAwait(false);

            foreach (BrowsingContextInfo context in tree.ContextTree)
            {
                Browser defaultBrowser = new(driver, context.UserContextId, repository, settings);
                defaultBrowser.AddPage(new Page(driver, context.BrowsingContextId, repository.inspector));
                repository.browsers.Add(defaultBrowser);
            }

            return repository;
        }
        catch
        {
            await driver.DisposeAsync().ConfigureAwait(false);
            await launcher.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
