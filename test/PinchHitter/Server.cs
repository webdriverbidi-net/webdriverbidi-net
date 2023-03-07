// <copyright file="Server.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// An abstract base class for a server listening on a port for TCP messages and able
/// to process incoming data received on that port.
/// </summary>
public class Server
{
    private readonly ConcurrentDictionary<string, ClientConnection> activeConnections = new();
    private readonly TcpListener listener;
    private readonly CancellationTokenSource listenerCancelationTokenSource = new();
    private readonly List<string> serverLog = new();
    private readonly HttpRequestProcessor httpProcessor = new();
    private int port = 0;
    private int bufferSize = 1024;
    private bool isAcceptingConnections = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    public Server()
        : this(0)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class listening on a specific port.
    /// </summary>
    /// <param name="port">The port on which to listen. Passing zero (0) for the port will select a random port.</param>
    public Server(int port)
    {
        this.port = port;
        this.listener = new(new IPEndPoint(IPAddress.Loopback, this.port));
    }

    /// <summary>
    /// Event raised when data is received by the server.
    /// </summary>
    public event EventHandler<ServerDataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// Event raised when a client connects to the server.
    /// </summary>
    public event EventHandler<ClientConnectionEventArgs>? ClientConnected;

    /// <summary>
    /// Event raised when a client disconnects from the server.
    /// </summary>
    public event EventHandler<ClientConnectionEventArgs>? ClientDisconnected;

    /// <summary>
    /// Gets the port on which the server is listening for connections.
    /// </summary>
    public int Port => this.port;

    /// <summary>
    /// Gets the read-only communication log of the server.
    /// </summary>
    public IList<string> Log => this.serverLog.AsReadOnly();

    /// <summary>
    /// Gets or sets the size in bytes of the buffer for receiving incoming requests.
    /// Defaults to 1024 bytes. Cannot be set once the server has started listening.
    /// </summary>
    public int BufferSize
    {
        get
        {
            return this.bufferSize;
        }

        set
        {
            if (this.isAcceptingConnections)
            {
                throw new ArgumentException("Cannot set buffer size once server has started listening for requests");
            }

            this.bufferSize = value;
        }
    }

    /// <summary>
    /// Starts the server listening for incoming connections.
    /// </summary>
    public void Start()
    {
        this.listener.Start();
        IPEndPoint? localEndpoint = this.listener.LocalEndpoint as IPEndPoint;
        if (localEndpoint is not null)
        {
            this.port = localEndpoint.Port;
        }

        _ = Task.Run(() => this.AcceptConnections()).ConfigureAwait(false);
        this.isAcceptingConnections = true;
    }

    /// <summary>
    /// Stops the server from listening for incomeing connections.
    /// </summary>
    public void Stop()
    {
        if (this.isAcceptingConnections)
        {
            // Stop accepting connections, so that the population of the
            // dictionary of connections is stable.
            this.isAcceptingConnections = false;
            foreach (KeyValuePair<string, ClientConnection> pair in this.activeConnections)
            {
                pair.Value.StopReceiving();
            }

            this.listenerCancelationTokenSource.Cancel();
            this.listener.Stop();
        }
    }

