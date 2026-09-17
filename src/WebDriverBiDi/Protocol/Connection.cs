// <copyright file="Connection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Text;
using WebDriverBiDi.Internal;

/// <summary>
/// Represents a connection to a WebDriver Bidi remote end.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Connection"/> class is an abstract base class that defines the contract for
/// transport-layer communication with a browser. It is wrapped by the <see cref="Transport"/> class,
/// which handles protocol-level concerns like JSON serialization and command/response correlation.
/// </para>
/// <para>
/// Most users will never need to interact with <see cref="Connection"/> objects directly.
/// The <see cref="BiDiDriver"/> class manages connections automatically. Custom connection
/// implementations are only needed for specialized transport mechanisms.
/// </para>
/// <para>
/// Available implementations:
/// <list type="bullet">
/// <item><term><see cref="WebSocketConnection"/></term><description>Standard WebSocket transport (recommended for all scenarios)</description></item>
/// <item><term><see cref="PipeConnection"/></term><description>Anonymous pipes transport (specialized for high-performance local Chromium automation)</description></item>
/// </list>
/// </para>
/// <para>
/// <see cref="StartAsync"/>, <see cref="StopAsync"/> and <see cref="DisposeAsync"/> are implemented by
/// this class and are not overridable: the sequence each performs is the same for every transport, and
/// several of its steps are only correct in a particular order. A derived class supplies the
/// transport-specific parts of them:
/// <list type="bullet">
/// <item><term><see cref="ResolveConnectionString"/></term><description>Interprets the connection string, rejecting a value this transport could never connect to. Optional; the default accepts every value</description></item>
/// <item><term><see cref="StartConnectionAsync"/></term><description>Establishes the connection</description></item>
/// <item><term><see cref="StopConnectionAsync"/></term><description>Whatever this transport must exchange with the remote end to close by agreement</description></item>
/// <item><term><see cref="SendConnectionDataAsync"/></term><description>Writes one message to the transport</description></item>
/// <item><term><see cref="ReceiveDataAsync"/></term><description>The receive loop, started once the connection is established</description></item>
/// <item><term><see cref="DisposeAsyncCore"/></term><description>Releases the resources the derived class owns</description></item>
/// <item><term><see cref="IsConnectionOpen"/></term><description>Reports whether the transport-specific connection is open</description></item>
/// </list>
/// </para>
/// <para>
/// <see cref="IsActive"/> is implemented by this class as well. A connection is active only while it is open and its
/// receive loop has not reported that it ended. A receive loop reports its end by calling
/// <see cref="NotifyRemoteDisconnectedObserversAsync"/> or <see cref="NotifyConnectionErrorObserversAsync"/>.
/// </para>
/// <para>
/// A derived class raises this class's events only through the methods that keep each event consistent with the
/// state of the connection: <see cref="NotifyDataReceivedObserverAsync(MessageBuffer)"/>, or its overload taking
/// pooled memory directly, for <see cref="OnDataReceived"/>; <see cref="LogAsync(string, WebDriverBiDiLogLevel)"/>
/// for <see cref="OnLogMessage"/>; and the two notification methods above for <see cref="OnRemoteDisconnected"/>
/// and <see cref="OnConnectionError"/>. The events are not exposed for raising in any other way, so no
/// implementation can deliver a message without transferring its memory, log a message that
/// <see cref="LogLevel"/> excludes, or report the end of its receive loop and leave the connection active.
/// </para>
/// <para>
/// Thread safety: Connection implementations use internal synchronization to ensure thread-safe operation.
/// Multiple threads can safely call <see cref="SendDataAsync"/> concurrently. The <see cref="Transport"/>
/// class that wraps connections provides additional synchronization for <see cref="StartAsync"/> and
/// <see cref="StopAsync"/> operations.
/// </para>
/// </remarks>
public abstract class Connection : IAsyncDisposable
{
    /// <summary>
    /// Gets the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Connection";

    /// <summary>
    /// The prefix for logging message content during a send operation.
    /// </summary>
    protected const string LogSendMessagePrefix = "SEND >>> ";

    /// <summary>
    /// The prefix for logging message content during a receive operation.
    /// </summary>
    protected const string LogReceiveMessagePrefix = "RECV <<< ";

    // Default buffer size is 2^20 bytes, or 1MB.
    private const int BufferSizeInBytes = 1 << 20;
    private const string DataReceivedEventName = "connection.dataReceived";
    private const string LogMessageEventName = "connection.logMessage";
    private const string ConnectionErrorEventName = "connection.connectionError";
    private const string RemoteDisconnectedEventName = "connection.remoteDisconnected";

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    // Each event is raised only through the method that keeps what it reports consistent with the connection's own
    // state: NotifyDataReceivedObserverAsync transfers ownership of the message's pooled memory, and returns it
    // when nothing takes it; LogAsync applies LogLevel; and NotifyConnectionErrorObserversAsync and
    // NotifyRemoteDisconnectedObserversAsync record that the receive loop ended before notifying anyone. Exposing
    // the events themselves to derived classes would let an implementation skip that work, which is how a
    // connection whose receive loop had ended once went on reporting itself active.
    private readonly ObservableEventInvocable<ConnectionDataReceivedEventArgs> dataReceivedObservableEvent = new(DataReceivedEventName, 1);
    private readonly ObservableEventInvocable<ConnectionErrorEventArgs> connectionErrorObservableEvent = new(ConnectionErrorEventName);
    private readonly ObservableEventInvocable<ConnectionDisconnectedEventArgs> remoteDisconnectedObservableEvent = new(RemoteDisconnectedEventName);
    private readonly ObservableEventInvocable<LogMessageEventArgs> logMessageObservableEvent = new(LogMessageEventName);

    private TimeSpan startupTimeout = DefaultTimeout;
    private TimeSpan shutdownTimeout = DefaultTimeout;
    private TimeSpan dataTimeout = DefaultTimeout;

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int isDisposedFlag;

    // Set when the receive loop reports that it ended, and cleared immediately before the next session's loop
    // is started.
    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int receiveLoopEndedFlag;

