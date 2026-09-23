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

    private DownloadServer()
    {
    }

    /// <summary>
    /// Gets the path and query of each request received, in order.
    /// </summary>
    public IReadOnlyList<string> RequestedUrls => [.. this.requestedUrls];

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
        this.requestedUrls.Enqueue(uri is null ? string.Empty : uri.IsAbsoluteUri ? uri.PathAndQuery : uri.OriginalString);
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

    private sealed class RecordingNotFoundHandler(Action<HttpRequest> record) : NotFoundRequestHandler("Not found")
    {
        protected override Task<HttpResponse> ProcessRequestAsync(HttpRequest request)
        {
            record(request);
            return base.ProcessRequestAsync(request);
        }
    }
}
