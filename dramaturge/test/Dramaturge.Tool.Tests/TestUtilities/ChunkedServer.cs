// <copyright file="ChunkedServer.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Dramaturge.Tool.TestUtilities;

using System.Net;
using System.Net.Sockets;

/// <summary>
/// Serves one file over HTTP in chunks, without a Content-Length, so its size is unknown until it has been read.
/// </summary>
public sealed class ChunkedServer : IDisposable
{
    private readonly HttpListener listener = new();
    private readonly byte[] content;
    private readonly Task serving;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkedServer"/> class, and starts serving.
    /// </summary>
    /// <param name="content">The file to serve at every path.</param>
    public ChunkedServer(byte[] content)
    {
        this.content = content;
        using TcpListener portFinder = new(IPAddress.Loopback, 0);
        portFinder.Start();
        int port = ((IPEndPoint)portFinder.LocalEndpoint).Port;
        portFinder.Stop();
        this.Url = new Uri($"http://127.0.0.1:{port}/build");
        this.listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        this.listener.Start();
        this.serving = Task.Run(this.ServeAsync);
    }

    /// <summary>
    /// Gets the URL of the file.
    /// </summary>
    public Uri Url { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.listener.Close();
    }

    private async Task ServeAsync()
    {
        while (this.listener.IsListening)
        {
            HttpListenerContext context;
            try
            {
                context = await this.listener.GetContextAsync();
            }
            catch (Exception ex) when (ex is HttpListenerException || ex is ObjectDisposedException)
            {
                return;
            }

            context.Response.SendChunked = true;
            await context.Response.OutputStream.WriteAsync(this.content);
            context.Response.Close();
        }
    }
}
