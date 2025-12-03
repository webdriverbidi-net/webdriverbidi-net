// <copyright file="NetworkRequest.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Client.Network;

using System.Text;
using System.Threading.Tasks;
using WebDriverBiDi.Network;

/// <summary>
/// Represents a network request and its response.
/// </summary>
public class NetworkRequest
{
    private readonly string requestId;
    private readonly string requestUrl;
    private readonly string requestMethod;
    private readonly List<ReadOnlyHeader> requestHeaders = [];
    private readonly Task<GetDataCommandResult>? requestBodyRetrieveTask;
    private readonly TaskCompletionSource<bool> responseReceivedTaskCompletionSource = new();
    private string requestBody = string.Empty;
    private bool isRequestBodyBase64Encoded = false;
    private ulong responseStatusCode = 0;
    private string responseStatusText = string.Empty;
    private string responseProtocol = string.Empty;
    private List<ReadOnlyHeader> responseHeaders = [];
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

        this.responseReceivedTaskCompletionSource.SetResult(true);
    }
}
