// <copyright file="TestPageServer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Automation.TestUtilities;

using PinchHitter;

/// <summary>
/// A web server for the pages in this project's content directory.
/// </summary>
public sealed class TestPageServer : IAsyncDisposable
{
    private readonly Server server = new();

    private TestPageServer()
    {
        foreach (string file in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "content")))
        {
            this.server.RegisterHandler($"/{Path.GetFileName(file)}", new WebResourceRequestHandler(File.ReadAllBytes(file)) { MimeType = "text/html;charset=utf-8" });
        }
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

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.server.StopAsync();
    }
}
