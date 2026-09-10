// <copyright file="Transport.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;
using WebDriverBiDi.Internal;
using WebDriverBiDi.JsonConverters;

/// <summary>
/// The transport object used for serializing and deserializing JSON data used in the WebDriver Bidi protocol.
/// It uses a <see cref="Connection"/> object to communicate with the remote end, and does no further processing
/// of the objects serialized or deserialized. Consumers of this class are expected to handle things like awaiting
/// the response of a WebDriver BiDi command message.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Message Queue Architecture:</strong>
/// This transport uses an unbounded <see cref="System.Threading.Channels.Channel{T}"/> for message processing.
/// Messages received from the connection are queued and processed sequentially by a dedicated reader task.
/// </para>
/// <para>
/// <strong>Memory Considerations:</strong>
/// The unbounded queue means there is no limit on the number of messages that can be buffered if they
/// arrive faster than they can be processed. In typical usage, message processing is fast enough that
/// queue depth remains minimal. However, in high-throughput scenarios (e.g., thousands of rapid events
/// or very slow event handlers), memory consumption could grow significantly.
/// </para>
/// <para>
/// <strong>Performance Characteristics:</strong>
/// The single-reader, single-writer queue design provides optimal throughput for the typical case where
/// messages arrive sequentially and are processed quickly. Event handlers that perform slow operations
/// (I/O, CPU-intensive work) should use <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>
/// to avoid blocking the message processing thread.
/// </para>
/// <para>
/// <strong>Monitoring Queue Behavior:</strong>
/// Use <see cref="IncomingQueueDepth"/> to monitor the number of messages buffered in the queue.
/// A persistently growing value indicates that event handlers are not keeping up with the incoming
/// message rate. If you suspect message backlog issues, consider:
/// <list type="bullet">
/// <item><description>Using <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> for all event handlers that perform I/O or take more than a few milliseconds</description></item>
/// <item><description>Reducing the frequency of subscribed events if not all are needed</description></item>
/// <item><description>Implementing throttling or filtering logic in your event handlers</description></item>
/// </list>
/// </para>
/// <para>
/// <strong>Thread Safety:</strong>
/// This class is thread-safe. <see cref="SendCommandAsync"/> may be called concurrently from multiple threads. Connection lifecycle operations
/// (<see cref="ConnectAsync"/>, <see cref="DisconnectAsync(CancellationToken)"/>) are serialized via an internal semaphore.
/// Both <see cref="DisconnectAsync(CancellationToken)"/> and <see cref="SendCommandAsync"/> check
/// the connected state before taking the semaphore (and again after acquiring it). This prevents a
/// deadlock when a disconnect is triggered while another caller already holds the lock, and lets a
/// command sent from a synchronous event handler while
/// <see cref="DisconnectAsync(CancellationToken)"/> is in progress fail immediately with
/// <see cref="WebDriverBiDiConnectionException"/> rather than blocking on the semaphore for the
/// duration of the shutdown wait. Event observers may be added or removed concurrently with message
/// processing.
/// </para>
/// </remarks>
public class Transport : IAsyncDisposable, ITransportConfiguration, ITransportDiagnostics
{
    /// <summary>
    /// Gets the component name for this class to use in log messages.
    /// </summary>
    public const string LoggerComponentName = "Transport";

    private const string EventReceivedEventName = "transport.eventReceived";
    private const string ErrorReceivedEventName = "transport.errorReceived";
    private const string UnknownMessageReceivedEventName = "transport.unknownMessageReceived";
    private const string EventHandlerErrorOccurredEventName = "transport.eventHandlerErrorOccurred";
    private const string LogMessageEventName = "transport.logMessage";
    private const string TransportEventObserverDescription = "transport event observer";
    private const string TransportErrorObserverDescription = "transport error observer";
    private const string TransportUnknownMessageObserverDescription = "transport unknown message observer";
    private const string TransportLogMessageObserverDescription = "transport log message observer";

    private const string NormalShutdownReason = "Normal shutdown";

    private readonly ObservableEventInvocable<EventReceivedEventArgs> invocableEventReceivedObservableEvent;
    private readonly ObservableEventInvocable<ErrorReceivedEventArgs> invocableErrorReceivedObservableEvent;
    private readonly ObservableEventInvocable<UnknownMessageReceivedEventArgs> invocableUnknownMessageReceivedObservableEvent;
    private readonly ObservableEventInvocable<EventHandlerErrorOccurredEventArgs> invocableErrorHandlerErrorOccurredObservableEvent;
    private readonly ObservableEventInvocable<LogMessageEventArgs> invocableLogMessageObservableEvent;

    private readonly ConcurrentDictionary<string, EventMessageRegistration> eventMessageTypes = [];
    private readonly ConcurrentDictionary<Type, JsonTypeInfo> responseTypeInfoCache = [];
    private readonly SemaphoreSlim connectDisconnectSemaphore = new(1, 1);

    // Guards the publication of the Connecting state against work that is only legal while this
    // transport is disconnected (see TryExecuteWhileDisconnected). It is deliberately an instance
    // lock: the state it protects is per-transport, and the action passed to
    // TryExecuteWhileDisconnected runs while the lock is held, so a process-wide lock would let
    // one transport's registration block every other transport in the process.
    private readonly object connectionStateLock = new();

    private readonly EventObserver<ConnectionDataReceivedEventArgs> connectionDataReceivedObserver;
    private readonly EventObserver<ConnectionErrorEventArgs> connectionErrorObserver;
    private readonly EventObserver<ConnectionDisconnectedEventArgs> connectionRemoteDisconnectObserver;
    private readonly EventObserver<LogMessageEventArgs> connectionLogMessageObserver;

    // These are re-derived by RebuildSerializerStateWithResolver when a type info resolver is
    // registered on a disconnected transport, so they are not readonly.
    private JsonTypeInfo<Command> commandJsonTypeInfo;
    private JsonTypeInfo<ErrorResponseMessage> errorResponseJsonTypeInfo;
    private JsonSerializerOptions options = new()
    {
        TypeInfoResolver = CreateTypeInfoResolver(),
        RespectNullableAnnotations = true,
    };

    private IncomingMessageQueue incomingMessageQueue = new();

    private Task messageQueueProcessingTask = Task.CompletedTask;
    private long nextCommandId = 0;
    private string terminationReason = NormalShutdownReason;

