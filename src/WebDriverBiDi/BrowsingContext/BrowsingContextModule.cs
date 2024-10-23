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

    private readonly ObservableEvent<BrowsingContextEventArgs> onContextCreatedEvent = new();
    private readonly ObservableEvent<BrowsingContextEventArgs> onContextDestroyedEvent = new();
    private readonly ObservableEvent<NavigationEventArgs> onNavigationStartedEvent = new();
    private readonly ObservableEvent<NavigationEventArgs> onFragmentNavigatedEvent = new();
    private readonly ObservableEvent<NavigationEventArgs> onDomContentLoadedEvent = new();
    private readonly ObservableEvent<NavigationEventArgs> onLoadEvent = new();
    private readonly ObservableEvent<NavigationEventArgs> onDownloadWillBeginEvent = new();
    private readonly ObservableEvent<NavigationEventArgs> onNavigationAbortedEvent = new();
    private readonly ObservableEvent<NavigationEventArgs> onNavigationFailedEvent = new();
    private readonly ObservableEvent<HistoryUpdatedEventArgs> onHistoryUpdatedEvent = new();
    private readonly ObservableEvent<UserPromptClosedEventArgs> onUserPromptClosedEvent = new();
    private readonly ObservableEvent<UserPromptOpenedEventArgs> onUserPromptOpenedEvent = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public BrowsingContextModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<BrowsingContextInfo>("browsingContext.contextCreated", this.OnContextCreatedAsync);
        this.RegisterAsyncEventInvoker<BrowsingContextInfo>("browsingContext.contextDestroyed", this.OnContextDestroyedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>("browsingContext.navigationStarted", this.OnNavigationStartedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>("browsingContext.fragmentNavigated", this.OnFragmentNavigatedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>("browsingContext.domContentLoaded", this.OnDomContentLoadedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>("browsingContext.load", this.OnLoadAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>("browsingContext.downloadWillBegin", this.OnDownloadWillBeginAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>("browsingContext.navigationAborted", this.OnNavigationAbortedAsync);
        this.RegisterAsyncEventInvoker<NavigationEventArgs>("browsingContext.navigationFailed", this.OnNavigationFailedAsync);
        this.RegisterAsyncEventInvoker<HistoryUpdatedEventArgs>("browsingContext.historyUpdated", this.OnHistoryUpdatedAsync);
        this.RegisterAsyncEventInvoker<UserPromptClosedEventArgs>("browsingContext.userPromptClosed", this.OnUserPromptClosedAsync);
        this.RegisterAsyncEventInvoker<UserPromptOpenedEventArgs>("browsingContext.userPromptOpened", this.OnUserPromptOpenedAsync);
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
    public ObservableEvent<NavigationEventArgs> OnDownloadWillBegin => this.onDownloadWillBeginEvent;

    /// <summary>
    /// Gets an observable event that notifies when the content in a browsing context is loaded.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnLoad => this.onLoadEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is aborted.
    /// </summary>
    public ObservableEvent<NavigationEventArgs> OnNavigationAborted => this.onNavigationAbortedEvent;

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
    public async Task<EmptyResult> ActivateAsync(ActivateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
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
    public async Task<EmptyResult> CloseAsync(CloseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
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
    public async Task<EmptyResult> HandleUserPromptAsync(HandleUserPromptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
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
    public async Task<NavigationResult> NavigateAsync(NavigateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<NavigationResult>(commandProperties).ConfigureAwait(false);
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
    public async Task<NavigationResult> ReloadAsync(ReloadCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<NavigationResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Sets the viewport of a browsing context to the specified dimensions.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<EmptyResult> SetViewportAsync(SetViewportCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Traverses the history entries of the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<EmptyResult> TraverseHistoryAsync(TraverseHistoryCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
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

    private async Task OnDownloadWillBeginAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onDownloadWillBeginEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnNavigationAbortedAsync(EventInfo<NavigationEventArgs> eventData)
    {
        NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
        await this.onNavigationAbortedEvent.NotifyObserversAsync(eventArgs);
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
