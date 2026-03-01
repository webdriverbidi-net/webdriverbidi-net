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
    /// The name of the network module.
    /// </summary>
    public const string NetworkModuleName = "network";

    private const string AuthRequiredEventName = $"{NetworkModuleName}.authRequired";
    private const string BeforeRequestSentEventName = $"{NetworkModuleName}.beforeRequestSent";
    private const string FetchErrorEventName = $"{NetworkModuleName}.fetchError";
    private const string ResponseStartedEventName = $"{NetworkModuleName}.responseStarted";
    private const string ResponseCompletedEventName = $"{NetworkModuleName}.responseCompleted";

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkModule"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> used in the module commands and events.</param>
    public NetworkModule(BiDiDriver driver)
        : base(driver)
    {
        this.RegisterObservableEvent(this.OnAuthRequired);
        this.RegisterObservableEvent(this.OnBeforeRequestSent);
        this.RegisterObservableEvent(this.OnFetchError);
        this.RegisterObservableEvent(this.OnResponseStarted);
        this.RegisterObservableEvent(this.OnResponseCompleted);
    }

    /// <summary>
    /// Gets an observable event that notifies when an authorization required response is received.
    /// </summary>
    public ObservableEvent<AuthRequiredEventArgs> OnAuthRequired { get; } = new(AuthRequiredEventName);

    /// <summary>
    /// Gets an observable event that notifies before a network request is sent.
    /// </summary>
    public ObservableEvent<BeforeRequestSentEventArgs> OnBeforeRequestSent { get; } = new(BeforeRequestSentEventName);

    /// <summary>
    /// Gets an observable event that notifies when an error is encountered fetching data.
    /// </summary>
    public ObservableEvent<FetchErrorEventArgs> OnFetchError { get; } = new(FetchErrorEventName);

    /// <summary>
    /// Gets an observable event that notifies when network response has started.
    /// </summary>
    public ObservableEvent<ResponseStartedEventArgs> OnResponseStarted { get; } = new(ResponseStartedEventName);

    /// <summary>
    /// Gets an observable event that notifies when network response has completed.
    /// </summary>
    public ObservableEvent<ResponseCompletedEventArgs> OnResponseCompleted { get; } = new(ResponseCompletedEventName);

    /// <summary>
    /// Gets the module name.
    /// </summary>
    public override string ModuleName => NetworkModuleName;

    /// <summary>
    /// Adds an intercept for network traffic matching specific phases of the traffic and URL patterns.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing a network interception ID.</returns>
    public Task<AddInterceptCommandResult> AddInterceptAsync(AddInterceptCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Adds collector for network data like response bodies for specific phases of the traffic.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing a network interception ID.</returns>
    public Task<AddDataCollectorCommandResult> AddDataCollectorAsync(AddDataCollectorCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Continues a paused request intercepted by the driver.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<ContinueRequestCommandResult> ContinueRequestAsync(ContinueRequestCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Continues a paused response intercepted by the driver after the response has been received from the server,
    /// but before presented to the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<ContinueResponseCommandResult> ContinueResponseAsync(ContinueResponseCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Continues a paused request intercepted by the driver with authentication information.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<ContinueWithAuthCommandResult> ContinueWithAuthAsync(ContinueWithAuthCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Releases data from a network data collector for a specific request.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<DisownDataCommandResult> DisownDataAsync(DisownDataCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Fails a paused request intercepted by the driver.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<FailRequestCommandResult> FailRequestAsync(FailRequestCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Gets the collected data from a network data collector for a specific network request.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command containing a network interception ID.</returns>
    public Task<GetDataCommandResult> GetDataAsync(GetDataCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync<GetDataCommandResult>(commandParameters);
    }

    /// <summary>
    /// Provides a full response for request intercepted by the driver without sending the request to the server.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<ProvideResponseCommandResult> ProvideResponseAsync(ProvideResponseCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Removes a network data collector.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<RemoveDataCollectorCommandResult> RemoveDataCollectorAsync(RemoveDataCollectorCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Removes an added intercept for network traffic matching specific phases of the traffic and URL patterns.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<RemoveInterceptCommandResult> RemoveInterceptAsync(RemoveInterceptCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Sets the cache behavior of the browser.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<SetCacheBehaviorCommandResult> SetCacheBehaviorAsync(SetCacheBehaviorCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }

    /// <summary>
    /// Sets extra headers to be set for each request sent by the browser. This will overwrite existing header values for those requests.
    /// </summary>
    /// <param name="commandParameters">The parameters for the command.</param>
    /// <returns>The result of the command.</returns>
    public Task<SetExtraHeadersCommandResult> SetExtraHeadersAsync(SetExtraHeadersCommandParameters commandParameters)
    {
        return this.Driver.ExecuteCommandAsync(commandParameters);
    }
}
