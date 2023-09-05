// <copyright file="BrowsingContextModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

/// <summary>
/// The BrowsingContext module contains commands and events relating to browsing contexts.
/// </summary>
public sealed class BrowsingContextModule : Module
{
    /// <summary>
    /// The name of the browsingContext module.
    /// </summary>
    public const string BrowsingContextModuleName = "browsingContext";

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public BrowsingContextModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker<BrowsingContextInfo>("browsingContext.contextCreated", this.OnContextCreated);
        this.RegisterEventInvoker<BrowsingContextInfo>("browsingContext.contextDestroyed", this.OnContextDestroyed);
        this.RegisterEventInvoker<NavigationEventArgs>("browsingContext.navigationStarted", this.OnNavigationStarted);
        this.RegisterEventInvoker<NavigationEventArgs>("browsingContext.fragmentNavigated", this.OnFragmentNavigated);
        this.RegisterEventInvoker<NavigationEventArgs>("browsingContext.domContentLoaded", this.OnDomContentLoaded);
        this.RegisterEventInvoker<NavigationEventArgs>("browsingContext.load", this.OnLoad);
        this.RegisterEventInvoker<NavigationEventArgs>("browsingContext.downloadWillBegin", this.OnDownloadWillBegin);
        this.RegisterEventInvoker<NavigationEventArgs>("browsingContext.navigationAborted", this.OnNavigationAborted);
        this.RegisterEventInvoker<NavigationEventArgs>("browsingContext.navigationFailed", this.OnNavigationFailed);
        this.RegisterEventInvoker<UserPromptClosedEventArgs>("browsingContext.userPromptClosed", this.OnUserPromptClosed);
        this.RegisterEventInvoker<UserPromptOpenedEventArgs>("browsingContext.userPromptOpened", this.OnUserPromptOpened);
    }

    /// <summary>
    /// Occurs when a browsing context is created.
    /// </summary>
    public event EventHandler<BrowsingContextEventArgs>? ContextCreated;

    /// <summary>
    /// Occurs when a browsing context is destroyed.
    /// </summary>
    public event EventHandler<BrowsingContextEventArgs>? ContextDestroyed;

    /// <summary>
    /// Occurs when a browsing context navigation is started.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? NavigationStarted;

    /// <summary>
    /// Occurs when a browsing context fragment is navigated.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? FragmentNavigated;

    /// <summary>
    /// Occurs when the DOM content in a browsing context is loaded.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? DomContentLoaded;

    /// <summary>
    /// Occurs when a download in a browsing context is about to begin.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? DownloadWillBegin;

    /// <summary>
    /// Occurs when the content in a browsing context is loaded.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? Load;

    /// <summary>
    /// Occurs when a browsing context navigation is aborted.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? NavigationAborted;

    /// <summary>
    /// Occurs when a browsing context navigation fails.
    /// </summary>
    public event EventHandler<NavigationEventArgs>? NavigationFailed;

    /// <summary>
    /// Occurs when a user prompt is opened.
    /// </summary>
    public event EventHandler<UserPromptOpenedEventArgs>? UserPromptOpened;

    /// <summary>
    /// Occurs when a user prompt is closed.
    /// </summary>
    public event EventHandler<UserPromptClosedEventArgs>? UserPromptClosed;

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
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Captures a screenshot of the current page in the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<CaptureScreenshotCommandResult> CaptureScreenshotAsync(CaptureScreenshotCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<CaptureScreenshotCommandResult>(commandProperties);
    }

    /// <summary>
    /// Closes the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> CloseAsync(CloseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Creates a new browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command including the ID of the new context.</returns>
    public async Task<CreateCommandResult> CreateAsync(CreateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<CreateCommandResult>(commandProperties);
    }

    /// <summary>
    /// Gets a tree of browsing contexts associated with a specified context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The tree associated browsing contexts.</returns>
    public async Task<GetTreeCommandResult> GetTreeAsync(GetTreeCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<GetTreeCommandResult>(commandProperties);
    }

    /// <summary>
    /// Handles a user prompt.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> HandleUserPromptAsync(HandleUserPromptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Navigates a browsing context to a new URL.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<NavigationResult> NavigateAsync(NavigateCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<NavigationResult>(commandProperties);
    }

    /// <summary>
    /// Prints a PDF of the current page in the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>>The result of the command containing a base64-encoded PDF of the current page.</returns>
    public async Task<PrintCommandResult> PrintAsync(PrintCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<PrintCommandResult>(commandProperties);
    }

    /// <summary>
    /// Reloads a browsing context to a new URL.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<NavigationResult> ReloadAsync(ReloadCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<NavigationResult>(commandProperties);
    }

    /// <summary>
    /// Sets the viewport of a browsing context to the specified dimensions.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<EmptyResult> SetViewportAsync(SetViewportCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    private void OnContextCreated(EventInfo<BrowsingContextInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowsingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowsingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        if (this.ContextCreated is not null)
        {
            BrowsingContextEventArgs eventArgs = eventData.ToEventArgs<BrowsingContextEventArgs>();
            this.ContextCreated(this, eventArgs);
        }
    }

    private void OnContextDestroyed(EventInfo<BrowsingContextInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowsingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowsingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        if (this.ContextDestroyed is not null)
        {
            BrowsingContextEventArgs eventArgs = eventData.ToEventArgs<BrowsingContextEventArgs>();
            this.ContextDestroyed(this, eventArgs);
        }
    }

    private void OnNavigationStarted(EventInfo<NavigationEventArgs> eventData)
    {
        if (this.NavigationStarted is not null)
        {
            NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
            this.NavigationStarted(this, eventArgs);
        }
    }

    private void OnFragmentNavigated(EventInfo<NavigationEventArgs> eventData)
    {
        if (this.FragmentNavigated is not null)
        {
            NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
            this.FragmentNavigated(this, eventArgs);
        }
     }

    private void OnDomContentLoaded(EventInfo<NavigationEventArgs> eventData)
    {
        if (this.DomContentLoaded is not null)
        {
            NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
            this.DomContentLoaded(this, eventArgs);
        }
    }

    private void OnLoad(EventInfo<NavigationEventArgs> eventData)
    {
        if (this.Load is not null)
        {
            NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
            this.Load(this, eventArgs);
        }
    }

    private void OnDownloadWillBegin(EventInfo<NavigationEventArgs> eventData)
    {
        if (this.DownloadWillBegin is not null)
        {
            NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
            this.DownloadWillBegin(this, eventArgs);
        }
    }

    private void OnNavigationAborted(EventInfo<NavigationEventArgs> eventData)
    {
        if (this.NavigationAborted is not null)
        {
            NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
            this.NavigationAborted(this, eventArgs);
        }
    }

    private void OnNavigationFailed(EventInfo<NavigationEventArgs> eventData)
    {
        if (this.NavigationFailed is not null)
        {
            NavigationEventArgs eventArgs = eventData.ToEventArgs<NavigationEventArgs>();
            this.NavigationFailed(this, eventArgs);
        }
    }

    private void OnUserPromptClosed(EventInfo<UserPromptClosedEventArgs> eventData)
    {
        if (this.UserPromptClosed is not null)
        {
            UserPromptClosedEventArgs eventArgs = eventData.ToEventArgs<UserPromptClosedEventArgs>();
            this.UserPromptClosed(this, eventArgs);
        }
    }

    private void OnUserPromptOpened(EventInfo<UserPromptOpenedEventArgs> eventData)
    {
        if (this.UserPromptOpened is not null)
        {
            UserPromptOpenedEventArgs eventArgs = eventData.ToEventArgs<UserPromptOpenedEventArgs>();
            this.UserPromptOpened(this, eventArgs);
        }
    }
}