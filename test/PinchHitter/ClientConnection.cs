// <copyright file="ClientConnection.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Handles a client connection to the PinchHitter server.
/// </summary>
public class ClientConnection
{
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly string connectionId = Guid.NewGuid().ToString();
    private readonly Socket clientSocket;
    private readonly int bufferSize;
    private readonly HttpRequestProcessor httpProcessor;
    private WebSocketState state = WebSocketState.None;
    private bool ignoreCloseRequest = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnection"/> class.
    /// </summary>
    /// <param name="clientSocket">The Socket used to communicate with the client.</param>
    /// <param name="httpProcessor">An HttpRequestProcessor used to process HTTP requests.</param>
    /// <param name="bufferSize">The size of the buffer used for socket communication.</param>
    public ClientConnection(Socket clientSocket, HttpRequestProcessor httpProcessor, int bufferSize = 1024)
    {
        this.clientSocket = clientSocket;
        this.bufferSize = bufferSize;
        this.httpProcessor = httpProcessor;
    }

    /// <summary>
    /// Event raised when the connection is starting and ready to receive data from the client.
    /// </summary>
    public event EventHandler<ClientConnectionEventArgs>? Starting;

    /// <summary>
    /// Event raised when the connection is stopped and can no longer receive data from the client.
    /// </summary>
    public event EventHandler<ClientConnectionEventArgs>? Stopped;

    /// <summary>
    /// Event raised when data is received from this client connection.
    /// </summary>
    public event EventHandler<ClientConnectionDataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Event raised when messages should be logged from this client connection.
    /// </summary>
    public event EventHandler<ClientConnectionLogMessageEventArgs>? LogMessage;

    /// <summary>
    /// Gets the unique ID of this client connection.
    /// </summary>
    public string ConnectionId => this.connectionId;

    /// <summary>
    /// Gets or sets a value indicating whether this client connection should ignore WebSocket close
    /// requests. Allows the server to simulate malformed clients who do not correctly complete the
    /// WebSocket close handshake.
    /// </summary>
    public bool IgnoreCloseRequest { get => this.ignoreCloseRequest; set => this.ignoreCloseRequest = value; }

    /// <summary>
    /// Starts receiving data on this client connection.
    /// </summary>
    public void StartReceiving()
    {
        _ = Task.Run(() => this.ReceiveData());
    }

    /// <summary>
    /// Stops receiving data on this client connection.
    /// </summary>
    public void StopReceiving()
    {
        this.cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Asynchrounously forcibly disconnects the server without following the appropriate shutdown procedure.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Disconnect()
    {
        if (this.state == WebSocketState.None)
        {
            this.cancellationTokenSource.Cancel();
        }

        if (this.state == WebSocketState.Open)
        {
            await this.SendCloseFrame("Initiating close");
            this.state = WebSocketState.CloseSent;
        }
    }

    /// <summary>
    /// Asynchronously sends data to the client requesing data from this server.
    /// </summary>
    /// <param name="data">A byte array representing the data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="PinchHitterException">Thrown when there is no client socket connected.</exception>
    public async Task SendData(byte[] data)
    {
        int bytesSent = await this.clientSocket.SendAsync(data, SocketFlags.None);
        this.OnLogMessage(new ClientConnectionLogMessageEventArgs($"SEND {bytesSent} bytes"));
    }

    /// <summary>
    /// Raises the DataReceived event.
    /// </summary>
    /// <param name="e">The ClientConnectionDataReceivedEventArgs object containing data about the event.</param>
    protected void OnDataReceived(ClientConnectionDataReceivedEventArgs e)
    {
        if (this.DataReceived is not null)
        {
            this.DataReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the LogMessage event.
    /// </summary>
    /// <param name="e">The ClientConnectionLogMessageEventArgs object containing data about the event.</param>
    protected void OnLogMessage(ClientConnectionLogMessageEventArgs e)
    {
        if (this.LogMessage is not null)
        {
            this.LogMessage(this, e);
        }
    }

    /// <summary>
    /// Raises the Starting event.
    /// </summary>
    /// <param name="e">The ClientConnectionEventArgs object containing data about the event.</param>
    protected void OnStarting(ClientConnectionEventArgs e)
    {
        if (this.Starting is not null)
        {
            this.Starting(this, e);
        }
    }

    /// <summary>
    /// Raises the Stopped event.
    /// </summary>
    /// <param name="e">The ClientConnectionEventArgs object containing data about the event.</param>
    protected void OnStopped(ClientConnectionEventArgs e)
    {
        if (this.Stopped is not null)
        {
            this.Stopped(this, e);
        }
    }

    private async Task ReceiveData()
    {
        this.OnStarting(new ClientConnectionEventArgs(this.connectionId));
        try
        {
            while (this.state != WebSocketState.Closed)
            {
                byte[] buffer = new byte[this.bufferSize];
                using NetworkStream networkStream = new(this.clientSocket);
                using MemoryStream memoryStream = new();
                do
                {
                    // Use a NetworkStream to read the data from the socket, then write
                    // the received data to a MemoryStream. This allows the server to
                    // read requests that exceed the size of the buffer.
                    int receivedLength = await networkStream.ReadAsync(buffer, this.cancellationTokenSource.Token);
                    await memoryStream.WriteAsync(buffer.AsMemory(0, receivedLength), this.cancellationTokenSource.Token);
                }
                while (networkStream.DataAvailable);

                if (memoryStream.Length > 0)
                {
                    // Reset the memory stream position, and copy the data into a buffer
                    // suitable for processing.
                    int totalReceived = Convert.ToInt32(memoryStream.Length);
                    memoryStream.Position = 0;
                    byte[] messageBytes = new byte[totalReceived];
                    await memoryStream.ReadAsync(messageBytes.AsMemory(0, totalReceived), this.cancellationTokenSource.Token);
                    await this.ProcessIncomingData(messageBytes, totalReceived);
                }
            }
        }
        finally
        {
            this.clientSocket.Close();
            this.OnStopped(new ClientConnectionEventArgs(this.connectionId));
        }
    }

    /// <summary>
    /// Asynchronously processes incoming data from the client.
    /// </summary>
    /// <param name="buffer">A byte array buffer containing the data.</param>
    /// <param name="receivedLength">The length of the data in the buffer.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    private async Task ProcessIncomingData(byte[] buffer, int receivedLength)
    {
        this.OnLogMessage(new ClientConnectionLogMessageEventArgs($"RECV {receivedLength} bytes"));
        if (this.state == WebSocketState.None)
        {
            // A WebSocket connection has not yet been established. Treat the
            // incoming data as a standard HTTP request. If the HTTP request
            // is a request to upgrade the connection, we will handle sending
            // the expected response in ProcessHttpRequest.
            string rawRequest = Encoding.UTF8.GetString(buffer, 0, receivedLength);
            this.OnDataReceived(new ClientConnectionDataReceivedEventArgs(this.connectionId, rawRequest));
            HttpRequest request = HttpRequest.Parse(rawRequest);
            HttpResponse response = this.httpProcessor.ProcessRequest(request);
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
                this.OnDataReceived(new ClientConnectionDataReceivedEventArgs(this.connectionId, text));
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