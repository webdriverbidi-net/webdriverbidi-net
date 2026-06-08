// <copyright file="NetworkRequest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using System.Text;
using WebDriverBiDi.Network;

/// <summary>
/// Represents a network request and its response.
/// </summary>
public class NetworkRequest
{
    private static readonly List<string> HttpMethodsWithBodies = [
        "POST",
        "PUT",
        "PATCH"
    ];

    private readonly string requestId;
    private readonly string requestUrl;
    private readonly string requestMethod;
    private readonly List<ReadOnlyHeader> requestHeaders = [];
    private readonly List<Cookie> requestCookies = [];
    private readonly List<ReadOnlyHeader> responseHeaders = [];
    private readonly Task<GetDataCommandResult>? requestBodyRetrieveTask;
    private readonly TaskCompletionSource<bool> responseReceivedTaskCompletionSource = new();
    private readonly DateTime startedDateTime;
    private readonly FetchTimingInfo timings;
    private readonly ulong? requestHeadersSize;
    private readonly ulong? requestBodySize;
    private string requestBody = string.Empty;
    private bool isRequestBodyBase64Encoded = false;
    private ulong responseStatusCode = 0;
    private string responseStatusText = string.Empty;
    private string responseProtocol = string.Empty;
    private string responseMimeType = string.Empty;
    private ulong? responseHeadersSize;
    private ulong? responseBodySize;
    private ulong responseContentSize;
    private Task<GetDataCommandResult>? responseBodyAvailableTask;
    private string responseBody = string.Empty;
    private bool isResponseBodyBase64Encoded = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkRequest"/> class.
    /// </summary>
    /// <param name="requestData">The data describing the HTTP request.</param>
    /// <param name="startedDateTime">The date and time the request was initiated.</param>
    /// <param name="requestBodyRetrieveTask">A <see cref="Task"/> object that will be fulfilled once the request body has been retrieved.</param>
    public NetworkRequest(RequestData requestData, DateTime startedDateTime, Task<GetDataCommandResult>? requestBodyRetrieveTask = null)
    {
        this.requestId = requestData.RequestId;
        this.requestUrl = requestData.Url;
        this.requestMethod = requestData.Method;
        this.requestHeaders.AddRange(requestData.Headers);
        this.requestCookies.AddRange(requestData.Cookies);
        this.requestHeadersSize = requestData.HeadersSize;
        this.requestBodySize = requestData.BodySize;
        this.timings = requestData.Timings;
        this.startedDateTime = startedDateTime;
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
    /// Gets the HTTP method of the network request.
    /// </summary>
    public string Method => this.requestMethod;

    /// <summary>
    /// Gets the date and time the request was started.
    /// </summary>
    public DateTime StartedDateTime => this.startedDateTime;

    /// <summary>
    /// Gets the fetch timing info for the request.
    /// </summary>
    public FetchTimingInfo Timings => this.timings;

    /// <summary>
    /// Gets the request headers.
    /// </summary>
    public IReadOnlyList<ReadOnlyHeader> RequestHeaders => this.requestHeaders.AsReadOnly();

    /// <summary>
    /// Gets the request cookies.
    /// </summary>
    public IReadOnlyList<Cookie> RequestCookies => this.requestCookies.AsReadOnly();

    /// <summary>
    /// Gets the size in bytes of the request headers, or <see langword="null"/> if not available.
    /// </summary>
    public ulong? RequestHeadersSize => this.requestHeadersSize;

    /// <summary>
    /// Gets the size in bytes of the request body, or <see langword="null"/> if not available.
    /// </summary>
    public ulong? RequestBodySize => this.requestBodySize;

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public ulong ResponseStatusCode => this.responseStatusCode;

    /// <summary>
    /// Gets the HTTP status text of the response.
    /// </summary>
    public string ResponseStatusText => this.responseStatusText;

    /// <summary>
    /// Gets the HTTP protocol of the response.
    /// </summary>
    public string ResponseProtocol => this.responseProtocol;

    /// <summary>
    /// Gets the MIME type of the response.
    /// </summary>
    public string ResponseMimeType => this.responseMimeType;

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    public IReadOnlyList<ReadOnlyHeader> ResponseHeaders => this.responseHeaders.AsReadOnly();

    /// <summary>
    /// Gets the size in bytes of the response headers, or <see langword="null"/> if not available.
    /// </summary>
    public ulong? ResponseHeadersSize => this.responseHeadersSize;

    /// <summary>
    /// Gets the size in bytes of the response body, or <see langword="null"/> if not available.
    /// </summary>
    public ulong? ResponseBodySize => this.responseBodySize;

    /// <summary>
    /// Gets the decoded size in bytes of the response content body.
    /// </summary>
    public ulong ResponseContentSize => this.responseContentSize;

    /// <summary>
    /// Gets the body text of the request after it has been retrieved.
    /// </summary>
    public string RequestBody => this.requestBody;

    /// <summary>
    /// Gets a value indicating whether the request body is base64 encoded.
    /// </summary>
    public bool IsRequestBodyBase64Encoded => this.isRequestBodyBase64Encoded;

    /// <summary>
    /// Gets the body text of the response after it has been retrieved.
    /// </summary>
    public string ResponseBody => this.responseBody;

    /// <summary>
    /// Gets a value indicating whether the response body is base64 encoded.
    /// </summary>
    public bool IsResponseBodyBase64Encoded => this.isResponseBodyBase64Encoded;

    /// <summary>
    /// Asynchronously waits for the request to have received a response.
    /// </summary>
    /// <returns>A task representing information about the asynchronous operation.</returns>
    public Task<bool> WaitForResponseReceivedAsync()
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
    /// <returns>The request formatted as HTTP text.</returns>
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
                    requestBuilder.Append(Encoding.UTF8.GetString(Convert.FromBase64String(this.requestBody)));
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
    /// <returns>The response formatted as HTTP text.</returns>
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
                responseBuilder.Append(Encoding.UTF8.GetString(Convert.FromBase64String(this.responseBody)));
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
        this.responseMimeType = responseData.MimeType;
        this.responseHeaders.AddRange(responseData.Headers);
        this.responseHeadersSize = responseData.HeadersSize;
        this.responseBodySize = responseData.BodySize;
        this.responseContentSize = responseData.Content.Size;
        this.responseBodyAvailableTask = responseBodyRetrieveTask;

        this.responseReceivedTaskCompletionSource.SetResult(true);
    }

    /// <summary>
    /// Returns whether this request method may carry a body.
    /// </summary>
    /// <param name="method">The HTTP method to check.</param>
    /// <returns><see langword="true"/> if the HTTP method may carry a request body.</returns>
    internal static bool MethodMayHaveBody(string method) =>
        HttpMethodsWithBodies.Contains(method.ToUpperInvariant());
}
