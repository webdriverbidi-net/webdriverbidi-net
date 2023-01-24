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

    private void OnContextCreated(EventInvocationData<BrowsingContextInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        if (this.ContextCreated is not null)
        {
            BrowsingContextEventArgs eventArgs = new(eventData.EventData)
            {
                AdditionalData = eventData.AdditionalData,
            };
            this.ContextCreated(this, eventArgs);
        }
    }

    private void OnContextDestroyed(EventInvocationData<BrowsingContextInfo> eventData)
    {
        // Special case here. The specification indicates that the parameters
        // for this event are a BrowingContextInfo object, so rather than
        // duplicate the properties to directly deserialize the
        // BrowsingContextEventArgs instance, the protocol transport will
        // deserialize to a BrowingContextInfo, then use that here to create
        // the appropriate EventArgs instance.
        // Note that the base class for a protocol module should not allow
        // eventData to be any other type than the expected type.
        if (this.ContextDestroyed is not null)
        {
            BrowsingContextEventArgs eventArgs = new(eventData.EventData)
            {
                AdditionalData = eventData.AdditionalData,
            };
            this.ContextDestroyed(this, eventArgs);
        }
    }

    private void OnNavigationStarted(EventInvocationData<NavigationEventArgs> eventData)
    {
        if (this.NavigationStarted is not null)
        {
            NavigationEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.NavigationStarted(this, eventArgs);
        }
    }

    private void OnFragmentNavigated(EventInvocationData<NavigationEventArgs> eventData)
    {
        if (this.FragmentNavigated is not null)
        {
            NavigationEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.FragmentNavigated(this, eventArgs);
        }
     }

    private void OnDomContentLoaded(EventInvocationData<NavigationEventArgs> eventData)
    {
        if (this.DomContentLoaded is not null)
        {
            NavigationEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.DomContentLoaded(this, eventArgs);
        }
    }

    private void OnLoad(EventInvocationData<NavigationEventArgs> eventData)
    {
        if (this.Load is not null)
        {
            NavigationEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.Load(this, eventArgs);
        }
    }

    private void OnDownloadWillBegin(EventInvocationData<NavigationEventArgs> eventData)
    {
        if (this.DownloadWillBegin is not null)
        {
            NavigationEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.DownloadWillBegin(this, eventArgs);
        }
    }

    private void OnNavigationAborted(EventInvocationData<NavigationEventArgs> eventData)
    {
        if (this.NavigationAborted is not null)
        {
            NavigationEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.NavigationAborted(this, eventArgs);
        }
    }

    private void OnNavigationFailed(EventInvocationData<NavigationEventArgs> eventData)
    {
        if (this.NavigationFailed is not null)
        {
            NavigationEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.NavigationFailed(this, eventArgs);
        }
    }

    private void OnUserPromptClosed(EventInvocationData<UserPromptClosedEventArgs> eventData)
    {
        if (this.UserPromptClosed is not null)
        {
            UserPromptClosedEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.UserPromptClosed(this, eventArgs);
        }
    }

    private void OnUserPromptOpened(EventInvocationData<UserPromptOpenedEventArgs> eventData)
    {
        if (this.UserPromptOpened is not null)
        {
            UserPromptOpenedEventArgs eventArgs = eventData.EventData;
            eventArgs.AdditionalData = eventData.AdditionalData;
            this.UserPromptOpened(this, eventArgs);
        }
    }
}