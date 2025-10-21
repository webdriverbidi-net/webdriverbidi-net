// <copyright file="BrowsingContextModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.BrowsingContext;

/// <summary>
/// The BrowsingContext module contains commands and events relating to browsing contexts.
/// </summary>
public sealed class BrowsingContextModule : Module
{
    /// <summary>
    /// The name of the browsingContext module.
    /// </summary>
    public const string BrowsingContextModuleName = "browsingContext";

    private const string ContextCreatedEventName = $"{BrowsingContextModuleName}.contextCreated";
    private const string ContextDestroyedEventName = $"{BrowsingContextModuleName}.contextDestroyed";
    private const string NavigationStartedEventName = $"{BrowsingContextModuleName}.navigationStarted";
    private const string FragmentNavigatedEventName = $"{BrowsingContextModuleName}.fragmentNavigated";
    private const string DomContentLoadedEventName = $"{BrowsingContextModuleName}.domContentLoaded";
    private const string LoadEventName = $"{BrowsingContextModuleName}.load";
    private const string DownloadWillBeginEventName = $"{BrowsingContextModuleName}.downloadWillBegin";
    private const string DownloadEndEventName = $"{BrowsingContextModuleName}.downloadEnd";
    private const string NavigationAbortedEventName = $"{BrowsingContextModuleName}.navigationAborted";
    private const string NavigationCommittedEventName = $"{BrowsingContextModuleName}.navigationCommitted";
    private const string NavigationFailedEventName = $"{BrowsingContextModuleName}.navigationFailed";
    private const string HistoryUpdatedEventName = $"{BrowsingContextModuleName}.historyUpdated";
    private const string UserPromptClosedEventName = $"{BrowsingContextModuleName}.userPromptClosed";
    private const string UserPromptOpenedEventName = $"{BrowsingContextModuleName}.userPromptOpened";

