// <copyright file="WebSocketConnection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using WebDriverBiDi.Internal;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end over a WebSocket.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="WebSocketConnection"/> is the standard and recommended transport mechanism for WebDriver BiDi.
/// It uses the <see cref="ClientWebSocket"/> class to communicate with the browser
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
/// <item><description>Automatic retry on startup (retries every 500ms within StartupTimeout; both each attempt and the pause between attempts are bounded by the remaining StartupTimeout, so neither a host that never answers nor one that refuses immediately can hold startup open past the timeout)</description></item>
/// <item><description>Supports reconnection after calling StopAsync</description></item>
/// <item><description>Configurable socket options (request headers, proxy, keep-alive interval, certificate validation) through an override of <see cref="CreateClientWebSocket"/></description></item>
/// </list>
/// </para>
/// <para>
/// Most users will never create a <see cref="WebSocketConnection"/> directly. The <see cref="BiDiDriver"/>
/// creates one automatically when constructed without a custom transport.
/// </para>
/// </remarks>
public class WebSocketConnection : Connection
{
    // How long to wait before retrying a connection attempt that failed because the remote end
    // was not yet listening. The pause is charged against StartupTimeout, never added to it.
    private static readonly TimeSpan ConnectionRetryInterval = TimeSpan.FromMilliseconds(500);

    // This is initialized with a placeholder that has never connected, so that IsActive, StopAsync
    // and disposal have a socket to consult before the first start.
    private ClientWebSocket client = new();

    // The URI resolved from the connection string of the attempt now in progress. Connection.StartAsync
    // calls ResolveConnectionString, which assigns this, on every path that reaches
    // StartConnectionAsync, which reads it, and StartAsync is not overridable, so the two cannot come
    // apart.
    private Uri? websocketUri;

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required.
    // Which end's close this session is ending with, as a WebSocketCloseInitiator value. It is held as an int
    // because Interlocked.CompareExchange accepts an enum only from .NET 9, and this library targets earlier
    // frameworks. It is claimed at most once per session, by StopConnectionAsync before it begins the close
    // handshake or by the receive loop, which runs on its own task, before it answers a Close frame from the
    // remote end.
    private int closeInitiator = (int)WebSocketCloseInitiator.None;

    // Completed once the Close frame of a close this end initiated has been sent, or has failed to send, so
    // that the receive loop can tell when the socket has finished recording the send. Replaced for each session.
    private TaskCompletionSource<int> localCloseFrameSentSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketConnection" /> class.
    /// </summary>
    public WebSocketConnection()
    {
    }

    /// <summary>
    /// Gets a value indicating the type of data transport used by this connection, in this case, a WebSocket connection.
    /// </summary>
    public override ConnectionKind ConnectionKind => ConnectionKind.WebSocket;

    /// <summary>
    /// Gets a value indicating whether the underlying WebSocket is open.
    /// </summary>
    /// <remarks>
    /// A socket that is still connecting does not count as open, so <see cref="Connection.IsActive"/>
    /// stays <see langword="false"/> until the connection is established. A socket that has begun
    /// its close handshake still counts as open, because it can still receive the remote end's
    /// answer. Once the receive loop has reported that it ended, <see cref="Connection.IsActive"/>
    /// is <see langword="false"/>, whatever state the socket is left in.
    /// </remarks>
    protected override bool IsConnectionOpen => this.IsClientInOpenState;

    /// <summary>
    /// Gets a value indicating whether the <see cref="ClientWebSocket"/> is in an open state.
    /// </summary>
    private bool IsClientInOpenState => this.client.State == WebSocketState.Open || this.client.State == WebSocketState.CloseSent || this.client.State == WebSocketState.CloseReceived;

