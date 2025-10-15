namespace WebDriverBiDi.Demo;

using System.Text;
using WebDriverBiDi.Network;
using WebDriverBiDi.Session;

/// <summary>
/// Values describing how to display base64 encoded binary data in requests and responses.
/// </summary>
public enum Base64DisplayBehavior
{
    /// <summary>
    /// Do not display the data; instead display a message showing binary data was captured, and its length.
    /// </summary>
    NoDisplay,

    /// <summary>
    /// Display the base64 encoded data.
    /// </summary>
    Display,

    /// <summary>
    /// Decode the base64 data into a byte array and display the decoded data. This may include unprintable characters.
    /// </summary>
    Decode
}

/// <summary>
/// A simple class for monitoring network traffic. This is a naive implementation, intended solely for illustrative purposes.
/// </summary>
public class NetworkTrafficMonitor
{
    private static readonly List<string> httpMethodsWithBodies = [
        "POST",
        "PUT",
        "PATCH"
    ];
    private readonly BiDiDriver driver;
    private readonly Dictionary<string, NetworkRequest> pendingRequests = new();
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
        AddDataCollectorCommandResult collectorResult = await driver.Network.AddDataCollectorAsync(addCollectorParameters);
        this.bodyCollectorId = collectorResult.CollectorId;

        // Subscribe to the events.
        List<string> subscribedEvents = [
            this.driver.Network.OnBeforeRequestSent.EventName,
            this.driver.Network.OnResponseCompleted.EventName
        ];
        List<string> subscribedContexts = [browsingContextId];
        SubscribeCommandParameters subscribe = new(subscribedEvents, subscribedContexts);
        SubscribeCommandResult subscribeResult = await driver.Session.SubscribeAsync(subscribe);
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
            await driver.Session.UnsubscribeAsync(unsubscribe);
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
        List<Task> requestCompletionTasks = new();
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

        List<Task> responseCompletionTasks = new();
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
        if (httpMethodsWithBodies.Contains(e.Request.Method.ToUpperInvariant()))
        {
            // Only POST, PUT, and PATCH requests should have bodies. Others
            // may, but that is incorrect, so we will ignore capturing it.
            GetDataCommandParameters getBodyParameters = new(requestId)
            {
                CollectorId = this.bodyCollectorId,
                DataType = DataType.Request,
                DisownCollectedData = true,
            };
            requestBodyTask = driver.Network.GetDataAsync(getBodyParameters);
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
        Task<GetDataCommandResult> responseBodyTask = driver.Network.GetDataAsync(getBodyParameters);
        this.pendingRequests[requestId].SetResponseReceived(e.Response, responseBodyTask);
    }

    /// <summary>
    /// Represents a network request and its response.
    /// </summary>
    public class NetworkRequest
    {
        private readonly string requestId;
        private readonly string requestUrl;
        private readonly string requestMethod;
        private readonly List<ReadOnlyHeader> requestHeaders = new List<ReadOnlyHeader>();
        private readonly Task<GetDataCommandResult>? requestBodyRetrieveTask;
        private readonly TaskCompletionSource responseReceivedTaskCompletionSource = new();
        private string requestBody = string.Empty;
        private bool isRequestBodyBase64Encoded = false;
        private ulong responseStatusCode = 0;
        private string responseStatusText = string.Empty;
        private string responseProtocol = string.Empty;
        private List<ReadOnlyHeader> responseHeaders = new List<ReadOnlyHeader>();
        private Task<GetDataCommandResult>? responseBodyAvailableTask;
        private string responseBody = string.Empty;
        private bool isResponseBodyBase64Encoded = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkRequest"/> class.
        /// </summary>
        /// <param name="requestData">The data describing the HTTP request.</param>
        /// <param name="requestBodyRetrieveTask">A <see cref="Task"/> object that will be fulfilled once the request body has been retrieved.</param>
        public NetworkRequest(RequestData requestData, Task<GetDataCommandResult>? requestBodyRetrieveTask = null)
        {
            this.requestId = requestData.RequestId;
            this.requestUrl = requestData.Url;
            this.requestMethod = requestData.Method;
            this.requestHeaders.AddRange(requestData.Headers);
            this.requestBodyRetrieveTask = requestBodyRetrieveTask;
        }

        /// <summary>
        /// Gets the ID of the network request.
        /// </summary>
        public string RequestId => this.requestId;

        /// <summary>
        /// Gets the URL of the network request.
        /// </summary>
        public string Url => this.requestUrl;

        /// <summary>
        /// Asynchronously waits for the request to have received a response.
        /// </summary>
        /// <returns>A task representing information about the asynchronous operation.</returns>
        public Task WaitForResponseReceivedAsync()
        {
            return this.responseReceivedTaskCompletionSource.Task;
        }