    // Deliberately not exposed to derived classes. Reading the Token property of a disposed
    // CancellationTokenSource throws ObjectDisposedException, and the receive loop and any
    // in-flight send can outlive disposal, so subclasses are given the cached
    // ConnectionCancellationToken instead. It is cancelled and replaced only by StartAsync and
    // StopAsync, which are the whole of this class's lifecycle.
    private CancellationTokenSource connectionCancellationTokenSource = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Connection"/> class.
    /// </summary>
    /// <remarks>
    /// The cached <see cref="ConnectionCancellationToken"/> is taken here rather than only when a
    /// session starts, so that it always belongs to the connection's current
    /// <see cref="CancellationTokenSource"/>. Left unassigned until the first
    /// <see cref="StartAsync"/>, it would be the default token, which can never be canceled, and
    /// the cancellation <see cref="StopAsync"/> performs on a connection that was never started
    /// would be requested on a source nothing is watching.
    /// </remarks>
    protected Connection()
    {
        this.ConnectionCancellationToken = this.connectionCancellationTokenSource.Token;
    }

    /// <summary>
    /// Gets a value indicating whether this connection is active: open, and still read by its receive loop.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A connection is active when <see cref="IsConnectionOpen"/> reports that the transport-specific connection
    /// is open and its receive loop has not reported that it ended, by calling
    /// <see cref="NotifyRemoteDisconnectedObserversAsync"/> or <see cref="NotifyConnectionErrorObserversAsync"/>.
    /// The receive loop is the only reader a connection has, so a connection whose loop has ended cannot receive
    /// the response to anything sent on it, even while the connection itself is still open. Such a connection
    /// reports itself inactive from the moment its loop reports the end, before any observer is notified, until
    /// the next <see cref="StartAsync"/> starts a new loop.
    /// </para>
    /// <para>
    /// This matters most to <see cref="Transport.ConnectAsync"/>, which adopts an active connection rather than
    /// starting it again. A connection that went on reporting itself active after its loop ended would be adopted
    /// by a reconnect, and no command sent on the new session would ever be answered.
    /// </para>
    /// </remarks>
    public bool IsActive => this.IsConnectionOpen && !this.IsReceiveLoopEnded;

    /// <summary>
    /// Gets a value indicating the kind of data transport used by this connection.
    /// </summary>
    public abstract ConnectionKind ConnectionKind { get; }

    /// <summary>
    /// Gets the buffer size for communication used by this connection.
    /// </summary>
    public int BufferSize { get; } = BufferSizeInBytes;

    /// <summary>
    /// Gets the ID of this <see cref="Connection"/>.
    /// </summary>
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the string naming the remote end this connection is connected to. For a
    /// <see cref="WebSocketConnection"/> this is the WebSocket URL. A <see cref="PipeConnection"/> does not
    /// interpret the value at all -- the anonymous pipes it uses are its own -- so the string only labels
    /// the session there.
    /// </summary>
    public string ConnectionString { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the timeout to wait before throwing an error when starting up the connection.
    /// </summary>
    /// <remarks>
    /// The timeout is a single budget for the whole of startup: it bounds each individual connection
    /// attempt as well as the retries between them, so a remote end that accepts slowly is cut off at
    /// the deadline rather than allowed to complete late. Name resolution and address fallback (for
    /// example <c>localhost</c> resolving to an IPv6 address first) count against the budget, so avoid
    /// sub-second values when connecting by host name.
    /// <para>
    /// Because the startup budget is computed by subtracting elapsed time, it must be a finite value;
    /// <see cref="Timeout.InfiniteTimeSpan"/> is not permitted.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative, is <see cref="Timeout.InfiniteTimeSpan"/>, or exceeds the
    /// maximum timer duration supported by the runtime.
    /// </exception>
    public TimeSpan StartupTimeout
    {
        get => this.startupTimeout;
        set
        {
            if (!TimeoutUtilities.IsValidTimeout(value, allowInfinite: false))
            {
                throw new ArgumentOutOfRangeException(nameof(value), TimeoutUtilities.GetInvalidTimeoutMessage("Startup timeout", allowInfinite: false));
            }

            this.startupTimeout = value;
        }
    }

    /// <summary>
    /// Gets or sets the value of the timeout that bounds shutting the connection down.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It bounds the transport's own close of the connection, such as a WebSocket close handshake, and
    /// it separately bounds the wait for the receive loop to finish. That wait happens at both ends of
    /// a session: <see cref="StopAsync"/> waits for the loop it is ending, and
    /// <see cref="StartAsync"/> waits for a loop a previous <see cref="StopAsync"/> had to abandon,
    /// because a second loop must not run alongside it.
    /// </para>
    /// <para>
    /// A wait for the receive loop that is not satisfied does not throw. It raises a
    /// <see cref="WebDriverBiDiLogLevel.Warn"/> message on <see cref="OnLogMessage"/> and proceeds:
    /// <see cref="StopAsync"/> returns with the loop left running in the background, and
    /// <see cref="StartAsync"/> refuses to begin a new session while it is.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds
    /// the maximum timer duration supported by the runtime.
    /// </exception>
    public TimeSpan ShutdownTimeout
    {
        get => this.shutdownTimeout;
        set
        {
            if (!TimeoutUtilities.IsValidTimeout(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), TimeoutUtilities.GetInvalidTimeoutMessage("Shutdown timeout"));
            }

            this.shutdownTimeout = value;
        }
    }

