// <copyright file="TestPageServer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.TestUtilities;

using PinchHitter;

/// <summary>
/// A web server for the pages in this project's content directory.
/// </summary>
public sealed class TestPageServer : IAsyncDisposable
{
    // A one-pixel GIF.
    private static readonly byte[] GatedImage = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

    private readonly Server server = new();
    private readonly TaskCompletionSource<bool> gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private TestPageServer()
    {
        foreach (string file in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "content")))
        {
            // A worker's script must be served as JavaScript.
            string mimeType = Path.GetExtension(file) == ".js" ? "text/javascript;charset=utf-8" : "text/html;charset=utf-8";
            this.server.RegisterHandler($"/{Path.GetFileName(file)}", new WebResourceRequestHandler(File.ReadAllBytes(file)) { MimeType = mimeType });
        }

        this.server.RegisterHandler("/gated.gif", new GatedHandler(this.gate.Task));
    }

    /// <summary>
    /// Starts a server.
    /// </summary>
    /// <returns>The started server.</returns>
    public static async Task<TestPageServer> StartAsync()
    {
        TestPageServer pageServer = new();
        await pageServer.server.StartAsync();
        return pageServer;
    }

    /// <summary>
    /// Gets the URL of a page.
    /// </summary>
    /// <param name="page">The page's file name, such as "index.html".</param>
    /// <returns>The URL.</returns>
    public string UrlFor(string page) => $"http://localhost:{this.server.Port}/{page}";

    /// <summary>
    /// Lets requests for "gated.gif", which the server holds until now, complete, so that a page loading it can
    /// finish loading.
    /// </summary>
    public void ReleaseGatedResource()
    {
        this.gate.TrySetResult(true);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        this.ReleaseGatedResource();
        await this.server.StopAsync();
    }

    private sealed class GatedHandler(Task released) : HttpRequestHandler([])
    {
        protected override async Task<HttpResponse> ProcessRequestAsync(HttpRequest request)
        {
            await released;
            this.MimeType = "image/gif";
            HttpResponse response = this.CreateHttpResponse(request.Id, System.Net.HttpStatusCode.OK);
            response.SetBodyContent(GatedImage);
            this.AddStandardResponseHeaders(response);
            return response;
        }
    }
}