    /// <summary>
    /// Gets a value indicating whether the close now in progress was initiated by this end.
    /// </summary>
    /// <remarks>
    /// A local close is completed by the remote end answering the close handshake, which ends the receive
    /// loop the same way a remote-initiated close does. The receive loop cannot tell the two apart from the
    /// socket state alone, so <see cref="StopConnectionAsync(CancellationToken)"/> records which case it is
    /// by claiming the close ownership before it begins the handshake.
    /// </remarks>
    private bool IsLocalCloseInitiated => Interlocked.CompareExchange(ref this.closeInitiator, (int)WebSocketCloseInitiator.None, (int)WebSocketCloseInitiator.None) == (int)WebSocketCloseInitiator.Local;

    private bool IsRemoteCloseInitiated => Interlocked.CompareExchange(ref this.closeInitiator, (int)WebSocketCloseInitiator.None, (int)WebSocketCloseInitiator.None) == (int)WebSocketCloseInitiator.Remote;

    /// <summary>
    /// Resolves the connection string into the URI of the WebSocket server to connect to.
    /// </summary>
    /// <param name="connectionString">The connection string to interpret. It must be a valid WebSocket URL.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="connectionString"/> is not a valid absolute URI, or does not have a
    /// WebSocket scheme.
    /// </exception>
    /// <remarks>
    /// Parsing the URL is what proves it is one, so this method keeps what it parsed for
    /// <see cref="StartConnectionAsync"/> rather than validating for a later parse to repeat. It is
    /// called from <see cref="Connection.StartAsync"/> before that method does anything that can take
    /// time, so a malformed URL is reported at once.
    /// </remarks>
    protected override void ResolveConnectionString(string connectionString)
    {
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? resolvedUri))
        {
            throw new ArgumentException($"The value '{connectionString}' is not a valid absolute URI", nameof(connectionString));
        }

        if (resolvedUri.Scheme != "ws" && resolvedUri.Scheme != "wss")
        {
            throw new ArgumentException($"The URI scheme must be 'ws' or 'wss'; received '{resolvedUri.Scheme}'", nameof(connectionString));
        }

        this.websocketUri = resolvedUri;
    }

    /// <summary>
    /// Asynchronously opens the WebSocket to the remote end, retrying until the remote end accepts the
    /// connection or the startup budget is spent.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when the connection is not established within the startup timeout.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    protected override async Task StartConnectionAsync(CancellationToken cancellationToken)
    {
        // ResolveConnectionString assigned this for the attempt now in progress. Connection.StartAsync
        // calls it on every path that reaches this method and is not overridable, so the URI is present
        // here and the null-forgiving operator is appropriate.
        Uri websocketUri = this.websocketUri!;

        // Every session connects on a socket obtained from CreateClientWebSocket immediately before it.
        // Connection.StartAsync has already established that this connection is not active, so the socket
        // held here is the unconfigured placeholder created with this connection, a socket a previous
        // session left closed or aborted, or a socket left by an earlier failed start. The first two
        // cannot carry this session: a ClientWebSocket connects at most once, and takes its options only
        // before it does. Replacing the socket unconditionally, rather than only once it has been used, is
        // what applies an override's configuration to the first session, and it keeps the rule simple for
        // the third case too: the configuration used is always the one the override produces at the start.
        this.client.Dispose();
        this.client = this.CreateClientWebSocket();

        // A previous session may have ended with a close that either end claimed; this session has not.
        this.ResetCloseOwnership();

        bool connected = false;
        bool startupTimedOut = false;
        long startupTimestamp = this.TimeProvider.GetTimestamp();
        while (!connected && !startupTimedOut)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Calculate the time remaining in the budget for startup. This is the only place the
            // elapsed time is sampled, so the value that bounds an attempt cannot disagree with the
            // value that decided to make it. Sampling once for a loop condition and again for the
            // deadline allows the budget to lapse between the two, which hands the
            // CancellationTokenSource constructor a negative delay: an ArgumentOutOfRangeException
            // escaping StartAsync in place of the documented WebDriverBiDiTimeoutException or, at
            // exactly -1 millisecond, an attempt that is never bounded at all.
            TimeSpan remainingStartupTime = this.StartupTimeout - this.TimeProvider.GetElapsedTime(startupTimestamp);
            if (remainingStartupTime <= TimeSpan.Zero)
            {
                break;
            }

            using CancellationTokenSource attemptTimeoutTokenSource = TimeoutUtilities.CreateCancellationTokenSource(this.TimeProvider, remainingStartupTime);
            using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.ConnectionCancellationToken, attemptTimeoutTokenSource.Token);
            try
            {
                await this.ConnectWebSocketAsync(websocketUri, linkedTokenSource.Token).ConfigureAwait(false);
                connected = true;
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested by the caller or by StopAsync/Dispose propagates
                // unchanged. Any other cancellation came from the startup deadline above.
                if (cancellationToken.IsCancellationRequested || this.ConnectionCancellationToken.IsCancellationRequested)
                {
                    throw;
                }

                // A canceled connect leaves the socket in an unusable (aborted) state, so
                // discard it. The startup budget is exhausted, so no further attempts are made.
                this.client.Dispose();
                this.client = this.CreateClientWebSocket();
                startupTimedOut = true;
            }
            catch (WebSocketException)
            {
                // If the server-side socket is not yet ready, it leaves the client socket in a closed state,
                // which sees the object as disposed, so we must create a new one to try again. Note that
                // we will also explicitly call Dispose on the object, to make sure resources are disposed.
                // Replacing the socket before the retry delay rather than after it means every exit from
                // this loop, including a canceled delay, leaves a usable client behind.
                this.client.Dispose();
                this.client = this.CreateClientWebSocket();

                // The pause before retrying comes out of the startup budget rather than being added
                // to it, so it is clamped to whatever remains. Left unclamped, a remote end that
                // refuses connections immediately holds startup open for the full retry interval
                // past StartupTimeout.
                TimeSpan remainingRetryTime = this.StartupTimeout - this.TimeProvider.GetElapsedTime(startupTimestamp);
                if (remainingRetryTime <= TimeSpan.Zero)
                {
                    break;
                }

                TimeSpan retryDelay = remainingRetryTime < ConnectionRetryInterval ? remainingRetryTime : ConnectionRetryInterval;
                await TimeoutUtilities.DelayAsync(this.TimeProvider, retryDelay, cancellationToken).ConfigureAwait(false);
            }
        }

        if (!connected)
        {
            throw new WebDriverBiDiTimeoutException($"Could not connect to remote WebSocket server within {this.StartupTimeout.TotalSeconds} seconds");
        }
    }

    /// <summary>
    /// Asynchronously performs the WebSocket close handshake with the remote end.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// The handshake is performed here, before <see cref="Connection.StopAsync"/> cancels the
    /// connection, because cancelling the connection aborts a pending WebSocket receive rather than
    /// letting it observe the remote end's answer to the handshake.
    /// </remarks>
    protected override async Task StopConnectionAsync(CancellationToken cancellationToken)
    {
        if (this.client.State != WebSocketState.Open || !this.TryClaimCloseOwnership(WebSocketCloseInitiator.Local))
        {
            // The socket is no longer open, or the receive loop has already claimed the close in order to answer
            // a Close frame from the remote end, so this call starts no close handshake.
            await this.LogAsync($"Client state is {this.client.State}", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
            if (this.IsRemoteCloseInitiated && this.DataReceiveTask is not null)
            {
                // The receive loop is answering the remote end's Close frame, and sends its answer with the
                // connection's cancellation token, which the caller cancels as soon as this method returns.
                // Returning now would abort the answer, and the remote end would see an abnormal closure; wait
                // for the loop to finish instead, bounded as the wait for a close this end starts is.
                using CancellationTokenSource timeoutTokenSource = TimeoutUtilities.CreateCancellationTokenSource(this.TimeProvider, this.ShutdownTimeout);
                using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
                await Task.WhenAny(this.DataReceiveTask, TimeoutUtilities.DelayAsync(this.TimeProvider, Timeout.InfiniteTimeSpan, linkedTokenSource.Token)).ConfigureAwait(false);
            }
        }
        else
        {
            // This end is starting the handshake, so the close that ends the receive loop is ours. Claim it
            // before the handshake begins, as CloseClientWebSocketAsync awaits the receive loop.
            await this.CloseClientWebSocketAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously receives data from the remote end of this connection.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task ReceiveDataAsync()
    {
        CancellationToken connectionCancellationToken = this.ConnectionCancellationToken;
        using MessageBuffer messageBuffer = new();
        using IMemoryOwner<byte> receivedDataBufferOwner = MemoryPool<byte>.Shared.Rent(this.BufferSize);
        try
        {
            // MemoryPool<byte>.Shared is backed by ArrayPool, so TryGetArray always succeeds here.
            // We need the underlying array to pass to ReceiveAsync, which requires ArraySegment<byte>.
            MemoryMarshal.TryGetArray(receivedDataBufferOwner.Memory.Slice(0, this.BufferSize), out ArraySegment<byte> socketFrameBuffer);

            // A Close frame from the remote end is the remote end closing, and it is what ends this loop.
            // The socket reaching WebSocketState.Closed is the same thing seen through the local state
            // machine, and it is kept as a condition because a close this end started ends the loop that
            // way. It is not enough on its own: acknowledging the frame leaves the socket Closed only when
            // the socket itself saw the frame, and where it lands in CloseSent instead, a loop waiting for
            // Closed would receive forever on a connection the remote end has already finished with.
            bool remoteCloseFrameReceived = false;
            while (!remoteCloseFrameReceived && this.client.State != WebSocketState.Closed && !connectionCancellationToken.IsCancellationRequested)
            {
                // Only one receive operation at a time can be active on a ClientWebSocket instance,
                // so we should synchronize receive access to the socket. However, this receive
                // operation is private and should only be accessible by a single thread, that of the
                // Task running this method, so we will forego use of a semaphore to serialize such
                // access. If there is a use case where this could happen, we will resolve it at that
                // time.
                WebSocketReceiveResult receiveResult = await this.ReadWebSocketDataAsync(socketFrameBuffer, connectionCancellationToken).ConfigureAwait(false);

                // If the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted and it can't be used
                if (!connectionCancellationToken.IsCancellationRequested)
                {
                    // The server is notifying us that the connection will close. If this end did not
                    // initiate the close, send acknowledgement.
                    //
                    // Whether this end initiated the close is decided by the claim and never by the socket's
                    // state. The socket records that a Close frame was sent only once the send has returned, so
                    // the remote end's answer to a close this end started can be read here while the socket
                    // still reports CloseReceived. Acknowledging on the strength of that state sends a second
                    // Close frame, concurrently with the first, to a remote end that has finished with the
                    // connection; when the first send then completes the handshake and disposes the socket's
                    // stream, the second fails, and a clean local close is reported as a connection error.
                    remoteCloseFrameReceived = receiveResult.MessageType == WebSocketMessageType.Close;
                    if (remoteCloseFrameReceived)
                    {
                        if (this.TryClaimCloseOwnership(WebSocketCloseInitiator.Remote))
                        {
                            // If this end did not initiate the close, send an acknowledgement of the close.
                            await this.LogAsync($"Acknowledging Close frame received from server (client state: {this.client.State})", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
                            await this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Acknowledge Close frame", connectionCancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            // The close is initiated by this end, and the frame just read is the remote
                            // end's answer. Let the send of this end's Close frame finish before ending the loop.
                            await this.LogAsync("Close frame received from server responding to close initiated by this end", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
                            await this.localCloseFrameSentSignal.Task.ConfigureAwait(false);
                        }
                    }

                    // The message received from the WebSocket contains text or binary data
                    if (this.client.State == WebSocketState.Open && receiveResult.MessageType != WebSocketMessageType.Close)
                    {
                        // Every data frame accumulates the same way; an intermediate frame simply falls
                        // through to the next iteration, with the accumulator carrying the message across
                        // frames until a final frame arrives.
                        messageBuffer.Append(socketFrameBuffer.AsSpan(0, receiveResult.Count));
                        if (receiveResult.EndOfMessage)
                        {
                            // We've received the final frame of the message, whether single-frame
                            // or multi-frame. Notifying with an empty accumulator delivers nothing
                            // (an empty frame never starts an accumulation) so no guard against
                            // empty data is required here.
                            await this.NotifyDataReceivedObserverAsync(messageBuffer).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        // A fragmented message was being assembled, but the socket
                        // is no longer in the Open state or a Close frame arrived.
                        // Discard the partial data to prevent it from corrupting a
                        // subsequent message.
                        messageBuffer.Discard();
                    }
                }
            }

            // If the loop exited without cancellation, and this end did not start the close, the remote end
            // closed the connection gracefully. A close this end initiated ends the loop the same way once the
            // remote end answers the handshake, but it is not a remote disconnection and must not be reported
            // as one.
            if (!connectionCancellationToken.IsCancellationRequested && !this.IsLocalCloseInitiated)
            {
                await this.NotifyRemoteDisconnectedObserversAsync().ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it
        }
        catch (WebSocketException e)
        {
            await this.NotifyConnectionErrorObserversAsync($"Unexpected error during receive of data: {e.Message}", e).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Any other failure inside the loop -- from a derived connection's read, for example; a failing
            // observer of this connection's events is reported rather than thrown -- is captured here.
            // Otherwise the loop would stop silently, which pending commands could not tell apart from a
            // remote end that has simply gone quiet: they would wait for responses that never arrive.
            //
            // Nothing failed on the wire, so the socket is still open. Reporting the error marks the
            // connection inactive, but nothing will ever read from this socket again, so abort it
            // rather than leave it open until the connection is stopped: the remote end learns at once
            // that this end has gone, instead of sending into a socket that no one reads.
            this.client.Abort();
            await this.NotifyConnectionErrorObserversAsync($"Unexpected error processing received data: {e.Message}", e).ConfigureAwait(false);
        }
        finally
        {
            await this.LogAsync($"Ending processing loop in state {this.client.State}").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected override ValueTask DisposeAsyncCore()
    {
        this.client.Dispose();
        return default;
    }

    /// <summary>
    /// Asynchronously sends data to the underlying WebSocket of this connection.
    /// </summary>
    /// <param name="messageBuffer">The buffer containing the data to be sent to the remote end of this connection via the WebSocket.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when an exception is encountered sending data to the WebSocket.</exception>
    protected override async Task SendConnectionDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
        try
        {
            await this.WriteWebSocketDataAsync(messageBuffer, cancellationToken).ConfigureAwait(false);
        }
        catch (WebSocketException ex)
        {
            throw new WebDriverBiDiConnectionException($"An error occurred while sending data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates the <see cref="ClientWebSocket"/> on which a connection attempt is made.
    /// </summary>
    /// <returns>A new <see cref="ClientWebSocket"/> that has not been connected.</returns>
    /// <remarks>
    /// <para>
    /// Override this method to configure the socket through <see cref="ClientWebSocket.Options"/> before it
    /// connects: for example, to add a request header that the remote end requires to authenticate the
    /// connection, to route the connection through a proxy, to change the keep-alive interval, or to
    /// validate the certificate of a <c>wss</c> endpoint that the operating system does not trust. Those
    /// options can be set only before a socket connects, which is why they are applied here rather than
    /// exposed as properties of the connection. Call the base implementation to obtain the socket, then
    /// configure and return it.
    /// </para>
    /// <para>
    /// <see cref="Connection.StartAsync"/> calls this method immediately before every session connects,
    /// including the first, and again after each connection attempt that the remote end refuses or that
    /// runs out of the <see cref="Connection.StartupTimeout"/> budget, because a socket whose connect did not
    /// succeed cannot be used again. A single start can therefore call it more than once. It is never
    /// called while the connection is being constructed, so an override may rely on state that its own
    /// constructor, or an object initializer, has assigned.
    /// </para>
    /// <para>
    /// Return a new instance from every call. The connection owns each socket this method returns: it
    /// disposes a socket when it replaces it, and disposes the socket it holds when the connection itself
    /// is disposed. A socket shared between calls, or with other code, would be disposed out from under its
    /// other users.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public class AuthenticatedWebSocketConnection : WebSocketConnection
    /// {
    ///     private readonly string accessToken;
    ///
    ///     public AuthenticatedWebSocketConnection(string accessToken)
    ///     {
    ///         this.accessToken = accessToken;
    ///     }
    ///
    ///     protected override ClientWebSocket CreateClientWebSocket()
    ///     {
    ///         ClientWebSocket socket = base.CreateClientWebSocket();
    ///         socket.Options.SetRequestHeader("Authorization", $"Bearer {this.accessToken}");
    ///         return socket;
    ///     }
    /// }
    /// </code>
    /// </example>
    protected virtual ClientWebSocket CreateClientWebSocket()
    {
        return new ClientWebSocket();
    }

    /// <summary>
    /// Asynchronously connects the underlying WebSocket of this connection to the remote end.
    /// </summary>
    /// <param name="websocketUri">The URI of the WebSocket server to connect to.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that is canceled when the caller cancels, when the connection is stopped, or when
    /// the remaining <see cref="Connection.StartupTimeout"/> budget for this attempt elapses.
    /// </param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method is <see langword="protected virtual"/> to allow test doubles to substitute the
    /// connect operation, for example to simulate a remote end that never completes the handshake.
    /// </remarks>
    protected virtual Task ConnectWebSocketAsync(Uri websocketUri, CancellationToken cancellationToken)
    {
        return this.client.ConnectAsync(websocketUri, cancellationToken);
    }

    /// <summary>
    /// Asynchronously writes data to the underlying WebSocket of this connection.
    /// </summary>
    /// <param name="messageBuffer">The data to write to the WebSocket.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected virtual async Task WriteWebSocketDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default)
    {
#if NET5_0_OR_GREATER
        await this.client.SendAsync(messageBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken).ConfigureAwait(false);
#else
        await this.client.SendAsync(new ArraySegment<byte>(messageBuffer.ToArray()), WebSocketMessageType.Text, endOfMessage: true, cancellationToken).ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Asynchronously receives data from the underlying WebSocket of this connection.
    /// </summary>
    /// <param name="buffer">The buffer to receive the data into.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing the receive result.</returns>
    protected virtual Task<WebSocketReceiveResult> ReadWebSocketDataAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        return this.client.ReceiveAsync(buffer, cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends this end's Close frame to the remote WebSocket, beginning the close handshake.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token that is canceled when the caller of <see cref="Connection.StopAsync"/> cancels, or when
    /// <see cref="Connection.ShutdownTimeout"/> elapses.
    /// </param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="StopConnectionAsync(CancellationToken)"/> calls this method to send the frame when the socket is
    /// open, then waits, bounded by <see cref="Connection.ShutdownTimeout"/>, for the receive loop to observe the
    /// remote end's answer. By the time the returned task completes, the frame has been written and the socket has
    /// recorded that it was sent. The receive loop, for its part, does not end on the remote end's answer until the
    /// returned task has completed, so that it ends with the socket in the same state however quickly the answer
    /// arrives. This is the only step of the close that a derived connection can replace, and it is the only Close
    /// frame this end sends: the receive loop acknowledges a Close frame only when the remote end began the close.
    /// </para>
    /// <para>
    /// This method is <see langword="protected virtual"/> to allow test doubles to observe the point at which the
    /// frame has been sent, for example to act only once the handshake wait is certain to have begun.
    /// </para>
    /// </remarks>
    protected virtual Task SendWebSocketCloseFrameAsync(CancellationToken cancellationToken)
    {
        return this.client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
    }

    /// <summary>
    /// Asynchronously sends a close handshake to the remote WebSocket and waits, bounded by
    /// <see cref="Connection.ShutdownTimeout"/>, for the receive loop to observe the server's
    /// close response.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    private async Task CloseClientWebSocketAsync(CancellationToken cancellationToken)
    {
        // Close the socket first, because ReceiveAsync leaves an invalid socket (state = aborted) when the token is cancelled
        using CancellationTokenSource timeoutTokenSource = TimeoutUtilities.CreateCancellationTokenSource(this.TimeProvider, this.ShutdownTimeout);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
        try
        {
            try
            {
                // After this, the socket state will change to CloseSent
                await this.SendWebSocketCloseFrameAsync(linkedTokenSource.Token).ConfigureAwait(false);
            }
            finally
            {
                // The receive loop waits for this once it has read the remote end's answer, and this method
                // waits for the receive loop next, so the signal is raised however the send ended.
                this.localCloseFrameSentSignal.TrySetResult(0);
            }

            // Wait for the receive loop to process the server's close response, which will transition the
            // socket to the Closed state. If the server does not respond within the shutdown timeout,
            // proceed anyway; StopAsync will cancel the token to abort the socket.
            //
            // The infinite delay is intentional: its sole purpose is to convert the CancellationToken defined
            // earlier in this method into a Task that Task.WhenAny can race against DataReceiveTask. The delay
            // itself never elapses; it completes only when the linked token fires (i.e., ShutdownTimeout expires
            // or the external cancellation token is canceled), which is the desired fallback behavior.
            if (this.DataReceiveTask is not null)
            {
                await Task.WhenAny(this.DataReceiveTask, TimeoutUtilities.DelayAsync(this.TimeProvider, Timeout.InfiniteTimeSpan, linkedTokenSource.Token)).ConfigureAwait(false);
            }

            await this.LogAsync($"Client state is {this.client.State}").ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // An OperationCanceledException is normal upon task/token cancellation, so disregard it
        }
        catch (WebSocketException ex)
        {
            // The Close frame could not be sent, typically because the remote end has gone while the receive
            // loop has not yet noticed. The close is best-effort, so the stop still completes rather than failing,
            // but no handshake can happen now: abort the socket, which also ends any read the receive loop has in
            // progress. Relying on the cancellation that follows would leave the socket reporting itself open
            // whenever the loop was between reads, and so exited on the token without touching the socket.
            this.client.Abort();
            await this.LogAsync($"Could not send the Close frame to the remote end; the connection was closed without the close handshake: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Attempts to claim ownership of this session's close for the specified end.
    /// </summary>
    /// <param name="initiator">The end claiming the close.</param>
    /// <returns><see langword="true"/> if the close was unclaimed and now belongs to the specified end; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// The end that claims the close is the only one that sends this end's Close frame:
    /// <see cref="StopConnectionAsync(CancellationToken)"/> to begin the handshake, or the receive loop to answer
    /// the remote end's. The two run on different tasks, and a close from each end can cross on the wire, so the
    /// claim is atomic rather than inferred from the socket's state.
    /// </remarks>
    private bool TryClaimCloseOwnership(WebSocketCloseInitiator initiator)
    {
        return Interlocked.CompareExchange(ref this.closeInitiator, (int)initiator, (int)WebSocketCloseInitiator.None) == (int)WebSocketCloseInitiator.None;
    }

    private void ResetCloseOwnership()
    {
        Interlocked.Exchange(ref this.closeInitiator, (int)WebSocketCloseInitiator.None);
        Interlocked.Exchange(ref this.localCloseFrameSentSignal, new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously));
    }
}
