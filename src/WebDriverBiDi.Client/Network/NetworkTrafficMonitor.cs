// <copyright file="NetworkTrafficMonitor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

/// <summary>
/// Monitors network traffic, with optional support for request interception, modification, authentication, and HAR capture.
/// This is a demonstration implementation and is not intended for production use.
/// </summary>
public class NetworkTrafficMonitor
{
    private readonly BiDiDriver driver;
    private readonly Dictionary<string, NetworkRequest> pendingRequests = [];
    private readonly List<NetworkRequestModification> requestModifications = [];
    private readonly List<AuthCredentials> authCredentials = [];
    private string eventSubscriptionId = string.Empty;
    private string bodyCollectorId = string.Empty;
    private string requestInterceptId = string.Empty;
    private string responseInterceptId = string.Empty;
    private string authInterceptId = string.Empty;
    private EventObserver<BeforeRequestSentEventArgs>? requestObserver;
    private EventObserver<ResponseCompletedEventArgs>? responseObserver;
    private EventObserver<AuthRequiredEventArgs>? authObserver;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkTrafficMonitor"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used to monitor the traffic.</param>
    public NetworkTrafficMonitor(BiDiDriver driver)
    {
        this.driver = driver;
    }

    /// <summary>
    /// Gets the list of request modifications applied to intercepted requests.
    /// Add <see cref="NetworkRequestModification"/> instances before calling <see cref="StartMonitoringAsync"/> to activate interception.
    /// </summary>
    public List<NetworkRequestModification> RequestModifications => this.requestModifications;

    /// <summary>
    /// Gets the list of credentials to supply when an auth challenge is received.
    /// The first set of credentials matching any auth challenge realm/scheme will be used.
    /// Add <see cref="AuthCredentials"/> instances before calling <see cref="StartMonitoringAsync"/> to activate auth interception.
    /// </summary>
    public List<AuthCredentials> AuthCredentials => this.authCredentials;

    /// <summary>
    /// Asynchronously starts monitoring network traffic.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to monitor traffic.</param>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    public async Task StartMonitoringAsync(string browsingContextId)
    {
        this.requestObserver = this.driver.Network.OnBeforeRequestSent.AddObserver(this.HandleBeforeRequestSent);
        this.responseObserver = this.driver.Network.OnResponseCompleted.AddObserver(this.HandleResponseCompleted);

        AddDataCollectorCommandParameters addCollectorParameters = new(200000000, DataType.Request, DataType.Response);
        addCollectorParameters.BrowsingContexts.Add(browsingContextId);
        AddDataCollectorCommandResult collectorResult = await this.driver.Network.AddDataCollectorAsync(addCollectorParameters).ConfigureAwait(false);
        this.bodyCollectorId = collectorResult.CollectorId;

        List<string> subscribedEvents = [
            this.driver.Network.OnBeforeRequestSent.EventName,
            this.driver.Network.OnResponseCompleted.EventName,
        ];

        if (this.requestModifications.Count > 0)
        {
            AddInterceptCommandParameters requestIntercept = new(InterceptPhase.BeforeRequestSent)
            {
                BrowsingContextIds = [browsingContextId],
                UrlPatterns = [.. this.requestModifications.Select(m => (UrlPattern)new UrlPatternString(m.UrlPattern))],
            };
            AddInterceptCommandResult requestInterceptResult = await this.driver.Network.AddInterceptAsync(requestIntercept).ConfigureAwait(false);
            this.requestInterceptId = requestInterceptResult.InterceptId;
        }

        if (this.authCredentials.Count > 0)
        {
            this.authObserver = this.driver.Network.OnAuthRequired.AddObserver(this.HandleAuthRequired);

            AddInterceptCommandParameters authIntercept = new(InterceptPhase.AuthRequired)
            {
                BrowsingContextIds = [browsingContextId],
            };
            AddInterceptCommandResult authInterceptResult = await this.driver.Network.AddInterceptAsync(authIntercept).ConfigureAwait(false);
            this.authInterceptId = authInterceptResult.InterceptId;

            subscribedEvents.Add(this.driver.Network.OnAuthRequired.EventName);
        }

        List<string> subscribedContexts = [browsingContextId];
        SubscribeCommandParameters subscribe = new(subscribedEvents, subscribedContexts);
        SubscribeCommandResult subscribeResult = await this.driver.Session.SubscribeAsync(subscribe).ConfigureAwait(false);
        this.eventSubscriptionId = subscribeResult.SubscriptionId;
    }

    /// <summary>
    /// Asynchronously stops monitoring network traffic and removes all intercepts and collectors.
    /// </summary>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    public async Task StopMonitoringAsync()
    {
        this.requestObserver?.Unobserve();
        this.responseObserver?.Unobserve();
        this.authObserver?.Unobserve();

        List<Task> cleanupTasks = [];

        if (!string.IsNullOrEmpty(this.bodyCollectorId))
        {
            cleanupTasks.Add(this.driver.Network.RemoveDataCollectorAsync(new RemoveDataCollectorCommandParameters(this.bodyCollectorId)));
        }

        if (!string.IsNullOrEmpty(this.requestInterceptId))
        {
            cleanupTasks.Add(this.driver.Network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(this.requestInterceptId)));
        }

