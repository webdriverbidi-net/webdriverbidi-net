// <copyright file="WebServer.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Text;

/// <summary>
/// A simple in-memory web server that can serve content registered to a specific URL.
/// This server uses a TcpListener instead of an HttpListener so as to avoid the
/// restriction of having to register non-localhost prefixes as an admin on Windows.
/// </summary>
public class WebServer : Server
{
    private readonly HttpRequestProcessor httpProcessor = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebServer"/> class listening on a random port.
    /// </summary>
    public WebServer()
        : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebServer"/> class listening on a specific port.
    /// </summary>
    /// <param name="port">The port on which to listen. Passing zero (0) for the port will select a random port.</param>
    public WebServer(int port)
        : base(port)
    {
    }

    /// <summary>
    /// Registers a resource with this web server to be returned when requested.
    /// </summary>
    /// <param name="url">The relative URL associated with this resource.</param>
    /// <param name="resource">The web resource to return when requested.</param>
    public void RegisterResource(string url, WebResource resource)
    {
        this.httpProcessor.RegisterResource(url, resource);
    }

    /// <summary>
    /// Asynchronously processes incoming data from the client.
    /// </summary>
    /// <param name="buffer">A byte array buffer containing the data.</param>
    /// <param name="receivedLength">The length of the data in the buffer.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ProcessIncomingData(byte[] buffer, int receivedLength)
    {
        this.LogMessage($"RECV {receivedLength} bytes");
        string rawRequest = Encoding.UTF8.GetString(buffer, 0, receivedLength);
        this.OnDataReceived(new ServerDataReceivedEventArgs(rawRequest));
        HttpRequest request = HttpRequest.Parse(rawRequest);
        HttpResponse responseData = this.httpProcessor.ProcessRequest(request);
        await this.SendData(responseData.ToByteArray());
    }
}