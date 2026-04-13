// <copyright file="WebSocketConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end over a WebSocket.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="WebSocketConnection"/> is the standard and recommended transport mechanism for WebDriver BiDi.
/// It uses the <see cref="System.Net.WebSockets.ClientWebSocket"/> class to communicate with the browser
/// over the WebSocket protocol (ws:// or wss:// schemes).
/// </para>
/// <para>
/// <strong>When to use WebSocket connections:</strong>
/// <list type="bullet">
/// <item><description>All standard automation scenarios (local or remote browsers)</description></item>
/// <item><description>Containerized browser environments</description></item>
/// <item><description>Cross-machine browser debugging</description></item>
/// <item><description>Any browser supporting WebDriver BiDi (Chrome, Edge, Firefox)</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Key characteristics:</strong>
/// <list type="bullet">
/// <item><description>Universal browser support</description></item>
/// <item><description>Network flexibility (local and remote)</description></item>
/// <item><description>Low latency (1-3ms per message for local connections)</description></item>
/// <item><description>Automatic retry on startup (retries every 500ms within StartupTimeout)</description></item>
/// <item><description>Supports reconnection after calling StopAsync</description></item>
/// </list>
/// </para>
/// <para>
/// Most users will never create a <see cref="WebSocketConnection"/> directly. The <see cref="BiDiDriver"/>
/// creates one automatically when constructed without a custom transport.
/// </para>
/// </remarks>
public class WebSocketConnection : Connection
{
    private readonly SemaphoreSlim dataSendSemaphore = new(1, 1);
    private Task? dataReceiveTask;
    private ClientWebSocket client = new();
    private CancellationTokenSource clientTokenSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketConnection" /> class.
    /// </summary>
    public WebSocketConnection()
    {
    }

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public override bool IsActive => this.client.State != WebSocketState.None && this.client.State != WebSocketState.Closed && this.client.State != WebSocketState.Aborted;

    /// <summary>
    /// Gets a value indicating the type of data transport used by this connection, in this case, a WebSocket connection.
    /// </summary>
    public override ConnectionType ConnectionType => ConnectionType.WebSocket;

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="url">The connection string used to connect to the remote end. It must be a valid URL.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the connection is not established within the startup timeout.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the WebSocket is already connected.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is not a valid absolute URI, or does not have a WebSocket scheme.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public override async Task StartAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? websocketUri))
        {
            throw new ArgumentException($"The value '{url}' is not a valid absolute URI", nameof(url));
        }
        else if (websocketUri.Scheme != "ws" && websocketUri.Scheme != "wss")
        {
            throw new ArgumentException($"The URI scheme must be 'ws' or 'wss'; received '{websocketUri.Scheme}'", nameof(url));
        }

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
            throw new WebDriverBiDiConnectionException($"The WebSocket is already connected to {this.ConnectionString}; call the Stop method to disconnect before calling Start");
        }

        await this.LogAsync($"Opening connection to URL {url}").ConfigureAwait(false);
        bool connected = false;
        Stopwatch initializationStopwatch = Stopwatch.StartNew();
        while (!connected && initializationStopwatch.Elapsed <= this.StartupTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.clientTokenSource.Token);
                await this.client.ConnectAsync(websocketUri, linkedTokenSource.Token).ConfigureAwait(false);
                connected = true;
                this.ConnectionString = url;
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
        await this.LogAsync($"Connection opened").ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.LogAsync($"Closing connection").ConfigureAwait(false);
        if (this.client.State != WebSocketState.Open)
        {
            await this.LogAsync($"Client state is {this.client.State}").ConfigureAwait(false);
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

        this.ConnectionString = string.Empty;
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
    public override async Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default)
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
                await this.LogAsync($"SEND >>> {Encoding.UTF8.GetString(data)}", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
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
    /// Asynchronously receives data from the remote end of this connection.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ReceiveDataAsync()
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
                        await this.LogAsync($"Acknowledging Close frame received from server (client state: {this.client.State})").ConfigureAwait(false);
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
                                    await this.LogAsync($"RECV <<< {Encoding.UTF8.GetString(bytes)}", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
                                }

                                await this.OnDataReceived.NotifyObserversAsync(new ConnectionDataReceivedEventArgs(bytes)).ConfigureAwait(false);
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

            await this.LogAsync($"Ending processing loop in state {this.client.State}").ConfigureAwait(false);

            // If the loop exited without cancellation, the remote end closed the connection gracefully.
            if (!cancellationToken.IsCancellationRequested)
            {
                await this.OnRemoteDisconnected.NotifyObserversAsync(new ConnectionDisconnectedEventArgs()).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it
        }
        catch (WebSocketException e)
        {
            await this.LogAsync($"Unexpected error during receive of data: {e.Message}").ConfigureAwait(false);
            await this.OnConnectionError.NotifyObserversAsync(new ConnectionErrorEventArgs(e)).ConfigureAwait(false);
        }
        finally
        {
            memoryStream?.Dispose();
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// Override this method in derived classes to add custom async cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected override async ValueTask DisposeAsyncCore()
    {
        if (this.SetDisposed())
        {
            try
            {
                if (this.IsActive)
                {
                    await this.StopAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }

            this.dataSendSemaphore.Dispose();
            this.clientTokenSource.Dispose();
            this.client.Dispose();
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
    /// Asynchronously raises a logging event at the Info log level.
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

            await this.LogAsync($"Client state is {this.client.State}").ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it
        }
    }
}
