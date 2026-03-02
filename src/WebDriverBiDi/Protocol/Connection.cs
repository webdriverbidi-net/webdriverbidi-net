// <copyright file="Connection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end.
/// </summary>
public class Connection : IAsyncDisposable
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private readonly SemaphoreSlim dataSendSemaphore = new(1, 1);
    private Task? dataReceiveTask;
    private ClientWebSocket client = new();
    private CancellationTokenSource clientTokenSource = new();
    private int isDisposedFlag;

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection" /> class.
    /// </summary>
    public Connection()
    {
    }

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public virtual bool IsActive => this.client.State != WebSocketState.None && this.client.State != WebSocketState.Closed && this.client.State != WebSocketState.Aborted;

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
    /// Gets an observable event that notifies when an error occurs on this connection.
    /// </summary>
    public ObservableEvent<ConnectionErrorEventArgs> OnConnectionError { get; } = new("connection.connectionError");

    /// <summary>
    /// Gets an observable event that notifies when a log message is written.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage { get; } = new("connection.logMessage");

    /// <summary>
    /// Gets a value indicating whether this connection has been disposed.
    /// </summary>
    protected bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="url">The URL used to connect to the remote end.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the connection is not established within the startup timeout.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the WebSocket is already connected.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task StartAsync(string url, CancellationToken cancellationToken = default)
    {
        if (this.client.State == WebSocketState.Closed || this.client.State == WebSocketState.Aborted)
        {
            // A ClientWebSocket in a closed or aborted state means that we had
            // a connection at one time that was in use, and is no longer valid.
            // Replace that ClientWebSocket with a new one to allow for reuse of
            // the connection, disposing the old one first.
            this.client.Dispose();
            this.client = new ClientWebSocket();
            this.clientTokenSource.Dispose();
            this.clientTokenSource = new CancellationTokenSource();
        }

        if (this.client.State != WebSocketState.None)
        {
            // Since we've already ruled out closed or aborted sockets in the above
            // code, ClientWebSocket in any state other than none is already connected.
            throw new WebDriverBiDiConnectionException($"The WebSocket is already connected to {this.ConnectedUrl}; call the Stop method to disconnect before calling Start");
        }

        await this.LogAsync($"Opening connection to URL {url}");
        bool connected = false;
        Stopwatch initializationStopwatch = Stopwatch.StartNew();
        while (!connected && initializationStopwatch.Elapsed <= this.StartupTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.clientTokenSource.Token);
                await this.client.ConnectAsync(new Uri(url), linkedTokenSource.Token).ConfigureAwait(false);
                connected = true;
                this.ConnectedUrl = url;
            }
            catch (WebSocketException)
            {
                // If the server-side socket is not yet ready, it leaves the client socket in a closed state,
                // which sees the object as disposed, so we must create a new one to try again. Note that
                // we will also explicitly call Dispose on the object, to make sure resources are disposed.
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
                this.client.Dispose();
                this.client = new ClientWebSocket();
            }
        }

        initializationStopwatch.Stop();
        if (!connected)
        {
            throw new WebDriverBiDiTimeoutException($"Could not connect to remote WebSocket server within {this.StartupTimeout.TotalSeconds} seconds");
        }

        this.dataReceiveTask = Task.Run(async () => await this.ReceiveDataAsync().ConfigureAwait(false));
        await this.LogAsync($"Connection opened");
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.LogAsync($"Closing connection");
        if (this.client.State != WebSocketState.Open)
        {
            await this.LogAsync($"Socket already closed (Socket state: {this.client.State})");
        }
        else
        {
            await this.CloseClientWebSocketAsync(cancellationToken).ConfigureAwait(false);
        }

        // Whether we closed the socket or timed out, we cancel the token causing ReceiveAsync to abort the socket.
        this.clientTokenSource.Cancel();
        if (this.dataReceiveTask is not null)
        {
            await this.dataReceiveTask.ConfigureAwait(false);
        }

        this.ConnectedUrl = string.Empty;
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsyncCore().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously sends data to the remote end of this connection.
    /// </summary>
    /// <param name="data">The data to be sent to the remote end of this connection.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the WebSocket is not active.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when exclusive access to the WebSocket for sending times out.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (!this.IsActive)
        {
            throw new WebDriverBiDiConnectionException("The WebSocket has not been initialized; you must call the Start method before sending data");
        }

        // Only one send operation at a time can be active on a ClientWebSocket instance,
        // so we must synchronize send access to the socket in case multiple threads are
        // attempting to send commands or other data simultaneously.
        if (!await this.dataSendSemaphore.WaitAsync(this.DataTimeout, cancellationToken).ConfigureAwait(false))
        {
            throw new WebDriverBiDiTimeoutException("Timed out waiting to access WebSocket for sending; only one send operation is permitted at a time.");
        }

        try
        {
            if (!this.IsActive)
            {
                throw new WebDriverBiDiConnectionException("The WebSocket connection was closed before the send could be completed");
            }

            if (this.OnLogMessage.CurrentObserverCount > 0)
            {
                await this.LogAsync($"SEND >>> {Encoding.UTF8.GetString(data)}", WebDriverBiDiLogLevel.Debug);
            }

            try
            {
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.clientTokenSource.Token);
                await this.SendWebSocketDataAsync(new ArraySegment<byte>(data), linkedTokenSource.Token).ConfigureAwait(false);
            }
            catch (WebSocketException ex)
            {
                throw new WebDriverBiDiConnectionException($"An error occurred while sending data: {ex.Message}", ex);
            }
        }
        finally
        {
            this.dataSendSemaphore.Release();
        }
    }

    /// <summary>
    /// Asynchronously sends data to the underlying WebSocket of this connection.
    /// </summary>
    /// <param name="messageBuffer">The buffer containing the data to be sent to the remote end of this connection via the WebSocket.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task SendWebSocketDataAsync(ArraySegment<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        await this.client.SendAsync(messageBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously receives data from the underlying WebSocket of this connection.
    /// </summary>
    /// <param name="buffer">The buffer to receive the data into.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the receive result.</returns>
    protected virtual async Task<WebSocketReceiveResult> ReceiveWebSocketDataAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        return await this.client.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// Override this method in derived classes to add custom async cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (this.SetDisposed())
        {
            try
            {
                if (this.IsActive)
                {
                    await this.StopAsync();
                }
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn);
            }

            this.dataSendSemaphore.Dispose();
            this.clientTokenSource.Dispose();
            this.client.Dispose();
        }
    }

    /// <summary>
    /// Marks this <see cref="Connection"/> as disposed. Use this method to ensure
    /// thread-safe operations for setting object being disposed.
    /// </summary>
    /// <returns><see langword="true"/> if the object was not already disposed before calling this method; otherwise, <see langword="false"/>.</returns>
    protected bool SetDisposed()
    {
        return Interlocked.Exchange(ref this.isDisposedFlag, 1) == 0;
    }

    /// <summary>
    /// Asynchronously closes the client WebSocket.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task CloseClientWebSocketAsync(CancellationToken cancellationToken = default)
    {
        // Close the socket first, because ReceiveAsync leaves an invalid socket (state = aborted) when the token is cancelled
        using CancellationTokenSource timeoutTokenSource = new(this.ShutdownTimeout);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
        try
        {
            // After this, the socket state will change to CloseSent
            await this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", linkedTokenSource.Token).ConfigureAwait(false);

            // Wait for the receive loop to process the server's close response, which will transition the
            // socket to the Closed state. If the server does not respond within the shutdown timeout,
            // proceed anyway; StopAsync will cancel the token to abort the socket.
            //
            // The infinite delay is intentional: its sole purpose is to convert the CancellationToken defined
            // earlier in this method into a Task that Task.WhenAny can race against dataReceiveTask. The delay
            // itself never elapses; it completes only when the linked token fires (i.e., ShutdownTimeout expires
            // or the external cancellation token is canceled), which is the desired fallback behavior.
            if (this.dataReceiveTask is not null)
            {
                await Task.WhenAny(this.dataReceiveTask, Task.Delay(Timeout.InfiniteTimeSpan, linkedTokenSource.Token)).ConfigureAwait(false);
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
        MemoryStream? memoryStream = null;
        try
        {
            ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(this.BufferSize, this.BufferSize);
            while (this.client.State != WebSocketState.Closed && !cancellationToken.IsCancellationRequested)
            {
                // Only one receive operation at a time can be active on a ClientWebSocket instance,
                // so we should synchronize receive access to the socket. However, this receive
                // operation is private and should only be accessible by a single thread, that of the
                // Task running this method, so we will forego use of a semaphore to serialize such
                // access. If there is a use case where this could happen, we will resolve it at that
                // time.
                WebSocketReceiveResult receiveResult = await this.ReceiveWebSocketDataAsync(buffer, cancellationToken).ConfigureAwait(false);

                // If the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
                if (!cancellationToken.IsCancellationRequested)
                {
                    // The server is notifying us that the connection will close, and we did
                    // not initiate the close; send acknowledgement
                    if (receiveResult.MessageType == WebSocketMessageType.Close && this.client.State != WebSocketState.Closed && this.client.State != WebSocketState.CloseSent)
                    {
                        await this.LogAsync($"Acknowledging Close frame received from server (client state: {this.client.State})");
                        await this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", cancellationToken).ConfigureAwait(false);
                    }

                    // Display text or binary data
                    if (this.client.State == WebSocketState.Open && receiveResult.MessageType != WebSocketMessageType.Close)
                    {
                        if (!receiveResult.EndOfMessage)
                        {
                            // Intermediate frame of a multi-frame message; accumulate data into a MemoryStream.
                            // Note use of the null-forgiving operator (!) here, as the Array property of tbe buffer
                            // ArraySegment should never be null.
                            memoryStream ??= new MemoryStream();
                            memoryStream.Write(buffer.Array!, 0, receiveResult.Count);
                        }
                        else
                        {
                            byte[] bytes;
                            if (memoryStream is not null)
                            {
                                // Final frame of a multi-frame message; write the last frame and extract the assembled message.
                                // Note use of the null-forgiving operator (!) here, as the Array property of tbe buffer
                                // ArraySegment should never be null.
                                memoryStream.Write(buffer.Array!, 0, receiveResult.Count);
                                bytes = memoryStream.ToArray();
                                memoryStream.Dispose();
                                memoryStream = null;
                            }
                            else
                            {
                                // Single-frame message; take data directly from the buffer.
                                bytes = buffer.AsSpan(0, receiveResult.Count).ToArray();
                            }

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
                    else
                    {
                        // A fragmented message was being assembled, but the socket
                        // is no longer in the Open state or a Close frame arrived.
                        // Discard the partial data to prevent it from corrupting a
                        // subsequent message.
                        memoryStream?.Dispose();
                        memoryStream = null;
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
            await this.OnConnectionError.NotifyObserversAsync(new ConnectionErrorEventArgs(e));
        }
        finally
        {
            memoryStream?.Dispose();
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
