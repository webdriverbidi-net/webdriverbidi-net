// <copyright file="NetworkTrafficMonitor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Network;

using System.Collections.Concurrent;
using WebDriverBiDi.Session;

/// <summary>
/// Records the network traffic of browsing contexts, optionally modifying requests and answering authentication
/// challenges, for inspection or for a HAR file (see <see cref="HarGenerator"/>).
/// </summary>
public sealed class NetworkTrafficMonitor : IAsyncDisposable
{
    private readonly BiDiDriver driver;
    private readonly NetworkTrafficMonitorOptions options;
    private readonly ConcurrentDictionary<string, NetworkRequest> pendingRequests = new();
    private readonly ConcurrentDictionary<string, int> authAttempts = new();
    private readonly SemaphoreSlim stateLock = new(1, 1);
    private MonitoringSession? session;
    private int droppedRequestCount;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkTrafficMonitor"/> class.
    /// </summary>
    /// <param name="driver">The driver, which must be connected before monitoring starts.</param>
    /// <param name="options">The settings, read when monitoring starts, or <see langword="null"/> for the defaults.</param>
    public NetworkTrafficMonitor(BiDiDriver driver, NetworkTrafficMonitorOptions? options = null)
    {
        this.driver = driver;
        this.options = options ?? new NetworkTrafficMonitorOptions();
    }

    /// <summary>
    /// Gets a value indicating whether traffic is being monitored.
    /// </summary>
    public bool IsMonitoring => Volatile.Read(ref this.session) is not null;

    /// <summary>
    /// Gets the number of requests not recorded because <see cref="NetworkTrafficMonitorOptions.MaxRetainedRequests"/>
    /// requests were already waiting to be retrieved.
    /// </summary>
    public int DroppedRequestCount => Volatile.Read(ref this.droppedRequestCount);

    /// <summary>
    /// Starts monitoring, with the options as they are now. If starting fails partway, everything it set up in the
    /// browser is removed again before the exception is thrown.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels starting.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when monitoring has already started.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the monitor has been disposed.</exception>
    public async Task StartMonitoringAsync(CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        await this.stateLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.session is not null)
            {
                throw new InvalidOperationException("Monitoring has already started; stop it before starting again.");
            }

