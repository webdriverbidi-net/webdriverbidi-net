// <copyright file="NetworkTrafficMonitor.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

/// <summary>
/// A simple class for monitoring network traffic. This is a naive implementation, intended solely for illustrative purposes.
/// </summary>
public class NetworkTrafficMonitor
{
    private static readonly List<string> HttpMethodsWithBodies = [
        "POST",
        "PUT",
        "PATCH"
    ];

    private readonly BiDiDriver driver;
    private readonly Dictionary<string, NetworkRequest> pendingRequests = [];
    private string eventSubscriptionId = string.Empty;
    private string bodyCollectorId = string.Empty;
    private EventObserver<BeforeRequestSentEventArgs>? requestObserver;
    private EventObserver<ResponseCompletedEventArgs>? responseObserver;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkTrafficMonitor"/> class.
    /// </summary>
    /// <param name="driver">The <see cref="BiDiDriver"/> instance used to monitor the traffic.</param>
    public NetworkTrafficMonitor(BiDiDriver driver)
    {
        this.driver = driver;
    }

    /// <summary>
    /// Asynchronously starts monitoring network traffic.
    /// </summary>
    /// <param name="browsingContextId">The ID of the browsing context for which to monitor traffic.</param>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    public async Task StartMonitoringAsync(string browsingContextId)
    {
        // Set up event handlers on the BeforeRequestSent and ResponseCompleted events.
        this.requestObserver = this.driver.Network.OnBeforeRequestSent.AddObserver(this.HandleBeforeRequestSent);
        this.responseObserver = this.driver.Network.OnResponseCompleted.AddObserver(this.HandleResponseCompleted);

        // Add a data collector to collect request and response bodies.
        AddDataCollectorCommandParameters addCollectorParameters = new(200000000, DataType.Request, DataType.Response);
        addCollectorParameters.BrowsingContexts.Add(browsingContextId);
        AddDataCollectorCommandResult collectorResult = await this.driver.Network.AddDataCollectorAsync(addCollectorParameters);
        this.bodyCollectorId = collectorResult.CollectorId;

        // Subscribe to the events.
        List<string> subscribedEvents = [
            this.driver.Network.OnBeforeRequestSent.EventName,
            this.driver.Network.OnResponseCompleted.EventName
        ];
        List<string> subscribedContexts = [browsingContextId];
        SubscribeCommandParameters subscribe = new(subscribedEvents, subscribedContexts);
        SubscribeCommandResult subscribeResult = await this.driver.Session.SubscribeAsync(subscribe);
        this.eventSubscriptionId = subscribeResult.SubscriptionId;
    }

    /// <summary>
    /// Asynchronously stops monitoring network traffic.
    /// </summary>
    /// <returns>A <see cref="Task"/> object containing information about the asynchronous operation.</returns>
    public async Task StopMonitoringAsync()
    {
        // Remove the event handlers observing the network events.
        this.requestObserver?.Unobserve();
        this.responseObserver?.Unobserve();

        // Remove the data collector for request and response bodies.
        if (!string.IsNullOrEmpty(this.bodyCollectorId))
        {
            await this.driver.Network.RemoveDataCollectorAsync(new RemoveDataCollectorCommandParameters(this.bodyCollectorId));
        }

        // Unsubscribe to the network events.
        if (!string.IsNullOrEmpty(this.eventSubscriptionId))
        {
            UnsubscribeByIdsCommandParameters unsubscribe = new();
            unsubscribe.SubscriptionIds.Add(this.eventSubscriptionId);
            await this.driver.Session.UnsubscribeAsync(unsubscribe);
        }
    }

    /// <summary>
    /// Gets the traffic captured by the monitor.
    /// </summary>
    /// <returns>A list of HTTP requests and response captured by the monitor.</returns>
    public async Task<List<NetworkRequest>> GetCapturedTrafficAsync()
    {
        List<string> capturedRequestIds = [.. this.pendingRequests.Keys];
        List<NetworkRequest> capturedRequests = [.. this.pendingRequests.Values];
        List<Task> requestCompletionTasks = [];
        foreach (NetworkRequest request in capturedRequests)
        {
            // Tasks for each request to have a response received.
            requestCompletionTasks.Add(request.WaitForRequestBodyAsync());
            requestCompletionTasks.Add(request.WaitForResponseReceivedAsync());
        }

        // This waits for all requests to have a response, and for request bodies to
        // be populated.
        await Task.WhenAll(requestCompletionTasks);
        foreach (string capturedRequestId in capturedRequestIds)
        {
            this.pendingRequests.Remove(capturedRequestId);
        }

        List<Task> responseCompletionTasks = [];
        foreach (NetworkRequest request in capturedRequests)
        {
            // Tasks for each response body to be complete.
            responseCompletionTasks.Add(request.WaitForResponseBodyAsync());
        }

        await Task.WhenAll(responseCompletionTasks);
        return capturedRequests;
    }

    private void HandleBeforeRequestSent(BeforeRequestSentEventArgs e)
    {
        string requestId = e.Request.RequestId;
        Task<GetDataCommandResult>? requestBodyTask = null;
        if (HttpMethodsWithBodies.Contains(e.Request.Method.ToUpperInvariant()))
        {
            // Only POST, PUT, and PATCH requests should have bodies. Others
            // may, but that is incorrect, so we will ignore capturing it.
            GetDataCommandParameters getBodyParameters = new(requestId)
            {
                CollectorId = this.bodyCollectorId,
                DataType = DataType.Request,
                DisownCollectedData = true,
            };
            requestBodyTask = this.driver.Network.GetDataAsync(getBodyParameters);
        }

        this.pendingRequests[requestId] = new NetworkRequest(e.Request, requestBodyTask);
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
        this.pendingRequests[requestId].SetResponseReceived(e.Response, responseBodyTask);
    }
}