    private readonly ObservableEvent<BrowsingContextEventArgs> onContextCreatedEvent = new(ContextCreatedEventName);
    private readonly ObservableEvent<BrowsingContextEventArgs> onContextDestroyedEvent = new(ContextDestroyedEventName);
    private readonly ObservableEvent<NavigationEventArgs> onNavigationStartedEvent = new(NavigationStartedEventName);
    private readonly ObservableEvent<NavigationEventArgs> onFragmentNavigatedEvent = new(FragmentNavigatedEventName);
    private readonly ObservableEvent<NavigationEventArgs> onDomContentLoadedEvent = new(DomContentLoadedEventName);
    private readonly ObservableEvent<NavigationEventArgs> onLoadEvent = new(LoadEventName);
    private readonly ObservableEvent<DownloadWillBeginEventArgs> onDownloadWillBeginEvent = new(DownloadWillBeginEventName);
    private readonly ObservableEvent<DownloadEndEventArgs> onDownloadEndEvent = new(DownloadEndEventName);
    private readonly ObservableEvent<NavigationEventArgs> onNavigationAbortedEvent = new(NavigationAbortedEventName);
    private readonly ObservableEvent<NavigationEventArgs> onNavigationCommittedEvent = new(NavigationCommittedEventName);
    private readonly ObservableEvent<NavigationEventArgs> onNavigationFailedEvent = new(NavigationFailedEventName);
    private readonly ObservableEvent<HistoryUpdatedEventArgs> onHistoryUpdatedEvent = new(HistoryUpdatedEventName);
    private readonly ObservableEvent<UserPromptClosedEventArgs> onUserPromptClosedEvent = new(UserPromptClosedEventName);
    private readonly ObservableEvent<UserPromptOpenedEventArgs> onUserPromptOpenedEvent = new(UserPromptOpenedEventName);

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public BrowsingContextModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<BrowsingContextInfo>(ContextCreatedEventName, this.OnContextCreatedAsync);
        this.RegisterAsyncEventInvoker<BrowsingContextInfo>(ContextDestroyedEventName, this.OnContextDestroyedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>(NavigationStartedEventName, this.OnNavigationStartedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>(FragmentNavigatedEventName, this.OnFragmentNavigatedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>(DomContentLoadedEventName, this.OnDomContentLoadedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>(LoadEventName, this.OnLoadAsync);
        this.RegisterAsyncEventInvoker<DownloadWillBeginEventArgs>(DownloadWillBeginEventName, this.OnDownloadWillBeginAsync);
        this.RegisterAsyncEventInvoker<DownloadEndEventArgs>(DownloadEndEventName, this.OnDownloadEndAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>(NavigationAbortedEventName, this.OnNavigationAbortedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>(NavigationCommittedEventName, this.OnNavigationCommittedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>(NavigationFailedEventName, this.OnNavigationFailedAsync);
        this.RegisterAsyncEventInvoker<HistoryUpdatedEventArgs>(HistoryUpdatedEventName, this.OnHistoryUpdatedAsync);
        this.RegisterAsyncEventInvoker<UserPromptClosedEventArgs>(UserPromptClosedEventName, this.OnUserPromptClosedAsync);
        this.RegisterAsyncEventInvoker<UserPromptOpenedEventArgs>(UserPromptOpenedEventName, this.OnUserPromptOpenedAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when a browsing context is created.
    /// </summary>
    public ObservableEvent<BrowsingContextEventArgs> OnContextCreated => this.onContextCreatedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context is destroyed.
    /// </summary>
    public ObservableEvent<BrowsingContextEventArgs> OnContextDestroyed => this.onContextDestroyedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is started.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnNavigationStarted => this.onNavigationStartedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context fragment is navigated.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnFragmentNavigated => this.onFragmentNavigatedEvent;

    /// <summary>
    /// Gets an observable event that notifies when the DOM content in a browsing context is loaded.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnDomContentLoaded => this.onDomContentLoadedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a download in a browsing context is about to begin.
    /// </summary>
    public ObservableEvent<DownloadWillBeginEventArgs> OnDownloadWillBegin => this.onDownloadWillBeginEvent;

    /// <summary>
    /// Gets an observable event that notifies when a download has ended.
    /// </summary>
    public ObservableEvent<DownloadEndEventArgs> OnDownloadEndEvent => this.onDownloadEndEvent;

    /// <summary>
    /// Gets an observable event that notifies when the content in a browsing context is loaded.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnLoad => this.onLoadEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is aborted.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnNavigationAborted => this.onNavigationAbortedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is committed.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnNavigationCommitted => this.onNavigationCommittedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation fails.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnNavigationFailed => this.onNavigationFailedEvent;

    /// <summary>
    /// Gets an observable event that notifies when the browser history is updated.
    /// </summary>
    public ObservableEvent<HistoryUpdatedEventArgs> OnHistoryUpdated => this.onHistoryUpdatedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a user prompt is opened.
    /// </summary>
    public ObservableEvent<UserPromptOpenedEventArgs> OnUserPromptOpened => this.onUserPromptOpenedEvent;

    /// <summary>
    /// Gets an observable event that notifies when a user prompt is closed.
    /// </summary>
    public ObservableEvent<UserPromptClosedEventArgs> OnUserPromptClosed => this.onUserPromptClosedEvent;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => BrowsingContextModuleName;

    /// <summary>
    /// Activates a browsing context by bringing it to the foreground.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<ActivateCommandResult> ActivateAsync(ActivateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<ActivateCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Captures a screenshot of the current page in the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<CaptureScreenshotCommandResult> CaptureScreenshotAsync(CaptureScreenshotCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<CaptureScreenshotCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Closes the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<CloseCommandResult> CloseAsync(CloseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<CloseCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command including the ID of the new context.</returns>
    public async Task<CreateCommandResult> CreateAsync(CreateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<CreateCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a tree of browsing contexts associated with a specified context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The tree associated browsing contexts.</returns>
    public async Task<GetTreeCommandResult> GetTreeAsync(GetTreeCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<GetTreeCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles a user prompt.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<HandleUserPromptCommandResult> HandleUserPromptAsync(HandleUserPromptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<HandleUserPromptCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Locates nodes within a browsing context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing the located nodes, if any.</returns>
    public async Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters commandParameters)
    {
        return await this.Driver.ExecuteCommandAsync<LocateNodesCommandResult>(commandParameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Navigates a browsing context to a new URL.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<NavigateCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Prints a PDF of the current page in the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>>The result of the command containing a base64-encoded PDF of the current page.</returns>
    public async Task<PrintCommandResult> PrintAsync(PrintCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<PrintCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Reloads a browsing context to a new URL.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<ReloadCommandResult> ReloadAsync(ReloadCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<ReloadCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the viewport of a browsing context to the specified dimensions.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<SetViewportCommandResult> SetViewportAsync(SetViewportCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<SetViewportCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Traverses the history entries of the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<TraverseHistoryCommandResult> TraverseHistoryAsync(TraverseHistoryCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<TraverseHistoryCommandResult>(commandProperties).ConfigureAwait(false);
    }

    private async Task OnContextCreatedAsync(EventInfo<BrowsingContextInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowsingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowsingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        BrowsingContextEventArgs eventArgs = eventData.ToEventArgs<BrowsingContextEventArgs>();
        await this.onContextCreatedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnContextDestroyedAsync(EventInfo<BrowsingContextInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowsingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowsingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        BrowsingContextEventArgs eventArgs = eventData.ToEventArgs<BrowsingContextEventArgs>();
        await this.onContextDestroyedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnNavigationStartedAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onNavigationStartedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnFragmentNavigatedAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onFragmentNavigatedEvent.NotifyObserversAsync(eventArgs);
     }

    private async Task OnDomContentLoadedAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onDomContentLoadedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnLoadAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onLoadEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnDownloadWillBeginAsync(EventInfo<DownloadWillBeginEventArgs> eventData)
    {
        DownloadWillBeginEventArgs eventArgs = eventData.ToEventArgs<DownloadWillBeginEventArgs>();
        await this.onDownloadWillBeginEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnDownloadEndAsync(EventInfo<DownloadEndEventArgs> eventData)
    {
        DownloadEndEventArgs eventArgs = eventData.ToEventArgs<DownloadEndEventArgs>();
        await this.onDownloadEndEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnNavigationAbortedAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onNavigationAbortedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnNavigationCommittedAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onNavigationCommittedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnNavigationFailedAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onNavigationFailedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnHistoryUpdatedAsync(EventInfo<HistoryUpdatedEventArgs> eventData)
    {
        HistoryUpdatedEventArgs eventArgs = eventData.ToEventArgs<HistoryUpdatedEventArgs>();
        await this.onHistoryUpdatedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnUserPromptClosedAsync(EventInfo<UserPromptClosedEventArgs> eventData)
    {
        UserPromptClosedEventArgs eventArgs = eventData.ToEventArgs<UserPromptClosedEventArgs>();
        await this.onUserPromptClosedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnUserPromptOpenedAsync(EventInfo<UserPromptOpenedEventArgs> eventData)
    {
        UserPromptOpenedEventArgs eventArgs = eventData.ToEventArgs<UserPromptOpenedEventArgs>();
        await this.onUserPromptOpenedEvent.NotifyObserversAsync(eventArgs);
    }
}
