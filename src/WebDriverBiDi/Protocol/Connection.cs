// <copyright file="Connection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System;
using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end.
/// </summary>
public class Connection
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private readonly SemaphoreSlim dataSendSemaphore = new(1, 1);
    private Task? dataReceiveTask;
    private ClientWebSocket client = new();
    private CancellationTokenSource clientTokenSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection" /> class.
    /// </summary>
    public Connection()
    {
    }

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public bool IsActive => this.client.State != WebSocketState.None && this.client.State != WebSocketState.Closed && this.client.State != WebSocketState.Aborted;

    /// <summary>
    /// Gets the buffer size for communication used by this connection.
    /// </summary>
    public int BufferSize { get; } = Convert.ToInt32(Math.Pow(2, 20));

    /// <summary>
    /// Gets or sets the WebSocket URL to which the connection is connected.
    /// </summary>
    public string ConnectedUrl { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the timeout to wait before throwing an error when starting up the connection.
    /// </summary>
    public TimeSpan StartupTimeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets or sets the value of the timeout to wait before throwing an error when shutting down the connection.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets or sets the value of the timeout to wait for exclusive access when sending to or receiving data from the ClientWebSocket.
    /// </summary>
    public TimeSpan DataTimeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Gets an observable event that notifies when data is received from this connection.
    /// </summary>
    public ObservableEvent<ConnectionDataReceivedEventArgs> OnDataReceived { get; } = new("connection.dataReceived");

    /// <summary>
    /// Gets an observable event that notifies when a log message is written.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new("connection.logMessage");

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="url">The URL used to connect to the remote end.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="TimeoutException">Thrown when the connection is not established within the startup timeout.</exception>
    public virtual async Task StartAsync(string url)
    {
        if (this.client.State == WebSocketState.Closed || this.client.State == WebSocketState.Aborted)
        {
            // A ClientWebSocket in a closed or aborted state means that we had
            // a connection at one time that was in use, and is no longer valid.
            // replace that ClientWebSocket with a new one to allow for reuse of
            // the connection.
            this.client = new ClientWebSocket();
            this.clientTokenSource = new CancellationTokenSource();
        }

        if (this.client.State != WebSocketState.None)
        {
            // Since we've already ruled out closed or aborted sockets in the above
            // code, ClientWebSocket in any state other than none is already connected.
            throw new WebDriverBiDiException($"The WebSocket is already connected to {this.ConnectedUrl}; call the Stop method to disconnect before calling Start");
        }

        await this.LogAsync($"Opening connection to URL {url}");
        bool connected = false;
        DateTime timeout = DateTime.Now.Add(this.StartupTimeout);
        while (!connected && DateTime.Now <= timeout)
        {
            try
            {
                await this.client.ConnectAsync(new Uri(url), this.clientTokenSource.Token).ConfigureAwait(false);
                connected = true;
                this.ConnectedUrl = url;
            }
            catch (WebSocketException)
            {
                // If the server-side socket is not yet ready, it leaves the client socket in a closed state,
                // which sees the object as disposed, so we must create a new one to try again
                await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                this.client = new ClientWebSocket();
            }
        }

        if (!connected)
        {
            throw new TimeoutException($"Could not connect to remote WebSocket server within {this.StartupTimeout.TotalSeconds} seconds");
        }

        this.dataReceiveTask = Task.Run(async () => await this.ReceiveDataAsync().ConfigureAwait(false));
        await this.LogAsync($"Connection opened");
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task StopAsync()
    {
        await this.LogAsync($"Closing connection");
        if (this.client.State != WebSocketState.Open)
        {
            await this.LogAsync($"Socket already closed (Socket state: {this.client.State})");
        }
        else
        {
            await this.CloseClientWebSocketAsync().ConfigureAwait(false);
        }

        // Whether we closed the socket or timed out, we cancel the token causing ReceiveAsync to abort the socket.
        // The finally block at the end of the processing loop will dispose of the ClientWebSocket object.
        this.clientTokenSource.Cancel();
        this.dataReceiveTask?.Wait();
        this.ConnectedUrl = string.Empty;
    }

    /// <summary>
    /// Asynchronously sends data to the remote end of this connection.
    /// </summary>
    /// <param name="data">The data to be sent to the remote end of this connection.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task SendDataAsync(byte[] data)
    {
        if (!this.IsActive)
        {
            throw new WebDriverBiDiException("The WebSocket has not been initialized; you must call the Start method before sending data");
        }

        // Only one send operation at a time can be active on a ClientWebSocket instance,
        // so we must synchronize send access to the socket in case multiple threads are
        // attempting to send commands or other data simultaneously.
        if (!await this.dataSendSemaphore.WaitAsync(this.DataTimeout).ConfigureAwait(false))
        {
            throw new WebDriverBiDiException("Timed out waiting to access WebSocket for sending; only one send operation is permitted at a time.");
        }

        if (this.OnLogMessage.CurrentObserverCount > 0)
        {
            await this.LogAsync($"SEND >>> {Encoding.UTF8.GetString(data)}", WebDriverBiDiLogLevel.Debug);
        }

        await this.SendWebSocketDataAsync(new ArraySegment<byte>(data)).ConfigureAwait(false);
        this.dataSendSemaphore.Release();
    }

    /// <summary>
    /// Asynchronously sends data to the underlying WebSocket of this connection.
    /// </summary>
    /// <param name="messageBuffer">The buffer containing the data to be sent to the remote end of this connection via the WebSocket.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task SendWebSocketDataAsync(ArraySegment<byte> messageBuffer)
    {
        await this.client.SendAsync(messageBuffer, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously closes the client WebSocket.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task CloseClientWebSocketAsync()
    {
        // Close the socket first, because ReceiveAsync leaves an invalid socket (state = aborted) when the token is cancelled
        CancellationTokenSource timeout = new(this.ShutdownTimeout);
        try
        {
            // After this, the socket state which change to CloseSent
            await this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token).ConfigureAwait(false);

            // Now we wait for the server response, which will close the socket
            while (this.client.State != WebSocketState.Closed && this.client.State != WebSocketState.Aborted && !timeout.Token.IsCancellationRequested)
            {
                // The loop may be too tight for the cancellation token to get triggered, so add a small delay
                await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
            }

            await this.LogAsync($"Client state is {this.client.State}");
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it
        }
    }

    private async Task ReceiveDataAsync()
    {
        CancellationToken cancellationToken = this.clientTokenSource.Token;
        try
        {
            MemoryStream? memoryStream = null;
            ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(this.BufferSize, this.BufferSize);
            while (this.client.State != WebSocketState.Closed && !cancellationToken.IsCancellationRequested)
            {
                // Only one receive operation at a time can be active on a ClientWebSocket instance,
                // so we should synchronize receive access to the socket. However, this receive
                // operation is private and should only be accessible by a single thread, that of the
                // Task running this method, so we will forego use of a semaphore to serialize such
                // access. If there is a use case where this could happen, we will resolve it at that
                // time.
                WebSocketReceiveResult receiveResult = await this.client.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                // If the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
                if (!cancellationToken.IsCancellationRequested)
                {
                    // The server is notifying us that the connection will close, and we did
                    // not initiate the close; send acknowledgement
                    if (receiveResult.MessageType == WebSocketMessageType.Close && this.client.State != WebSocketState.Closed && this.client.State != WebSocketState.CloseSent)
                    {
                        await this.LogAsync($"Acknowledging Close frame received from server (client state: {this.client.State})");
                        await this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", CancellationToken.None).ConfigureAwait(false);
                    }

                    // Display text or binary data
                    if (this.client.State == WebSocketState.Open && receiveResult.MessageType != WebSocketMessageType.Close)
                    {
                        if (memoryStream is null && !receiveResult.EndOfMessage)
                        {
                            memoryStream = new MemoryStream();
                        }

                        // Note: Use the null-forgiving operator here, as the Array
                        // property of the buffer ArraySegment should never be null.
                        memoryStream?.Write(buffer.Array!, 0, receiveResult.Count);

                        if (receiveResult.EndOfMessage)
                        {
                            byte[] bytes = memoryStream is null ? buffer.AsSpan(0, receiveResult.Count).ToArray() : memoryStream.ToArray();
                            memoryStream = null;
                            if (bytes.Length > 0)
                            {
                                if (this.OnLogMessage.CurrentObserverCount > 0)
                                {
                                    await this.LogAsync($"RECV <<< {Encoding.UTF8.GetString(bytes)}", WebDriverBiDiLogLevel.Debug);
                                }

                                await this.OnDataReceived.NotifyObserversAsync(new ConnectionDataReceivedEventArgs(bytes));
                            }
                        }
                    }
                }
            }

            await this.LogAsync($"Ending processing loop in state {this.client.State}");
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it
        }
        catch (WebSocketException e)
        {
            await this.LogAsync($"Unexpected error during receive of data: {e.Message}");
        }
        finally
        {
            this.client.Dispose();
        }
    }

    private async Task LogAsync(string message)
    {
        await this.LogAsync(message, WebDriverBiDiLogLevel.Info);
    }

    private async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, level, "Connection"));
    }
}
