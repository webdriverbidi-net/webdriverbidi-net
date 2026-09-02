// <copyright file="Connection.cs" company="WebDriverBiDi.NET Committers">
// Copyright (c) WebDriverBiDi.NET Committers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace WebDriverBiDi.Protocol;

using System.Buffers;
using System.Runtime.InteropServices;
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

    // Default buffer size is 2^20 bytes, or 1MB.
    private const int BufferSizeInBytes = 1 << 20;
    private const string DataReceivedEventName = "connection.dataReceived";
    private const string LogMessageEventName = "connection.logMessage";
    private const string ConnectionErrorEventName = "connection.connectionError";
    private const string RemoteDisconnectedEventName = "connection.remoteDisconnected";
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

    private TimeSpan startupTimeout = DefaultTimeout;
    private TimeSpan shutdownTimeout = DefaultTimeout;
    private TimeSpan dataTimeout = DefaultTimeout;

    // Note: Interlocked operations provide necessary memory barriers; volatile keyword not required
    private int isDisposedFlag;

    // Deliberately not exposed to derived classes. Reading the Token property of a disposed
    // CancellationTokenSource throws ObjectDisposedException, and the receive loop and any
    // in-flight send can outlive disposal, so subclasses are given the cached
    // ConnectionCancellationToken instead. They cancel through CancelConnection and start a
    // new session through ResetConnectionCancellation.
    private CancellationTokenSource connectionCancellationTokenSource = new();

    /// <summary>
    /// Gets a value indicating whether this connection is active.
    /// </summary>
    public abstract bool IsActive { get; }

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
    /// Gets or sets the string containing data about which the connection is connected.
    /// For a WebSocket connection, this is its URL. For a named pipe connection, it is
    /// the name of the pipe.
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
    /// Gets or sets the value of the timeout to wait before throwing an error when shutting down the connection.
    /// </summary>
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
    /// Gets or sets the value of the timeout to wait for exclusive access when sending to or receiving data from the ClientWebSocket.
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
    /// Gets an observable event that notifies when data is received from this connection.
    /// </summary>
    /// <remarks>
    /// Due to the the shared-memory nature of the data received, one, and only one,
    /// <see cref="EventObserver{ConnectionDataReceivedEventArgs}"/> can be observing this
    /// event at a time. Attempting to connect a second observer will throw an exception.
    /// </remarks>
    public ObservableEvent<ConnectionDataReceivedEventArgs> OnDataReceived => this.InvocableConnectionDataReceivedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a communication error occurs on this connection.
    /// </summary>
    public ObservableEvent<ConnectionErrorEventArgs> OnConnectionError => this.InvocableConnectionErrorObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when the remote end gracefully closes this connection.
    /// </summary>
    public ObservableEvent<ConnectionDisconnectedEventArgs> OnRemoteDisconnected => this.InvocableRemoteDisconnectedObservableEvent;

    /// <summary>
    /// Gets an observable event that notifies when a log message is written.
    /// </summary>
    public ObservableEvent<LogMessageEventArgs> OnLogMessage => this.InvocableLogMessageObservableEvent;

    /// <summary>
    /// Gets a value indicating whether this connection has been disposed.
    /// </summary>
    protected bool IsDisposed => Interlocked.CompareExchange(ref this.isDisposedFlag, 0, 0) == 1;

    /// <summary>
    /// Gets an ObservableEventInvocable that subclasses can use to raise the OnDataReceived event.
    /// </summary>
    protected ObservableEventInvocable<ConnectionDataReceivedEventArgs> InvocableConnectionDataReceivedObservableEvent { get; } = new(DataReceivedEventName, 1);

    /// <summary>
    /// Gets an ObservableEventInvocable that subclasses can use to raise the OnConnectionError event.
    /// </summary>
    protected ObservableEventInvocable<ConnectionErrorEventArgs> InvocableConnectionErrorObservableEvent { get; } = new(ConnectionErrorEventName);

    /// <summary>
    /// Gets an ObservableEventInvocable that subclasses can use to raise the OnRemoteDisconnected event.
    /// </summary>
    protected ObservableEventInvocable<ConnectionDisconnectedEventArgs> InvocableRemoteDisconnectedObservableEvent { get; } = new(RemoteDisconnectedEventName);

    /// <summary>
    /// Gets an ObservableEventInvocable that subclasses can use to raise the OnLogMessage event.
    /// </summary>
    protected ObservableEventInvocable<LogMessageEventArgs> InvocableLogMessageObservableEvent { get; } = new(LogMessageEventName);

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

    /// <summary>
    /// Asynchronously starts communication with the remote end of this connection.
    /// </summary>
    /// <param name="connectionString">The connection string used to connect to the remote end.</param>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public abstract Task StartAsync(string connectionString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously stops communication with the remote end of this connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token used to propagate notification that the operation should be canceled.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    public abstract Task StopAsync(CancellationToken cancellationToken = default);

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
            throw new WebDriverBiDiConnectionException($"The {this.ConnectionKind} has not been initialized; you must call the Start method before sending data");
        }

        // Notify log-message observers before acquiring the send semaphore to avoid
        // potential deadlocks in a malformed observer on the logging event.
        if (this.OnLogMessage.CurrentObserverCount > 0)
        {
#if NET5_0_OR_GREATER
            await this.LogAsync($"SEND >>> {Encoding.UTF8.GetString(data.Span)}", WebDriverBiDiLogLevel.Trace).ConfigureAwait(false);
#else
            await this.LogAsync($"SEND >>> {Encoding.UTF8.GetString(data.ToArray())}", WebDriverBiDiLogLevel.Trace).ConfigureAwait(false);
#endif
        }

        // Only one send operation at a time can be active on a ClientWebSocket instance,
        // so we must synchronize send access to the socket in case multiple threads are
        // attempting to send commands or other data simultaneously.
        if (!await this.DataSendSemaphore.WaitAsync(this.DataTimeout, cancellationToken).ConfigureAwait(false))
        {
            throw new WebDriverBiDiTimeoutException("Timed out waiting to access WebSocket for sending; only one send operation is permitted at a time.");
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
    public async ValueTask DisposeAsync()
    {
        try
        {
            await this.DisposeAsyncCore().ConfigureAwait(false);
        }
        finally
        {
            this.DataSendSemaphore.Dispose();
            this.connectionCancellationTokenSource.Dispose();
        }

        GC.SuppressFinalize(this);
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
        this.InvocableConnectionDataReceivedObservableEvent.InvokeSetObserverErrorReporter(reporter);
        this.InvocableConnectionErrorObservableEvent.InvokeSetObserverErrorReporter(reporter);
        this.InvocableRemoteDisconnectedObservableEvent.InvokeSetObserverErrorReporter(reporter);
        this.InvocableLogMessageObservableEvent.InvokeSetObserverErrorReporter(reporter);
    }

    /// <summary>
    /// Takes ownership of byte array containing data for a completed message, copying it into a local pool-based memory block.
    /// </summary>
    /// <param name="messageDataBuffer">A reference to the byte array containing the message data.</param>
    /// <param name="messageLength">The length of the message in the byte array, as the byte array may be bigger than the message content.</param>
    /// <returns>
    /// An <see cref="IMemoryOwner&lt;T&gt;"/> object that has ownership of the pool-based memory block.
    /// When disposed, the returned object returns the memory block to the pool.
    /// </returns>
    protected static IMemoryOwner<byte> TakeOwnershipOfReceivedData(byte[] messageDataBuffer, int messageLength)
    {
        // Creates a pool-based memory buffer, with specific, well-defined ownership semantics.
        // This allows the calling process to specifically take ownership of the memory buffer
        // rented from the pool, and then be responsible for returning it to the pool by calling
        // Dispose on it. Once the buffer is created, get a pointer to it as an array, and copy
        // the contents of the passed-in byte array buffer to the buffer rented from the pool.
        // TryGetArray will always succeed here, so the Array property of ArraySegment is never
        // null, and the null-forgiving operator (!) is appropriate here.
        IMemoryOwner<byte> messageBufferOwner = MemoryPool<byte>.Shared.Rent(messageLength);
        MemoryMarshal.TryGetArray(messageBufferOwner.Memory.Slice(0, messageLength), out ArraySegment<byte> messageBuffer);
        Buffer.BlockCopy(messageDataBuffer, 0, messageBuffer.Array!, messageBuffer.Offset, messageLength);
        return messageBufferOwner;
    }

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
    /// Asynchronously releases the resources used by this <see cref="Connection"/>.
    /// Override this method in derived classes to add custom async cleanup logic.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    protected abstract ValueTask DisposeAsyncCore();

    /// <summary>
    /// Starts the task that receives data on this connection, as defined by the implementation
    /// of <see cref="ReceiveDataAsync"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method attaches a continuation to the task on which the receive loop runs. The
    /// receive loop runs on a fire-and-forget task, and shutdown paths are not guaranteed
    /// to await it: <see cref="PipeConnection.StopAsync(CancellationToken)"/> abandons the loop
    /// when it does not finish within <see cref="ShutdownTimeout"/>, and disposal skips
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
    protected void StartDataReceiveTask()
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
    /// Call this from <see cref="StartAsync(string, CancellationToken)"/> before any operation
    /// that consumes <see cref="ConnectionCancellationToken"/>. Because
    /// <see cref="StopAsync(CancellationToken)"/> cancels the source unconditionally -- including
    /// when the connection never started -- a session that reuses the previous source would begin
    /// with cancellation already requested and could never connect.
    /// </remarks>
    protected void ResetConnectionCancellation()
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
    protected void CancelConnection()
    {
        this.connectionCancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Waits for the data receive task to complete, bounded by <see cref="ShutdownTimeout"/>. If it does not
    /// finish in time, logs a warning and returns, leaving the receive task to complete on its own.
    /// Call after <see cref="CancelConnection"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the state of the asynchronous operation.</returns>
    protected async Task WaitForReceiveTaskCompletionAsync()
    {
        if (this.DataReceiveTask is not null)
        {
            using CancellationTokenSource shutdownDelayCancelTokenSource = new();
            Task completedTask = await Task.WhenAny(this.DataReceiveTask, Task.Delay(this.ShutdownTimeout, shutdownDelayCancelTokenSource.Token)).ConfigureAwait(false);
            if (completedTask != this.DataReceiveTask)
            {
                await this.LogAsync($"Timed out waiting for {this.ConnectionKind} connection receive loop to complete during shutdown", WebDriverBiDiLogLevel.Warn).ConfigureAwait(false);
            }
            else
            {
                shutdownDelayCancelTokenSource.Cancel();
            }
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
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message)
    {
        await this.LogAsync(message, WebDriverBiDiLogLevel.Info).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously raises a logging event at the specified log level.
    /// </summary>
    /// <param name="message">The log message to raise in the event.</param>
    /// <param name="level">The <see cref="WebDriverBiDiLogLevel"/> at which to raise the event.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected async Task LogAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.InvocableLogMessageObservableEvent.InvokeNotifyObserversAsync(new LogMessageEventArgs(message, level, LoggerComponentName)).ConfigureAwait(false);
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