        /// <summary>
        /// Asynchronously waits for the request body to be captured.
        /// </summary>
        /// <returns>A task representing information about the asynchronous operation.</returns>
        public async Task WaitForRequestBodyAsync()
        {
            if (this.requestBodyRetrieveTask is not null)
            {
                GetDataCommandResult bodyResult = await this.requestBodyRetrieveTask;
                this.isRequestBodyBase64Encoded = bodyResult.Bytes.Type == BytesValueType.Base64;
                this.requestBody = bodyResult.Bytes.Value;
            }
        }

        /// <summary>
        /// Asynchronously waits for the response body to be captured.
        /// </summary>
        /// <returns>A task representing information about the asynchronous operation.</returns>
        public async Task WaitForResponseBodyAsync()
        {
            if (this.responseBodyAvailableTask is not null)
            {
                GetDataCommandResult bodyResult = await this.responseBodyAvailableTask;
                this.isResponseBodyBase64Encoded = bodyResult.Bytes.Type == BytesValueType.Base64;
                this.responseBody = bodyResult.Bytes.Value;
            }
        }

        /// <summary>
        /// Gets the HTTP request formatted as it would be sent over the wire.
        /// </summary>
        /// <param name="base64EncodedBodyDisplayBehavior">A value indicating how to display base64 encoded binary data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public string GetRequestText(Base64DisplayBehavior base64EncodedBodyDisplayBehavior = Base64DisplayBehavior.NoDisplay)
        {
            StringBuilder requestBuilder = new($"{this.requestMethod} {this.requestUrl}");
            requestBuilder.AppendLine();
            foreach (ReadOnlyHeader header in this.requestHeaders)
            {
                requestBuilder.AppendLine($"{header.Name}: {header.Value.Value}");
            }

            requestBuilder.AppendLine();
            if (!string.IsNullOrEmpty(this.requestBody))
            {
                if (this.isRequestBodyBase64Encoded && base64EncodedBodyDisplayBehavior != Base64DisplayBehavior.Display)
                {
                    if (base64EncodedBodyDisplayBehavior == Base64DisplayBehavior.Decode)
                    {
                        requestBuilder.Append(Convert.FromBase64String(this.requestBody));
                    }
                    else if (base64EncodedBodyDisplayBehavior == Base64DisplayBehavior.NoDisplay)
                    {
                        requestBuilder.AppendLine($"[Base64-encoded binary data (length: {this.requestBody.Length})]");
                    }
                }
                else
                {
                    requestBuilder.AppendLine(this.requestBody);
                }
            }

            return requestBuilder.ToString();
        }

        /// <summary>
        /// Gets the HTTP response formatted as it would be sent over the wire.
        /// </summary>
        /// <param name="base64EncodedBodyDisplayBehavior">A value indicating how to display base64 encoded binary data.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public string GetResponseText(Base64DisplayBehavior base64EncodedBodyDisplayBehavior = Base64DisplayBehavior.NoDisplay)
        {
            StringBuilder responseBuilder = new($"{this.responseProtocol} {this.responseStatusCode} {this.responseStatusText}");
            responseBuilder.AppendLine();
            foreach (ReadOnlyHeader header in this.responseHeaders)
            {
                responseBuilder.AppendLine($"{header.Name}: {header.Value.Value}");
            }

            responseBuilder.AppendLine();
            if (this.isResponseBodyBase64Encoded && base64EncodedBodyDisplayBehavior != Base64DisplayBehavior.Display)
            {
                if (base64EncodedBodyDisplayBehavior == Base64DisplayBehavior.Decode)
                {
                    responseBuilder.Append(Convert.FromBase64String(this.responseBody));
                }
                else if (base64EncodedBodyDisplayBehavior == Base64DisplayBehavior.NoDisplay)
                {
                    responseBuilder.AppendLine($"[Base64-encoded binary data (length: {this.responseBody.Length})]");
                }
            }
            else
            {
                responseBuilder.AppendLine(this.responseBody);
            }

            return responseBuilder.ToString();
        }

        /// <summary>
        /// Sets the response data for the request.
        /// </summary>
        /// <param name="responseData">The data describing the HTTP response.</param>
        /// <param name="responseBodyRetrieveTask">A <see cref="Task"/> object that will be fulfilled once the response body has been retrieved.</param>
        public void SetResponseReceived(ResponseData responseData, Task<GetDataCommandResult>? responseBodyRetrieveTask = null)
        {
            this.responseProtocol = responseData.Protocol;
            this.responseStatusCode = responseData.Status;
            this.responseStatusText = responseData.StatusText;
            this.responseHeaders.AddRange(responseData.Headers);
            this.responseBodyAvailableTask = responseBodyRetrieveTask;

            this.responseReceivedTaskCompletionSource.SetResult();
        }
    }
}
