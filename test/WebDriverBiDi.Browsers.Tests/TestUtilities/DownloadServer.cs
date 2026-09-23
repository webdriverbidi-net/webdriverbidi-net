// <copyright file="DownloadServer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Browsers.TestUtilities;

using System.Collections.Concurrent;
using System.Net;
using System.Text;
using PinchHitter;

/// <summary>
/// A local HTTP server standing in for the browser and driver download services, which
/// records every request it receives. Requests are routed by path; query strings are ignored.
/// </summary>
public sealed class DownloadServer : IAsyncDisposable
{
    private readonly Server server = new();
    private readonly ConcurrentQueue<string> requestedUrls = new();
    private readonly ConcurrentQueue<ReceivedRequest> requests = new();

    private DownloadServer()
    {
    }

    /// <summary>
    /// Gets the path and query of each request received, in order.
    /// </summary>
    public IReadOnlyList<string> RequestedUrls => [.. this.requestedUrls];

    /// <summary>
    /// Gets each request received, in order.
    /// </summary>
    public IReadOnlyList<ReceivedRequest> Requests => [.. this.requests];

    /// <summary>
    /// Starts a new server.
    /// </summary>
    /// <returns>The started server.</returns>
    public static async Task<DownloadServer> StartAsync()
    {
        DownloadServer downloadServer = new();
        await downloadServer.server.StartAsync();
        return downloadServer;
    }

    /// <summary>
    /// Gets the absolute URL of a path on this server.
    /// </summary>
    /// <param name="path">The path, which may include a query string.</param>
    /// <returns>The absolute URL.</returns>
    public Uri UrlFor(string path)
    {
        return new Uri($"http://localhost:{this.server.Port}/{path.TrimStart('/')}");
    }

    /// <summary>
    /// Serves content at a path.
    /// </summary>
    /// <param name="path">The path to serve.</param>
    /// <param name="content">The response body.</param>
    public void AddFile(string path, byte[] content)
    {
        this.server.RegisterHandler(path, new RecordingResourceHandler(content, this.Record));
    }

    /// <summary>
    /// Serves text at a path.
    /// </summary>
    /// <param name="path">The path to serve.</param>
    /// <param name="content">The response body.</param>
    public void AddText(string path, string content)
    {
        this.AddFile(path, Encoding.UTF8.GetBytes(content));
    }

    /// <summary>
    /// Redirects requests for a path.
    /// </summary>
    /// <param name="path">The path to redirect.</param>
    /// <param name="location">The redirect target.</param>
    public void AddRedirect(string path, Uri location)
    {
        this.server.RegisterHandler(path, new RecordingRedirectHandler(location.AbsoluteUri, this.Record));
    }

    /// <summary>
    /// Records requests for a path, and responds to them with 404 Not Found.
    /// </summary>
    /// <param name="path">The path.</param>
    public void AddNotFound(string path)
    {
        this.server.RegisterHandler(path, new RecordingNotFoundHandler(this.Record));
    }

    /// <summary>
    /// Responds to successive requests for a path with successive responses, repeating the last.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="responses">The responses.</param>
    public void AddResponses(string path, params ServedResponse[] responses)
    {
        this.AddResponses(path, HttpRequestMethod.Get, responses);
    }

    /// <summary>
    /// Responds to successive requests with a method for a path with successive responses, repeating the last.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="method">The request method.</param>
    /// <param name="responses">The responses.</param>
    public void AddResponses(string path, HttpRequestMethod method, params ServedResponse[] responses)
    {
        this.server.RegisterHandler(path, method, new ScriptedHandler(responses, this.Record));
    }

    /// <summary>
    /// Counts the requests received for a path, ignoring any query string.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>The number of requests for the path.</returns>
    public int RequestCount(string path)
    {
        return this.requestedUrls.Count(url => url.Split('?')[0] == path);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.server.DisposeAsync();
    }

    private void Record(HttpRequest request)
    {
        Uri? uri = request.Uri;
        string pathAndQuery = uri is null ? string.Empty : uri.IsAbsoluteUri ? uri.PathAndQuery : uri.OriginalString;
        this.requestedUrls.Enqueue(pathAndQuery);
        Dictionary<string, string> headers = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, List<string>> header in request.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        this.requests.Enqueue(new ReceivedRequest(request.Method.ToString().ToUpperInvariant(), pathAndQuery, headers, request.Body));
    }

    private sealed class RecordingResourceHandler(byte[] content, Action<HttpRequest> record) : WebResourceRequestHandler(content)
    {
        protected override Task<HttpResponse> ProcessRequestAsync(HttpRequest request)
        {
            record(request);
            return base.ProcessRequestAsync(request);
        }
    }

    private sealed class RecordingRedirectHandler(string location, Action<HttpRequest> record) : RedirectRequestHandler(location, HttpStatusCode.Found)
    {
        protected override Task<HttpResponse> ProcessRequestAsync(HttpRequest request)
        {
            record(request);
            return base.ProcessRequestAsync(request);
        }
    }

    private sealed class ScriptedHandler(ServedResponse[] responses, Action<HttpRequest> record) : HttpRequestHandler([])
    {
        private int requestCount;

        protected override Task<HttpResponse> ProcessRequestAsync(HttpRequest request)
        {
            record(request);
            ServedResponse served = responses[Math.Min(Interlocked.Increment(ref this.requestCount), responses.Length) - 1];
            this.MimeType = served.ContentType;
            HttpResponse response = this.CreateHttpResponse(request.Id, served.StatusCode);
            response.SetBodyContent(served.Body ?? []);
            this.AddStandardResponseHeaders(response);
            foreach (KeyValuePair<string, string> header in served.Headers ?? new Dictionary<string, string>())
            {
                response.Headers[header.Key] = [header.Value];
            }

            return Task.FromResult(response);
        }
    }

    private sealed class RecordingNotFoundHandler(Action<HttpRequest> record) : NotFoundRequestHandler("Not found")
    {
        protected override Task<HttpResponse> ProcessRequestAsync(HttpRequest request)
        {
            record(request);
            return base.ProcessRequestAsync(request);
        }
    }
}

/// <summary>
/// A response served by <see cref="DownloadServer.AddResponses"/>.
/// </summary>
/// <param name="StatusCode">The status.</param>
/// <param name="Body">The body, or <see langword="null"/> for none.</param>
/// <param name="Headers">Headers added to the response, or <see langword="null"/> for none.</param>
/// <param name="ContentType">The media type of the body.</param>
public sealed record ServedResponse(HttpStatusCode StatusCode, byte[]? Body = null, IReadOnlyDictionary<string, string>? Headers = null, string ContentType = "text/html;charset=utf-8");

/// <summary>
/// A request received by a <see cref="DownloadServer"/>.
/// </summary>
/// <param name="Method">The request method, in upper case.</param>
/// <param name="PathAndQuery">The path and query.</param>
/// <param name="Headers">The request headers.</param>
/// <param name="Body">The request body.</param>
public sealed record ReceivedRequest(string Method, string PathAndQuery, IReadOnlyDictionary<string, string> Headers, string Body);
