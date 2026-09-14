// <copyright file="NetworkTrafficMonitor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using System.Collections.Concurrent;
using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

/// <summary>
/// Monitors network traffic, with optional support for request interception, modification, authentication, and HAR capture.
/// This is a demonstration implementation and is not intended for production use.
/// </summary>
public class NetworkTrafficMonitor
{
    private readonly BiDiDriver driver;
    private readonly ConcurrentDictionary<string, NetworkRequest> pendingRequests = new();
    private readonly List<NetworkRequestModification> requestModifications = [];
    private readonly List<AuthChallengeCredentials> authCredentials = [];
    private readonly List<RequestInterceptModification> requestIntercepts = [];
    private string eventSubscriptionId = string.Empty;
    private string bodyCollectorId = string.Empty;
    private string responseInterceptId = string.Empty;
    private string authInterceptId = string.Empty;
    private EventObserver<BeforeRequestSentEventArgs>? requestObserver;
    private EventObserver<ResponseCompletedEventArgs>? responseObserver;
    private EventObserver<FetchErrorEventArgs>? fetchErrorObserver;
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
    /// Each modification is registered as an intercept of its own; a request that more than one modification's pattern
    /// matches is modified by the first of them in this list.
    /// </summary>
    public List<NetworkRequestModification> RequestModifications => this.requestModifications;

    /// <summary>
    /// Gets the list of credentials to supply when an auth challenge is received.
    /// The first entry that matches the scheme and realm of one of the challenges is supplied. When no entry matches,
    /// the request is continued with the browser's default behavior for the challenge.
    /// Add <see cref="AuthChallengeCredentials"/> instances before calling <see cref="StartMonitoringAsync"/> to activate auth interception.
    /// </summary>
    public List<AuthChallengeCredentials> AuthCredentials => this.authCredentials;