    // TaskCompletionSource used as a signal that DisconnectAsync owns the attempt
    // at shutdown of the Transport. This field is refreshed in ConnectAsync when
    // a new session is created. Used so that a terminated Connection does not
    // enter a circular lock.
    private TaskCompletionSource<int> disconnectOwnedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);

    // Records a connection loss reported while a connect attempt is still in flight, which
    // HandleConnectionDisconnectionAsync cannot act on because there is not yet a session to tear
    // down. ConnectAsync reads and clears it, and fails the attempt rather than publishing the
    // Connected state over a connection that is already gone. The exception the connection reported
    // is kept rather than a flag, so the failure names the cause.
    private WebDriverBiDiConnectionException? connectionLostWhileConnecting;

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required.
    // Backing store for the State property; holds a TransportState value. Zero is
    // TransportState.Disconnected, matching the field's default.
    private int transportStateValue = (int)TransportState.Disconnected;
    private int isDisposedFlag = 0;
    private int collectedErrorsReportedFlag = 0;

    private TimeSpan shutdownTimeout = TimeSpan.FromSeconds(10);
    private TimeSpan connectionLockTimeout = TimeSpan.FromSeconds(60);

    // Message/event sent/received statistics
    private long commandMessagesSent = 0;
    private long commandResponseMessagesReceived = 0;
    private long eventMessagesReceived = 0;
    private long errorMessagesReceived = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class.
    /// </summary>
    public Transport()
        : this(new WebSocketConnection())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Transport"/> class with a given command timeout and connection.
    /// </summary>
    /// <param name="connection">The <see cref="Connection"/> used to communicate with the protocol remote end.</param>
    /// <exception cref="ArgumentNullException">Thrown when a null is passed for the connection.</exception>
    /// <exception cref="ArgumentException">Thrown when the provided <see cref="Connection"/> already has an observer for its OnDataReceived event.</exception>
    public Transport(Connection connection)
    {
        if (connection is null)
        {
            throw new ArgumentNullException(nameof(connection), "Connection must not be null");
        }

        if (connection.OnDataReceived.CurrentObserverCount > 0)
        {
            throw new ArgumentException("The provided connection already has a listener for its OnDataReceived event.", nameof(connection));
        }

        this.Connection = connection;

        // Cache the JsonTypeInfo for JSON serialized or deserialized classes for perf reasons.
        this.commandJsonTypeInfo = (JsonTypeInfo<Command>)this.options.GetTypeInfo(typeof(Command));
        this.errorResponseJsonTypeInfo = (JsonTypeInfo<ErrorResponseMessage>)this.options.GetTypeInfo(typeof(ErrorResponseMessage));

        this.invocableEventReceivedObservableEvent = this.CreateObservableEvent<EventReceivedEventArgs>(EventReceivedEventName);
        this.invocableErrorReceivedObservableEvent = this.CreateObservableEvent<ErrorReceivedEventArgs>(ErrorReceivedEventName);
        this.invocableUnknownMessageReceivedObservableEvent = this.CreateObservableEvent<UnknownMessageReceivedEventArgs>(UnknownMessageReceivedEventName);
        this.invocableErrorHandlerErrorOccurredObservableEvent = this.CreateObservableEvent<EventHandlerErrorOccurredEventArgs>(EventHandlerErrorOccurredEventName);
        this.invocableLogMessageObservableEvent = this.CreateObservableEvent<LogMessageEventArgs>(LogMessageEventName);

        // Route failures of observers of the connection's own events through this transport's
        // unhandled-error pipeline, so that a fault in a user's asynchronously-run observer of
        // (for example) Connection.OnLogMessage is governed by EventHandlerExceptionBehavior
        // exactly like a fault in an observer of a transport or module event, rather than being
        // observed and then discarded. This applies to observers the connection already carries,
        // because the reporter is read when a fault is reported, not when an observer is added.
        connection.SetObserverErrorReporter(this.ReportEventObserverErrorAsync);

        this.connectionDataReceivedObserver = connection.OnDataReceived.AddObserver(this.OnConnectionDataReceivedAsync);
        this.connectionErrorObserver = connection.OnConnectionError.AddObserver(this.OnConnectionErrorAsync);
        this.connectionRemoteDisconnectObserver = connection.OnRemoteDisconnected.AddObserver(this.OnConnectionRemotelyDisconnectedAsync);
        this.connectionLogMessageObserver = connection.OnLogMessage.AddObserver(this.OnConnectionLogMessageAsync);
    }

    /// <summary>
    /// Gets an observable event that notifies when an event is received from the protocol.
    /// </summary>
    public ObservableEvent<EventReceivedEventArgs> OnEventReceived => this.invocableEventReceivedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when an error is received from the protocol
    /// that is not the result of a command execution.
    /// </summary>
    public ObservableEvent<ErrorReceivedEventArgs> OnErrorEventReceived => this.invocableErrorReceivedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when an unknown message is received from the protocol.
    /// </summary>
    public ObservableEvent<UnknownMessageReceivedEventArgs> OnUnknownMessageReceived => this.invocableUnknownMessageReceivedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when an error occurs in an observer of an observable event.
    /// </summary>
    public ObservableEvent<EventHandlerErrorOccurredEventArgs> OnEventHandlerErrorOccurred => this.invocableErrorHandlerErrorOccurredObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a log message is written.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.invocableLogMessageObservableEvent;

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when an
    /// unhandled exception in a handler for a defined protocol is encountered. Defaults to
    /// ignoring exceptions, in which case, those exceptions will never be surfaced to the user.
    /// </summary>
    public TransportErrorBehavior EventHandlerExceptionBehavior { get => this.UnhandledErrors.EventHandlerExceptionBehavior; set => this.UnhandledErrors.EventHandlerExceptionBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when a
    /// protocol error is encountered, such as invalid JSON or JSON missing required properties.
    /// Defaults to ignoring exceptions, in which case, those exceptions will never be surfaced
    /// to the user.
    /// </summary>
    public TransportErrorBehavior ProtocolErrorBehavior { get => this.UnhandledErrors.ProtocolErrorBehavior; set => this.UnhandledErrors.ProtocolErrorBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when an
    /// unknown message is encountered, such as valid JSON that does not match any protocol data
    /// structure. Defaults to ignoring exceptions, in which case, those exceptions will never
    /// be surfaced to the user. A response for a command that has timed out or been canceled is
    /// not an unknown message; it is recognized, logged, and discarded (see
    /// <see cref="CancelCommand(Command, CommandCancellationReason)"/>).
    /// </summary>
    public TransportErrorBehavior UnknownMessageBehavior { get => this.UnhandledErrors.UnknownMessageBehavior; set => this.UnhandledErrors.UnknownMessageBehavior = value; }

    /// <summary>
    /// Gets or sets a value indicating how this <see cref="Transport"/> should behave when an
    /// unexpected error is encountered, meaning an error response received with no corresponding
    /// command. Defaults to ignoring exceptions, in which case, those exceptions will never be
    /// surfaced to the user. An error response for a command that has timed out or been canceled is
    /// not an unexpected error; it is recognized, logged, and discarded (see
    /// <see cref="CancelCommand(Command, CommandCancellationReason)"/>).
    /// </summary>
    public TransportErrorBehavior UnexpectedErrorBehavior { get => this.UnhandledErrors.UnexpectedErrorBehavior; set => this.UnhandledErrors.UnexpectedErrorBehavior = value; }

    /// <summary>
    /// Gets or sets the minimum <see cref="WebDriverBiDiLogLevel"/> at which log messages are raised.
    /// Defaults to <see cref="WebDriverBiDiLogLevel.Info"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// By default this is the same setting as <see cref="Protocol.Connection.LogLevel"/> on the connection
    /// this transport wraps, which holds it for the whole pipeline; setting it here sets it for the
    /// connection's messages as well as this transport's own. Raise it to
    /// <see cref="WebDriverBiDiLogLevel.Debug"/> for per-command messages, or to
    /// <see cref="WebDriverBiDiLogLevel.Trace"/> to also see the raw protocol traffic the connection logs.
    /// </para>
    /// <para>
    /// A derived transport may override this to keep a level of its own rather than share the
    /// connection's. Note what that decouples: <see cref="IsLogLevelEnabled"/> and this transport's
    /// <c>LogAsync</c> read this property, and so does the driver through
    /// <see cref="BiDiDriver.TransportConfiguration"/>, so all three
    /// follow the override; the connection keeps filtering its own messages — the <c>SEND</c> and
    /// <c>RECV</c> traffic among them — by <see cref="Protocol.Connection.LogLevel"/>. An override whose
    /// setter also assigns <see cref="Protocol.Connection.LogLevel"/> keeps the whole pipeline together.
    /// </para>
    /// </remarks>
    public virtual WebDriverBiDiLogLevel LogLevel { get => this.Connection.LogLevel; set => this.Connection.LogLevel = value; }

    /// <summary>
    /// Gets or sets the timeout to wait for message processing to complete during shutdown.
    /// If message processing does not complete within this timeout, the shutdown stops waiting and
    /// proceeds, and any pending commands are canceled. The default is 10 seconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This timeout applies to waiting for the incoming message queue to empty, to waiting for
    /// the messages in the queue to be processed, and, during disposal, to waiting for an
    /// in-flight connect attempt to complete before the transport's resources are released.
    /// </para>
    /// <para>
    /// Abandoning the wait does not stop the reader. Messages already delivered to the queue go on
    /// being processed in the background, and their handlers go on running; what this timeout bounds is
    /// how long the shutdown waits for them, not whether they run. A subsequent <see cref="ConnectAsync"/>
    /// waits for that processing to finish, bounded by this same timeout, before opening a new
    /// connection.
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
    /// Gets or sets the timeout to wait for exclusive access to this transport's connection while
    /// another operation holds it. The default is 60 seconds.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="ConnectAsync"/>, <see cref="DisconnectAsync(CancellationToken)"/>,
    /// <see cref="SendCommandAsync"/> and <see cref="RegisterTypeInfoResolverAsync"/> each take
    /// exclusive access for the duration of their work, so one of them waits while another is in
    /// progress. Bounding that wait keeps an operation issued from code this transport itself
    /// invoked while holding the access — a synchronous observer of <see cref="OnLogMessage"/> that
    /// sends a command, for example — from waiting on an operation that is itself waiting on the
    /// observer to return. Such an operation fails with <see cref="WebDriverBiDiTimeoutException"/>
    /// instead of never completing. An observer that needs to drive the transport should be
    /// registered with <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/> so that
    /// it does not hold up the operation it was dispatched from.
    /// </para>
    /// <para>
    /// The default is deliberately longer than the longest legitimate hold, so that lowering it is
    /// a deliberate choice rather than a trap. A disconnect that exhausts every wait it is allowed
    /// holds the access for the connection's close handshake and for its receive-loop wait (each
    /// bounded by <see cref="Connection.ShutdownTimeout"/>), and then for the message-queue drain
    /// (bounded by <see cref="ShutdownTimeout"/>), which is about 30 seconds at the default
    /// settings. A value shorter than the longest hold a session can legitimately take will fail
    /// operations that would otherwise have succeeded. <see cref="TimeSpan.Zero"/> never waits, and
    /// <see cref="Timeout.InfiniteTimeSpan"/> restores an unbounded wait.
    /// </para>
    /// <para>
    /// Handling a lost connection waits for the access too, on the connection's receive loop. A wait
    /// abandoned there leaves the session standing rather than tearing it down without the access:
    /// the transport stays <see cref="TransportState.Connected"/>, its pending commands end at their
    /// own timeouts, and a <see cref="WebDriverBiDiLogLevel.Warn"/> message naming the cause is
    /// raised on <see cref="OnLogMessage"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is negative (other than <see cref="Timeout.InfiniteTimeSpan"/>) or exceeds
    /// the maximum timer duration supported by the runtime.
    /// </exception>
    public TimeSpan ConnectionLockTimeout
    {
        get => this.connectionLockTimeout;
        set
        {
            if (!TimeoutUtilities.IsValidTimeout(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), TimeoutUtilities.GetInvalidTimeoutMessage("Connection lock timeout"));
            }

            this.connectionLockTimeout = value;
        }
    }

    /// <summary>
    /// Gets the number of messages currently buffered in the incoming message queue and
    /// waiting to be processed by the reader task.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property surfaces the depth of the unbounded <see cref="Channel{T}"/>
    /// described in the class-level remarks. It is intended for diagnostics and observability:
    /// a persistently growing value indicates that event handlers are not keeping up with the
    /// incoming message rate, and should prompt investigation of handler duration or the use of
    /// <see cref="ObservableEventHandlerOptions.RunHandlerAsynchronously"/>.
    /// </para>
    /// <para>
    /// The value reflects the queue for the <em>current</em> connection. Each call to
    /// <see cref="ConnectAsync"/> installs a fresh queue whose depth begins at zero, and every
    /// message is counted against the queue it was written to for as long as that queue is being
    /// drained. A reconnect that gives up waiting for the previous connection's reader therefore
    /// reports only the current connection's backlog, even while the previous reader is still
    /// draining what remains of its own queue. Reading this property before
    /// <see cref="ConnectAsync"/> has ever been called, or after
    /// <see cref="DisconnectAsync(CancellationToken)"/>, returns the depth of the remaining
    /// (possibly drained) queue rather than throwing.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This property is safe to read concurrently with message
    /// production and consumption. The returned value is a snapshot and may be stale by the time
    /// the caller observes it.
    /// </para>
    /// </remarks>
    public virtual int IncomingQueueDepth => this.incomingMessageQueue.Depth;

    /// <summary>
    /// Gets the number of commands that have been sent to the remote end and are
    /// awaiting a response.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property is intended for diagnostics and observability alongside
    /// <see cref="IncomingQueueDepth"/>. A persistently high value suggests that the
    /// remote end is not responding promptly, or that a burst of commands is in flight
    /// without corresponding responses yet.
    /// </para>
    /// <para>
    /// Reading this property before <see cref="ConnectAsync"/> has ever been called, or
    /// after <see cref="DisconnectAsync(CancellationToken)"/>, returns the count of the
    /// pending-command collection in its current state rather than throwing. The
    /// collection is cleared during <see cref="DisconnectAsync(CancellationToken)"/>,
    /// so reads after a disconnect typically return zero.
    /// </para>
    /// <para>
    /// <strong>Thread Safety:</strong> This property is safe to read concurrently with
    /// command send and response processing. The returned value is a snapshot and may
    /// be stale by the time the caller observes it.
    /// </para>
    /// </remarks>
    public virtual int PendingCommandCount => this.PendingCommands.PendingCommandCount;

    /// <summary>
    /// Gets a value indicating the lifecycle state of this transport with respect to its connection
    /// to a remote end. The returned value is a snapshot and may be stale by the time the caller
    /// observes it.
    /// </summary>
    /// <remarks>
    /// Every state transition is performed while holding the connection lock (or, in the case of a
    /// connection-loss transition, while racing that lock for ownership), so the setter is an
    /// unconditional atomic publish; no compare-and-swap is required.
    /// </remarks>
    public TransportState State
    {
        get => (TransportState)Interlocked.CompareExchange(ref this.transportStateValue, 0, 0);
        private set => Interlocked.Exchange(ref this.transportStateValue, (int)value);
    }

    /// <summary>
    /// Gets a value indicating whether this transport is disposed.
    /// </summary>
    internal bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Gets or sets the collection of pending commands that have been sent and
    /// have not yet received a response. This collection is thread-safe.
    /// </summary>
    protected PendingCommandCollection PendingCommands { get; set; } = new();

    /// <summary>
    /// Gets the ID of the last command to be added.
    /// </summary>
    /// <remarks>
    /// This method uses <c>Interlocked.Read</c> for downlevel compatibility
    /// with legacy and 32-bit framework implementations.
    /// </remarks>
    protected long LastCommandId => Interlocked.Read(ref this.nextCommandId);

    /// <summary>
    /// Gets the connection used to communicate with the browser.
    /// </summary>
    protected Connection Connection { get; }

    /// <summary>
    /// Gets the collection of unhandled errors captured by this transport. This collection is thread-safe.
    /// Use this collection to inspect unhandled errors that have been captured, and to clear
    /// captured errors if desired.
    /// </summary>
    protected UnhandledErrorCollection UnhandledErrors { get; } = new();

    /// <summary>
    /// Gets or sets the <see cref="TimeProvider"/> whose clock measures this transport's
    /// <see cref="ShutdownTimeout"/> waits. Defaults to <see cref="TimeProvider.System"/>. A derived type
    /// may substitute another, for example to drive the waits with virtual time in a test, in the same
    /// way that <see cref="ObservableEvent{T}"/> exposes its provider to derived types. The
    /// <see cref="Connection"/> measures its own timeouts with its own provider.
    /// </summary>
    protected TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    private string TerminationReason
    {
        get
        {
            return Interlocked.CompareExchange(ref this.terminationReason, string.Empty, string.Empty);
        }

        set
        {
            Interlocked.Exchange(ref this.terminationReason, value);
        }
    }

    /// <summary>
    /// Asynchronously connects to the remote end web socket.
    /// </summary>
    /// <param name="connectionString">The URI used to connect to the web socket.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiConnectionException">
    /// Thrown when the transport is already connected to a remote end, when the <see cref="Connection"/>
    /// refuses to open, or when the connection is lost while the session is being established. The last
    /// case carries the loss the connection reported as its inner exception; the transport is left
    /// disconnected, so a further attempt may be made.
    /// </exception>
    /// <exception cref="WebDriverBiDiTimeoutException">
    /// Propagated from <see cref="Connection.StartAsync"/> when the connection is not established within
    /// its <see cref="Connection.StartupTimeout"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Propagated from <see cref="Connection.StartAsync"/> when <paramref name="connectionString"/> is not
    /// acceptable to the connection. <see cref="WebSocketConnection"/> throws this when the value is not a
    /// valid absolute URI, or when its scheme is neither <c>ws</c> nor <c>wss</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when attempting to call this method after the transport is disposed.</exception>
    /// <remarks>
    /// Connecting starts a new session and clears the errors the previous session accumulated under
    /// <see cref="TransportErrorBehavior.Collect"/>. Those errors are thrown only by
    /// <see cref="DisconnectAsync(CancellationToken)"/>. After a remote disconnect the transport is already in the
    /// <see cref="TransportState.Disconnected"/> state, so this method proceeds; to observe the errors
    /// collected up to the disconnect, call <see cref="DisconnectAsync(CancellationToken)"/> (which returns promptly and
    /// throws them) before reconnecting. Reconnecting directly discards them.
    /// </remarks>
    public virtual async Task ConnectAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        await this.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.State != TransportState.Disconnected)
            {
                throw new WebDriverBiDiConnectionException($"The transport is already connected to {this.Connection.ConnectionString}; you must disconnect before connecting to another URL");
            }

            // Publish the in-flight state before any awaits so that callers observing State (for
            // example the driver's registration guard) treat the transport as no longer idle for the
            // entire duration of the connect attempt, not only once it completes. The finally below
            // rolls this back to Disconnected if the attempt fails before reaching Connected.
            // The publication is made under the connection state lock so that it cannot interleave
            // with work another thread is performing under TryExecuteWhileDisconnected: such work
            // either completes entirely before this transport stops being idle, or observes the
            // transport as no longer idle and is rejected.
            lock (this.connectionStateLock)
            {
                this.State = TransportState.Connecting;
            }

            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpening(this.Connection.Id, connectionString);
            await this.LogAsync("Transport connecting", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);

            // SendCommandAsync requires that the pending commands collection be
            // replaced whenever the nextCommandId is reset, to prevent potentially
            // adding commands with colliding IDs to the collection. Every path for
            // disconnection closes the collection, marking it as not accepting
            // commands, so we must replace it here.
            if (!this.PendingCommands.IsAcceptingCommands)
            {
                this.PendingCommands.Dispose();
                this.PendingCommands = new PendingCommandCollection();
            }

            // When reconnecting after disconnect, the message processing of the previous
            // connection may not be complete, even if the reader is completed during
            // disconnect. Wait for that processing to complete before recreating the
            // message processing task.
            await this.WaitForMessageProcessingCompletionAsync(this.ShutdownTimeout, "Timed out waiting for message processing of the previous connection to complete before reconnecting").ConfigureAwait(false);

            this.incomingMessageQueue = new IncomingMessageQueue();
            Interlocked.Exchange(ref this.disconnectOwnedSignal, new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously));

            // Discard any loss recorded against a previous attempt, so that this attempt is judged
            // only by what happens to its own connection. This must precede Connection.StartAsync
            // below, because a loss reported from the moment that call begins belongs to this attempt.
            Interlocked.Exchange(ref this.connectionLostWhileConnecting, null);

            this.ResetCollectedErrors();

            // Reset the termination reason to its default so a reason carried over from a prior
            // Terminate-triggered session is not reported by a subsequent normal shutdown.
            this.TerminationReason = NormalShutdownReason;

            // Reset the command counter for each connection.
            Interlocked.Exchange(ref this.nextCommandId, 0);

            // Reset the message send/receive statistics for this session.
            Interlocked.Exchange(ref this.commandMessagesSent, 0);
            Interlocked.Exchange(ref this.commandResponseMessagesReceived, 0);
            Interlocked.Exchange(ref this.eventMessagesReceived, 0);
            Interlocked.Exchange(ref this.errorMessagesReceived, 0);

            if (!this.Connection.IsActive)
            {
                // Allow for the possibility of the connection to already being opened.
                await this.Connection.StartAsync(connectionString, cancellationToken).ConfigureAwait(false);
            }

            // The connection's receive loop is already running by the time StartAsync returns, so the
            // remote end can close, or the loop can fail, before the Connected state is published just
            // below. Record that loss of connection, if it exists.
            WebDriverBiDiConnectionException? connectionLost = Interlocked.Exchange(ref this.connectionLostWhileConnecting, null);
            if (connectionLost is not null)
            {
                // The reader is never started for this attempt, so anything the remote end managed to
                // push into the queue would otherwise be abandoned holding its pooled buffer.
                this.incomingMessageQueue.Drain();
                throw new WebDriverBiDiConnectionException("The connection was lost while the session was being established; the remote end closed it, or the connection reported an error, before the transport finished connecting.", connectionLost);
            }

            // Delaying starting the processing loop until after establishing the connection
            // shouldn't be an issue, as we are using a Channel for processing the data, which
            // should buffer the data until the first read. If the underlying data structure
            // changes, this logic may need to be refactored.
            this.State = TransportState.Connected;
            this.messageQueueProcessingTask = Task.Run(() => this.ReadIncomingMessagesAsync(), CancellationToken.None);

            // Defence-in-depth: ReadIncomingMessagesAsync catches per-message exceptions in
            // its inner loop, so under normal operation this continuation never fires. It
            // exists to observe a fault on the outer `await WaitToReadAsync()` path so that
            // the fault is logged and captured on the unhandled-error pipeline rather than
            // sitting unobserved on the task until DisconnectAsync awaits it. Accessing
            // Task.Exception inside the continuation both observes the fault (preventing
            // UnobservedTaskException on GC) and yields it for reporting.
            _ = this.messageQueueProcessingTask.ContinueWith(
                this.LogMessageProcessingFault,
                state: null,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            WebDriverBiDiEventSource.RaiseEvent.ConnectionOpened(this.Connection.Id, connectionString);
            WebDriverBiDiEventSource.RaiseEvent.TransportStarted();
        }
        finally
        {
            // If the attempt did not reach the Connected state (the body threw or was canceled after
            // Connecting was published), roll the transport back to Disconnected so a later
            // ConnectAsync is permitted and no observer is left seeing a stuck Connecting state. A
            // still-Connecting state is the exact signal that the attempt did not complete; reaching
            // Connected, or rejecting a non-idle transport before publishing Connecting, leaves the
            // state untouched here.
            if (this.State == TransportState.Connecting)
            {
                this.State = TransportState.Disconnected;
            }

            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">
    /// Thrown when <see cref="TransportErrorBehavior.Collect"/> is configured for any error category and one
    /// or more errors were collected during the session. The aggregated exceptions describe the collected
    /// errors. They are thrown at most once per session, by whichever disconnect claims them.
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await this.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously sends a command to the remote end.
    /// </summary>
    /// <param name="commandData">The command settings object containing all data required to execute the command.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    /// <exception cref="WebDriverBiDiException">Thrown if the command ID is already in use.</exception>
    /// <exception cref="WebDriverBiDiSerializationException">Thrown if the command parameters cannot be serialized to JSON.</exception>
    /// <exception cref="WebDriverBiDiConnectionException">Thrown when the transport is not connected to a remote end.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the command parameters are null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    public virtual async Task<Command> SendCommandAsync(CommandParameters commandData, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (commandData is null)
        {
            throw new ArgumentNullException(nameof(commandData), "Command parameters must not be null");
        }

        if (this.UnhandledErrors.TryGetExceptions(TransportErrorBehavior.Terminate, out IList<Exception> terminationExceptions))
        {
            await this.DisconnectAsync(false).ConfigureAwait(false);
            throw this.CreateTerminationException(terminationExceptions);
        }

        // Fast-path guard, mirroring the one in DisconnectAsync: DisconnectAsync marks the
        // transport disconnected before it starts waiting (up to ShutdownTimeout) for the
        // message-processing task while holding the connection lock. A command sent from a
        // synchronous event handler during that window would otherwise block on the lock for
        // the whole shutdown wait before failing; checking here lets it fail immediately. The
        // state is re-checked under the lock below, so this is an optimization, not the
        // correctness guarantee.
        if (this.State != TransportState.Connected)
        {
            throw new WebDriverBiDiConnectionException("Transport must be connected to a remote end to execute commands.");
        }

        // Capture the current pending command collection ID to allow us to detect
        // if the connection has been disconnected and reconnected before we actually
        // send the command down the wire.
        string currentPendingCommandCollectionId = this.PendingCommands.Id;

        // Serialize the command immediately. This happens synchronously, as
        // we are doing it before the first async call in this method. We do
        // this to make the command parameters immutable for this command
        // execution and prevent modification of the parameters while the
        // command is being sent.
        Command command = this.CreateCommand(commandData);
        byte[] commandJson;
        try
        {
            commandJson = this.SerializeCommand(command);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // A JsonException is raised for values JSON cannot represent (for example, a NaN
            // double); a NotSupportedException is raised when a value's type has no serialization
            // metadata (for example, an unregistered AdditionalData value type under AOT). Surface
            // both through the library's own serialization exception type, as is done for
            // failures to deserialize a response.
            throw new WebDriverBiDiSerializationException($"Could not serialize command '{command.CommandName}' (command ID: {command.CommandId}): {ex.Message}", ex);
        }

        // Notify log-message observers before acquiring the connection lock. Emitting this inside the
        // lock would deadlock if a synchronous observer re-entered the transport (for example, by
        // sending another command or disconnecting): the observer would block acquiring the same
        // non-reentrant lock the notifying call already holds. The command is already created and
        // serialized here, so its name and id are available, and this keeps the notification ordered
        // ahead of the send.
        if (this.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug))
        {
            await this.LogAsync($"Sending command data for command '{command.CommandName}' (command ID: {command.CommandId})", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
        }

        await this.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.State != TransportState.Connected)
            {
                throw new WebDriverBiDiConnectionException("Transport must be connected to a remote end to execute commands.");
            }

            if (currentPendingCommandCollectionId != this.PendingCommands.Id)
            {
                throw new WebDriverBiDiConnectionException("The connection was replaced while the command was being prepared; the command was not sent. Retry the command on the current connection.");
            }

            await this.PendingCommands.AddPendingCommandAsync(command, cancellationToken).ConfigureAwait(false);
            try
            {
                // Start timing and raise the command-sending event. The log-message notification for
                // this command was emitted before the connection lock was acquired (see above), so a
                // synchronous observer of it re-enters the transport without contending for the lock at
                // all. That hoist does not cover the connection's own Trace-level traffic message, which
                // Connection.SendDataAsync raises below while this lock is held: an observer of that
                // message which re-enters the transport waits for a lock this call holds, and is bounded
                // by ConnectionLockTimeout rather than waiting indefinitely.
                command.StartTiming();
                WebDriverBiDiEventSource.RaiseEvent.CommandSending(command.CommandId, command.CommandName);

                await this.Connection.SendDataAsync(commandJson, cancellationToken).ConfigureAwait(false);

                Interlocked.Increment(ref this.commandMessagesSent);
                WebDriverBiDiEventSource.RaiseEvent.PendingCommandCount(this.PendingCommands.PendingCommandCount);

                return command;
            }
            catch (Exception ex)
            {
                // Command failed to send, so roll back adding the command to the pending command
                // collection, and emit the event for the failure.
                if (this.PendingCommands.RemovePendingCommand(command.CommandId, out _))
                {
                    command.StopTiming();
                }

                WebDriverBiDiEventSource.RaiseEvent.CommandSendFailed(command.CommandId, command.CommandName, ex.GetType().ToString(), ex.Message, command.ElapsedMilliseconds);
                WebDriverBiDiEventSource.RaiseEvent.PendingCommandCount(this.PendingCommands.PendingCommandCount);
                throw;
            }
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Cancels a pending command and removes it from the pending command collection.
    /// If the command has already completed or been removed, this method is a safe no-op.
    /// </summary>
    /// <param name="command">The command to cancel.</param>
    /// <param name="reason">The reason the command is being canceled.</param>
    /// <returns>
    /// <see langword="true"/> if the cancellation took effect; <see langword="false"/> if the command
    /// had already completed with a result or fault, in which case that outcome stands.
    /// </returns>
    /// <remarks>
    /// The remote end does not know that the local end has stopped waiting, so it may still send a
    /// response for the canceled command. The command is remembered (see
    /// <see cref="PendingCommandCollection.CancelPendingCommand(Command, CommandCancellationReason)"/>)
    /// so that such a response is recognized and discarded rather than being reported as an unknown
    /// message or an unexpected error.
    /// </remarks>
    public virtual bool CancelCommand(Command command, CommandCancellationReason reason = CommandCancellationReason.Canceled)
    {
        return this.PendingCommands.CancelPendingCommand(command, reason);
    }

    /// <summary>
    /// Registers an additional <see cref="IJsonTypeInfoResolver"/> for JSON serialization
    /// and deserialization. This allows custom types, such as those from user-defined modules,
    /// to be serialized in AOT scenarios where reflection-based serialization is unavailable.
    /// This method must be called while the transport is not connected: before the first
    /// connection, or after a disconnect. Resolvers registered earlier remain in effect.
    /// </summary>
    /// <param name="resolver">The type info resolver to add.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the transport is already connected to a remote end.
    /// </exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual async Task RegisterTypeInfoResolverAsync(IJsonTypeInfoResolver resolver, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        if (resolver is null)
        {
            throw new ArgumentNullException(nameof(resolver), "The type info resolver must not be null");
        }

        await this.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.State == TransportState.Connected)
            {
                throw new InvalidOperationException("Cannot register a type info resolver after the transport is connected");
            }

            this.RebuildSerializerStateWithResolver(resolver);
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Gets a value indicating whether a message at the given level would be raised on
    /// <see cref="OnLogMessage"/>, so that a caller can avoid building a message that would be discarded.
    /// </summary>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> of the message the caller would raise.</param>
    /// <returns><see langword="true"/> if such a message would be raised; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// The transport uses this for its per-command <see cref="WebDriverBiDiLogLevel.Debug"/> messages,
    /// each of which composes a string naming the command; a custom transport should use it for the same
    /// purpose.
    /// <see cref="WebDriverBiDiLogLevel.Off"/> is never enabled. It selects "no messages at all" when
    /// assigned to <see cref="LogLevel"/>, and is not a level a message can carry; without the explicit
    /// test it would compare as enabled against every setting, because it is the highest value.
    /// </remarks>
    public bool IsLogLevelEnabled(WebDriverBiDiLogLevel level)
    {
        return level != WebDriverBiDiLogLevel.Off && level >= this.LogLevel && this.OnLogMessage.CurrentObserverCount > 0;
    }

    /// <summary>
    /// Registers an event message to be recognized when received from the connection.
    /// </summary>
    /// <typeparam name="T">The type of data to be returned in the event.</typeparam>
    /// <param name="eventName">The name of the event.</param>
    public virtual void RegisterEventMessage<T>(string eventName)
    {
        // T is a compile-time generic argument here, so building the envelope's type info with the
        // library-owned converter is ahead-of-time compatible and needs only T to be resolvable.
        this.AddEventMessageType(eventName, typeof(EventMessage<T>), static options => JsonMetadataServices.CreateValueInfo<EventMessage<T>>(options, new EventMessageJsonConverter<T>()));
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Transport"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// Disposing a transport that is already disposed does nothing, as <see cref="IAsyncDisposable"/>
    /// requires. The teardown cannot simply be repeated: it disposes the semaphore that guards the
    /// connection, and a second run would wait on that semaphore again if the first run left the
    /// transport in the <see cref="TransportState.Connecting"/> state, which happens when a connect
    /// attempt is still in flight and does not complete within <see cref="ShutdownTimeout"/>.
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (this.SetDisposed())
        {
            await this.DisposeAsyncCore().ConfigureAwait(false);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reports a late observer execution error to this transport's unhandled-error pipeline,
    /// optionally without raising <see cref="OnEventHandlerErrorOccurred"/>.
    /// </summary>
    /// <param name="errorInfo">The details of the observer failure.</param>
    /// <param name="notifyObservers">
    /// <see langword="true"/> to raise <see cref="OnEventHandlerErrorOccurred"/> for the failure;
    /// <see langword="false"/> to capture the failure without raising the event, which callers
    /// use when the failing observer belongs to an error-occurred event and re-raising would
    /// re-invoke that same observer in a feedback loop.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// This method never throws for a failure in an observer of
    /// <see cref="OnEventHandlerErrorOccurred"/> itself: the original handler failure is
    /// always captured, and the error observer's own failure is captured as a separate
    /// event-handler error without re-raising the error event.
    /// </remarks>
    internal async Task ReportEventObserverErrorAsync(EventObserverErrorInfo errorInfo, bool notifyObservers)
    {
        WebDriverBiDiEventSource.RaiseEvent.EventHandlerError(errorInfo.ObservableEventName, errorInfo.Exception.Message);
        Exception? errorObserverException = null;
        if (notifyObservers)
        {
            try
            {
                await this.invocableErrorHandlerErrorOccurredObservableEvent.InvokeNotifyObserversAsync(new EventHandlerErrorOccurredEventArgs(errorInfo)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Handle a synchronously throwing observer of OnEventHandlerErrorOccurred
                // so as not to derail the reporting of the original handler failure. This
                // prevents double-reporting of this as an error.
                errorObserverException = ex;
            }
        }

        this.CaptureUnhandledError(UnhandledErrorKind.EventHandlerException, errorInfo.Exception, this.GetEventHandlerTerminalReason(errorInfo.ObservableEventName));
        if (errorObserverException is not null)
        {
            this.CaptureUnhandledError(UnhandledErrorKind.EventHandlerException, errorObserverException, this.GetEventHandlerTerminalReason(EventHandlerErrorOccurredEventName));
        }
    }

    /// <summary>
    /// Tries to execute the given action while this Transport is disconnected.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <returns><see langword="true"/> if the Transport was disconnected and the execution completed; otherwise <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// The state is tested and <paramref name="action"/> is executed under the same lock that
    /// <see cref="ConnectAsync"/> takes to publish <see cref="TransportState.Connecting"/>, which is what
    /// makes the pair atomic with respect to a connect attempt beginning on another thread. This is the
    /// mechanism behind the driver's registration guard: a registration either completes in full while the
    /// transport is still idle, or is rejected because the transport is not.
    /// </para>
    /// <para>
    /// <paramref name="action"/> runs while the lock is held, so it must not block, must not wait on
    /// another thread, and must not re-enter this transport's connection lifecycle. It may call members
    /// that are themselves thread-safe and non-blocking, as the driver's registration actions do.
    /// </para>
    /// </remarks>
    internal bool TryExecuteWhileDisconnected(Action action)
    {
        lock (this.connectionStateLock)
        {
            if (this.State == TransportState.Disconnected)
            {
                action();
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Adds an event message type to the map of known event message types.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="eventMessageType">The type of data to be returned in the event.</param>
    protected virtual void AddEventMessageType(string eventName, Type eventMessageType)
    {
        this.AddEventMessageType(eventName, eventMessageType, null);
    }

    /// <summary>
    /// Creates a <see cref="Command"/> object from the specified command parameters.
    /// </summary>
    /// <param name="commandData">The <see cref="CommandParameters"/> object containing the command data.</param>
    /// <returns>The created <see cref="Command"/>.</returns>
    /// <remarks>
    /// This method allows a developer to override the creation of a command. This is useful
    /// for cases where additional properties need to be sent in the command envelope as
    /// opposed to the command parameters object.
    /// </remarks>
    protected virtual Command CreateCommand(CommandParameters commandData)
    {
        // The command times out on this transport's clock, so a transport running on virtual time
        // (in a test, say) times its commands out on that same clock.
        long commandId = this.GetNextCommandId();
        Command command = new(commandId, commandData, this.TimeProvider);
        return command;
    }

    /// <summary>
    /// Increments the command ID for the command to be sent.
    /// </summary>
    /// <returns>The command ID for the command to be sent.</returns>
    protected long GetNextCommandId()
    {
        return Interlocked.Increment(ref this.nextCommandId);
    }

    /// <summary>
    /// Serializes a command for transmission across the WebSocket connection.
    /// </summary>
    /// <param name="command">The command to serialize.</param>
    /// <returns>The serialized JSON string representing the command.</returns>
    protected virtual byte[] SerializeCommand(Command command)
    {
        // Use the JsonSerializer.Serialize() overload that takes a JsonTypeInfo
        // to remove warnings when publishing AOT compiled applications.
        return JsonSerializer.SerializeToUtf8Bytes(command, this.commandJsonTypeInfo);
    }

    /// <summary>
    /// Creates an <see cref="IncomingMessage"/> object for the data received by this <see cref="Transport"/>.
    /// </summary>
    /// <param name="owner">
    /// The <see cref="IMemoryOwner{T}"/> whose buffer contains the incoming message data.
    /// Ownership transfers to the returned <see cref="IncomingMessage"/>, which will dispose it on disposal.
    /// </param>
    /// <param name="length">The length, in bytes, of the incoming message within the owner's buffer.</param>
    /// <returns>The <see cref="IncomingMessage"/> object for the data received.</returns>
    protected virtual IncomingMessage CreateIncomingMessage(IMemoryOwner<byte> owner, int length)
    {
        return new IncomingMessage(owner, length);
    }

    /// <summary>
    /// Asynchronously disconnects from the remote end web socket.
    /// </summary>
    /// <param name="throwCollectedExceptions"> A value indicating whether to throw the collected exceptions.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual async Task DisconnectAsync(bool throwCollectedExceptions, CancellationToken cancellationToken = default)
    {
        // Fast-path guard: if not connected, there is nothing to disconnect.
        // This check runs before acquiring the semaphore to prevent a deadlock
        // when an event handler processed during shutdown calls SendCommandAsync,
        // which in turn calls DisconnectAsync on the thread that already holds
        // the semaphore.
        //
        // The transport may have already been marked disconnected by a remote
        // disconnect or connection error (see HandleConnectionDisconnectionAsync),
        // which completes the incoming message queue but does not run the rest of
        // the teardown below and therefore never reaches the collected-exceptions
        // throw at the end of this method. This is the only remaining opportunity
        // to surface Collect-mode errors to a caller of StopAsync/DisconnectAsync
        // for that session.
        if (this.State != TransportState.Connected)
        {
            this.ThrowIfCollectedExceptionsClaimed(throwCollectedExceptions);
            return;
        }

        await this.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check the connection state after acquiring the semaphore to prevent
            // race conditions where multiple threads pass the fast-path check simultaneously.
            // If another thread has already disconnected, we can safely return.
            if (this.State != TransportState.Connected)
            {
                this.ThrowIfCollectedExceptionsClaimed(throwCollectedExceptions);
                return;
            }

            // Mark disconnected before performing teardown so that any
            // re-entrant calls (e.g., from event handlers still executing
            // during shutdown) see the transport as disconnected and
            // short-circuit rather than attempting a redundant disconnect.
            WebDriverBiDiEventSource.RaiseEvent.ConnectionClosing(this.Connection.Id, this.TerminationReason);
            this.State = TransportState.Disconnected;

            // DisconnectAsync owns the shutdown, and we are about to await the completion
            // of the receive data loop. We release any connection-loss handler awaiting
            // the lock.
            this.disconnectOwnedSignal.TrySetResult(1);

            // Close the pending command collection to further addition of commands,
            // and stop the connection from receiving further communication traffic.
            await this.PendingCommands.CloseAsync().ConfigureAwait(false);
            try
            {
                await this.Connection.StopAsync(cancellationToken).ConfigureAwait(false);

                using CancellationTokenSource timeoutCancelTokenSource = new();
                Task timeoutTask = TimeoutUtilities.DelayAsync(this.TimeProvider, this.ShutdownTimeout, timeoutCancelTokenSource.Token);
                bool shutdownTimedOut = false;

                // Mark the incoming message queue as complete for writing, indicating
                // no further messages will be written to the queue. Existing messages
                // currently in the queue, however should still be processed. Then,
                // wait for the incoming message queue to consume the remaining messages
                // already in the queue. Note that having all items consumed from the
                // queue does not imply that processing of all items has completed; that
                // must be awaited separately. TryComplete is used rather than Complete
                // because the queue may already have been completed by a remote disconnect
                // or connection error that raced with this call.
                this.incomingMessageQueue.MessageChannel.Writer.TryComplete();
                Task messageQueueReaderCompleteTask = await Task.WhenAny(this.incomingMessageQueue.MessageChannel.Reader.Completion, timeoutTask).ConfigureAwait(false);
                if (messageQueueReaderCompleteTask != this.incomingMessageQueue.MessageChannel.Reader.Completion)
                {
                    shutdownTimedOut = true;
                    await this.LogAsync("Timed out waiting for message writer to complete during shutdown", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
                }

                // Clear the pending command collection. This will also cancel any tasks
                // associated with the remaining pending commands. Then wait for the
                // message processor to complete processing of the messages received from
                // the message queue, but with a timeout to prevent hanging if an
                // in-process event handler is stuck.
                this.PendingCommands.Clear();
                Task messageProcessingShutdownCompletedTask = await Task.WhenAny(this.messageQueueProcessingTask, timeoutTask).ConfigureAwait(false);
                if (messageProcessingShutdownCompletedTask != this.messageQueueProcessingTask)
                {
                    shutdownTimedOut = true;
                    await this.LogAsync("Timed out waiting for message processing to complete during shutdown", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
                }

                if (!shutdownTimedOut)
                {
                    timeoutCancelTokenSource.Cancel();
                }

                WebDriverBiDiEventSource.RaiseEvent.MessageStatistics(Interlocked.Read(ref this.commandMessagesSent), Interlocked.Read(ref this.commandResponseMessagesReceived), Interlocked.Read(ref this.eventMessagesReceived), Interlocked.Read(ref this.errorMessagesReceived));
                WebDriverBiDiEventSource.RaiseEvent.ConnectionClosed(this.Connection.Id);
                WebDriverBiDiEventSource.RaiseEvent.TransportStopped(this.TerminationReason);
                await this.LogAsync("Transport disconnected", WebDriverBiDiLogLevel.Info).ConfigureAwait(false);

                this.ThrowIfCollectedExceptionsClaimed(throwCollectedExceptions);
            }
            finally
            {
                // Completing the queue writer and clearing the pending commands must happen even
                // when the Connection.StopAsync throws (which could happen for a custom Connection
                // implementation). Both of these statements are no-ops if the try block completed
                // successfully, allowing a subsequent ConnectAsync to not wait for the full shutdown
                // timeout before reconnecting.
                this.incomingMessageQueue.MessageChannel.Writer.TryComplete();
                this.PendingCommands.Clear();
            }
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Asynchronously releases the resources used by this <see cref="Transport"/>.
    /// Override this method in derived classes to add custom cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    /// <remarks>
    /// <see cref="DisposeAsync"/> calls this method once, on the first disposal only, and records the
    /// disposal before calling it, so an override neither repeats that guard nor needs one of its own.
    /// Because the transport is marked disposed before the teardown rather than after it, an operation
    /// that rejects a disposed transport -- <see cref="ConnectAsync"/>,
    /// <see cref="SendCommandAsync"/> and <see cref="RegisterTypeInfoResolverAsync"/> -- fails from the
    /// moment disposal begins rather than only once it has finished.
    /// </remarks>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        // Account for the two potential waits, one for a concurrent attempt to connect,
        // and one for message processing to complete. Use one shutdown budget for both.
        long disposalTimestamp = this.TimeProvider.GetTimestamp();

        // A connect attempt that is still in flight owns the connect/disconnect semaphore
        // and is actively using the connection; disposing them out from under it would fail
        // the attempt with ObjectDisposedException rather than its normal rollback.
        // Serialize with the attempt by acquiring and releasing the lock, which cannot
        // succeed until the attempt has either published Connected or rolled back to
        // Disconnected; then tear down whichever state resulted. The wait is bounded by
        // ShutdownTimeout so a pathological disposal from code the connect attempt itself
        // invoked (such as a synchronous log observer) degrades to a logged, time-bounded
        // wait rather than a deadlock; on timeout, disposal proceeds exactly as it would
        // have without this serialization.
        if (this.State == TransportState.Connecting)
        {
            using CancellationTokenSource lockWaitCancellationTokenSource = TimeoutUtilities.CreateCancellationTokenSource(this.TimeProvider, this.ShutdownTimeout);
            try
            {
                await this.AcquireConnectionLockAsync(lockWaitCancellationTokenSource.Token).ConfigureAwait(false);
                this.ReleaseConnectionLock();
            }
            catch (OperationCanceledException)
            {
                await this.LogAsync("Timed out waiting for an in-flight connect attempt to complete during disposal", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }
        }

        if (this.State == TransportState.Connected)
        {
            try
            {
                await this.DisconnectAsync(false).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.LogAsync($"Unexpected exception during disposal: {ex.Message}", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }
        }
        else
        {
            // If we lost our connection, we still need to wait for delivered messages to
            // be processed. HandleConnectionDisconnectionAsync closes the queue, but does
            // not wait for message processing.
            await this.WaitForMessageProcessingCompletionAsync(TimeoutUtilities.GetRemainingTimeout(this.ShutdownTimeout, this.TimeProvider.GetElapsedTime(disposalTimestamp)), "Timed out waiting for message processing to complete during disposal").ConfigureAwait(false);
        }

        this.PendingCommands.Dispose();
        this.connectionDataReceivedObserver.Dispose();
        this.connectionErrorObserver.Dispose();
        this.connectionRemoteDisconnectObserver.Dispose();
        this.connectionLogMessageObserver.Dispose();
        await this.Connection.DisposeAsync().ConfigureAwait(false);
        this.connectDisconnectSemaphore.Dispose();
    }

    /// <summary>
    /// Asynchronously acquires the connection lock to ensure thread-safe operations for connection-related actions.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous acquire operation.</returns>
    /// <exception cref="WebDriverBiDiTimeoutException">
    /// Thrown when the lock is not acquired within <see cref="ConnectionLockTimeout"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    /// <remarks>
    /// <para>
    /// The uncontended case is the overwhelmingly common one, and it is taken without allocating: the
    /// timeout machinery is built only once the lock is found to be held. The bound is measured on this
    /// transport's <see cref="TimeProvider"/>, so a transport running on virtual time waits on that same
    /// clock.
    /// </para>
    /// <para>
    /// Bounding the wait is what keeps a re-entrant call from hanging forever. This transport dispatches
    /// observers while holding the lock — the connection raises its own traffic message from inside
    /// <see cref="Connection.SendDataAsync"/>, and both the connection and this transport log around
    /// connect and disconnect — so a synchronous observer that calls back into the transport arrives
    /// here for a lock its own caller holds. It now fails with a
    /// <see cref="WebDriverBiDiTimeoutException"/> naming the cause, and the operation it interrupted
    /// proceeds, rather than the two waiting on each other indefinitely.
    /// </para>
    /// </remarks>
    protected virtual async Task AcquireConnectionLockAsync(CancellationToken cancellationToken = default)
    {
        // Honor an already-canceled token before the fast path, because SemaphoreSlim.WaitAsync
        // observes the token even when the semaphore is free, and taking the lock instead would
        // change that behavior for a caller who has already given up.
        cancellationToken.ThrowIfCancellationRequested();
        if (this.connectDisconnectSemaphore.Wait(0))
        {
            return;
        }

        using CancellationTokenSource timeoutTokenSource = TimeoutUtilities.CreateCancellationTokenSource(this.TimeProvider, this.ConnectionLockTimeout);
        using CancellationTokenSource linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutTokenSource.Token);
        try
        {
            await this.connectDisconnectSemaphore.WaitAsync(linkedTokenSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // The caller did not cancel, so the linked token can only have fired for the timeout.
            throw new WebDriverBiDiTimeoutException($"Timed out after {this.ConnectionLockTimeout} waiting for exclusive access to the connection. An operation issued from an observer that this transport invoked while holding that access cannot proceed until the holder finishes; run such an observer asynchronously with ObservableEventHandlerOptions.RunHandlerAsynchronously.");
        }
    }

    /// <summary>
    /// Releases the connection lock to allow other threads to perform connection-related actions.
    /// </summary>
    protected virtual void ReleaseConnectionLock()
    {
        this.connectDisconnectSemaphore.Release();
    }

    /// <summary>
    /// Reads and processes messages from the incoming message queue until the queue is
    /// closed. This method is the body of the task started by <see cref="ConnectAsync"/>.
    /// </summary>
    /// <remarks>
    /// This method is <see langword="protected virtual"/> to allow test doubles to
    /// substitute the message-processing loop — for example, to simulate an
    /// unrecoverable fault on the outer await so that the fault continuation attached
    /// in <see cref="ConnectAsync"/> can be exercised.
    /// </remarks>
    /// <returns>A task representing the asynchronous message-processing loop.</returns>
    protected virtual async Task ReadIncomingMessagesAsync()
    {
        // Capture the queue this reader is bound to, and read and count against that capture for
        // the life of the loop. Reading the field directly in the code below would address the
        // wrong queue in the reconnect-after-disconnect scenario: a reconnect that gives up
        // waiting for this loop replaces the field while the loop is still draining the queue it
        // started on, so a depth adjustment made against the field would be applied to the new
        // connection's queue for a message that was never on it.
        IncomingMessageQueue queue = this.incomingMessageQueue;
        ChannelReader<IncomingMessage> reader = queue.MessageChannel.Reader;

        // In theory, we could accomplish this with an `await foreach` using
        // IAsyncEnumerable, but this would require additional dependencies,
        // which is challenging. Initial experiments has shown that simply
        // adding a reference to the Microsoft.Bcl.AsyncInterfaces assembly
        // is not enough by itself to enable compilation using that construct.
        while (await reader.WaitToReadAsync().ConfigureAwait(false))
        {
            while (reader.TryRead(out IncomingMessage? packet))
            {
                queue.DecrementDepth();
                try
                {
                    await this.ProcessMessageAsync(packet).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await this.LogAsync($"Unexpected error in message processing loop: {ex.Message}", WebDriverBiDiLogLevel.Error).ConfigureAwait(false);
                    this.CaptureUnhandledError(UnhandledErrorKind.ProtocolError, ex, "Unexpected error in message processing loop");
                }
            }
        }
    }

    /// <summary>
    /// Processes a single incoming message read from the connection.
    /// </summary>
    /// <param name="packet">The incoming message to process.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method is <see langword="protected virtual"/> to allow test doubles to observe or delay the
    /// processing of individual messages (for example to create a backlog of pending messages while the
    /// transport disconnects).
    /// </remarks>
    protected virtual async Task ProcessMessageAsync(IncomingMessage packet)
    {
        bool isProcessed = false;
        using (packet)
        {
            try
            {
                packet.Parse();
            }
            catch (JsonException e)
            {
                // JSON parsing errors are regarded as "unknown message" errors, rather than
                // "protocol errors." Protocol errors are defined as valid JSON messages that
                // resemble protocol data structures, but do not fit the payload definitions
                // of the protocol. Here, we just log the error; the error will be processed
                // to be added to the proper unhandled error collection in the "if (!isProcessed)"
                // block below.
                await this.LogAsync($"Unexpected error parsing JSON message: {e.Message}", WebDriverBiDiLogLevel.Error).ConfigureAwait(false);
            }

            if (packet.MessageKind == IncomingMessageKind.Filtered)
            {
                isProcessed = true;
            }
            else if (packet.MessageKind == IncomingMessageKind.CommandResponse)
            {
                isProcessed = await this.ProcessCommandResponseMessageAsync(packet).ConfigureAwait(false);
                Interlocked.Increment(ref this.commandResponseMessagesReceived);
            }
            else if (packet.MessageKind == IncomingMessageKind.ErrorResponse)
            {
                isProcessed = await this.ProcessErrorMessageAsync(packet).ConfigureAwait(false);
                Interlocked.Increment(ref this.errorMessagesReceived);
            }
            else if (packet.MessageKind == IncomingMessageKind.Event)
            {
                isProcessed = await this.ProcessEventMessageAsync(packet).ConfigureAwait(false);
                Interlocked.Increment(ref this.eventMessagesReceived);
            }

            if (!isProcessed)
            {
                string message = packet.MessageText;
                WebDriverBiDiEventSource.RaiseEvent.UnknownMessageReceived(packet.MessageKind, packet.MessageLength);
                await this.OnProtocolUnknownMessageReceivedAsync(new UnknownMessageReceivedEventArgs(message)).ConfigureAwait(false);
                this.CaptureUnhandledError(UnhandledErrorKind.UnknownMessage, new WebDriverBiDiException($"Received unknown message from protocol connection: {message}"), "Unknown message from connection");
            }
        }
    }

    /// <summary>
    /// Captures an unhandled error in the protocol.
    /// </summary>
    /// <param name="errorType">The <see cref="UnhandledErrorKind"/> describing the type of error.</param>
    /// <param name="ex">The exception thrown for the error.</param>
    /// <param name="terminalReason">The reason for terminating the session, if the error is a terminal error.</param>
    /// <remarks>
    /// This method is <see langword="protected virtual"/> to allow test doubles to
    /// precisely determine when an unhandled error has been captured and propagated
    /// to the unhandled errors collection.
    /// </remarks>
    protected virtual void CaptureUnhandledError(UnhandledErrorKind errorType, Exception ex, string terminalReason)
    {
        bool isTerminalError = false;
        switch (errorType)
        {
            case UnhandledErrorKind.ProtocolError:
                isTerminalError = this.ProtocolErrorBehavior == TransportErrorBehavior.Terminate;
                break;
            case UnhandledErrorKind.UnknownMessage:
                isTerminalError = this.UnknownMessageBehavior == TransportErrorBehavior.Terminate;
                break;
            case UnhandledErrorKind.UnexpectedError:
                isTerminalError = this.UnexpectedErrorBehavior == TransportErrorBehavior.Terminate;
                break;
            case UnhandledErrorKind.EventHandlerException:
                isTerminalError = this.EventHandlerExceptionBehavior == TransportErrorBehavior.Terminate;
                break;
        }

        // Note carefully that if handling of the error type is "Ignore", adding to the
        // unhandled errors collection is a no-op.
        this.UnhandledErrors.AddUnhandledError(errorType, ex);
        if (isTerminalError)
        {
            this.TerminationReason = terminalReason;
        }
    }

    /// <summary>
    /// Gets the type info resolver to be used for JSON serialization and deserialization.
    /// By default, this method returns a resolver that uses reflection, but if reflection-
    /// based serialization is not available (e.g., in AOT scenarios), it returns a source-
    /// generated resolver.
    /// </summary>
    /// <returns>The <see cref="IJsonTypeInfoResolver"/> to use.</returns>
    /// <remarks>
    /// This method is excluded from code coverage because the behavior it provides is
    /// environment-specific and cannot be reliably tested in a consistent manner across
    /// different test environments. The method's logic is straightforward and primarily
    /// serves to select the appropriate JSON type info resolver based on the capabilities
    /// of the runtime environment, rather than containing complex logic that would benefit
    /// from unit testing.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "Guarded by JsonSerializer.IsReflectionEnabledByDefault feature switch; trimmed away under PublishAot")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode", Justification = "Guarded by JsonSerializer.IsReflectionEnabledByDefault feature switch; trimmed away under PublishAot")]
    private static IJsonTypeInfoResolver CreateTypeInfoResolver()
    {
        return JsonSerializer.IsReflectionEnabledByDefault
            ? new DefaultJsonTypeInfoResolver()
            : WebDriverBiDiJsonSerializerContext.Default;
    }

    private static ReceivedDataDictionary ConvertPayloadExtensionData(Dictionary<string, JsonElement>? extensionData)
    {
        return extensionData is null ? ReceivedDataDictionary.EmptyDictionary : JsonConverterUtilities.ConvertIncomingExtensionData(extensionData);
    }

    private static string TruncateMessage(string message, int maxLength)
    {
        if (message.Length <= maxLength)
        {
            return message;
        }

#if NETSTANDARD2_0
        return string.Concat(message.Substring(0, maxLength), "...");
#else
        return string.Concat(message.AsSpan(0, maxLength), "...");
#endif
    }

    /// <summary>
    /// Waits, bounded by <see cref="ShutdownTimeout"/>, for the message-processing task to finish, logging a
    /// warning and returning if it does not.
    /// </summary>
    /// <param name="timeout">How long to wait before giving up, or <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.</param>
    /// <param name="timeoutLogMessage">The warning to log if the task does not finish within the timeout.</param>
    /// <returns>A task representing the asynchronous wait.</returns>
    /// <remarks>
    /// The task completes once the incoming message queue has been marked complete for writing and its remaining
    /// messages have been dispatched. Every caller therefore completes the queue first; this method only waits.
    /// It never throws, so a caller on a teardown path is not derailed by a stuck message handler.
    /// </remarks>
    private async Task WaitForMessageProcessingCompletionAsync(TimeSpan timeout, string timeoutLogMessage)
    {
        if (this.messageQueueProcessingTask.IsCompleted)
        {
            return;
        }

        // Logged only when there is something to wait for, so a Debug-level observer can see that a
        // reconnect or a disposal is blocked on the previous session's reader, and for how long at most.
        if (this.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug))
        {
            await this.LogAsync($"Waiting for message processing of the previous session to complete (timeout: {timeout})", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
        }

        using CancellationTokenSource processingWaitCancelTokenSource = new();
        Task processingWaitTask = TimeoutUtilities.DelayAsync(this.TimeProvider, timeout, processingWaitCancelTokenSource.Token);
        Task completedTask = await Task.WhenAny(this.messageQueueProcessingTask, processingWaitTask).ConfigureAwait(false);
        if (completedTask == this.messageQueueProcessingTask)
        {
            processingWaitCancelTokenSource.Cancel();
        }
        else
        {
            await this.LogAsync(timeoutLogMessage, WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Marks this <see cref="Transport"/> as disposed.
    /// </summary>
    /// <returns><see langword="true"/> if the object was not already disposed before calling this method; otherwise, <see langword="false"/>.</returns>
    private bool SetDisposed()
    {
        return Interlocked.Exchange(ref this.isDisposedFlag, 1) == 0;
    }

    private void ThrowIfDisposed()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }

    /// <summary>
    /// Adds an event message type to the map of known event message types, with an optional factory
    /// that creates its <see cref="JsonTypeInfo"/> instead of resolving it through the serializer options.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="eventMessageType">The type of data to be returned in the event.</param>
    /// <param name="typeInfoFactory">The factory creating the type info, or <see langword="null"/> to resolve <paramref name="eventMessageType"/> through the serializer options.</param>
    private void AddEventMessageType(string eventName, Type eventMessageType, Func<JsonSerializerOptions, JsonTypeInfo>? typeInfoFactory)
    {
        this.eventMessageTypes[eventName] = new EventMessageRegistration(eventMessageType, typeInfoFactory);
    }

    /// <summary>
    /// Rebuilds the serializer state so that the given <see cref="IJsonTypeInfoResolver"/> participates
    /// in serialization and deserialization, combined with the resolvers already in effect.
    /// </summary>
    /// <param name="resolver">The type info resolver to add.</param>
    /// <remarks>
    /// A <see cref="JsonSerializerOptions"/> becomes read-only once it has been used for (de)serialization,
    /// so a resolver cannot be appended to the options that served a previous connection. This builds a
    /// fresh, mutable copy that combines the existing resolvers with <paramref name="resolver"/>, then
    /// re-derives the cached command and error type infos and clears the per-connection response and
    /// event type info caches so that every subsequent lookup binds to the new options rather than the
    /// old. This runs only while the transport is disconnected and under the connection lock, so no
    /// message processing observes the swap.
    /// </remarks>
    private void RebuildSerializerStateWithResolver(IJsonTypeInfoResolver resolver)
    {
        this.options = new JsonSerializerOptions(this.options)
        {
            TypeInfoResolver = JsonTypeInfoResolver.Combine(this.options.TypeInfoResolver, resolver),
        };
        this.commandJsonTypeInfo = (JsonTypeInfo<Command>)this.options.GetTypeInfo(typeof(Command));
        this.errorResponseJsonTypeInfo = (JsonTypeInfo<ErrorResponseMessage>)this.options.GetTypeInfo(typeof(ErrorResponseMessage));
        this.responseTypeInfoCache.Clear();
        foreach (EventMessageRegistration registration in this.eventMessageTypes.Values)
        {
            registration.ResetCachedTypeInfo();
        }
    }

    /// <summary>
    /// Gets the type info used to deserialize responses to a command, preferring the command's own
    /// envelope type info (which needs only the result type to be resolvable) over the serializer options.
    /// </summary>
    /// <param name="command">The command awaiting a response.</param>
    /// <returns>The type info for the response envelope.</returns>
    private JsonTypeInfo GetResponseTypeInfo(Command command)
    {
        // Look the cache up before falling back to GetOrAdd: the factory lambda captures the command
        // and this transport, so passing it to GetOrAdd allocates a closure on every response, while
        // the cache misses only once per response type.
        if (this.responseTypeInfoCache.TryGetValue(command.ResponseType, out JsonTypeInfo? cachedTypeInfo))
        {
            return cachedTypeInfo;
        }

        return this.responseTypeInfoCache.GetOrAdd(
            command.ResponseType,
            _ => command.CommandParameters.CreateResponseTypeInfo(this.options) ?? this.options.GetTypeInfo(command.ResponseType));
    }

    private async Task OnProtocolEventReceivedAsync(EventReceivedEventArgs e)
    {
        try
        {
            await this.invocableEventReceivedObservableEvent.InvokeNotifyObserversAsync(e).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await this.ReportEventObserverErrorAsync(e.EventName, TransportEventObserverDescription, ex).ConfigureAwait(false);
        }
    }

    private async Task OnProtocolErrorEventReceivedAsync(ErrorReceivedEventArgs e)
    {
        try
        {
            await this.invocableErrorReceivedObservableEvent.InvokeNotifyObserversAsync(e).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await this.ReportEventObserverErrorAsync(this.OnErrorEventReceived.EventName, TransportErrorObserverDescription, ex).ConfigureAwait(false);
        }
    }

    private async Task OnProtocolUnknownMessageReceivedAsync(UnknownMessageReceivedEventArgs e)
    {
        try
        {
            await this.invocableUnknownMessageReceivedObservableEvent.InvokeNotifyObserversAsync(e).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await this.ReportEventObserverErrorAsync(this.OnUnknownMessageReceived.EventName, TransportUnknownMessageObserverDescription, ex).ConfigureAwait(false);
        }
    }

    private async Task OnConnectionLogMessageAsync(LogMessageEventArgs e)
    {
        await this.NotifyLogMessageObserversAsync(e).ConfigureAwait(false);
    }

    private Task OnConnectionDataReceivedAsync(ConnectionDataReceivedEventArgs e)
    {
        // TryWrite on an unbounded channel fails only after the writer has been completed,
        // which DisconnectAsync does once the connection has been asked to stop. A pipe
        // connection's receive loop can outlive that (see PipeConnection.StopAsync), so a
        // late message must be disposed here to return its pooled buffer rather than being
        // dropped on the floor.
        // Capture the queue once, so that the write and the count it produces cannot address
        // different queues if a reconnect replaces the field between them.
        IncomingMessageQueue queue = this.incomingMessageQueue;
        IncomingMessage message = this.CreateIncomingMessage(e.BufferOwner, e.DataLength);
        if (!queue.MessageChannel.Writer.TryWrite(message))
        {
            message.Dispose();
            return Task.CompletedTask;
        }

        queue.IncrementDepth();
        return Task.CompletedTask;
    }

    private async Task OnConnectionRemotelyDisconnectedAsync(ConnectionDisconnectedEventArgs e)
    {
        WebDriverBiDiConnectionException connectionException = new("Remote end closed the connection");
        string logMessage = "Remote end closed connection; pending commands failed";
        await this.HandleConnectionDisconnectionAsync(connectionException, logMessage, WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
    }

    private async Task OnConnectionErrorAsync(ConnectionErrorEventArgs e)
    {
        WebDriverBiDiEventSource.RaiseEvent.ConnectionError(this.Connection.Id, e.Exception.Message);
        WebDriverBiDiConnectionException connectionException = new($"Unexpected connection error: {e.Exception.Message}", e.Exception);
        string logMessage = $"Connection error; pending commands failed: {e.Exception.Message}";
        await this.HandleConnectionDisconnectionAsync(connectionException, logMessage, WebDriverBiDiLogLevel.Error).ConfigureAwait(false);
    }

    private async Task HandleConnectionDisconnectionAsync(WebDriverBiDiConnectionException connectionException, string logMessage, WebDriverBiDiLogLevel logLevel)
    {
        // Fast-path: if already disconnected, no work to do.
        // Prevents deadlock when connection error occurs during DisconnectAsync.
        TransportState stateAtNotification = this.State;
        if (stateAtNotification != TransportState.Connected)
        {
            if (stateAtNotification == TransportState.Connecting)
            {
                // The loss arrived while a connect attempt is still in flight, so there is no session to
                // tear down yet. Record the cause for ConnectAsync to fail the attempt with.
                Interlocked.CompareExchange(ref this.connectionLostWhileConnecting, connectionException, null);
            }

            return;
        }

        // This method is running on the connection's receive loop. If DisconnectAsync
        // already holds the lock, it will await this loop, so waiting for the lock
        // unconditionally is a cycle. Race the wait against DisconnectAsync's ownership
        // signal instead.
        Task disconnectOwnershipTask = Interlocked.CompareExchange(ref this.disconnectOwnedSignal, null!, null!).Task;
        Task lockAcquisitionTask = this.AcquireConnectionLockAsync(CancellationToken.None);
        Task firstCompletedTask = await Task.WhenAny(lockAcquisitionTask, disconnectOwnershipTask).ConfigureAwait(false);
        if (firstCompletedTask != lockAcquisitionTask)
        {
            // DisconnectAsync owns the teardown. The wait left outstanding here is settled by the
            // helper, which hands the lock back if the wait is granted and leaves it alone if the
            // wait is abandoned instead.
            _ = this.ReleaseConnectionLockWhenAcquiredAsync(lockAcquisitionTask);
            return;
        }

        // Task.WhenAny reports the first task to complete, and an abandoned wait has completed, so
        // awaiting the acquisition is what tells a granted access from an abandoned one. Neither the
        // teardown nor the release below may run without the access.
        try
        {
            await lockAcquisitionTask.ConfigureAwait(false);
        }
        catch (WebDriverBiDiTimeoutException)
        {
            // Tearing down without the access could interleave with a reconnect and close the
            // pending commands of a session this loss has nothing to do with. The loss still
            // surfaces, just not immediately: the connection is closed, so pending commands end at
            // their own timeouts and the next send fails.
            await this.LogAsync($"Timed out after {this.ConnectionLockTimeout} waiting for exclusive access to the connection to handle a connection loss; the transport was left connected and its pending commands left to time out. Stop the transport to tear the session down.", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            return;
        }

        try
        {
            // Only process if we were connected (or thought we were).
            // If we're still in DisconnectAsync, we'll run after it releases the lock,
            // so we'll see the correct post-connect state.
            if (this.State != TransportState.Connected)
            {
                // DisconnectAsync hasn't set it yet, or we're already disconnected
                return;
            }

            // Mark the transport as disconnected so that subsequent SendCommandAsync
            // calls fail immediately rather than queuing commands that can never
            // receive a response.
            this.State = TransportState.Disconnected;

            // Close the pending command collection and fail every in-flight command
            // with an appropriate exception.
            await this.PendingCommands.CloseAsync().ConfigureAwait(false);
            this.PendingCommands.FailAllPendingCommands(connectionException);

            // Complete the incoming message queue so that the reader task drains any
            // messages already received and then exits, rather than waiting forever on
            // a queue that will never be written to again. The reader is deliberately
            // not awaited here: this method runs on the connection's receive loop and
            // holds the connection lock, and an event handler still executing on the
            // reader may itself need that lock (e.g., to send a command, which will
            // fail because the transport is now disconnected). ConnectAsync waits for
            // the reader to finish before starting a new session.
            this.incomingMessageQueue.MessageChannel.Writer.TryComplete();

            // Log appropriate statistics and information.
            WebDriverBiDiEventSource.RaiseEvent.MessageStatistics(Interlocked.Read(ref this.commandMessagesSent), Interlocked.Read(ref this.commandResponseMessagesReceived), Interlocked.Read(ref this.eventMessagesReceived), Interlocked.Read(ref this.errorMessagesReceived));
            await this.LogAsync(logMessage, logLevel).ConfigureAwait(false);
        }
        finally
        {
            this.ReleaseConnectionLock();
        }
    }

    /// <summary>
    /// Hands the connection lock back once a wait that was left outstanding is granted, and does
    /// nothing if that wait is abandoned instead.
    /// </summary>
    /// <param name="lockAcquisitionTask">The outstanding wait for the connection lock.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// The wait is the one <see cref="HandleConnectionDisconnectionAsync"/> leaves outstanding when
    /// <see cref="DisconnectAsync(bool, CancellationToken)"/> wins the ownership race. Releasing an
    /// abandoned wait would raise the semaphore's count and let two operations hold the connection at
    /// once; awaiting it also observes the failure a discarded task would leave unobserved.
    /// </remarks>
    private async Task ReleaseConnectionLockWhenAcquiredAsync(Task lockAcquisitionTask)
    {
        try
        {
            await lockAcquisitionTask.ConfigureAwait(false);
        }
        catch (WebDriverBiDiTimeoutException)
        {
            return;
        }

        this.ReleaseConnectionLock();
    }

    private void LogMessageProcessingFault(Task faultedTask, object? state)
    {
        // Task.Exception is guaranteed non-null here because this continuation is
        // scheduled with OnlyOnFaulted; the null-forgiving operator is appropriate.
        AggregateException aggregate = faultedTask.Exception!;
        Exception exception = aggregate.InnerExceptions.Count == 1 ? aggregate.InnerExceptions[0] : aggregate;

        // Use EventSource rather than LogAsync to avoid the async path on this
        // fire-and-forget fault handler. Capture onto the unhandled-error pipeline
        // for symmetry with per-message failures in ReadIncomingMessagesAsync.
        WebDriverBiDiEventSource.RaiseEvent.ProtocolError(exception.Message, "Message processing loop faulted");
        this.CaptureUnhandledError(UnhandledErrorKind.ProtocolError, exception, "Message processing loop faulted");
    }

    private async Task<bool> ProcessCommandResponseMessageAsync(IncomingMessage packet)
    {
        if (packet.TryGetCommandId(out long responseId))
        {
            if (this.PendingCommands.RemovePendingCommand(responseId, out Command? executedCommand))
            {
                // Stop timing and log completion
                executedCommand.StopTiming();
                WebDriverBiDiEventSource.RaiseEvent.CommandCompleted(responseId, executedCommand.CommandName, executedCommand.ElapsedMilliseconds);
                try
                {
                    JsonTypeInfo responseTypeInfo = this.GetResponseTypeInfo(executedCommand);
                    CommandResponseMessage response = packet.DeserializeCommandResponseMessage(responseTypeInfo);
                    CommandResult commandResult = response.Result;

                    // Extension properties live in two distinct places and are never merged: inside the
                    // result object (AdditionalData) and on the response envelope (AdditionalResponseProperties).
                    commandResult.AdditionalData = ConvertPayloadExtensionData(packet.CollectPayloadExtensionData("result", this.options.GetTypeInfo(commandResult.GetType())));
                    commandResult.AdditionalResponseProperties = response.AdditionalData;
                    if (this.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug))
                    {
                        await this.LogAsync($"Received result for command '{executedCommand.CommandName}' (command ID: {executedCommand.CommandId})", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
                    }

                    executedCommand.SetResult(commandResult);
                }
                catch (Exception ex)
                {
                    executedCommand.SetException(new WebDriverBiDiSerializationException($"Response did not contain properly formed JSON for response type (response JSON:{packet.MessageText})", ex));
                }

                return true;
            }

            if (this.PendingCommands.TryRemoveCanceledCommand(responseId, out CanceledCommandInfo? canceledCommand))
            {
                // The local end stopped waiting for this command (timeout, cancellation, or
                // shutdown), but the remote end answered anyway. This is not a protocol anomaly,
                // so it is reported for diagnostics and discarded rather than being routed to the
                // unknown-message pipeline.
                await this.ReportDiscardedCanceledCommandResponseAsync(canceledCommand).ConfigureAwait(false);
                return true;
            }
        }

        return false;
    }

    private async Task<bool> ProcessErrorMessageAsync(IncomingMessage packet)
    {
        try
        {
            // If the message doesn't match the schema of an actual error message,
            // an exception will be thrown by the JSON serializer, and we can log
            // the malformed response.
            if (packet.TryGetErrorResponse(this.errorResponseJsonTypeInfo, out ErrorResponseMessage? errorMessage))
            {
                ErrorResult result = errorMessage.GetErrorResponseData();
                if (errorMessage.CommandId.HasValue && this.PendingCommands.RemovePendingCommand(errorMessage.CommandId.Value, out Command? executedCommand))
                {
                    // Stop timing and log error
                    executedCommand.StopTiming();
                    WebDriverBiDiEventSource.RaiseEvent.CommandError(errorMessage.CommandId.Value, executedCommand.CommandName, result.ErrorCode, result.ErrorType.ToString(), result.ErrorMessage);
                    if (this.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug))
                    {
                        await this.LogAsync($"Received error response for command '{executedCommand.CommandName}' (command ID: {errorMessage.CommandId.Value})", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
                    }

                    executedCommand.SetResult(result);
                }
                else if (errorMessage.CommandId.HasValue && this.PendingCommands.TryRemoveCanceledCommand(errorMessage.CommandId.Value, out CanceledCommandInfo? canceledCommand))
                {
                    // An error response for a command the local end stopped waiting for; see
                    // the equivalent branch in ProcessCommandResponseMessageAsync.
                    await this.ReportDiscardedCanceledCommandResponseAsync(canceledCommand).ConfigureAwait(false);
                }
                else
                {
                    string commandIdDescription = errorMessage.CommandId.HasValue ? $"for unknown command ID {errorMessage.CommandId.Value}" : "with no command ID";
                    await this.OnProtocolErrorEventReceivedAsync(new ErrorReceivedEventArgs(result)).ConfigureAwait(false);
                    this.CaptureUnhandledError(UnhandledErrorKind.UnexpectedError, new WebDriverBiDiProtocolException($"Received {result.ErrorCode} ('{result.ErrorType}') error {commandIdDescription}: {result.ErrorMessage}", result), $"Received error {commandIdDescription}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            string messageString = packet.MessageText;

            // If the malformed payload correlates to a pending command (denoted by the
            // payload having an `id` property and a command with the value of that ID
            // existing in the pending command collection), resolve the command so that
            // the caller does not have to wait for the full command timeout. Note that
            // the invalid error payload is deliberately not routed to the transport
            // unhandled error pipeline.
            if (packet.TryGetCommandId(out long commandId) && this.PendingCommands.RemovePendingCommand(commandId, out Command? executedCommand))
            {
                executedCommand.StopTiming();
                WebDriverBiDiEventSource.RaiseEvent.CommandError(commandId, executedCommand.CommandName, ErrorCode.UnsetErrorCode, "invalid error json", "Error response contained incorrect JSON for a protocol error");
                executedCommand.SetException(new WebDriverBiDiSerializationException($"Error response for command {commandId} contained incorrect JSON for protocol error (response JSON: {messageString})", ex));
                return true;
            }

            await this.LogAsync($"Unexpected error parsing error JSON: {ex.Message} (JSON: {messageString})", WebDriverBiDiLogLevel.Error).ConfigureAwait(false);
            this.CaptureUnhandledError(UnhandledErrorKind.ProtocolError, ex, $"Invalid JSON in protocol error response: {messageString}");
            WebDriverBiDiEventSource.RaiseEvent.ProtocolError(ex.Message, TruncateMessage(messageString, 100));

            // The message was recognized as an error response, and its malformed payload has
            // been captured as a protocol error above, so the message is handled.
            return true;
        }
    }

    private async Task<bool> ProcessEventMessageAsync(IncomingMessage packet)
    {
        if (!packet.TryGetEventName(out string eventName) || !this.eventMessageTypes.TryGetValue(eventName, out EventMessageRegistration? registration))
        {
            // An event with no method name, or with a method name that is not registered,
            // is reported as an unknown message by the caller: the message-level
            // forward-compatibility path for events the library does not know.
            return false;
        }

        Type eventMessageType = registration.EventMessageType;
        try
        {
            JsonTypeInfo eventTypeInfo = registration.GetTypeInfo(this.options);
            if (!packet.TryDeserializeEventMessage(eventTypeInfo, out EventMessage? eventMessageData))
            {
                throw new WebDriverBiDiSerializationException($"Deserialization of event message returned null for event type {eventMessageType}");
            }

            WebDriverBiDiEventSource.RaiseEvent.EventReceived(eventName);
            if (this.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug))
            {
                await this.LogAsync($"Received event {eventName}", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
            }

            // As for responses, extension properties inside the params object and on the event
            // envelope are exposed separately (AdditionalData and AdditionalEventProperties). The
            // converter rejects a null or missing 'params', so a deserialized event always carries
            // non-null data whose runtime type drives which payload properties are extension data.
            ReceivedDataDictionary payloadExtensionData = ConvertPayloadExtensionData(packet.CollectPayloadExtensionData("params", this.options.GetTypeInfo(eventMessageData.EventData!.GetType())));
            await this.OnProtocolEventReceivedAsync(new EventReceivedEventArgs(eventMessageData, payloadExtensionData)).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            string messageString = packet.MessageText;
            await this.LogAsync($"Unexpected error parsing event JSON: {ex.Message} (JSON: {messageString})", WebDriverBiDiLogLevel.Error).ConfigureAwait(false);
            this.CaptureUnhandledError(UnhandledErrorKind.ProtocolError, ex, $"Invalid JSON in event message: {messageString}");
            WebDriverBiDiEventSource.RaiseEvent.ProtocolError(ex.Message, TruncateMessage(messageString, 100));

            // The message was recognized as a registered event; its malformed payload has
            // been captured as a protocol error above, so the message is handled and must
            // not additionally be reported as an unknown message by the caller.
            return true;
        }
    }

    private async Task ReportDiscardedCanceledCommandResponseAsync(CanceledCommandInfo canceledCommand)
    {
        long millisecondsSinceCancellation = (long)canceledCommand.TimeSinceCancellation.TotalMilliseconds;
        WebDriverBiDiEventSource.RaiseEvent.CanceledCommandResponseDiscarded(canceledCommand.CommandId, canceledCommand.CommandName, canceledCommand.Reason, millisecondsSinceCancellation);
        if (this.IsLogLevelEnabled(WebDriverBiDiLogLevel.Debug))
        {
            await this.LogAsync($"Discarding late response for command '{canceledCommand.CommandName}' (command ID: {canceledCommand.CommandId}); the command was canceled ({canceledCommand.Reason}) {millisecondsSinceCancellation} ms before this response arrived", WebDriverBiDiLogLevel.Debug).ConfigureAwait(false);
        }
    }

    private async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        if (!this.IsLogLevelEnabled(level))
        {
            return;
        }

        await this.NotifyLogMessageObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
    }

    private async Task NotifyLogMessageObserversAsync(LogMessageEventArgs e)
    {
        // Without this, a synchronously throwing log observer would propagate into
        // whatever operation happened to emit the log message. Routing the exception
        // here keeps it governed by EventHandlerExceptionBehavior, the same as a
        // fault raised by any other event handler. Note that ReportEventObserverErrorAsync
        // does not emit log messages, so this action cannot recurse.
        try
        {
            await this.invocableLogMessageObservableEvent.InvokeNotifyObserversAsync(e).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await this.ReportEventObserverErrorAsync(this.OnLogMessage.EventName, TransportLogMessageObserverDescription, ex).ConfigureAwait(false);
        }
    }

    private Exception CreateTerminationException(IList<Exception> exceptions, TransportErrorBehavior errorBehavior = TransportErrorBehavior.Terminate)
    {
        string message = $"Unhandled exception during transport operations. Transport was terminated with the following reason: {this.TerminationReason}";
        if (errorBehavior != TransportErrorBehavior.Collect && exceptions.Count == 1)
        {
            return new WebDriverBiDiException(message, exceptions[0]);
        }

        return new AggregateException(message, exceptions);
    }

    private void ThrowIfCollectedExceptionsClaimed(bool throwCollectedExceptions)
    {
        // Throws the collected Collect-mode exceptions for the current session, if any are
        // pending and this call successfully claims them via ClaimCollectedErrors.
        // Claiming is reset for each new session by ResetCollectedErrors.
        if (throwCollectedExceptions && this.UnhandledErrors.TryGetExceptions(TransportErrorBehavior.Collect, out IList<Exception> collectedExceptions) && this.ClaimCollectedErrors())
        {
            throw this.CreateTerminationException(collectedExceptions, TransportErrorBehavior.Collect);
        }
    }

    private bool ClaimCollectedErrors()
    {
        // Atomically checks whether the Collect-mode errors for the current session have
        // already been reported and, if not, marks them reported, in a single indivisible
        // operation. This guarantees that <see cref="DisconnectAsync(bool, CancellationToken)"/>
        // throws its collected exceptions at most once per session, even if multiple callers
        // race on the pre-semaphore fast-path guard.
        return Interlocked.CompareExchange(ref this.collectedErrorsReportedFlag, 1, 0) == 0;
    }

    private void ResetCollectedErrors()
    {
        // Clears UnhandledErrors of every category's errors from the prior session, and
        // resets the collected-errors-reported flag so ClaimCollectedErrors can claim
        // the new session's Collect-mode errors.
        this.UnhandledErrors.ClearUnhandledErrors();
        Interlocked.Exchange(ref this.collectedErrorsReportedFlag, 0);
    }

    private string GetEventHandlerTerminalReason(string observableEventName)
    {
        if (observableEventName == this.OnErrorEventReceived.EventName)
        {
            return "Unhandled exception in user event handler for error event";
        }

        if (observableEventName == this.OnUnknownMessageReceived.EventName)
        {
            return "Unhandled exception in user event handler for unknown message event";
        }

        return $"Unhandled exception in user event handler for event name {observableEventName}";
    }

    private ObservableEventInvocable<T> CreateObservableEvent<T>(string eventName)
        where T : WebDriverBiDiEventArgs
    {
        ObservableEventInvocable<T> observableEvent = new(eventName);
        observableEvent.InvokeSetObserverErrorReporter(this.ReportEventObserverErrorAsync);
        return observableEvent;
    }

    private async Task ReportEventObserverErrorAsync(string eventName, string observerDescription, Exception exception)
    {
        await this.ReportEventObserverErrorAsync(new EventObserverErrorInfo()
        {
            ObservableEventName = eventName,
            ObserverId = string.Empty,
            ObserverDescription = observerDescription,
            Exception = exception,
            IsAsynchronousHandler = false,
            FaultOccurredAfterHandlerReturned = false,
        }).ConfigureAwait(false);
    }

    private async Task ReportEventObserverErrorAsync(EventObserverErrorInfo errorInfo)
    {
        // A failure in an observer of OnEventHandlerErrorOccurred itself must not be reported
        // by re-raising OnEventHandlerErrorOccurred: that would invoke the same failing
        // observer again, and for an asynchronously faulting observer would produce an
        // unbounded feedback loop of error events. Capture such a failure without notifying
        // the event that produced it.
        bool notifyObservers = errorInfo.ObservableEventName != EventHandlerErrorOccurredEventName;
        await this.ReportEventObserverErrorAsync(errorInfo, notifyObservers).ConfigureAwait(false);
    }

    private sealed class IncomingMessageQueue
    {
        private int depth = 0;

        public IncomingMessageQueue()
        {
            // We are using an unbounded channel by design. This decision was
            // carefully considered, as the rate of incoming messages is unlikely
            // to cause memory issues by exceeding the rate of processing. Should
            // real-world usage indicate otherwise, we will update this behavior
            // with a bounded channel, and add monitoring of the queue depth to
            // the transport events.
            this.MessageChannel = Channel.CreateUnbounded<IncomingMessage>(new UnboundedChannelOptions()
            {
                SingleReader = true,
                SingleWriter = true,
            });
        }

        /// <summary>
        /// Gets the channel that received incoming messages.
        /// </summary>
        public Channel<IncomingMessage> MessageChannel { get; }

        /// <summary>
        /// Gets the current count of the unread received incoming messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Interlocked-maintained mirror of the unread depth of <see cref="MessageChannel"/>.
        /// The SingleConsumerUnboundedChannel implementation returned by
        /// Channel.CreateUnbounded&lt;T&gt;(new UnboundedChannelOptions { SingleReader = true,
        /// SingleWriter = true }) does not support ChannelReader&lt;T&gt;.Count (CanCount is
        /// false), so the depth is maintained here: incremented after a successful
        /// Writer.TryWrite, decremented after a successful Reader.TryRead.
        /// </para>
        /// <para>
        /// The count belongs to this queue alone, which is what keeps it accurate across a
        /// reconnect. A connect installs a new queue rather than resetting a shared counter, and
        /// both the producer and the reader capture the queue they are operating on, so a write
        /// and the increment it produces cannot address different queues, and a reader still
        /// draining a previous connection's queue cannot decrement the current one. The value is
        /// therefore never negative and carries no transient over-count.
        /// </para>
        /// <para>
        /// This counter is used solely for observability, through
        /// <see cref="IncomingQueueDepth"/>; it does not affect correctness.
        /// </para>
        /// </remarks>
        public int Depth => Interlocked.CompareExchange(ref this.depth, 0, 0);

        /// <summary>
        /// Increments the queue depth.
        /// </summary>
        public void IncrementDepth()
        {
            Interlocked.Increment(ref this.depth);
        }

        /// <summary>
        /// Decrements the queue depth.
        /// </summary>
        public void DecrementDepth()
        {
            Interlocked.Decrement(ref this.depth);
        }

        /// <summary>
        /// Closes this queue to further writes and disposes everything still buffered in it.
        /// </summary>
        /// <remarks>
        /// Used when a queue is abandoned without a reader ever having run over it, which is what a
        /// failed connect attempt leaves behind. Each buffered <see cref="IncomingMessage"/> owns a
        /// pooled buffer that only its disposal returns, and <see cref="DecrementDepth"/> is paired with
        /// each read so that <see cref="Depth"/>, and through it
        /// <see cref="IncomingQueueDepth"/>, does not go on reporting messages that no longer exist.
        /// Completing the writer first means a late arrival from a receive loop that has not yet
        /// unwound fails its write and is disposed by the producer, rather than being added to a queue
        /// that nothing will drain again.
        /// </remarks>
        public void Drain()
        {
            this.MessageChannel.Writer.TryComplete();
            while (this.MessageChannel.Reader.TryRead(out IncomingMessage? bufferedMessage))
            {
                this.DecrementDepth();
                bufferedMessage.Dispose();
            }
        }
    }
}