    /// <summary>
    /// Asynchrounously forcibly disconnects the server without following the appropriate shutdown procedure.
    /// </summary>
    /// <param name="connectionId">The ID of the client connection to disconnect.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task Disconnect(string connectionId)
    {
        if (!this.activeConnections.TryGetValue(connectionId, out ClientConnection? connection))
        {
            throw new PinchHitterException($"Unknown connecxtion ID {connectionId}");
        }

        await connection.Disconnect();
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
    /// Asynchronously sends data to the client connected via this client connection.
    /// </summary>
    /// <param name="connectionId">The ID of the client connection to send data to.</param>
    /// <param name="data">The data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public async Task SendData(string connectionId, string data)
    {
        WebSocketFrame frame = WebSocketFrame.Encode(data, WebSocketOpcodeType.Text);
        await this.SendData(connectionId, frame.Data);
    }

    /// <summary>
    /// Sets a value indicating whether the client connection should ignore requests
    /// from the client to close the WebSocket. This allows simulating servers that
    /// do not properly implement cleanly closing a WebSocket.
    /// </summary>
    /// <param name="connectionId">The ID of the connection for which to set the close request behavior.</param>
    /// <param name="ignoreCloseConnectionRequest"><see langword="true"/> to have the client connection ignore close requests; otherwise, <see langword="false"/>.</param>
    /// <exception cref="PinchHitterException">Thrown when an invalid connection ID is specified.</exception>
    public void IgnoreCloseConnectionRequest(string connectionId, bool ignoreCloseConnectionRequest)
    {
        if (!this.activeConnections.TryGetValue(connectionId, out ClientConnection? connection))
        {
            throw new PinchHitterException($"Unknown connecxtion ID {connectionId}");
        }

        connection.IgnoreCloseRequest = ignoreCloseConnectionRequest;
    }

    /// <summary>
    /// Asynchronously sends data to the client requesing data from this server.
    /// </summary>
    /// <param name="connectionId">The ID of the client connection to send data to.</param>
    /// <param name="data">A byte array representing the data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="PinchHitterException">Thrown when an invalid connection ID is specified.</exception>
    protected async Task SendData(string connectionId, byte[] data)
    {
        if (!this.activeConnections.TryGetValue(connectionId, out ClientConnection? connection))
        {
            throw new PinchHitterException($"Unknown connecxtion ID {connectionId}");
        }

        await connection.SendData(data);
    }

    /// <summary>
    /// Adds a message to the server log.
    /// </summary>
    /// <param name="message">The message to add.</param>
    protected void LogMessage(string message)
    {
        this.serverLog.Add(message);
    }

    /// <summary>
    /// Raises the DataReceived event.
    /// </summary>
    /// <param name="e">The ServerDataReceivedEventArgs object containing information about the event.</param>
    protected virtual void OnDataReceived(ServerDataReceivedEventArgs e)
    {
        if (this.DataReceived is not null)
        {
            this.DataReceived(this, e);
        }
    }

    /// <summary>
    /// Raises the ClientConnected event.
    /// </summary>
    /// <param name="e">The ClientConnectionEventArgs object containing information about the event.</param>
    protected virtual void OnClientConnected(ClientConnectionEventArgs e)
    {
        if (this.ClientConnected is not null)
        {
            this.ClientConnected(this, e);
        }
    }

    /// <summary>
    /// Raises the ClientDisconnected event.
    /// </summary>
    /// <param name="e">The ClientConnectionEventArgs object containing information about the event.</param>
    protected virtual void OnClientDisconnected(ClientConnectionEventArgs e)
    {
        if (this.ClientDisconnected is not null)
        {
            this.ClientDisconnected(this, e);
        }
    }

    private async Task AcceptConnections()
    {
        this.isAcceptingConnections = true;
        while (true)
        {
            Socket socket = await this.listener.AcceptSocketAsync(this.listenerCancelationTokenSource.Token);
            if (this.isAcceptingConnections)
            {
                ClientConnection clientConnection = new(socket, this.httpProcessor, this.bufferSize);
                clientConnection.DataReceived += (sender, e) =>
                {
                    this.OnDataReceived(new ServerDataReceivedEventArgs(e.ConnectionId, e.DataReceived));
                };
                clientConnection.LogMessage += (sender, e) =>
                {
                this.LogMessage(e.Message);
                };
                clientConnection.Starting += (sender, e) =>
                {
                    this.OnClientConnectionStarting(clientConnection);
                };
                clientConnection.Stopped += (sender, e) =>
                {
                    this.OnClientConnectionStopped(e.ConnectionId);
                };
                clientConnection.StartReceiving();
                this.LogMessage("Client connected");
            }
        }
    }

    private void OnClientConnectionStarting(ClientConnection connection)
    {
        this.activeConnections.TryAdd(connection.ConnectionId, connection);
        this.OnClientConnected(new ClientConnectionEventArgs(connection.ConnectionId));
    }

    private void OnClientConnectionStopped(string connectionId)
    {
        this.activeConnections.TryRemove(connectionId, out ClientConnection _);
        this.OnClientDisconnected(new ClientConnectionEventArgs(connectionId));
    }
}