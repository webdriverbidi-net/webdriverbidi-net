// <copyright file="BrowserGroup.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation;

using WebDriverBiDi.Browser;
using WebDriverBiDi.Browsers;
using WebDriverBiDi.BrowsingContext;
using WebDriverBiDi.Emulation;
using WebDriverBiDi.Permissions;
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
    private readonly Dictionary<string, Frame> frames = [];
    private readonly List<IDisposable> observers = [];
    private HashSet<string>? contextsDestroyedWhileStarting = [];
    private string? subscriptionId;
    private int isDisposed;

    private BrowserGroup(BiDiDriver driver, AutomationOptions options, BrowserLauncher? launcher, bool ownsSession)
    {
        this.Driver = driver;
        this.Options = options;
        this.launcher = launcher;
        this.ownsSession = ownsSession;
        this.observers.Add(driver.BrowsingContext.OnContextCreated.AddObserver(e => this.AddContextAsync(e.BrowsingContextId, e.Parent, e.UserContextId, e.Url)));
        this.observers.Add(driver.BrowsingContext.OnContextDestroyed.AddObserver(this.OnContextDestroyedAsync));
        this.observers.Add(driver.BrowsingContext.OnNavigationCommitted.AddObserver(e => this.SetUrl(e.BrowsingContextId, e.Url)));
        this.observers.Add(driver.BrowsingContext.OnFragmentNavigated.AddObserver(e => this.SetUrl(e.BrowsingContextId, e.Url)));
        this.observers.Add(driver.BrowsingContext.OnHistoryUpdated.AddObserver(e => this.SetUrl(e.BrowsingContextId, e.Url)));
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
    /// Creates a browser: a new user context, with its settings applied before it opens any page. If a setting
    /// cannot be applied, the user context is removed.
    /// </summary>
    /// <param name="options">The browser's settings, or <see langword="null"/> for the browser's defaults.</param>
    /// <param name="cancellationToken">A token that cancels the commands.</param>
    /// <returns>The browser.</returns>
    public async Task<Browser> CreateBrowserAsync(BrowserOptions? options = null, CancellationToken cancellationToken = default)
    {
        CreateUserContextCommandParameters parameters = new()
        {
            AcceptInsecureCerts = options?.AcceptInsecureCerts,
            Proxy = options?.Proxy,
            UnhandledPromptBehavior = options?.UnhandledPromptBehavior,
        };
        CreateUserContextCommandResult result = await this.Driver.Browser.CreateUserContextAsync(parameters, cancellationToken: cancellationToken).ConfigureAwait(false);
        lock (this.lockObject)
        {
            this.createdBrowserIds.Add(result.UserContextId);
        }

        Browser browser = this.GetOrAddBrowser(result.UserContextId);
        if (options is not null)
        {
            try
            {
                await this.ApplyOptionsAsync(browser.Id, options, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                await this.RunDisposalStepAsync($"Closing browser {browser.Id}", () => browser.CloseAsync()).ConfigureAwait(false);
                throw;
            }
        }

        return browser;
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

        foreach (IDisposable observer in this.observers)
        {
            observer.Dispose();
        }

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
    /// Records a frame by the ID of its browsing context.
    /// </summary>
    /// <param name="frame">The frame.</param>
    internal void RegisterFrame(Frame frame)
    {
        lock (this.lockObject)
        {
            this.frames[frame.Id] = frame;
        }
    }

    /// <summary>
    /// Detaches a closed browsing context's frame and those within it, closing its page if it is a main frame.
    /// A context the group does not track, or has already removed, is ignored.
    /// </summary>
    /// <param name="contextId">The ID of the browsing context.</param>
    /// <returns>A task that completes when observers are notified.</returns>
    internal async Task RemoveContextAsync(string contextId)
    {
        Frame? frame;
        lock (this.lockObject)
        {
            if (!this.frames.TryGetValue(contextId, out frame))
            {
                return;
            }

            foreach (Frame removed in frame.Page.RemoveFrame(frame))
            {
                this.frames.Remove(removed.Id);
            }
        }

        if (frame.IsMainFrame)
        {
            await frame.Page.Browser.RemovePageAsync(frame.Page).ConfigureAwait(false);
        }
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
            BrowsingContextModule module = this.Driver.BrowsingContext;
            SubscribeCommandParameters subscription = new([module.OnContextCreated.EventName, module.OnContextDestroyed.EventName, module.OnNavigationCommitted.EventName, module.OnFragmentNavigated.EventName, module.OnHistoryUpdated.EventName]);
            this.subscriptionId = (await this.Driver.Session.SubscribeAsync(subscription, cancellationToken: cancellationToken).ConfigureAwait(false)).SubscriptionId;
            GetUserContextsCommandResult userContexts = await this.Driver.Browser.GetUserContextsAsync(new GetUserContextsCommandParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
            foreach (UserContextInfo userContext in userContexts.UserContexts)
            {
                this.GetOrAddBrowser(userContext.UserContextId);
            }

            GetTreeCommandResult tree = await this.Driver.BrowsingContext.GetTreeAsync(new GetTreeCommandParameters(), cancellationToken: cancellationToken).ConfigureAwait(false);
            HashSet<string> destroyed;
            lock (this.lockObject)
            {
                destroyed = this.contextsDestroyedWhileStarting!;
                this.contextsDestroyedWhileStarting = null;
            }

            await this.AddTreeAsync(tree.ContextTree, null, destroyed).ConfigureAwait(false);
        }
        catch
        {
            await this.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private async Task ApplyOptionsAsync(string userContextId, BrowserOptions options, CancellationToken cancellationToken)
    {
        if (options.Viewport is not null || options.DevicePixelRatio is not null)
        {
            SetViewportCommandParameters viewport = new() { Viewport = options.Viewport, DevicePixelRatio = options.DevicePixelRatio };
            viewport.UserContexts.Add(userContextId);
            await this.Driver.BrowsingContext.SetViewportAsync(viewport, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (options.Locale is not null)
        {
            SetLocaleOverrideCommandParameters locale = new() { Locale = options.Locale };
            locale.UserContexts.Add(userContextId);
            await this.Driver.Emulation.SetLocaleOverrideAsync(locale, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (options.TimeZone is not null)
        {
            SetTimeZoneOverrideCommandParameters timeZone = new() { TimeZone = options.TimeZone };
            timeZone.UserContexts.Add(userContextId);
            await this.Driver.Emulation.SetTimeZoneOverrideAsync(timeZone, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (options.UserAgent is not null)
        {
            SetUserAgentOverrideCommandParameters userAgent = new() { UserAgent = options.UserAgent };
            userAgent.UserContexts.Add(userContextId);
            await this.Driver.Emulation.SetUserAgentOverrideAsync(userAgent, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (options.MediaFeatures is not null)
        {
            SetMediaFeaturesOverrideCommandParameters mediaFeatures = new() { Features = options.MediaFeatures };
            mediaFeatures.UserContexts.Add(userContextId);
            await this.Driver.Emulation.SetMediaFeaturesOverrideAsync(mediaFeatures, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        if (options.Geolocation is not null)
        {
            SetGeolocationOverrideCoordinatesCommandParameters geolocation = new() { Coordinates = options.Geolocation };
            geolocation.UserContexts.Add(userContextId);
            await this.Driver.Emulation.SetGeolocationOverrideAsync(geolocation, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        foreach (PermissionGrant grant in options.Permissions)
        {
            SetPermissionCommandParameters permission = new(grant.Descriptor, grant.State, grant.Origin) { UserContextId = userContextId };
            await this.Driver.Permissions.SetPermissionAsync(permission, cancellationToken: cancellationToken).ConfigureAwait(false);
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

    private async Task AddTreeAsync(IList<BrowsingContextInfo> contexts, string? parentId, HashSet<string> destroyed)
    {
        foreach (BrowsingContextInfo context in contexts.Where(context => !destroyed.Contains(context.BrowsingContextId)))
        {
            await this.AddContextAsync(context.BrowsingContextId, parentId, context.UserContextId, context.Url).ConfigureAwait(false);
            if (context.Children is not null)
            {
                await this.AddTreeAsync(context.Children, context.BrowsingContextId, destroyed).ConfigureAwait(false);
            }
        }
    }

    // A top-level context is a page; a child context is a frame of its parent's page, if the group tracks the parent.
    private async Task AddContextAsync(string contextId, string? parentId, string userContextId, string url)
    {
        if (parentId is null)
        {
            await this.GetOrAddBrowser(userContextId).AddPageAsync(contextId, url).ConfigureAwait(false);
            return;
        }

        lock (this.lockObject)
        {
            if (this.frames.ContainsKey(contextId) || !this.frames.TryGetValue(parentId, out Frame? parent))
            {
                return;
            }

            Frame frame = new(parent.Page, contextId, parent, url);
            parent.Page.AddFrame(frame);
            this.frames[contextId] = frame;
        }
    }

    private Task OnContextDestroyedAsync(ContextDestroyedEventArgs e)
    {
        lock (this.lockObject)
        {
            this.contextsDestroyedWhileStarting?.Add(e.BrowsingContextId);
        }

        return this.RemoveContextAsync(e.BrowsingContextId);
    }

    private void SetUrl(string contextId, string url)
    {
        lock (this.lockObject)
        {
            if (this.frames.TryGetValue(contextId, out Frame? frame))
            {
                frame.SetUrl(url);
            }
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
