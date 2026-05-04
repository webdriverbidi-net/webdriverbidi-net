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

    private readonly ObservableEventInvocable<BrowsingContextEventArgs> invocableContextCreatedObservableEvent = new(ContextCreatedEventName);
    private readonly ObservableEventInvocable<BrowsingContextEventArgs> invocableContextDestroyedObservableEvent = new(ContextDestroyedEventName);
    private readonly ObservableEventInvocable<NavigationEventArgs> invocableNavigationStartedObservableEvent = new(NavigationStartedEventName);
    private readonly ObservableEventInvocable<NavigationEventArgs> invocableFragmentNavigatedObservableEvent = new(FragmentNavigatedEventName);
    private readonly ObservableEventInvocable<NavigationEventArgs> invocableDomContentLoadedObservableEvent = new(DomContentLoadedEventName);
    private readonly ObservableEventInvocable<NavigationEventArgs> invocableLoadObservableEvent = new(LoadEventName);
    private readonly ObservableEventInvocable<NavigationEventArgs> invocableNavigationAbortedObservableEvent = new(NavigationAbortedEventName);
    private readonly ObservableEventInvocable<NavigationEventArgs> invocableNavigationCommittedObservableEvent = new(NavigationCommittedEventName);
    private readonly ObservableEventInvocable<NavigationEventArgs> invocableNavigationFailedObservableEvent = new(NavigationFailedEventName);
    private readonly ObservableEventInvocable<DownloadWillBeginEventArgs> invocableDownloadWillBeginObservableEvent = new(DownloadWillBeginEventName);
    private readonly ObservableEventInvocable<DownloadEndEventArgs> invocableDownloadEndObservableEvent = new(DownloadEndEventName);
    private readonly ObservableEventInvocable<HistoryUpdatedEventArgs> invocableHistoryUpdatedObservableEvent = new(HistoryUpdatedEventName);
    private readonly ObservableEventInvocable<UserPromptClosedEventArgs> invocableUserPromptClosedObservableEvent = new(UserPromptClosedEventName);
    private readonly ObservableEventInvocable<UserPromptOpenedEventArgs> invocableUserPromptOpenedObservableEvent = new(UserPromptOpenedEventName);

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public BrowsingContextModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
        this.RegisterObservableEvent<BrowsingContextInfo, BrowsingContextEventArgs>(this.invocableContextCreatedObservableEvent, info => new BrowsingContextEventArgs(info));
        this.RegisterObservableEvent<BrowsingContextInfo, BrowsingContextEventArgs>(this.invocableContextDestroyedObservableEvent, info => new BrowsingContextEventArgs(info));
        this.RegisterObservableEvent(this.invocableNavigationStartedObservableEvent);
        this.RegisterObservableEvent(this.invocableFragmentNavigatedObservableEvent);
        this.RegisterObservableEvent(this.invocableDomContentLoadedObservableEvent);
        this.RegisterObservableEvent(this.invocableLoadObservableEvent);
        this.RegisterObservableEvent(this.invocableDownloadWillBeginObservableEvent);
        this.RegisterObservableEvent(this.invocableDownloadEndObservableEvent);
        this.RegisterObservableEvent(this.invocableNavigationAbortedObservableEvent);
        this.RegisterObservableEvent(this.invocableNavigationCommittedObservableEvent);
        this.RegisterObservableEvent(this.invocableNavigationFailedObservableEvent);
        this.RegisterObservableEvent(this.invocableHistoryUpdatedObservableEvent);
        this.RegisterObservableEvent(this.invocableUserPromptClosedObservableEvent);
        this.RegisterObservableEvent(this.invocableUserPromptOpenedObservableEvent);
    }

    /// <summary>
    /// Gets an observable event that notifies when a browsing context is created.
    /// </summary>
    [ObservableEventName(ContextCreatedEventName)]
    public ObservableEvent<BrowsingContextEventArgs> OnContextCreated => this.invocableContextCreatedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context is destroyed.
    /// </summary>
    [ObservableEventName(ContextDestroyedEventName)]
    public ObservableEvent<BrowsingContextEventArgs> OnContextDestroyed => this.invocableContextDestroyedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is started.
    /// </summary>
    [ObservableEventName(NavigationStartedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationStarted => this.invocableNavigationStartedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context fragment is navigated.
    /// </summary>
    [ObservableEventName(FragmentNavigatedEventName)]
    public ObservableEvent<NavigationEventArgs> OnFragmentNavigated => this.invocableFragmentNavigatedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when the DOM content in a browsing context is loaded.
    /// </summary>
    [ObservableEventName(DomContentLoadedEventName)]
    public ObservableEvent<NavigationEventArgs> OnDomContentLoaded => this.invocableDomContentLoadedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a download in a browsing context is about to begin.
    /// </summary>
    [ObservableEventName(DownloadWillBeginEventName)]
    public ObservableEvent<DownloadWillBeginEventArgs> OnDownloadWillBegin => this.invocableDownloadWillBeginObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a download has ended.
    /// </summary>
    [ObservableEventName(DownloadEndEventName)]
    public ObservableEvent<DownloadEndEventArgs> OnDownloadEnd => this.invocableDownloadEndObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when the content in a browsing context is loaded.
    /// </summary>
    [ObservableEventName(LoadEventName)]
    public ObservableEvent<NavigationEventArgs> OnLoad => this.invocableLoadObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is aborted.
    /// </summary>
    [ObservableEventName(NavigationAbortedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationAborted => this.invocableNavigationAbortedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is committed.
    /// </summary>
    [ObservableEventName(NavigationCommittedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationCommitted => this.invocableNavigationCommittedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation fails.
    /// </summary>
    [ObservableEventName(NavigationFailedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationFailed => this.invocableNavigationFailedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when the browser history is updated.
    /// </summary>
    [ObservableEventName(HistoryUpdatedEventName)]
    public ObservableEvent<HistoryUpdatedEventArgs> OnHistoryUpdated => this.invocableHistoryUpdatedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a user prompt is opened.
    /// </summary>
    [ObservableEventName(UserPromptOpenedEventName)]
    public ObservableEvent<UserPromptOpenedEventArgs> OnUserPromptOpened => this.invocableUserPromptOpenedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a user prompt is closed.
    /// </summary>
    [ObservableEventName(UserPromptClosedEventName)]
    public ObservableEvent<UserPromptClosedEventArgs> OnUserPromptClosed => this.invocableUserPromptClosedObservableEvent;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => BrowsingContextModuleName;

    /// <summary>
    /// Activates a browsing context by bringing it to the foreground.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<ActivateCommandResult> ActivateAsync(ActivateCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Captures a screenshot of the current page in the browsing context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public Task<CaptureScreenshotCommandResult> CaptureScreenshotAsync(CaptureScreenshotCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Closes the browsing context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<CloseCommandResult> CloseAsync(CloseCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Creates a new browsing context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command including the ID of the new context.</returns>
    public Task<CreateCommandResult> CreateAsync(CreateCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Gets a tree of browsing contexts associated with a specified context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The tree associated browsing contexts.</returns>
    public Task<GetTreeCommandResult> GetTreeAsync(GetTreeCommandParameters? commandParameters = null, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters ?? new(), timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Handles a user prompt.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>An empty command result.</returns>
    public Task<HandleUserPromptCommandResult> HandleUserPromptAsync(HandleUserPromptCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Locates nodes within a browsing context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command containing the located nodes, if any.</returns>
    public Task<LocateNodesCommandResult> LocateNodesAsync(LocateNodesCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Navigates a browsing context to a new URL.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<NavigateCommandResult> NavigateAsync(NavigateCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Prints a PDF of the current page in the browsing context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command containing a base64-encoded PDF of the current page.</returns>
    public Task<PrintCommandResult> PrintAsync(PrintCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Reloads a browsing context to a new URL.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<ReloadCommandResult> ReloadAsync(ReloadCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets whether to bypass content security policies (CSP) for a browsing context.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<SetBypassCSPCommandResult> SetBypassCSPAsync(SetBypassCSPCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Sets the viewport of a browsing context to the specified dimensions.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<SetViewportCommandResult> SetViewportAsync(SetViewportCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }

    /// <summary>
    /// Traverses the history entries of the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <param name="timeoutOverride">The timeout override to use for the command. If omitted, the value of <see cref="BiDiDriver.DefaultCommandTimeout"/> is used.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled. Omitting this argument is the equivalent of using <see cref="CancellationToken.None"/>.</param>
    /// <returns>The result of the command.</returns>
    public Task<TraverseHistoryCommandResult> TraverseHistoryAsync(TraverseHistoryCommandParameters commandParameters, TimeSpan? timeoutOverride = null, CancellationToken cancellationToken = default)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters, timeoutOverride, cancellationToken);
    }
}