    /// <summary>
    /// Asynchronously starts monitoring network traffic.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to monitor traffic.</param>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    public async Task StartMonitoringAsync(string browsingContextId)
    {
        this.requestObserver = this.driver.Network.OnBeforeRequestSent.AddObserver(this.HandleBeforeRequestSentAsync, ObservableEventHandlerOptions.RunHandlerAsynchronously);
        this.responseObserver = this.driver.Network.OnResponseCompleted.AddObserver(this.HandleResponseCompleted, ObservableEventHandlerOptions.RunHandlerAsynchronously);
        this.fetchErrorObserver = this.driver.Network.OnFetchError.AddObserver(this.HandleFetchError, ObservableEventHandlerOptions.RunHandlerAsynchronously);

        AddDataCollectorCommandParameters addCollectorParameters = new(200000000, DataType.Request, DataType.Response);
        addCollectorParameters.Contexts.Add(browsingContextId);
        AddDataCollectorCommandResult collectorResult = await this.driver.Network.AddDataCollectorAsync(addCollectorParameters).ConfigureAwait(false);
        this.bodyCollectorId = collectorResult.CollectorId;

        List<string> subscribedEvents = [
            this.driver.Network.OnBeforeRequestSent.EventName,
            this.driver.Network.OnResponseCompleted.EventName,
            this.driver.Network.OnFetchError.EventName,
        ];

        // Each modification gets an intercept of its own, so the intercept IDs a blocked request carries identify
        // exactly which modifications matched it, by the same URL pattern rules the browser used to block it.
        foreach (NetworkRequestModification modification in this.requestModifications)
        {
            AddInterceptCommandParameters requestIntercept = new(InterceptPhase.BeforeRequestSent)
            {
                Contexts = { browsingContextId },
            };
            requestIntercept.UrlPatterns.Add(new UrlPatternString(modification.UrlPattern));
            AddInterceptCommandResult requestInterceptResult = await this.driver.Network.AddInterceptAsync(requestIntercept).ConfigureAwait(false);
            this.requestIntercepts.Add(new RequestInterceptModification(requestInterceptResult.InterceptId, modification));
        }

        if (this.authCredentials.Count > 0)
        {
            this.authObserver = this.driver.Network.OnAuthRequired.AddObserver(this.HandleAuthRequiredAsync, ObservableEventHandlerOptions.RunHandlerAsynchronously);

            AddInterceptCommandParameters authIntercept = new(InterceptPhase.AuthRequired)
            {
                Contexts = { browsingContextId },
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
        this.fetchErrorObserver?.Unobserve();
        this.authObserver?.Unobserve();

        // Nothing reports an outcome for a request once the observers are gone. A request still pending is failed, so
        // that a capture after stopping returns instead of waiting for it; one that already has an outcome ignores this.
        foreach (NetworkRequest request in this.pendingRequests.Values)
        {
            request.SetFailed("Monitoring stopped before the request completed.");
        }

        List<Task> cleanupTasks = [];

        if (!string.IsNullOrEmpty(this.bodyCollectorId))
        {
            cleanupTasks.Add(this.driver.Network.RemoveDataCollectorAsync(new RemoveDataCollectorCommandParameters(this.bodyCollectorId)));
        }

        foreach (RequestInterceptModification requestIntercept in this.requestIntercepts)
        {
            cleanupTasks.Add(this.driver.Network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(requestIntercept.InterceptId)));
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
            UnsubscribeByIdsCommandParameters unsubscribe = new(this.eventSubscriptionId);
            await this.driver.Session.UnsubscribeAsync(unsubscribe).ConfigureAwait(false);
        }

        this.requestIntercepts.Clear();
        this.responseInterceptId = string.Empty;
        this.authInterceptId = string.Empty;
        this.bodyCollectorId = string.Empty;
        this.eventSubscriptionId = string.Empty;
    }

    /// <summary>
    /// Gets the traffic captured by the monitor, waiting for every pending request either to complete or to fail.
    /// </summary>
    /// <param name="timeout">
    /// The longest time to wait, or <see langword="null"/> to wait until every pending request has completed or failed.
    /// A request still in flight when the timeout elapses is not returned; it stays pending and is returned by a later
    /// call once it has completed.
    /// </param>
    /// <returns>A list of HTTP requests and responses captured by the monitor.</returns>
    /// <remarks>
    /// A request that fails, as reported by a <c>network.fetchError</c> event or because monitoring stopped before it
    /// completed, is returned with <see cref="NetworkRequest.IsFailed"/> set and no response. A body that could not be
    /// retrieved does not fail the capture: the request is returned with <see cref="NetworkRequest.RequestBodyErrorText"/>
    /// or <see cref="NetworkRequest.ResponseBodyErrorText"/> set.
    /// </remarks>
    public async Task<List<NetworkRequest>> GetCapturedTrafficAsync(TimeSpan? timeout = null)
    {
        // The handlers record requests concurrently while this runs, so the pending requests are read as one snapshot.
        KeyValuePair<string, NetworkRequest>[] capturedEntries = this.pendingRequests.ToArray();
        Task[] completionTasks = [.. capturedEntries.Select(entry => entry.Value.WaitForCompletionAsync())];
        Task allCompleted = Task.WhenAll(completionTasks);

        if (timeout is null)
        {
            await allCompleted.ConfigureAwait(false);
        }
        else
        {
            // A completion task never faults, so one abandoned here when the timeout elapses leaves no unobserved
            // exception behind.
            using CancellationTokenSource delayCancellation = new();
            Task elapsed = Task.Delay(timeout.Value, delayCancellation.Token);
            if (await Task.WhenAny(allCompleted, elapsed).ConfigureAwait(false) == allCompleted)
            {
                delayCancellation.Cancel();
            }
        }

        List<NetworkRequest> capturedRequests = [];
        for (int i = 0; i < capturedEntries.Length; i++)
        {
            // A request still in flight stays pending for a later capture.
            if (!completionTasks[i].IsCompleted)
            {
                continue;
            }

            // Removes the entry only while it still holds the captured request, so a request recorded again under the
            // same ID since the snapshot was taken is left for the next capture.
            ((ICollection<KeyValuePair<string, NetworkRequest>>)this.pendingRequests).Remove(capturedEntries[i]);
            capturedRequests.Add(capturedEntries[i].Value);
        }

        return capturedRequests;
    }

    private async Task HandleBeforeRequestSentAsync(BeforeRequestSentEventArgs e)
    {
        string requestId = e.Request.RequestId;

        Task<GetDataCommandResult>? requestBodyTask = null;
        if (NetworkRequest.MethodMayHaveBody(e.Request.Method))
        {
            await Task.Yield();
            GetDataCommandParameters getBodyParameters = new(requestId, DataType.Request)
            {
                CollectorId = this.bodyCollectorId,
                DisownCollectedData = true,
            };
            requestBodyTask = this.driver.Network.GetDataAsync(getBodyParameters);
        }

        // An asynchronous handler runs on the thread dispatching the event until its first await, and the next event
        // is not dispatched before then. The request is therefore recorded here, ahead of its response. Recording it
        // after continuing the request would race that response: once continued, the request can complete, and its
        // response handler run, before this handler resumes. That is also why this handler does not await first.
        this.pendingRequests[requestId] = new NetworkRequest(e.Request, e.Timestamp, requestBodyTask);

        if (!e.IsBlocked || e.Intercepts is null)
        {
            return;
        }

        // A request blocked by one of this monitor's intercepts stays blocked until the monitor continues it. A request
        // blocked only by an intercept that some other code added is left for that code to continue.
        foreach (RequestInterceptModification intercept in this.requestIntercepts)
        {
            if (e.Intercepts.Contains(intercept.InterceptId))
            {
                await this.ApplyModificationAsync(requestId, intercept.Modification, e.Request).ConfigureAwait(false);
                return;
            }
        }
    }

    private void HandleResponseCompleted(ResponseCompletedEventArgs e)
    {
        string requestId = e.Request.RequestId;
        GetDataCommandParameters getBodyParameters = new(requestId, DataType.Response)
        {
            CollectorId = this.bodyCollectorId,
            DisownCollectedData = true,
        };
        Task<GetDataCommandResult> responseBodyTask = this.driver.Network.GetDataAsync(getBodyParameters);

        if (this.pendingRequests.TryGetValue(requestId, out NetworkRequest? networkRequest))
        {
            networkRequest.SetResponseReceived(e.Response, responseBodyTask);
        }
    }

    private void HandleFetchError(FetchErrorEventArgs e)
    {
        // A request that fails never has a completed response, so waiting for one would never end. The failure
        // completes the request instead. It is recorded before this handler runs, because the before-request-sent
        // handler records it on the thread dispatching events, ahead of any later event for the request.
        if (this.pendingRequests.TryGetValue(e.Request.RequestId, out NetworkRequest? networkRequest))
        {
            networkRequest.SetFailed(e.ErrorText);
        }
    }

    private async Task HandleAuthRequiredAsync(AuthRequiredEventArgs e)
    {
        // Only a request blocked by this monitor's own auth intercept is the monitor's to continue.
        if (!e.IsBlocked || e.Intercepts is null || !e.Intercepts.Contains(this.authInterceptId))
        {
            return;
        }

        // With no matching credentials the request is still continued, with the browser's default behavior for the
        // challenge, rather than being left blocked.
        await Task.Yield();
        ContinueWithAuthCommandParameters authParams = new(e.Request.RequestId);
        AuthChallengeCredentials? credentials = this.authCredentials.FirstOrDefault(candidate => candidate.Matches(e.Response.AuthChallenges));
        if (credentials is not null)
        {
            authParams.Action = ContinueWithAuthActionType.ProvideCredentials;
            authParams.Credentials = credentials.Credentials;
        }

        try
        {
            await this.driver.Network.ContinueWithAuthAsync(authParams).ConfigureAwait(false);
        }
        catch (WebDriverBiDiException)
        {
            // A challenge that is not answered leaves the request blocked, so it is canceled instead. The original
            // exception is rethrown, and the driver reports it through OnEventHandlerErrorOccurred.
            await this.ReleaseBlockedRequestAsync(() => this.driver.Network.ContinueWithAuthAsync(new ContinueWithAuthCommandParameters(e.Request.RequestId) { Action = ContinueWithAuthActionType.Cancel })).ConfigureAwait(false);
            throw;
        }
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

        try
        {
            await this.driver.Network.ContinueRequestAsync(continueParams).ConfigureAwait(false);
        }
        catch (WebDriverBiDiException)
        {
            // A request that is not continued stays blocked, and the page waits on it until the browser gives up. It is
            // failed instead, so the page sees a network error. The original exception is rethrown, and the driver
            // reports it through OnEventHandlerErrorOccurred.
            await this.ReleaseBlockedRequestAsync(() => this.driver.Network.FailRequestAsync(new FailRequestCommandParameters(requestId))).ConfigureAwait(false);
            throw;
        }
    }

    private async Task ReleaseBlockedRequestAsync(Func<Task> release)
    {
        try
        {
            await release().ConfigureAwait(false);
        }
        catch (WebDriverBiDiException)
        {
            // The request may no longer be blocked: the command that failed may have taken effect before its response
            // was lost, or the page may have gone away. There is then nothing to release, and the error worth reporting
            // is the original one, which the caller rethrows.
        }
    }

    private record RequestInterceptModification
    {
        public RequestInterceptModification(string interceptId, NetworkRequestModification modification)
        {
            this.InterceptId = interceptId;
            this.Modification = modification;
        }

        public string InterceptId { get; private set; }

        public NetworkRequestModification Modification { get; private set; }
    }
}
