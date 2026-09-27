// <copyright file="BrowserGroup.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Browser;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Session;

/// <summary>
/// A browser process and the WebDriver BiDi session driving it, from which browsers (user contexts) and their
/// pages are automated.
/// </summary>
public sealed class BrowserGroup : IAsyncDisposable
{
    /// <summary>
    /// The component name of the group's log messages.
    /// </summary>
    public const string LoggerComponentName = "Browser Group";

    private readonly BrowserLauncher? launcher;
    private readonly bool ownsSession;
    private readonly ObservableEventInvocable<LogMessageEventArgs> onLogMessage = new("browserGroup.logMessage");
    private readonly object lockObject = new();
    private readonly List<Browser> browsers = [];
    private readonly HashSet<string> createdBrowserIds = [];
    private readonly EventObserver<ContextCreatedEventArgs> contextCreatedObserver;
    private readonly EventObserver<ContextDestroyedEventArgs> contextDestroyedObserver;
    private HashSet<string>? contextsDestroyedWhileStarting = [];
    private string? subscriptionId;
    private int isDisposed;

    private BrowserGroup(BiDiDriver driver, AutomationOptions options, BrowserLauncher? launcher, bool ownsSession)
    {
        this.Driver = driver;
        this.Options = options;
        this.launcher = launcher;
        this.ownsSession = ownsSession;
        this.contextCreatedObserver = driver.BrowsingContext.OnContextCreated.AddObserver(this.OnContextCreatedAsync);
        this.contextDestroyedObserver = driver.BrowsingContext.OnContextDestroyed.AddObserver(this.OnContextDestroyedAsync);
    }

    /// <summary>
    /// Gets the driver that sends the group's commands.
    /// </summary>
    public BiDiDriver Driver { get; }

    /// <summary>
    /// Gets the options for the group and everything it creates.
    /// </summary>
    public AutomationOptions Options { get; }

    /// <summary>
    /// Gets an observable event raised when the group logs a message, such as a failure while it is disposed.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.onLogMessage;

    /// <summary>
    /// Gets the group's browsers: those it found when it started, those it created, and those created otherwise
    /// once a page opens in them.
    /// </summary>
    public IReadOnlyList<Browser> Browsers
    {
        get
        {
            lock (this.lockObject)
            {
                return [.. this.browsers];
            }
        }
    }

    /// <summary>
    /// Gets the browser of the default user context.
    /// </summary>
    public Browser DefaultBrowser => this.GetOrAddBrowser(Browser.DefaultBrowserId);

