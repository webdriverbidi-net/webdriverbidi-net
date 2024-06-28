// <copyright file="NetworkModule.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

/// <summary>
/// The Network module contains commands and events relating to network traffic.
/// </summary>
public sealed class NetworkModule : Module
{
    /// <summary>
    /// The name of the log module.
    /// </summary>
    public const string NetworkModuleName = "network";

    private ObservableEvent<AuthRequiredEventArgs> onAuthRequiredEvent = new();
    private ObservableEvent<BeforeRequestSentEventArgs> onBeforeRequestSentEvent = new();
    private ObservableEvent<FetchErrorEventArgs> onFetchErrorEvent = new();
    private ObservableEvent<ResponseStartedEventArgs> onResponseStartedEvent = new();
    private ObservableEvent<ResponseCompletedEventArgs> onResponseCompletedEvent = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public NetworkModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterAsyncEventInvoker<AuthRequiredEventArgs>("network.authRequired", this.OnAuthRequiredAsync);
        this.RegisterAsyncEventInvoker<BeforeRequestSentEventArgs>("network.beforeRequestSent", this.OnBeforeRequestSentAsync);
        this.RegisterAsyncEventInvoker<FetchErrorEventArgs>("network.fetchError", this.OnFetchErrorAsync);
        this.RegisterAsyncEventInvoker<ResponseStartedEventArgs>("network.responseStarted", this.OnResponseStartedAsync);
        this.RegisterAsyncEventInvoker<ResponseCompletedEventArgs>("network.responseCompleted", this.OnResponseCompletedAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when an authorization required response is received.
    /// </summary>
    public ObservableEvent<AuthRequiredEventArgs> OnAuthRequired => this.onAuthRequiredEvent;

    /// <summary>
    /// Gets an observable event that notifies before a network request is sent.
    /// </summary>
    public ObservableEvent<BeforeRequestSentEventArgs> OnBeforeRequestSent => this.onBeforeRequestSentEvent;

    /// <summary>
    /// Gets an observable event that notifies when an error is encountered fetching data.
    /// </summary>
    public ObservableEvent<FetchErrorEventArgs> OnFetchError => this.onFetchErrorEvent;

    /// <summary>
    /// Gets an observable event that notifies when network response has started.
    /// </summary>
    public ObservableEvent<ResponseStartedEventArgs> OnResponseStarted => this.onResponseStartedEvent;

    /// <summary>
    /// Gets an observable event that notifies when network response has completed.
    /// </summary>
    public ObservableEvent<ResponseCompletedEventArgs> OnResponseCompleted => this.onResponseCompletedEvent;

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => NetworkModuleName;

    /// <summary>
    /// Adds an intercept for network traffic matching specific phases of the traffic and URL patterns.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<AddInterceptCommandResult> AddInterceptAsync(AddInterceptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<AddInterceptCommandResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Continues a paused request intercepted by the driver.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ContinueRequestAsync(ContinueRequestCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Continues a paused response intercepted by the driver after the response has been received from the server,
    /// but before presented to the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ContinueResponseAsync(ContinueResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Continues a paused request intercepted by the driver with authentication information.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ContinueWithAuthAsync(ContinueWithAuthCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Fails a paused request intercepted by the driver.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> FailRequestAsync(FailRequestCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Provides a full response for request intercepted by the driver without sending the request to the server.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ProvideResponseAsync(ProvideResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    /// <summary>
    /// Removes an added intercept for network traffic matching specific phases of the traffic and URL patterns.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> RemoveInterceptAsync(RemoveInterceptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties).ConfigureAwait(false);
    }

    private async Task OnAuthRequiredAsync(EventInfo<AuthRequiredEventArgs> eventData)
    {
        AuthRequiredEventArgs eventArgs = eventData.ToEventArgs<AuthRequiredEventArgs>();
        await this.onAuthRequiredEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnBeforeRequestSentAsync(EventInfo<BeforeRequestSentEventArgs> eventData)
    {
        BeforeRequestSentEventArgs eventArgs = eventData.ToEventArgs<BeforeRequestSentEventArgs>();
        await this.onBeforeRequestSentEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnFetchErrorAsync(EventInfo<FetchErrorEventArgs> eventData)
    {
        FetchErrorEventArgs eventArgs = eventData.ToEventArgs<FetchErrorEventArgs>();
        await this.onFetchErrorEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnResponseStartedAsync(EventInfo<ResponseStartedEventArgs> eventData)
    {
        ResponseStartedEventArgs eventArgs = eventData.ToEventArgs<ResponseStartedEventArgs>();
        await this.onResponseStartedEvent.NotifyObserversAsync(eventArgs);
    }

    private async Task OnResponseCompletedAsync(EventInfo<ResponseCompletedEventArgs> eventData)
    {
        ResponseCompletedEventArgs eventArgs = eventData.ToEventArgs<ResponseCompletedEventArgs>();
        await this.onResponseCompletedEvent.NotifyObserversAsync(eventArgs);
    }
}
