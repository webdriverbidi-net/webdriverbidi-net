// <copyright file="Server.cs" company="PinchHitter Committers">
// Copyright (c) PinchHitter Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace PinchHitter;

using System;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// An abstract base class for a server listening on a port for TCP messages and able
/// to process incoming data received on that port.
/// </summary>
public abstract class Server
{
    private readonly TcpListener listener;
    private readonly CancellationTokenSource listenerCancelationTokenSource = new();
    private readonly List<string> serverLog = new();
    private readonly HttpRequestProcessor httpProcessor = new();
    private Socket? clientSocket;
    private int port = 0;
    private int bufferSize = 1024;
    private bool isStarted = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class listening on a specific port.
    /// </summary>
    /// <param name="port">The port on which to listen. Passing zero (0) for the port will select a random port.</param>
    protected Server(int port)
    {
        this.port = port;
        this.listener = new(new IPEndPoint(IPAddress.Loopback, this.port));
    }

    /// <summary>
    /// Event raised when data is received by the server.
    /// </summary>
    public event EventHandler<ServerDataReceivedEventArgs>? DataReceived;

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
            if (this.isStarted)
            {
                throw new ArgumentException("Cannot set buffer size once server has started listening for requests");
            }

            this.bufferSize = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the server has a current client socket assigned.
    /// </summary>
    protected bool HasClientSocket => this.clientSocket is not null;

    /// <summary>
    /// Gets a value indicating whether the server should continue listening for incoming connections.
    /// </summary>
    protected virtual bool ContinueRunning
    {
        get { return !this.listenerCancelationTokenSource.Token.IsCancellationRequested; }
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

        // this.receiveDataTask = Task.Run(() => this.ReceiveData());
        // this.receiveDataTask.ConfigureAwait(false);
        _ = Task.Run(() => this.ReceiveData()).ConfigureAwait(false);
        this.isStarted = true;
    }

    /// <summary>
    /// Stops the server from listening for incomeing connections.
    /// </summary>
    public void Stop()
    {
        if (this.isStarted)
        {
            this.listenerCancelationTokenSource.Cancel();
            this.listener.Stop();
            this.isStarted = false;
        }
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
    /// Asynchronously sends data to the client requesing data from this server.
    /// </summary>
    /// <param name="data">A byte array representing the data to be sent.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="PinchHitterException">Thrown when there is no client socket connected.</exception>
    protected async Task SendData(byte[] data)
    {
        if (this.clientSocket is null)
        {
            throw new PinchHitterException("No attached client");
        }

        int bytesSent = await this.clientSocket.SendAsync(data, SocketFlags.None);
        this.serverLog.Add($"SEND {bytesSent} bytes");
    }

    /// <summary>
    /// Asynchronously processes incoming data from the client.
    /// </summary>
    /// <param name="buffer">A byte array buffer containing the data.</param>
    /// <param name="receivedLength">The length of the data in the buffer.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected abstract Task ProcessIncomingData(byte[] buffer, int receivedLength);

    /// <summary>
    /// Processes an incoming HTTP request.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <returns>The response for the request.</returns>
    protected HttpResponse ProcessHttpRequest(HttpRequest request)
    {
        return this.httpProcessor.ProcessRequest(request);
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
    /// <param name="e">The WebServerDataReceivedEventArgs object containing information about the DataReceived event.</param>
    protected virtual void OnDataReceived(ServerDataReceivedEventArgs e)
    {
        if (this.DataReceived is not null)
        {
            this.DataReceived(this, e);
        }
    }

    private async Task ReceiveData()
    {
        this.clientSocket = await this.listener.AcceptSocketAsync(this.listenerCancelationTokenSource.Token);
        this.LogMessage("Socket connected");
        while (this.ContinueRunning)
        {
            byte[] buffer = new byte[this.bufferSize];
            using NetworkStream networkStream = new(this.clientSocket);
            using MemoryStream memoryStream = new();
            do
            {
                // Use a NetworkStream to read the data from the socket, then write
                // the received data to a MemoryStream. This allows the server to
                // read requests that exceed the size of the buffer.
                int receivedLength = await networkStream.ReadAsync(buffer, this.listenerCancelationTokenSource.Token);
                await memoryStream.WriteAsync(buffer.AsMemory(0, receivedLength), this.listenerCancelationTokenSource.Token);
            }
            while (networkStream.DataAvailable);

            if (memoryStream.Length > 0)
            {
                // Reset the memory stream position, and copy the data into a buffer
                // suitable for processing.
                int totalReceived = Convert.ToInt32(memoryStream.Length);
                memoryStream.Position = 0;
                byte[] messageBytes = new byte[totalReceived];
                await memoryStream.ReadAsync(messageBytes.AsMemory(0, totalReceived), this.listenerCancelationTokenSource.Token);
                await this.ProcessIncomingData(messageBytes, totalReceived);
            }
        }

        this.clientSocket.Close();
    }
}