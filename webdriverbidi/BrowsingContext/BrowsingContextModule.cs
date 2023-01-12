// <copyright file="BrowsingContextModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.BrowsingContext;

/// <summary>
/// The BrowsingContext module.
/// </summary>
public sealed class BrowsingContextModule : ProtocolModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BrowsingContextModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public BrowsingContextModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker("browsingContext.contextCreated", typeof(BrowsingContextInfo), this.OnContextCreated);
        this.RegisterEventInvoker("browsingContext.contextDestroyed", typeof(BrowsingContextInfo), this.OnContextDestroyed);
        this.RegisterEventInvoker("browsingContext.navigationStarted", typeof(NavigationEventArgs), this.OnNavigationStarted);
        this.RegisterEventInvoker("browsingContext.fragmentNavigated", typeof(NavigationEventArgs), this.OnFragmentNavigated);
        this.RegisterEventInvoker("browsingContext.domContentLoaded", typeof(NavigationEventArgs), this.OnDomContentLoaded);
        this.RegisterEventInvoker("browsingContext.load", typeof(NavigationEventArgs), this.OnLoad);
        this.RegisterEventInvoker("browsingContext.downloadWillBegin", typeof(NavigationEventArgs), this.OnDownloadWillBegin);
        this.RegisterEventInvoker("browsingContext.navigationAborted", typeof(NavigationEventArgs), this.OnNavigationAborted);
        this.RegisterEventInvoker("browsingContext.navigationFailed", typeof(NavigationEventArgs), this.OnNavigationFailed);
        this.RegisterEventInvoker("browsingContext.userPromptClosed", typeof(UserPromptClosedEventArgs), this.OnUserPromptClosed);
        this.RegisterEventInvoker("browsingContext.userPromptOpened", typeof(UserPromptOpenedEventArgs), this.OnUserPromptOpened);
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
    /// Captures a screenshot of the current page in the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<CaptureScreenshotCommandResult> CaptureScreenshot(CaptureScreenshotCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<CaptureScreenshotCommandResult>(commandProperties);
    }

    /// <summary>
    /// Closes the browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> Close(CloseCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Creates a new browsing context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command including the ID of the new context.</returns>
    public async Task<CreateCommandResult> Create(CreateCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<CreateCommandResult>(commandProperties);
    }

    /// <summary>
    /// Gets a tree of browsing contexts associated with a specified context.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The tree associated browsing contexts.</returns>
    public async Task<GetTreeCommandResult> GetTree(GetTreeCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<GetTreeCommandResult>(commandProperties);
    }

    /// <summary>
    /// Handles a user prompt.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>An empty command result.</returns>
    public async Task<EmptyResult> HandleUserPrompt(HandleUserPromptCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Navigates a browsing context to a new URL.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<BrowsingContextNavigateResult> Navigate(NavigateCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<BrowsingContextNavigateResult>(commandProperties);
    }

    /// <summary>
    /// Reloads a browsing context to a new URL.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public async Task<BrowsingContextNavigateResult> Reload(ReloadCommandSettings commandProperties)
    {
        return await this.Driver.ExecuteCommand<BrowsingContextNavigateResult>(commandProperties);
    }

    private void OnContextCreated(object eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        if (eventData is not BrowsingContextInfo info)
        {
            throw new WebDriverBidiException("Unable to cast event data to BrowsingContextEventArgs");
        }

        if (this.ContextCreated is not null)
        {
            this.ContextCreated(this, new BrowsingContextEventArgs(info));
        }
    }

    private void OnContextDestroyed(object eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        if (eventData is not BrowsingContextInfo info)
        {
            throw new WebDriverBidiException("Unable to cast event data to BrowsingContextEventArgs");
        }

        if (this.ContextDestroyed is not null)
        {
            this.ContextDestroyed(this, new BrowsingContextEventArgs(info));
        }
    }

    private void OnNavigationStarted(object eventData)
    {
        if (eventData is not NavigationEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to NavigationEventArgs");
        }

        if (this.NavigationStarted is not null)
        {
            this.NavigationStarted(this, eventArgs);
        }
    }

    private void OnFragmentNavigated(object eventData)
    {
        if (eventData is not NavigationEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to NavigationEventArgs");
        }

        if (this.FragmentNavigated is not null)
        {
            this.FragmentNavigated(this, eventArgs);
        }
    }

    private void OnDomContentLoaded(object eventData)
    {
        if (eventData is not NavigationEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to NavigationEventArgs");
        }

        if (this.DomContentLoaded is not null)
        {
            this.DomContentLoaded(this, eventArgs);
        }
    }

    private void OnLoad(object eventData)
    {
        if (eventData is not NavigationEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to NavigationEventArgs");
        }

        if (this.Load is not null)
        {
            this.Load(this, eventArgs);
        }
    }

    private void OnDownloadWillBegin(object eventData)
    {
        if (eventData is not NavigationEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to NavigationEventArgs");
        }

        if (this.DownloadWillBegin is not null)
        {
            this.DownloadWillBegin(this, eventArgs);
        }
    }

    private void OnNavigationAborted(object eventData)
    {
        if (eventData is not NavigationEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to NavigationEventArgs");
        }

        if (this.NavigationAborted is not null)
        {
            this.NavigationAborted(this, eventArgs);
        }
    }

    private void OnNavigationFailed(object eventData)
    {
        if (eventData is not NavigationEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to NavigationEventArgs");
        }

        if (this.NavigationFailed is not null)
        {
            this.NavigationFailed(this, eventArgs);
        }
    }

    private void OnUserPromptClosed(object eventData)
    {
        if (eventData is not UserPromptClosedEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to UserPromptClosedEventArgs");
        }

        if (this.UserPromptClosed is not null)
        {
            this.UserPromptClosed(this, eventArgs);
        }
    }

    private void OnUserPromptOpened(object eventData)
    {
        if (eventData is not UserPromptOpenedEventArgs eventArgs)
        {
            throw new WebDriverBidiException("Unable to cast event data to UserPromptOpenedEventArgs");
        }

        if (this.UserPromptOpened is not null)
        {
            this.UserPromptOpened(this, eventArgs);
        }
    }
}