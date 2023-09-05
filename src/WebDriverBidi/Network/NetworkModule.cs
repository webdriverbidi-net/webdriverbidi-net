// <copyright file="NetworkModule.cs" company="WebDriverBidi.NET Committers">
// Copyright (c) WebDriverBidi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBidi.Network;

/// <summary>
/// The Network module contains commands and events relating to network traffic.
/// </summary>
public sealed class NetworkModule : Module
{
    /// <summary>
    /// The name of the log module.
    /// </summary>
    public const string NetworkModuleName = "network";

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="Driver"/> used in the module commands and events.</param>
    public NetworkModule(Driver driver)
        : base(driver)
    {
        this.RegisterEventInvoker<AuthRequiredEventArgs>("network.authRequired", this.OnAuthRequired);
        this.RegisterEventInvoker<BeforeRequestSentEventArgs>("network.beforeRequestSent", this.OnBeforeRequestSent);
        this.RegisterEventInvoker<FetchErrorEventArgs>("network.fetchError", this.OnFetchError);
        this.RegisterEventInvoker<ResponseStartedEventArgs>("network.responseStarted", this.OnResponseStarted);
        this.RegisterEventInvoker<ResponseCompletedEventArgs>("network.responseCompleted", this.OnResponseCompleted);
    }

    /// <summary>
    /// Occurs when an authorization required response is received.
    /// </summary>
    public event EventHandler<AuthRequiredEventArgs>? AuthRequired;

    /// <summary>
    /// Occurs before a network request is sent.
    /// </summary>
    public event EventHandler<BeforeRequestSentEventArgs>? BeforeRequestSent;

    /// <summary>
    /// Occurs when an error is encountered fetching data.
    /// </summary>
    public event EventHandler<FetchErrorEventArgs>? FetchError;

    /// <summary>
    /// Occurs when network response has started.
    /// </summary>
    public event EventHandler<ResponseStartedEventArgs>? ResponseStarted;

    /// <summary>
    /// Occurs when network response has completed.
    /// </summary>
    public event EventHandler<ResponseCompletedEventArgs>? ResponseCompleted;

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
        return await this.Driver.ExecuteCommandAsync<AddInterceptCommandResult>(commandProperties);
    }

    /// <summary>
    /// Continues a paused request intercepted by the driver.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ContinueRequestAsync(ContinueRequestCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Continues a paused response intercepted by the driver after the response has been received from the server,
    /// but before presented to the browser.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ContinueResponseAsync(ContinueResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Continues a paused request intercepted by the driver with authentication information.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ContinueWithAuthAsync(ContinueWithAuthCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Fails a paused request intercepted by the driver.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> FailRequestAsync(FailRequestCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Provides a full response for request intercepted by the driver without sending the request to the server.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> ProvideResponseAsync(ProvideResponseCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    /// <summary>
    /// Removes an added intercept for network traffic matching specific phases of the traffic and URL patterns.
    /// </summary>
    /// <param name="commandProperties">The parameters for the command.</param>
    /// <returns>The result of the command containing a base64-encoded screenshot.</returns>
    public async Task<EmptyResult> RemoveInterceptAsync(RemoveInterceptCommandParameters commandProperties)
    {
        return await this.Driver.ExecuteCommandAsync<EmptyResult>(commandProperties);
    }

    private void OnAuthRequired(EventInfo<AuthRequiredEventArgs> eventData)
    {
        if (this.AuthRequired is not null)
        {
            AuthRequiredEventArgs eventArgs = eventData.ToEventArgs<AuthRequiredEventArgs>();
            this.AuthRequired(this, eventArgs);
        }
    }

    private void OnBeforeRequestSent(EventInfo<BeforeRequestSentEventArgs> eventData)
    {
        if (this.BeforeRequestSent is not null)
        {
            BeforeRequestSentEventArgs eventArgs = eventData.ToEventArgs<BeforeRequestSentEventArgs>();
            this.BeforeRequestSent(this, eventArgs);
        }
    }

    private void OnFetchError(EventInfo<FetchErrorEventArgs> eventData)
    {
        if (this.FetchError is not null)
        {
            FetchErrorEventArgs eventArgs = eventData.ToEventArgs<FetchErrorEventArgs>();
            this.FetchError(this, eventArgs);
        }
    }

    private void OnResponseStarted(EventInfo<ResponseStartedEventArgs> eventData)
    {
        if (this.ResponseStarted is not null)
        {
            ResponseStartedEventArgs eventArgs = eventData.ToEventArgs<ResponseStartedEventArgs>();
            this.ResponseStarted(this, eventArgs);
        }
    }

    private void OnResponseCompleted(EventInfo<ResponseCompletedEventArgs> eventData)
    {
        if (this.ResponseCompleted is not null)
        {
            ResponseCompletedEventArgs eventArgs = eventData.ToEventArgs<ResponseCompletedEventArgs>();
            this.ResponseCompleted(this, eventArgs);
        }
    }
}
