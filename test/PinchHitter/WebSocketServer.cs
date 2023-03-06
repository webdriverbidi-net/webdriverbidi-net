// <copyright file="WebSocketServer.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A simple in-memory web server that can serve have its protocol upgraded from
/// HTTP to using a WebSocket. This server uses a TcpListener instead of an
/// HttpListener so as to avoid the restriction of having to register non-localhost
/// prefixes as an admin on Windows.
/// </summary>
public class WebSocketServer : Server
{
    private WebSocketState state = WebSocketState.None;
    private bool ignoreCloseRequest = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketServer"/> class listening on a random port.
    /// </summary>
    public WebSocketServer()
        : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketServer"/> class listening on a specific port.
    /// </summary>
    /// <param name="port">The port on which to listen. Passing zero (0) for the port will select a random port.</param>
    public WebSocketServer(int port)
        : base(port)
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether the server should ignore requests
    /// from the client to close the WebSocket. This allows simulating servers that
    /// do not properly implement cleanly closing a WebSocket.
    /// </summary>
    public bool IgnoreCloseRequest { get => this.ignoreCloseRequest; set => this.ignoreCloseRequest = value; }

    /// <summary>
    /// Gets a value indicating whether the server should continue listening for incoming connections.
    /// </summary>
    protected override bool ContinueRunning => base.ContinueRunning && this.state != WebSocketState.Closed;

    /// <summary>
    /// Asynchronously sends data to the connected WebSocket client.
    /// </summary>
    /// <param name="data">A string representing the data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task SendData(string data)
    {
        WebSocketFrame frame = WebSocketFrame.Encode(data);
        await this.SendData(frame.Data);
    }

    /// <summary>
    /// Asynchrounously forcibly disconnects the server without following the appropriate shutdown procedure.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Disconnect()
    {
        if (this.HasClientSocket && this.state == WebSocketState.Open)
        {
            await this.SendCloseFrame("Initiating close");
            this.state = WebSocketState.CloseSent;
        }
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
        if (this.state == WebSocketState.None)
        {
            // A WebSocket connection has not yet been established. Treat the
            // incoming data as a standard HTTP request. If the HTTP request
            // is a request to upgrade the connection, we will handle sending
            // the expected response in ProcessHttpRequest.
            string rawRequest = Encoding.UTF8.GetString(buffer, 0, receivedLength);
            HttpRequest request = HttpRequest.Parse(rawRequest);
            HttpResponse response = this.ProcessHttpRequest(request);
            if (request.IsWebSocketHandshakeRequest)
            {
                this.state = WebSocketState.Connecting;
            }

            await this.SendData(response.ToByteArray());

            if (request.IsWebSocketHandshakeRequest)
            {
                this.state = WebSocketState.Open;
            }
        }
        else
        {
            // Note: We do not handle continuation frames (WebSocketOpcodeType.Fragment)
            // in this implementation. Consider it a feature for a future iteration.
            // Likewise, we do not handle non-text frames (WebSocketOpcodeType.Binary)
            // in this implementation.
            // Finally, we do not handle ping and pong frames.
            WebSocketFrame frame = WebSocketFrame.Decode(buffer);
            if (frame.Opcode == WebSocketOpcodeType.Text)
            {
                string text = Encoding.UTF8.GetString(frame.Data);
                this.OnDataReceived(new ServerDataReceivedEventArgs(text));
            }

            if (frame.Opcode == WebSocketOpcodeType.ClosedConnection)
            {
                if (!this.ignoreCloseRequest)
                {
                    this.state = WebSocketState.CloseReceived;
                    await this.SendCloseFrame("Acknowledge close");
                }

                this.state = WebSocketState.Closed;
            }
        }
    }

    private async Task SendCloseFrame(string message)
    {
        WebSocketFrame closeFrame = WebSocketFrame.Encode(message, WebSocketOpcodeType.ClosedConnection);
        await this.SendData(closeFrame.Data);
    }
}