        if (!string.IsNullOrEmpty(this.responseInterceptId))
        {
            cleanupTasks.Add(this.driver.Network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(this.responseInterceptId)));
        }

        if (!string.IsNullOrEmpty(this.authInterceptId))
        {
            cleanupTasks.Add(this.driver.Network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(this.authInterceptId)));
        }

        await Task.WhenAll(cleanupTasks).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(this.eventSubscriptionId))
        {
            UnsubscribeByIdsCommandParameters unsubscribe = new();
            unsubscribe.SubscriptionIds.Add(this.eventSubscriptionId);
            await this.driver.Session.UnsubscribeAsync(unsubscribe).ConfigureAwait(false);
        }

        this.requestInterceptId = string.Empty;
        this.responseInterceptId = string.Empty;
        this.authInterceptId = string.Empty;
        this.bodyCollectorId = string.Empty;
        this.eventSubscriptionId = string.Empty;
    }

    /// <summary>
    /// Gets the traffic captured by the monitor, waiting for all pending requests to complete.
    /// </summary>
    /// <returns>A list of HTTP requests and responses captured by the monitor.</returns>
    public async Task<List<NetworkRequest>> GetCapturedTrafficAsync()
    {
        List<string> capturedRequestIds = [.. this.pendingRequests.Keys];
        List<NetworkRequest> capturedRequests = [.. this.pendingRequests.Values];
        List<Task> requestCompletionTasks = [];
        foreach (NetworkRequest request in capturedRequests)
        {
            requestCompletionTasks.Add(request.WaitForRequestBodyAsync());
            requestCompletionTasks.Add(request.WaitForResponseReceivedAsync());
        }

        await Task.WhenAll(requestCompletionTasks).ConfigureAwait(false);
        foreach (string capturedRequestId in capturedRequestIds)
        {
            this.pendingRequests.Remove(capturedRequestId);
        }

        List<Task> responseCompletionTasks = [];
        foreach (NetworkRequest request in capturedRequests)
        {
            responseCompletionTasks.Add(request.WaitForResponseBodyAsync());
        }

        await Task.WhenAll(responseCompletionTasks).ConfigureAwait(false);
        return capturedRequests;
    }

    private void HandleBeforeRequestSent(BeforeRequestSentEventArgs e)
    {
        string requestId = e.Request.RequestId;

        NetworkRequestModification? modification = this.requestModifications
            .FirstOrDefault(m => e.Request.Url.Contains(m.UrlPattern, StringComparison.OrdinalIgnoreCase));

        if (modification is not null && e.IsBlocked)
        {
            // Fire-and-forget the async continuation; event handlers must be synchronous.
            _ = this.ApplyModificationAsync(e.Request.RequestId, modification, e.Request);
        }

        Task<GetDataCommandResult>? requestBodyTask = null;
        if (NetworkRequest.MethodMayHaveBody(e.Request.Method))
        {
            GetDataCommandParameters getBodyParameters = new(requestId)
            {
                CollectorId = this.bodyCollectorId,
                DataType = DataType.Request,
                DisownCollectedData = true,
            };
            requestBodyTask = this.driver.Network.GetDataAsync(getBodyParameters);
        }

        this.pendingRequests[requestId] = new NetworkRequest(e.Request, e.Timestamp, requestBodyTask);
    }

    private void HandleResponseCompleted(ResponseCompletedEventArgs e)
    {
        string requestId = e.Request.RequestId;
        GetDataCommandParameters getBodyParameters = new(requestId)
        {
            CollectorId = this.bodyCollectorId,
            DataType = DataType.Response,
            DisownCollectedData = true,
        };
        Task<GetDataCommandResult> responseBodyTask = this.driver.Network.GetDataAsync(getBodyParameters);

        if (this.pendingRequests.TryGetValue(requestId, out NetworkRequest? networkRequest))
        {
            networkRequest.SetResponseReceived(e.Response, responseBodyTask);
        }
    }

    private void HandleAuthRequired(AuthRequiredEventArgs e)
    {
        if (this.authCredentials.Count == 0)
        {
            return;
        }

        _ = this.ProvideAuthAsync(e.Request.RequestId);
    }

    private async Task ApplyModificationAsync(string requestId, NetworkRequestModification modification, RequestData originalRequest)
    {
        ContinueRequestCommandParameters continueParams = new(requestId);

        if (modification.ReplacementUrl is not null)
        {
            continueParams.Url = modification.ReplacementUrl;
        }

        if (modification.ReplacementMethod is not null)
        {
            continueParams.Method = modification.ReplacementMethod;
        }

        if (modification.ReplacementBody is not null)
        {
            continueParams.Body = BytesValue.FromString(modification.ReplacementBody);
        }

        if (modification.AdditionalHeaders.Count > 0)
        {
            continueParams.Headers = [.. originalRequest.Headers.Select(h => new Header(h.Name, h.Value.Value))];
            foreach (KeyValuePair<string, string> additionalHeader in modification.AdditionalHeaders)
            {
                continueParams.Headers.RemoveAll(h => string.Equals(h.Name, additionalHeader.Key, StringComparison.OrdinalIgnoreCase));
                continueParams.Headers.Add(new Header(additionalHeader.Key, additionalHeader.Value));
            }
        }

        await this.driver.Network.ContinueRequestAsync(continueParams).ConfigureAwait(false);
    }

    private async Task ProvideAuthAsync(string requestId)
    {
        AuthCredentials credentials = this.authCredentials[0];
        ContinueWithAuthCommandParameters authParams = new(requestId)
        {
            Action = ContinueWithAuthActionType.ProvideCredentials,
            Credentials = credentials,
        };
        await this.driver.Network.ContinueWithAuthAsync(authParams).ConfigureAwait(false);
    }
}
