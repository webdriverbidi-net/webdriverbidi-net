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

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="IBiDiCommandExecutor"/> used in the module commands and events.</param>
    public BrowsingContextModule(IBiDiCommandExecutor driver)
        : base(driver)
    {
        this.RegisterObservableEvent<BrowsingContextInfo, BrowsingContextEventArgs>(this.OnContextCreated, info => new BrowsingContextEventArgs(info));
        this.RegisterObservableEvent<BrowsingContextInfo, BrowsingContextEventArgs>(this.OnContextDestroyed, info => new BrowsingContextEventArgs(info));
        this.RegisterObservableEvent(this.OnNavigationStarted);
        this.RegisterObservableEvent(this.OnFragmentNavigated);
        this.RegisterObservableEvent(this.OnDomContentLoaded);
        this.RegisterObservableEvent(this.OnLoad);
        this.RegisterObservableEvent(this.OnDownloadWillBegin);
        this.RegisterObservableEvent(this.OnDownloadEnd);
        this.RegisterObservableEvent(this.OnNavigationAborted);
        this.RegisterObservableEvent(this.OnNavigationCommitted);
        this.RegisterObservableEvent(this.OnNavigationFailed);
        this.RegisterObservableEvent(this.OnHistoryUpdated);
        this.RegisterObservableEvent(this.OnUserPromptClosed);
        this.RegisterObservableEvent(this.OnUserPromptOpened);
    }

    /// <summary>
    /// Gets an observable event that notifies when a browsing context is created.
    /// </summary>
    [ObservableEventName(ContextCreatedEventName)]
    public ObservableEvent<BrowsingContextEventArgs> OnContextCreated { get; } = new(ContextCreatedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a browsing context is destroyed.
    /// </summary>
    [ObservableEventName(ContextDestroyedEventName)]
    public ObservableEvent<BrowsingContextEventArgs> OnContextDestroyed { get; } = new(ContextDestroyedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is started.
    /// </summary>
    [ObservableEventName(NavigationStartedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationStarted { get; } = new(NavigationStartedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a browsing context fragment is navigated.
    /// </summary>
    [ObservableEventName(FragmentNavigatedEventName)]
    public ObservableEvent<NavigationEventArgs> OnFragmentNavigated { get; } = new(FragmentNavigatedEventName);

    /// <summary>
    /// Gets an observable event that notifies when the DOM content in a browsing context is loaded.
    /// </summary>
    [ObservableEventName(DomContentLoadedEventName)]
    public ObservableEvent<NavigationEventArgs> OnDomContentLoaded { get; } = new(DomContentLoadedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a download in a browsing context is about to begin.
    /// </summary>
    [ObservableEventName(DownloadWillBeginEventName)]
    public ObservableEvent<DownloadWillBeginEventArgs> OnDownloadWillBegin { get; } = new(DownloadWillBeginEventName);

    /// <summary>
    /// Gets an observable event that notifies when a download has ended.
    /// </summary>
    [ObservableEventName(DownloadEndEventName)]
    public ObservableEvent<DownloadEndEventArgs> OnDownloadEnd { get; } = new(DownloadEndEventName);

    /// <summary>
    /// Gets an observable event that notifies when the content in a browsing context is loaded.
    /// </summary>
    [ObservableEventName(LoadEventName)]
    public ObservableEvent<NavigationEventArgs> OnLoad { get; } = new(LoadEventName);

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is aborted.
    /// </summary>
    [ObservableEventName(NavigationAbortedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationAborted { get; } = new(NavigationAbortedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation is committed.
    /// </summary>
    [ObservableEventName(NavigationCommittedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationCommitted { get; } = new(NavigationCommittedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a browsing context navigation fails.
    /// </summary>
    [ObservableEventName(NavigationFailedEventName)]
    public ObservableEvent<NavigationEventArgs> OnNavigationFailed { get; } = new(NavigationFailedEventName);

    /// <summary>
    /// Gets an observable event that notifies when the browser history is updated.
    /// </summary>
    [ObservableEventName(HistoryUpdatedEventName)]
    public ObservableEvent<HistoryUpdatedEventArgs> OnHistoryUpdated { get; } = new(HistoryUpdatedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a user prompt is opened.
    /// </summary>
    [ObservableEventName(UserPromptOpenedEventName)]
    public ObservableEvent<UserPromptOpenedEventArgs> OnUserPromptOpened { get; } = new(UserPromptOpenedEventName);

    /// <summary>
    /// Gets an observable event that notifies when a user prompt is closed.
    /// </summary>
    [ObservableEventName(UserPromptClosedEventName)]
    public ObservableEvent<UserPromptClosedEventArgs> OnUserPromptClosed { get; } = new(UserPromptClosedEventName);

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