    /// <summary>
    /// Gets or sets the value of the timeout to wait for exclusive access when sending data over the connection.
    /// It bounds only that wait, not the send itself and not any receive.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds
    /// the maximum timer duration supported by the runtime.
    /// </exception>
    public TimeSpan DataTimeout
    {
        get => this.dataTimeout;
        set
        {
            if (!TimeoutUtilities.IsValidTimeout(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), TimeoutUtilities.GetInvalidTimeoutMessage("Data timeout"));
            }

            this.dataTimeout = value;
        }
    }

    /// <summary>
    /// Gets or sets the minimum <see cref="WebDriverBiDiLogLevel"/> at which this connection raises
    /// <see cref="OnLogMessage"/>. Messages below this level are never built or raised. Defaults to
    /// <see cref="WebDriverBiDiLogLevel.Info"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the single setting for the whole log pipeline: <see cref="Transport.LogLevel"/> and
    /// <see cref="ITransportConfiguration.LogLevel"/>, reached from
    /// <see cref="BiDiDriver.TransportConfiguration"/>, read and write this property, so setting it on
    /// any of the three sets it for all of them.
    /// </para>
    /// <para>
    /// The default excludes the two most voluminous levels. Every message this connection sends and
    /// receives is logged at <see cref="WebDriverBiDiLogLevel.Trace"/>, and each such message is decoded
    /// from UTF-8 into a string only when that level is enabled, so raising the level to
    /// <see cref="WebDriverBiDiLogLevel.Trace"/> to inspect protocol traffic also opts in to that cost.
    /// <see cref="WebDriverBiDiLogLevel.Off"/> suppresses every message, including
    /// <see cref="WebDriverBiDiLogLevel.Fatal"/>; it is only meaningful here, and is never the level of a
    /// message that is raised.
    /// </para>
    /// </remarks>
    public WebDriverBiDiLogLevel LogLevel { get; set; } = WebDriverBiDiLogLevel.Info;

    /// <summary>
    /// Gets an observable event that notifies when data is received from this connection.
    /// </summary>
    /// <remarks>
    /// Due to the shared-memory nature of the data received, one, and only one,
    /// <see cref="EventObserver{ConnectionDataReceivedEventArgs}"/> can be observing this
    /// event at a time. Attempting to connect a second observer will throw an exception.
    /// </remarks>
    public ObservableEvent<ConnectionDataReceivedEventArgs> OnDataReceived => this.dataReceivedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a communication error occurs on this connection.
    /// </summary>
    public ObservableEvent<ConnectionErrorEventArgs> OnConnectionError => this.connectionErrorObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when the remote end gracefully closes this connection.
    /// </summary>
    public ObservableEvent<ConnectionDisconnectedEventArgs> OnRemoteDisconnected => this.remoteDisconnectedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a log message is written.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.logMessageObservableEvent;

    /// <summary>
    /// Gets a value indicating whether this connection has been disposed.
    /// </summary>
    protected bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Gets a value indicating whether the transport-specific connection is open.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the transport-specific part of <see cref="IsActive"/>, which also requires that the receive loop
    /// has not reported that it ended. An implementation reports only whether the underlying channel is open, and
    /// need not account for the receive loop at all.
    /// </para>
    /// <para>
    /// <see cref="DisposeAsync"/> stops a connection for which this reports <see langword="true"/>, even when the
    /// connection is inactive because its receive loop has ended, so that an open channel is always shut down
    /// before its resources are released.
    /// </para>
    /// </remarks>
    protected abstract bool IsConnectionOpen { get; }

    /// <summary>
    /// Gets or sets the <see cref="TimeProvider"/> whose clock measures this connection's timeouts:
    /// <see cref="StartupTimeout"/>, <see cref="ShutdownTimeout"/> and <see cref="DataTimeout"/>.
    /// Defaults to <see cref="TimeProvider.System"/>. A derived type may substitute another, for
    /// example to drive the timeouts with virtual time in a test, in the same way that
    /// <see cref="ObservableEvent{T}"/> exposes its provider to derived types.
    /// </summary>
    protected TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>
    /// Gets a <see cref="SemaphoreSlim"/> to serialize sending data across the connection, ensuring sending data to be an atomic action.
    /// </summary>
    protected SemaphoreSlim DataSendSemaphore { get; } = new(1, 1);

    /// <summary>
    /// Gets the <see cref="Task"/> object representing the method that receives data from the connection.
    /// </summary>
    protected Task? DataReceiveTask { get; private set; }

    /// <summary>
    /// Gets the <see cref="CancellationToken"/> used to cancel the operations of this connection.
    /// </summary>
    /// <remarks>
    /// This is a cached copy of the token of the connection's <see cref="CancellationTokenSource"/>,
    /// taken while that source is known to be alive. A <see cref="CancellationToken"/> obtained
    /// beforehand remains safe to use after its source is disposed, whereas reading the source's
    /// Token property after disposal throws <see cref="ObjectDisposedException"/>. Both the
    /// background receive loop and an in-flight send can still be running when the connection is
    /// disposed, so they must use this property rather than the source directly.
    /// </remarks>
    protected CancellationToken ConnectionCancellationToken { get; private set; }

    private bool IsReceiveLoopEnded
    {
        get => Interlocked.CompareExchange(ref this.receiveLoopEndedFlag, 0, 0) == 1;
        set => Interlocked.Exchange(ref this.receiveLoopEndedFlag, value ? 1 : 0);
    }

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="connectionString">The connection string used to connect to the remote end.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to start a disposed connection.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">
    /// Thrown when this connection is already active, or when the receive loop of a previous session
    /// is still running after a bounded wait.
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <remarks>
    /// <para>
    /// This method is the whole of the startup sequence every connection performs, and it is not
    /// overridable. A derived class supplies only the transport-specific parts of it, by implementing
    /// <see cref="StartConnectionAsync"/> and, where the connection string admits values it could never
    /// connect to, <see cref="ResolveConnectionString"/>.
    /// </para>
    /// <para>
    /// The steps this method performs around <see cref="StartConnectionAsync"/> are each required for
    /// correctness, and several are required in this order: the previous session's receive loop must be
    /// accounted for before a new one can be started over the same transport; the cancellation source
    /// must be replaced before anything reads <see cref="ConnectionCancellationToken"/>, because
    /// <see cref="StopAsync"/> cancels it unconditionally and a session that reused it would begin
    /// already canceled; and <see cref="ConnectionString"/> must be assigned before the receive loop
    /// starts, because the loop reads it. Leaving those steps to each implementation made them a
    /// contract that could only be described, and therefore forgotten.
    /// </para>
    /// </remarks>
    public async Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(this.GetType().Name, $"This {this.ConnectionKind} connection has been disposed; the connection cannot be restarted after disposal.");
        }

        // Interpret the connection string before anything that can take time. Rejecting a value this
        // connection could never connect to costs nothing, and a caller who passes one gets that answer
        // immediately rather than behind the bounded wait for a previous session's receive loop below,
        // which on a reconnect can be as long as ShutdownTimeout.
        this.ResolveConnectionString(connectionString);

        if (this.IsActive)
        {
            throw new WebDriverBiDiConnectionException($"The {this.ConnectionKind} connection is already connected to {this.ConnectionString}; call the Stop method to disconnect before calling Start");
        }

        // Honor a caller who has already given up before any work is done. Each implementation of
        // StartConnectionAsync observes the token as far as its own transport allows, which for some
        // transports is not at all, so this is the one place the check is guaranteed to happen.
        cancellationToken.ThrowIfCancellationRequested();

        // StopAsync may have abandoned the previous session's receive loop when it did not finish
        // within ShutdownTimeout. Starting a second loop over the same transport would interleave the
        // two loops' reads arbitrarily, corrupting message framing and routing stale data into the new
        // session. Give a loop that is still unwinding a bounded chance to finish, and refuse to start
        // while it runs.
        await this.WaitForReceiveTaskCompletionAsync($"Timed out waiting for the receive loop of the previous {this.ConnectionKind} connection session to complete before starting a new one").ConfigureAwait(false);
        if (this.DataReceiveTask is not null && !this.DataReceiveTask.IsCompleted)
        {
            throw new WebDriverBiDiConnectionException($"Cannot start the {this.ConnectionKind} connection: the receive loop from a previous session has not yet completed, most likely because it is blocked in a read that did not honor cancellation; the connection cannot be restarted until that read completes");
        }

        this.ResetConnectionCancellation();

        await this.LogAsync($"Opening {this.ConnectionKind} connection to {connectionString}").ConfigureAwait(false);

        // Publish the connection string before the connect rather than after it, so that an
        // implementation of StartConnectionAsync, and anything it logs, can name the remote end it is
        // connecting to without being handed the string a second time. An attempt that fails leaves
        // nothing behind.
        this.ConnectionString = connectionString;
        try
        {
            await this.StartConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            this.ConnectionString = string.Empty;
            throw;
        }

        // A loop that reported its end belongs to a previous session, and that loop is known to have finished,
        // so nothing can report an end after this point except the loop about to start.
        this.IsReceiveLoopEnded = false;
        this.StartDataReceiveTask();
        await this.LogAsync($"{this.ConnectionKind} connection opened").ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This method is the whole of the shutdown sequence every connection performs, and it is not
    /// overridable. A derived class supplies only the transport-specific part of it, by implementing
    /// <see cref="StopConnectionAsync"/>.
    /// </para>
    /// <para>
    /// The transport-specific shutdown runs first, because a graceful close of a transport that has one
    /// must complete before the connection is canceled; cancellation aborts such a close rather than
    /// completing it. Cancellation and the wait for the receive loop then happen unconditionally,
    /// whatever state the connection was in and whether or not it was ever started, so that stopping is
    /// always well defined and always leaves the connection able to be started again.
    /// </para>
    /// <para>
    /// Waiting for the receive loop to finish is bounded by <see cref="ShutdownTimeout"/>. If the loop
    /// does not finish within it, a warning is logged and this method returns anyway; the receive task
    /// continues running in the background until its read unblocks on its own, and
    /// <see cref="StartAsync"/> refuses to begin a new session while it does.
    /// </para>
    /// </remarks>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await this.LogAsync($"Closing {this.ConnectionKind} connection").ConfigureAwait(false);
        try
        {
            await this.StopConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // Whether or not the transport-specific shutdown closed the connection cleanly -- and
            // whether or not it failed outright -- cancel the connection so that the receive loop and
            // any in-flight send stop, then wait for the loop. A stop that reports a failure still
            // leaves the connection canceled and able to be started again, rather than leaving a
            // receive loop running against a connection its owner believes is finished with.
            this.CancelConnection();
            await this.WaitForReceiveTaskCompletionAsync($"Timed out waiting for {this.ConnectionKind} connection receive loop to complete during shutdown").ConfigureAwait(false);
            this.ConnectionString = string.Empty;
        }

        await this.LogAsync($"{this.ConnectionKind} connection closed").ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends data to the remote end of this connection.
    /// </summary>
    /// <param name="data">The data to be sent to the remote end of this connection.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the connection is not active.</exception>
    /// <exception cref="WebDriverBiDiTimeoutException">Thrown when exclusive access to the connection for sending times out.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task SendDataAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (!this.IsActive)
        {
            // IsActive is false for a connection that has never been started, for one that has been closed,
            // whether by StopAsync or by the remote end, and for one whose receive loop has ended, and this
            // guard does not tell those apart. The message therefore names all of them, rather than telling a
            // caller who did start the connection that they forgot to.
            throw new WebDriverBiDiConnectionException($"The {this.ConnectionKind} connection is not active; it has not been started, it has already been closed, or its receive loop has ended. Call the Start method to open it before sending data.");
        }

        // Notify log-message observers before acquiring the send semaphore to avoid
        // potential deadlocks in a malformed observer on the logging event. Decoding the payload for
        // the message is only worth doing when a Trace message would actually be raised.
        await this.LogMessageContentAsync(LogSendMessagePrefix, data, data.Length).ConfigureAwait(false);

        // Only one send operation at a time can be active for many connection types (e.g.,
        // a ClientWebSocket instance), so we must synchronize send access to the connection
        // for sending in case multiple threads are attempting to send commands or other data
        // simultaneously.
        if (!await this.WaitForSendAccessAsync(cancellationToken).ConfigureAwait(false))
        {
            throw new WebDriverBiDiTimeoutException("Timed out waiting to access connection for sending; only one send operation is permitted at a time.");
        }

        try
        {
            if (!this.IsActive)
            {
                throw new WebDriverBiDiConnectionException($"The {this.ConnectionKind} connection was closed before the send could be completed");
            }

            CancellationToken effectiveCancellationToken = this.ConnectionCancellationToken;
            CancellationTokenSource? linkedTokenSource = null;
            try
            {
                if (cancellationToken != CancellationToken.None)
                {
                    linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, effectiveCancellationToken);
                    effectiveCancellationToken = linkedTokenSource.Token;
                }

                await this.SendConnectionDataAsync(data, effectiveCancellationToken).ConfigureAwait(false);
            }
            catch (ObjectDisposedException ex)
            {
                throw new WebDriverBiDiConnectionException($"An error occurred while sending data: {ex.Message}", ex);
            }
            finally
            {
                linkedTokenSource?.Dispose();
            }
        }
        finally
        {
            this.DataSendSemaphore.Release();
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// A connection that is still open is stopped before its resources are released, even when its receive loop
    /// has already ended, so that the remote end sees
    /// the shutdown the transport defines rather than the connection simply disappearing. A failure to
    /// stop is logged and does not prevent disposal, because disposal must release the connection's
    /// resources whatever state it is in. A derived class releases its own resources by implementing
    /// <see cref="DisposeAsyncCore"/>; it neither performs the stop nor records the disposal
    /// itself, both of which happen here.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (this.SetDisposed())
            {
                try
                {
                    // Keyed on the channel rather than on IsActive: a connection whose receive loop has ended is
                    // inactive, but its channel is still open until it is stopped, and releasing its resources
                    // without stopping it would skip the shutdown the transport defines.
                    if (this.IsConnectionOpen)
                    {
                        await this.StopAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
                }

                await this.DisposeAsyncCore().ConfigureAwait(false);
            }
        }
        finally
        {
            this.DataSendSemaphore.Dispose();
            this.connectionCancellationTokenSource.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets a value indicating whether a message at the given level would be raised on
    /// <see cref="OnLogMessage"/>, so that a caller can avoid building a message that would be discarded.
    /// </summary>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> of the message the caller would raise.</param>
    /// <returns><see langword="true"/> if such a message would be raised; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Test this before composing any log message whose construction is not free. The connection uses it
    /// for the <c>SEND</c> and <c>RECV</c> traffic messages, whose construction decodes the whole payload
    /// from UTF-8; a custom <see cref="Connection"/> should use it for the same purpose.
    /// <see cref="WebDriverBiDiLogLevel.Off"/> is never enabled. It selects "no messages at all" when
    /// assigned to <see cref="LogLevel"/>, and is not a level a message can carry; without the explicit
    /// test it would compare as enabled against every setting, because it is the highest value.
    /// </remarks>
    public bool IsLogLevelEnabled(WebDriverBiDiLogLevel level)
    {
        return level != WebDriverBiDiLogLevel.Off && level >= this.LogLevel && this.OnLogMessage.CurrentObserverCount > 0;
    }

    /// <summary>
    /// Installs the reporter used to surface failures of observers of this connection's events
    /// that occur after the observer's handler has already returned to the caller.
    /// </summary>
    /// <param name="reporter">The reporter callback.</param>
    /// <remarks>
    /// <para>
    /// A failure of that kind cannot propagate to a caller, because the handler has already
    /// returned. Without a reporter it is observed (so it never surfaces as a
    /// <see cref="TaskScheduler.UnobservedTaskException"/>) but is otherwise discarded.
    /// <see cref="Transport"/> calls this so that such a failure is instead routed through the
    /// same unhandled-error pipeline as a failure in an observer of a transport, driver, or
    /// module event, and is therefore governed by
    /// <see cref="Transport.EventHandlerExceptionBehavior"/>.
    /// </para>
    /// <para>
    /// The reporter applies to observers added before this call as well as after it, because
    /// <see cref="EventObserver{T}"/> reads it when a fault is reported rather than capturing it
    /// when the observer is created.
    /// </para>
    /// </remarks>
    internal void SetObserverErrorReporter(Func<EventObserverErrorInfo, Task> reporter)
    {
        this.dataReceivedObservableEvent.InvokeSetObserverErrorReporter(reporter);
        this.connectionErrorObservableEvent.InvokeSetObserverErrorReporter(reporter);
        this.remoteDisconnectedObservableEvent.InvokeSetObserverErrorReporter(reporter);
        this.logMessageObservableEvent.InvokeSetObserverErrorReporter(reporter);
    }

    /// <summary>
    /// Asynchronously establishes the transport-specific connection to the remote end, to the target
    /// that <see cref="ResolveConnectionString"/> resolved.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This is the transport-specific part of <see cref="StartAsync"/>. When it is called, the
    /// connection is known not to be disposed, not to be active, and to have no receive loop of its own
    /// still running; <see cref="ResolveConnectionString"/> has been called with the connection string
    /// of this attempt and has accepted it; <see cref="ConnectionString"/> already names the remote end
    /// being connected to; and <see cref="ConnectionCancellationToken"/> belongs to the session now
    /// beginning, so an implementation may use it to bound its own work.
    /// </para>
    /// <para>
    /// Not being active does not mean being closed. A connection whose receive loop reported that it ended is
    /// inactive, but stays open until it is stopped, so this method can be called while
    /// <see cref="IsConnectionOpen"/> still reports <see langword="true"/>. An implementation replaces or reuses
    /// whatever it holds, as it would when starting after a stop.
    /// </para>
    /// <para>
    /// It takes no connection string, because by the time it runs the string has already been
    /// interpreted: an implementation that needed a parsed form of it has that form, and one that wants
    /// the string itself reads <see cref="ConnectionString"/>.
    /// </para>
    /// <para>
    /// An implementation returns only once the connection is established, which is to say once
    /// <see cref="IsConnectionOpen"/> would report <see langword="true"/>; <see cref="StartAsync"/> starts the
    /// receive loop immediately afterwards. It reports a failure to connect by throwing, which clears
    /// <see cref="ConnectionString"/>, leaves the connection stopped, and lets the caller of
    /// <see cref="StartAsync"/> see why.
    /// </para>
    /// </remarks>
    protected abstract Task StartConnectionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously performs the transport-specific shutdown of the connection to the remote end.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// This is the transport-specific part of <see cref="StopAsync"/>, and the only part of shutdown a
    /// derived class supplies. It is where a transport that closes by agreement with the remote end
    /// performs that exchange, because it runs before the connection is canceled, and cancellation
    /// aborts such an exchange rather than completing it.
    /// </para>
    /// <para>
    /// It is called on every call to <see cref="StopAsync"/>, including when the connection is not
    /// active and when it was never started, so an implementation that has nothing to do in those cases
    /// tests for them itself. Cancelling the connection and waiting for the receive loop are not its
    /// concern; <see cref="StopAsync"/> does both after it returns, whether it succeeded or not.
    /// </para>
    /// </remarks>
    protected abstract Task StopConnectionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously sends data to the underlying mechanism of this connection.
    /// </summary>
    /// <param name="messageBuffer">The buffer containing the data to be sent to the remote end of this connection.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when an exception is encountered sending data to the remote end of the connection.</exception>
    protected abstract Task SendConnectionDataAsync(ReadOnlyMemory<byte> messageBuffer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously receives data from the remote end of this connection.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected abstract Task ReceiveDataAsync();

    /// <summary>
    /// Asynchronously releases the resources held by this <see cref="Connection"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// This is called once, from <see cref="DisposeAsync"/>, after an active connection has been
    /// stopped. An implementation releases only the resources it owns itself; the resources the base
    /// class owns are released by <see cref="DisposeAsync"/>, which also records that the connection
    /// has been disposed.
    /// </remarks>
    protected abstract ValueTask DisposeAsyncCore();

    /// <summary>
    /// Interprets the connection string given to <see cref="StartAsync"/>, rejecting a value this
    /// connection could never connect to and keeping whatever <see cref="StartConnectionAsync"/> will
    /// need from it.
    /// </summary>
    /// <param name="connectionString">The connection string to interpret.</param>
    /// <exception cref="ArgumentException">
    /// Thrown by an implementation when <paramref name="connectionString"/> is a value it could never
    /// connect to.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is the only place a connection string is interpreted. The base class carries the value but
    /// never reads it, because what counts as a usable connection string is exactly what a transport
    /// knows and nothing above it does. A transport that accepts any string, as a pipe connection does,
    /// need not override this method; the default implementation accepts every value.
    /// </para>
    /// <para>
    /// An implementation rejects a value it could never connect to by throwing
    /// <see cref="ArgumentException"/>. That exception type is the distinction the caller acts on: an
    /// <see cref="ArgumentException"/> out of <see cref="StartAsync"/> means the connection string must
    /// be corrected before starting is worth attempting again, where every other failure means the
    /// attempt itself did not succeed.
    /// </para>
    /// <para>
    /// Validating a connection string and deriving what a connect needs from it are usually the same
    /// act -- parsing a URL both proves it is one and produces the value to connect with -- so this
    /// method does both, and an implementation that must parse keeps the result for
    /// <see cref="StartConnectionAsync"/> rather than parsing a second time there. That is safe to rely
    /// on because <see cref="StartAsync"/> is not overridable: it calls this method on every path that
    /// reaches <see cref="StartConnectionAsync"/>, and nothing between the two can invalidate what this
    /// method resolved.
    /// </para>
    /// <para>
    /// This runs before the state of the connection is examined and before any operation that can take
    /// time, so that rejecting a malformed connection string costs nothing. Were it left to
    /// <see cref="StartConnectionAsync"/>, a caller who passed one would wait out the bounded wait for
    /// a previous session's receive loop first, which on a reconnect can be as long as
    /// <see cref="ShutdownTimeout"/>.
    /// </para>
    /// </remarks>
    protected virtual void ResolveConnectionString(string connectionString)
    {
    }

    /// <summary>
    /// Notifies the single allowed observer of the <see cref="OnDataReceived"/> event that a message
    /// has been received, transferring ownership of the message's pooled memory from
    /// <paramref name="messageBuffer"/> to that observer.
    /// </summary>
    /// <param name="messageBuffer">The <see cref="MessageBuffer"/> holding the accumulated message.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageBuffer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>
    /// This method does nothing when <paramref name="messageBuffer"/> holds no data, so a receive loop may
    /// call it at every point where a message may have completed without first testing
    /// <see cref="MessageBuffer.HasData"/>.
    /// </para>
    /// <para>
    /// When the buffer does hold data, this method takes ownership of its pooled memory block, which leaves
    /// <paramref name="messageBuffer"/> empty and ready to accumulate the next message. Ownership then passes
    /// to the consumer of the <see cref="OnDataReceived"/> event by way of
    /// <see cref="ConnectionDataReceivedEventArgs.BufferOwner"/>, and that consumer is responsible for
    /// returning the block to the pool. A caller must therefore neither dispose the memory nor read from it
    /// after this method returns.
    /// </para>
    /// <para>
    /// The content of a delivered message is logged at the <see cref="WebDriverBiDiLogLevel.Trace"/> level
    /// before the observer is notified.
    /// </para>
    /// <para>
    /// When no observer is attached to <see cref="OnDataReceived"/> there is no consumer to take that
    /// ownership, so the message is discarded and its memory returned to the pool by this method rather
    /// than by an observer. Such a message is not logged, because the logging above describes traffic that
    /// was delivered. A <see cref="Transport"/> attaches its observer before the receive loop starts, so
    /// this applies only to a <see cref="Connection"/> driven without one.
    /// </para>
    /// </remarks>
    protected async Task NotifyDataReceivedObserverAsync(MessageBuffer messageBuffer)
    {
        if (messageBuffer is null)
        {
            throw new ArgumentNullException(nameof(messageBuffer), "The message buffer must not be null");
        }

        if (!messageBuffer.HasData)
        {
            return;
        }

        IMemoryOwner<byte> messageOwner = messageBuffer.TakeOwnership(out int messageLength);
        await this.NotifyDataReceivedObserverAsync(messageOwner, messageLength).ConfigureAwait(false);
    }

    /// <summary>
    /// Notifies the single allowed observer of the <see cref="OnDataReceived"/> event that a message has been
    /// received, transferring ownership of the pooled memory that holds the message to that observer.
    /// </summary>
    /// <param name="messageOwner">
    /// The owner of the memory holding the message, at its start. Ownership passes to this method with the call.
    /// </param>
    /// <param name="messageLength">The length, in bytes, of the message within the memory of <paramref name="messageOwner"/>.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageOwner"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="messageLength"/> is negative or exceeds the length of the memory of
    /// <paramref name="messageOwner"/>. The memory is returned to its pool before the exception is thrown.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Use this overload when the receive loop already holds a complete message in pooled memory, so that it can
    /// hand the message over without first copying it into a <see cref="MessageBuffer"/>. The
    /// <see cref="NotifyDataReceivedObserverAsync(MessageBuffer)"/> overload delivers through this one.
    /// </para>
    /// <para>
    /// Ownership of <paramref name="messageOwner"/> passes to this method whatever the outcome, so a caller neither
    /// disposes the memory nor reads from it after the call. When an observer is attached, ownership passes on to it
    /// by way of <see cref="ConnectionDataReceivedEventArgs.BufferOwner"/>, and that observer returns the memory to
    /// the pool. When no observer is attached, or when the call fails before the observer is notified, this method
    /// returns the memory itself.
    /// </para>
    /// <para>
    /// The message is logged at the <see cref="WebDriverBiDiLogLevel.Trace"/> level before the observer is notified.
    /// It is not logged when no observer is attached, because the logging describes traffic that was delivered.
    /// </para>
    /// </remarks>
    protected async Task NotifyDataReceivedObserverAsync(IMemoryOwner<byte> messageOwner, int messageLength)
    {
        if (messageOwner is null)
        {
            throw new ArgumentNullException(nameof(messageOwner), "The owner of the message memory must not be null");
        }

        int memoryLength = messageOwner.Memory.Length;
        if (messageLength < 0 || messageLength > memoryLength)
        {
            // Ownership passed to this method with the call, so the memory is returned here even though the call is
            // rejected: the caller has no further claim on it, and nothing else will return it.
            messageOwner.Dispose();
            throw new ArgumentOutOfRangeException(nameof(messageLength), $"The message length must be between zero and the length of the memory holding the message ({memoryLength} bytes); received {messageLength}");
        }

        if (this.dataReceivedObservableEvent.CurrentObserverCount == 0)
        {
            messageOwner.Dispose();
            return;
        }

        try
        {
            await this.LogMessageContentAsync(LogReceiveMessagePrefix, messageOwner.Memory, messageLength).ConfigureAwait(false);
        }
        catch
        {
            // A log observer that throws propagates its failure to here, and the notification below
            // will not run, so nothing downstream can return this block to the pool, and ownership has
            // already left the accumulator. Return the block to the pool here, then let the failure
            // travel on unchanged, to the receive loop's own handling, which ends the loop with a
            // connection error. The notification is deliberately outside this block; once it has
            // been entered, ownership belongs to the observer.
            messageOwner.Dispose();
            throw;
        }

        await this.dataReceivedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDataReceivedEventArgs(messageOwner, messageLength)).ConfigureAwait(false);
    }

    /// <summary>
    /// Reports that the receive loop has ended because of an error, logging the error and notifying the observers
    /// of <see cref="OnConnectionError"/>.
    /// </summary>
    /// <param name="logMessage">The message to log at the <see cref="WebDriverBiDiLogLevel.Error"/> level.</param>
    /// <param name="exception">The exception that ended the receive loop.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// <para>
    /// Call this from the receive loop, as its last act, when a failure ends the loop. It is the only way to raise
    /// <see cref="OnConnectionError"/>. The connection reports itself inactive (see <see cref="IsActive"/>) before
    /// the message is logged and before any observer is notified, so an observer reacting to the error, such as a
    /// <see cref="Transport"/> tearing its session down, already sees a connection that must be started again
    /// rather than adopted.
    /// </para>
    /// <para>
    /// The observers are notified even when logging the message fails because an observer of
    /// <see cref="OnLogMessage"/> throws. That failure still propagates to the caller once the observers have been
    /// notified, as the failure of a log observer does anywhere else, unless an observer of
    /// <see cref="OnConnectionError"/> also throws, in which case its failure is the one that propagates.
    /// </para>
    /// </remarks>
    protected async Task NotifyConnectionErrorObserversAsync(string logMessage, Exception exception)
    {
        this.IsReceiveLoopEnded = true;
        try
        {
            await this.LogAsync(logMessage, WebDriverBiDiLogLevel.Error).ConfigureAwait(false);
        }
        finally
        {
            await this.connectionErrorObservableEvent.InvokeNotifyObserversAsync(new ConnectionErrorEventArgs(exception)).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Reports that the receive loop has ended because the remote end closed the connection, notifying the
    /// observers of <see cref="OnRemoteDisconnected"/>.
    /// </summary>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// Call this from the receive loop, as its last act, when the remote end closes the connection. It is the only
    /// way to raise <see cref="OnRemoteDisconnected"/>. The connection reports itself inactive before any observer
    /// is notified, for the reason given on <see cref="NotifyConnectionErrorObserversAsync"/>.
    /// </remarks>
    protected async Task NotifyRemoteDisconnectedObserversAsync()
    {
        this.IsReceiveLoopEnded = true;
        await this.remoteDisconnectedObservableEvent.InvokeNotifyObserversAsync(new ConnectionDisconnectedEventArgs()).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously raises a logging event at the Info log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected Task LogAsync(string message)
    {
        return this.LogAsync(message, WebDriverBiDiLogLevel.Info);
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> at which to raise the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// A message below <see cref="LogLevel"/> is discarded here rather than raised. Callers whose
    /// message is expensive to compose should also test <see cref="IsLogLevelEnabled"/> first, because the
    /// message has already been built by the time it reaches this method.
    /// </remarks>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        if (!this.IsLogLevelEnabled(level))
        {
            return;
        }

        await this.logMessageObservableEvent.InvokeNotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
    }

    /// <summary>
    /// Logs the content of an incoming or outgoing message at the <see cref="WebDriverBiDiLogLevel.Trace"/> level.
    /// </summary>
    /// <param name="logPrefix">The prefix identifying the direction, either <see cref="LogSendMessagePrefix"/> or <see cref="LogReceiveMessagePrefix"/>.</param>
    /// <param name="messageData">The buffer containing the message, which may be longer than <paramref name="messageLength"/> when it comes from a pool.</param>
    /// <param name="messageLength">The length of the message within <paramref name="messageData"/>.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <remarks>
    /// If the <see cref="LogLevel"/> property is set to other than
    /// <see cref="WebDriverBiDiLogLevel.Trace"/>, or if there are no
    /// observers on the OnLogMessage event, this method does nothing.
    /// </remarks>
    private async Task LogMessageContentAsync(string logPrefix, ReadOnlyMemory<byte> messageData, int messageLength)
    {
        if (this.IsLogLevelEnabled(WebDriverBiDiLogLevel.Trace))
        {
#if NET5_0_OR_GREATER
            await this.LogAsync($"{logPrefix}{Encoding.UTF8.GetString(messageData.Span.Slice(0, messageLength))}", WebDriverBiDiLogLevel.Trace).ConfigureAwait(false);
#else
            await this.LogAsync($"{logPrefix}{Encoding.UTF8.GetString(messageData.Slice(0, messageLength).ToArray())}", WebDriverBiDiLogLevel.Trace).ConfigureAwait(false);
#endif
        }
    }

    /// <summary>
    /// Marks this <see cref="Connection"/> as disposed. Use this method to ensure
    /// thread-safe operations for setting object being disposed.
    /// </summary>
    /// <returns><see langword="true"/> if the object was not already disposed before calling this method; otherwise, <see langword="false"/>.</returns>
    private bool SetDisposed()
    {
        return Interlocked.Exchange(ref this.isDisposedFlag, 1) == 0;
    }

    /// <summary>
    /// Starts the task that receives data on this connection, as defined by the implementation
    /// of <see cref="ReceiveDataAsync"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method attaches a continuation to the task on which the receive loop runs. The
    /// receive loop runs on a fire-and-forget task, and shutdown paths are not guaranteed
    /// to await it: <see cref="StopAsync(CancellationToken)"/> abandons the loop when it does
    /// not finish within <see cref="ShutdownTimeout"/>, and disposal skips
    /// <see cref="StopAsync(CancellationToken)"/> entirely when the connection is already
    /// inactive. An abandoned task that faults would otherwise sit unobserved until the
    /// finalizer raises <see cref="TaskScheduler.UnobservedTaskException"/>, which surfaces
    /// as a failure in whatever code happens to be running when the garbage collector runs.
    /// </para>
    /// <para>
    /// Reading <see cref="Task.Exception"/> inside the continuation observes the fault and
    /// yields it for reporting. This does not prevent a shutdown path that does await the task
    /// from seeing the exception.
    /// </para>
    /// </remarks>
    private void StartDataReceiveTask()
    {
        // Start the receive loop
        this.DataReceiveTask = Task.Run(this.ReceiveDataAsync);

        _ = this.DataReceiveTask.ContinueWith(
            this.ReportReceiveLoopFault,
            state: null,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    /// <summary>
    /// Disposes and recreates the connection's <see cref="CancellationTokenSource"/>, readying
    /// this connection for a new session.
    /// </summary>
    /// <remarks>
    /// This is called from <see cref="StartAsync(string, CancellationToken)"/> before any operation
    /// that consumes <see cref="ConnectionCancellationToken"/>. Because
    /// <see cref="StopAsync(CancellationToken)"/> cancels the source unconditionally -- including
    /// when the connection never started -- a session that reuses the previous source would begin
    /// with cancellation already requested and could never connect.
    /// </remarks>
    private void ResetConnectionCancellation()
    {
        this.connectionCancellationTokenSource.Dispose();
        this.connectionCancellationTokenSource = new();

        // Refresh the cached token in the same step that replaces the source, so the two can
        // never disagree. Snapshotting later would leave a window in which the connection is
        // reported as active while the cached token still belongs to the previous, already
        // canceled session.
        this.ConnectionCancellationToken = this.connectionCancellationTokenSource.Token;
    }

    /// <summary>
    /// Requests cancellation of the operations of this connection, signaling the background
    /// receive loop and any in-flight send to stop.
    /// </summary>
    private void CancelConnection()
    {
        this.connectionCancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Waits for the data receive task to complete, bounded by <see cref="ShutdownTimeout"/>. If it does not
    /// finish in time, logs a warning and returns, leaving the receive task to complete on its own.
    /// </summary>
    /// <param name="timeoutLogMessage">
    /// The warning to log when the wait is not satisfied, describing what the connection was waiting to do.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the state of the asynchronous operation.</returns>
    /// <remarks>
    /// A receive loop is waited for at both ends of a session: on the way out, after the connection has been
    /// canceled, and on the way in, so that a loop abandoned by a previous session cannot run alongside the
    /// new one. The wait itself is the same in both cases; only what is reported when it is not satisfied
    /// differs, which is why the message is supplied by the caller.
    /// </remarks>
    private async Task WaitForReceiveTaskCompletionAsync(string timeoutLogMessage)
    {
        if (this.DataReceiveTask is not null)
        {
            using CancellationTokenSource shutdownDelayCancelTokenSource = new();
            Task completedTask = await Task.WhenAny(this.DataReceiveTask, TimeoutUtilities.DelayAsync(this.TimeProvider, this.ShutdownTimeout, shutdownDelayCancelTokenSource.Token)).ConfigureAwait(false);
            if (completedTask != this.DataReceiveTask)
            {
                await this.LogAsync(timeoutLogMessage, WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }
            else
            {
                shutdownDelayCancelTokenSource.Cancel();
            }
        }
    }

    /// <summary>
    /// Waits for exclusive send access, bounded by <see cref="DataTimeout"/> as measured by
    /// <see cref="TimeProvider"/>.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns><see langword="true"/> if access was acquired; <see langword="false"/> if the timeout elapsed first.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <remarks>
    /// The timeout is applied by canceling the wait rather than by racing it against a delay, so a
    /// wait that is abandoned can never acquire the semaphore later and leave it held forever. A
    /// zero timeout keeps its non-blocking meaning: the semaphore is taken only if it is free now.
    /// </remarks>
    private async Task<bool> WaitForSendAccessAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (this.DataSendSemaphore.Wait(0))
        {
            return true;
        }

        if (this.DataTimeout == TimeSpan.Zero)
        {
            return false;
        }

        using CancellationTokenSource timeoutTokenSource = TimeoutUtilities.CreateCancellationTokenSource(this.TimeProvider, this.DataTimeout);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
        try
        {
            await this.DataSendSemaphore.WaitAsync(linkedTokenSource.Token).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private void ReportReceiveLoopFault(Task faultedTask, object? state)
    {
        // Reading Task.Exception observes the fault, which is the point of this continuation.
        // The property is guaranteed non-null here because the continuation is scheduled with
        // OnlyOnFaulted, so the null-forgiving operator is appropriate. AggregateException.Message
        // already incorporates the messages of its inner exceptions, so no unwrapping is needed.
        AggregateException aggregateException = faultedTask.Exception!;

        // Use EventSource rather than LogAsync to keep this fire-and-forget fault handler
        // synchronous; awaiting the log pipeline here would create another unobserved task.
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError(this.Id, aggregateException.Message);
    }
}