    /// <summary>
    /// Launches a browser, connects a driver to it, and starts a session, if the launcher did not start one.
    /// Disposing the group ends that session and closes the browser.
    /// </summary>
    /// <param name="launcherBuilder">The configured builder of the browser's launcher.</param>
    /// <param name="options">The options for the group, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the launch; anything already started is stopped.</param>
    /// <returns>The group.</returns>
    public static async Task<BrowserGroup> LaunchAsync(BrowserLauncherBuilder launcherBuilder, AutomationOptions? options = null, CancellationToken cancellationToken = default)
    {
        BrowserLauncher launcher = launcherBuilder.Build();
        BiDiDriver? driver = null;
        BrowserGroup group;
        try
        {
            await launcher.LaunchAsync(cancellationToken).ConfigureAwait(false);
            driver = new BiDiDriver(launcher.CreateTransport());
            await driver.StartAsync(launcher.ConnectionString, cancellationToken).ConfigureAwait(false);
            bool ownsSession = !launcher.IsBiDiSessionInitialized;
            if (ownsSession)
            {
                await driver.Session.NewSessionAsync(new NewCommandParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            group = new BrowserGroup(driver, options ?? new AutomationOptions(), launcher, ownsSession);
        }
        catch
        {
            if (driver is not null)
            {
                await driver.DisposeAsync().ConfigureAwait(false);
            }

            await launcher.DisposeAsync().ConfigureAwait(false);
            throw;
        }

        await group.StartAsync(cancellationToken).ConfigureAwait(false);
        return group;
    }

    /// <summary>
    /// Creates a group for a driver that is already connected, with its session started. Disposing the group
    /// removes what the group added, and leaves the driver, its session, and the browser running.
    /// </summary>
    /// <param name="driver">The connected driver.</param>
    /// <param name="options">The options for the group, or <see langword="null"/> for the defaults.</param>
    /// <param name="cancellationToken">A token that cancels the connection.</param>
    /// <returns>The group.</returns>
    public static async Task<BrowserGroup> ConnectAsync(BiDiDriver driver, AutomationOptions? options = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        BrowserGroup group = new(driver, options ?? new AutomationOptions(), null, false);
        await group.StartAsync(cancellationToken).ConfigureAwait(false);
        return group;
    }

    /// <summary>
    /// Creates a browser: a new user context.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the command.</param>
    /// <returns>The browser.</returns>
    public async Task<Browser> CreateBrowserAsync(CancellationToken cancellationToken = default)
    {
        CreateUserContextCommandResult result = await this.Driver.Browser.CreateUserContextAsync(new CreateUserContextCommandParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
        lock (this.lockObject)
        {
            this.createdBrowserIds.Add(result.UserContextId);
        }

        return this.GetOrAddBrowser(result.UserContextId);
    }

    /// <summary>
    /// Removes what the group added: the browsers it created, and its event subscription. If the group launched
    /// the browser, it then ends the session it started and closes the browser. Disposal does not throw; a step
    /// that fails is reported through <see cref="OnLogMessage"/>, and the steps after it still run.
    /// </summary>
    /// <returns>A task that completes when the group is disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this.isDisposed, 1) == 1)
        {
            return;
        }

        this.contextCreatedObserver.Dispose();
        this.contextDestroyedObserver.Dispose();
        List<Browser> createdBrowsers;
        lock (this.lockObject)
        {
            createdBrowsers = this.browsers.FindAll(browser => this.createdBrowserIds.Contains(browser.Id));
        }

        foreach (Browser browser in createdBrowsers)
        {
            await this.RunDisposalStepAsync($"Closing browser {browser.Id}", () => browser.CloseAsync()).ConfigureAwait(false);
        }

        // Ending the session removes its subscriptions.
        if (this.subscriptionId is not null && !this.ownsSession)
        {
            await this.RunDisposalStepAsync("Removing the event subscription", () => this.Driver.Session.UnsubscribeAsync(new UnsubscribeByIdsCommandParameters(this.subscriptionId))).ConfigureAwait(false);
        }

        if (this.launcher is null)
        {
            return;
        }

        if (this.ownsSession)
        {
            await this.RunDisposalStepAsync("Ending the session", () => this.Driver.Session.EndAsync(new EndCommandParameters())).ConfigureAwait(false);
        }

        await this.Driver.DisposeAsync().ConfigureAwait(false);
        await this.launcher.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Removes a browser that has closed.
    /// </summary>
    /// <param name="browser">The browser.</param>
    internal void RemoveBrowser(Browser browser)
    {
        lock (this.lockObject)
        {
            this.browsers.Remove(browser);
            this.createdBrowserIds.Remove(browser.Id);
        }
    }

    // Subscribes before reading the current contexts, so none can be missed; a context closed in between is
    // remembered so that the tree read before its closing does not bring it back.
    private async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            SubscribeCommandParameters subscription = new([this.Driver.BrowsingContext.OnContextCreated.EventName, this.Driver.BrowsingContext.OnContextDestroyed.EventName]);
            this.subscriptionId = (await this.Driver.Session.SubscribeAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false)).SubscriptionId;
            GetUserContextsCommandResult userContexts = await this.Driver.Browser.GetUserContextsAsync(new GetUserContextsCommandParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
            foreach (UserContextInfo userContext in userContexts.UserContexts)
            {
                this.GetOrAddBrowser(userContext.UserContextId);
            }

            GetTreeCommandResult tree = await this.Driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters() { MaxDepth = 0 }, cancellationToken: cancellationToken).ConfigureAwait(false);
            HashSet<string> destroyed;
            lock (this.lockObject)
            {
                destroyed = this.contextsDestroyedWhileStarting!;
                this.contextsDestroyedWhileStarting = null;
            }

            foreach (BrowsingContextInfo context in tree.ContextTree.Where(context => !destroyed.Contains(context.BrowsingContextId)))
            {
                await this.GetOrAddBrowser(context.UserContextId).AddPageAsync(context.BrowsingContextId).ConfigureAwait(false);
            }
        }
        catch
        {
            await this.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private Browser GetOrAddBrowser(string userContextId)
    {
        lock (this.lockObject)
        {
            Browser? browser = this.browsers.Find(candidate => candidate.Id == userContextId);
            if (browser is null)
            {
                browser = new Browser(this, userContextId);
                this.browsers.Add(browser);
            }

            return browser;
        }
    }

    private Browser? FindBrowser(string userContextId)
    {
        lock (this.lockObject)
        {
            return this.browsers.Find(candidate => candidate.Id == userContextId);
        }
    }

    // Frames are child contexts; only top-level contexts are pages.
    private async Task OnContextCreatedAsync(ContextCreatedEventArgs e)
    {
        if (e.Parent is null)
        {
            await this.GetOrAddBrowser(e.UserContextId).AddPageAsync(e.BrowsingContextId).ConfigureAwait(false);
        }
    }

    private async Task OnContextDestroyedAsync(ContextDestroyedEventArgs e)
    {
        if (e.Parent is not null)
        {
            return;
        }

        lock (this.lockObject)
        {
            this.contextsDestroyedWhileStarting?.Add(e.BrowsingContextId);
        }

        Browser? browser = this.FindBrowser(e.UserContextId);
        if (browser is not null)
        {
            await browser.RemovePageAsync(e.BrowsingContextId).ConfigureAwait(false);
        }
    }

    private async Task RunDisposalStepAsync(string description, Func<Task> step)
    {
        try
        {
            await step().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await this.LogAsync($"{description} failed: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
        }
    }

    private Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        return this.onLogMessage.InvokeNotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName));
    }
}