            MonitoringSession starting = new(this.options);
            try
            {
                await this.StartSessionAsync(starting, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                try
                {
                    await this.CleanUpAsync(starting, CancellationToken.None).ConfigureAwait(false);
                }
                catch (WebDriverBiDiException)
                {
                    // The failure worth reporting is the one that stopped the start.
                }

                throw;
            }

            Volatile.Write(ref this.session, starting);
        }
        finally
        {
            this.stateLock.Release();
        }
    }

    /// <summary>
    /// Stops monitoring, and removes everything monitoring set up in the browser. A request still in flight is
    /// recorded as failed. Stopping when not monitoring does nothing.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels waiting for the browser to confirm the removals.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StopMonitoringAsync(CancellationToken cancellationToken = default)
    {
        await this.stateLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            MonitoringSession? stopping = this.session;
            if (stopping is null)
            {
                return;
            }

            Volatile.Write(ref this.session, null);
            await this.CleanUpAsync(stopping, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            this.stateLock.Release();
        }
    }

    /// <summary>
    /// Returns the requests recorded since the last call, in the order they started, once each has completed or
    /// failed and its bodies have been retrieved. A request still in flight when the wait ends is kept for a later call.
    /// </summary>
    /// <param name="timeout">How long to wait for requests in flight, or <see langword="null"/> to wait until all complete.</param>
    /// <param name="cancellationToken">A token that cancels the wait.</param>
    /// <returns>The completed requests. Each hop of a redirect chain is a request of its own.</returns>
    public async Task<IReadOnlyList<NetworkRequest>> GetCapturedTrafficAsync(TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        // The handlers record requests concurrently while this runs, so the pending requests are read as one snapshot.
        KeyValuePair<string, NetworkRequest>[] capturedEntries = this.pendingRequests.ToArray();
        Task[] completionTasks = [.. capturedEntries.Select(entry => entry.Value.WaitForCompletionAsync())];

        // A completion task never faults, so one abandoned when the wait ends leaves no unobserved exception behind.
        using CancellationTokenSource delayCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task waitEnded = Task.Delay(timeout ?? Timeout.InfiniteTimeSpan, delayCancellation.Token);
        await Task.WhenAny(Task.WhenAll(completionTasks), waitEnded).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        delayCancellation.Cancel();

        List<NetworkRequest> capturedRequests = [];
        for (int i = 0; i < capturedEntries.Length; i++)
        {
            // Removes the entry only while it still holds the captured request, so a request recorded again under the
            // same key since the snapshot was taken is left for the next capture.
            if (completionTasks[i].IsCompleted && ((ICollection<KeyValuePair<string, NetworkRequest>>)this.pendingRequests).Remove(capturedEntries[i]))
            {
                capturedRequests.Add(capturedEntries[i].Value);
            }
        }

        return [.. capturedRequests.OrderBy(request => request.StartedDateTime).ThenBy(request => request.RedirectCount)];
    }

    /// <summary>
    /// Stops monitoring, if it is in progress. Failures to remove what monitoring set up in the browser, which may
    /// already be gone, are ignored.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;
        try
        {
            await this.StopMonitoringAsync().ConfigureAwait(false);
        }
        catch (WebDriverBiDiException)
        {
            // The driver may have disconnected, taking the browser-side state with it.
        }
    }

    private static string GetRequestKey(string requestId, ulong redirectCount) => $"{requestId}#{redirectCount}";

    private static bool IsRedirect(ulong status) => status is 301 or 302 or 303 or 307 or 308;

    private static async Task ReleaseBlockedRequestAsync(Func<Task> release)
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

    // Observers are added before any command, so that no event of a subscription made below is missed.
    private async Task StartSessionAsync(MonitoringSession starting, CancellationToken cancellationToken)
    {
        NetworkModule network = this.driver.Network;
        starting.Observers.Add(network.OnBeforeRequestSent.AddObserver(e => this.HandleBeforeRequestSentAsync(starting, e), ObservableEventHandlerOptions.RunHandlerAsynchronously));
        starting.Observers.Add(network.OnResponseCompleted.AddObserver(e => this.HandleResponseCompleted(starting, e), ObservableEventHandlerOptions.RunHandlerAsynchronously));
        starting.Observers.Add(network.OnFetchError.AddObserver(e => this.HandleFetchError(e), ObservableEventHandlerOptions.RunHandlerAsynchronously));
        List<string> subscribedEvents = [network.OnBeforeRequestSent.EventName, network.OnResponseCompleted.EventName, network.OnFetchError.EventName];
        if (starting.Credentials.Length > 0)
        {
            starting.Observers.Add(network.OnAuthRequired.AddObserver(e => this.HandleAuthRequiredAsync(starting, e), ObservableEventHandlerOptions.RunHandlerAsynchronously));
            subscribedEvents.Add(network.OnAuthRequired.EventName);
        }

        if (starting.CaptureBodies)
        {
            AddDataCollectorCommandParameters collector = new(starting.MaxBodySize, DataType.Request, DataType.Response);
            collector.Contexts.AddRange(starting.BrowsingContextIds);
            starting.CollectorId = (await network.AddDataCollectorAsync(collector, cancellationToken: cancellationToken).ConfigureAwait(false)).CollectorId;
        }

        // Each modification gets an intercept of its own, so the intercept IDs a blocked request carries identify
        // exactly which modifications matched it, by the same URL pattern rules the browser used to block it.
        foreach (NetworkRequestModification modification in starting.Modifications)
        {
            AddInterceptCommandParameters intercept = new(InterceptPhase.BeforeRequestSent);
            intercept.Contexts.AddRange(starting.BrowsingContextIds);
            intercept.UrlPatterns.Add(new UrlPatternString(modification.UrlPattern));
            string interceptId = (await network.AddInterceptAsync(intercept, cancellationToken: cancellationToken).ConfigureAwait(false)).InterceptId;
            starting.RequestIntercepts.Add((interceptId, modification));
        }

        if (starting.Credentials.Length > 0)
        {
            AddInterceptCommandParameters intercept = new(InterceptPhase.AuthRequired);
            intercept.Contexts.AddRange(starting.BrowsingContextIds);
            starting.AuthInterceptId = (await network.AddInterceptAsync(intercept, cancellationToken: cancellationToken).ConfigureAwait(false)).InterceptId;
        }

        SubscribeCommandParameters subscribe = new(subscribedEvents, starting.BrowsingContextIds.Length > 0 ? [.. starting.BrowsingContextIds] : null);
        starting.SubscriptionId = (await this.driver.Session.SubscribeAsync(subscribe, cancellationToken: cancellationToken).ConfigureAwait(false)).SubscriptionId;
    }

    private async Task CleanUpAsync(MonitoringSession ending, CancellationToken cancellationToken)
    {
        foreach (IDisposable observer in ending.Observers)
        {
            observer.Dispose();
        }

        // Nothing reports an outcome for a request once the observers are gone. A request still pending is failed, so
        // that a capture after stopping returns instead of waiting for it; one that already has an outcome ignores this.
        foreach (NetworkRequest request in this.pendingRequests.Values)
        {
            request.SetFailed("Monitoring stopped before the request completed.");
        }

        NetworkModule network = this.driver.Network;
        List<Task> removals = [];
        if (ending.CollectorId is not null)
        {
            removals.Add(network.RemoveDataCollectorAsync(new RemoveDataCollectorCommandParameters(ending.CollectorId), cancellationToken: cancellationToken));
        }

        removals.AddRange(ending.RequestIntercepts.Select(intercept => network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(intercept.InterceptId), cancellationToken: cancellationToken)));
        if (ending.AuthInterceptId is not null)
        {
            removals.Add(network.RemoveInterceptAsync(new RemoveInterceptCommandParameters(ending.AuthInterceptId), cancellationToken: cancellationToken));
        }

        if (ending.SubscriptionId is not null)
        {
            removals.Add(this.driver.Session.UnsubscribeAsync(new UnsubscribeByIdsCommandParameters(ending.SubscriptionId), cancellationToken: cancellationToken));
        }

        // Every removal is attempted before any failure is reported.
        await Task.WhenAll(removals).ConfigureAwait(false);
        this.authAttempts.Clear();
    }

    private async Task HandleBeforeRequestSentAsync(MonitoringSession current, BeforeRequestSentEventArgs e)
    {
        // An asynchronous handler runs on the thread dispatching the event until its first await, and the next event
        // is not dispatched before then. The request is therefore recorded here, before anything awaits, and so ahead
        // of its response: once a blocked request is continued below, its response handler can run at any time.
        string requestId = e.Request.RequestId;
        if (this.pendingRequests.Count < current.MaxRetainedRequests)
        {
            Task<GetDataCommandResult>? requestBody = current.CollectorId is not null && e.Request.BodySize > 0
                ? Task.Run(() => this.driver.Network.GetDataAsync(new GetDataCommandParameters(requestId, DataType.Request) { CollectorId = current.CollectorId, DisownCollectedData = true }))
                : null;
            this.pendingRequests[GetRequestKey(requestId, e.RedirectCount)] = new NetworkRequest(e.Request, e.Timestamp, e.RedirectCount, e.BrowsingContextId, e.NavigationId, requestBody);
        }
        else
        {
            Interlocked.Increment(ref this.droppedRequestCount);
        }

        // A request blocked by one of this monitor's intercepts stays blocked until the monitor continues it, whether
        // or not it was recorded. A request blocked only by an intercept that other code added is left for that code.
        if (e.IsBlocked && e.Intercepts is not null)
        {
            foreach ((string interceptId, NetworkRequestModification modification) in current.RequestIntercepts)
            {
                if (e.Intercepts.Contains(interceptId))
                {
                    await this.ApplyModificationAsync(requestId, modification, e.Request).ConfigureAwait(false);
                    return;
                }
            }
        }
    }

    private void HandleResponseCompleted(MonitoringSession current, ResponseCompletedEventArgs e)
    {
        string requestId = e.Request.RequestId;
        this.authAttempts.TryRemove(requestId, out _);
        if (!this.pendingRequests.TryGetValue(GetRequestKey(requestId, e.RedirectCount), out NetworkRequest? networkRequest))
        {
            return;
        }

        // A redirect's body is not kept by browsers, and the collector holds data for the chain's final response only.
        Task<GetDataCommandResult>? responseBody = current.CollectorId is not null && !IsRedirect(e.Response.Status)
            ? Task.Run(() => this.driver.Network.GetDataAsync(new GetDataCommandParameters(requestId, DataType.Response) { CollectorId = current.CollectorId, DisownCollectedData = true }))
            : null;
        networkRequest.SetResponseReceived(e.Response, responseBody, e.Request.Timings);
    }

    private void HandleFetchError(FetchErrorEventArgs e)
    {
        // A request that fails never has a completed response, so waiting for one would never end. The failure
        // completes the request instead.
        this.authAttempts.TryRemove(e.Request.RequestId, out _);
        if (this.pendingRequests.TryGetValue(GetRequestKey(e.Request.RequestId, e.RedirectCount), out NetworkRequest? networkRequest))
        {
            networkRequest.SetFailed(e.ErrorText);
        }
    }

    private async Task HandleAuthRequiredAsync(MonitoringSession current, AuthRequiredEventArgs e)
    {
        // Only a request blocked by this monitor's own auth intercept is the monitor's to continue.
        if (!e.IsBlocked || e.Intercepts is null || !e.Intercepts.Contains(current.AuthInterceptId!))
        {
            return;
        }

        // A rejected answer brings the challenge back for the same request, so answers are counted, and once
        // they run out the challenge is canceled rather than answered again forever. With no matching
        // credentials, the browser's default behavior for the challenge applies.
        string requestId = e.Request.RequestId;
        int attempt = this.authAttempts.AddOrUpdate(requestId, 1, (_, previous) => previous + 1);
        AuthChallengeCredentials? credentials = current.Credentials.FirstOrDefault(candidate => candidate.Matches(e.Response.AuthChallenges));
        ContinueWithAuthCommandParameters authParams = new(requestId);
        if (credentials is not null && attempt <= current.MaxAuthAttempts)
        {
            authParams.Action = ContinueWithAuthActionType.ProvideCredentials;
            authParams.Credentials = credentials.Credentials;
        }
        else if (credentials is not null)
        {
            authParams.Action = ContinueWithAuthActionType.Cancel;
        }

        await Task.Yield();
        try
        {
            await this.driver.Network.ContinueWithAuthAsync(authParams).ConfigureAwait(false);
        }
        catch (WebDriverBiDiException)
        {
            // A challenge that is not answered leaves the request blocked, so it is canceled instead. The original
            // exception is rethrown, and the driver reports it through OnEventHandlerErrorOccurred.
            await ReleaseBlockedRequestAsync(() => this.driver.Network.ContinueWithAuthAsync(new ContinueWithAuthCommandParameters(requestId) { Action = ContinueWithAuthActionType.Cancel })).ConfigureAwait(false);
            throw;
        }
    }

    private async Task ApplyModificationAsync(string requestId, NetworkRequestModification modification, RequestData originalRequest)
    {
        ContinueRequestCommandParameters continueParams = new(requestId)
        {
            Url = modification.ReplacementUrl,
            Method = modification.ReplacementMethod,
        };
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
            await ReleaseBlockedRequestAsync(() => this.driver.Network.FailRequestAsync(new FailRequestCommandParameters(requestId))).ConfigureAwait(false);
            throw;
        }
    }

    private void ThrowIfDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException(nameof(NetworkTrafficMonitor));
        }
    }

    // The options as they were when monitoring started, and what monitoring set up in the browser.
    private sealed class MonitoringSession
    {
        public MonitoringSession(NetworkTrafficMonitorOptions options)
        {
            this.BrowsingContextIds = [.. options.BrowsingContextIds];
            this.Modifications = [.. options.RequestModifications.Select(modification => modification.Clone())];
            this.Credentials = [.. options.AuthCredentials.Select(credentials => credentials.Clone())];
            this.CaptureBodies = options.CaptureBodies;
            this.MaxBodySize = options.MaxBodySize;
            this.MaxRetainedRequests = options.MaxRetainedRequests;
            this.MaxAuthAttempts = options.MaxAuthAttempts;
        }

        public string[] BrowsingContextIds { get; }

        public NetworkRequestModification[] Modifications { get; }

        public AuthChallengeCredentials[] Credentials { get; }

        public bool CaptureBodies { get; }

        public ulong MaxBodySize { get; }

        public int MaxRetainedRequests { get; }

        public int MaxAuthAttempts { get; }

        public List<IDisposable> Observers { get; } = [];

        public List<(string InterceptId, NetworkRequestModification Modification)> RequestIntercepts { get; } = [];

        public string? CollectorId { get; set; }

        public string? AuthInterceptId { get; set; }

        public string? SubscriptionId { get; set; }
    }
}
