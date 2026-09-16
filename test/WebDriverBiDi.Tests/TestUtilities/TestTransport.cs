namespace WebDriverBiDi.TestUtilities;

using System.Buffers;
using System.Text.Json;
using System.Threading.Tasks;
using Protocol;
using WebDriverBiDi;

public class TestTransport : Transport
{
    private int deserializeThrowCount;
    private int disconnectCallCount;
    private int concurrentConnectLockAcquisitions = 0;
    private Func<Task>? afterAcquireLockAsyncCallback;

    public TestTransport(WebSocketConnection connection, TimeProvider? timeProvider = null) : base(connection)
    {
        if (timeProvider is not null)
        {
            this.TimeProvider = timeProvider;
        }
    }

    public long LastTestCommandId => this.LastCommandId;

    public int TestPendingCommandCount => this.PendingCommands.PendingCommandCount;

    /// <summary>
    /// Gets a value indicating whether the current pending command collection still accepts commands, which is
    /// what tells a session left standing from one whose collection a teardown has closed.
    /// </summary>
    public bool TestIsAcceptingCommands => this.PendingCommands.IsAcceptingCommands;

    /// <summary>
    /// Gets the ID of the current pending command collection, which changes when the collection
    /// is replaced by a reconnect. Used to assert that a reconnect actually replaced the
    /// collection that <see cref="Transport.SendCommandAsync"/> compares against.
    /// </summary>
    public string TestPendingCommandCollectionId => this.PendingCommands.Id;

    /// <summary>
    /// Gets the number of canceled commands the current pending command collection remembers, so that a
    /// test can assert that a capacity configured through <see cref="UseCanceledCommandTrackerCapacity"/>
    /// survives the replacement of the collection on reconnect.
    /// </summary>
    public uint TestMaxTrackedCanceledCommands => this.PendingCommands.MaxTrackedCanceledCommands;

    public bool IsDisposed { get; private set; }

    public bool ThrowOnDisconnect { get; set; }

    public bool ReturnCustomValue { get; set; }

    public bool ShouldCancelCommand { get; set; }

    public bool ReturnUncompletedCommand { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="TestCommand.CompletionBehavior"/> applied to the command returned
    /// when <see cref="ReturnUncompletedCommand"/> is set.
    /// </summary>
    public Func<TestCommand, bool>? UncompletedCommandBehavior { get; set; }

    public CommandResult? CustomReturnValue { get; set; }

    public int DisconnectCallCount => this.disconnectCallCount;

    public int ConcurrentConnectLockAcquisitions => this.concurrentConnectLockAcquisitions;

    /// <summary>
    /// Optional callback invoked before acquiring the connection lock.
    /// Used for precise test synchronization.
    /// </summary>
    public Func<Task>? BeforeAcquireLockCallback { get; set; }

    /// <summary>
    /// Optional callback invoked after acquiring the connection lock.
    /// Used for precise test synchronization.
    /// </summary>
    public Action? AfterAcquireLockCallback { get; set; }

    /// <summary>
    /// Optional callback invoked when a wait for the connection lock is abandoned rather than granted.
    /// </summary>
    public Action? AcquireLockFailedCallback { get; set; }

    /// <summary>
    /// Optional asynchronous callback invoked once, immediately after the next acquisition of
    /// the connection lock, and then cleared. Because the lock is held while it runs, the
    /// callback can deterministically stage work that must overlap the lock holder's critical
    /// section (for example, ending the connection's receive loop while
    /// <see cref="Transport.DisconnectAsync(CancellationToken)"/> holds the lock).
    /// </summary>
    public Func<Task>? AfterAcquireLockAsyncCallback
    {
        get => Interlocked.CompareExchange(ref this.afterAcquireLockAsyncCallback, null, null);
        set => Interlocked.Exchange(ref this.afterAcquireLockAsyncCallback, value);
    }

    /// <summary>
    /// Optional callback invoked after an unhandled error is captured by the
    /// Transport unhandled error mechanism.
    /// Used for precise test synchronization.
    /// </summary>
    /// <summary>
    /// Gets or sets a gate awaited by the message-processing loop before each message is processed. Use it to
    /// hold a message in flight deterministically instead of relying on a timed delay.
    /// </summary>
    public Func<Task>? MessageProcessingGate { get; set; }

    /// <summary>
    /// Gets or sets a callback invoked when the message-processing loop reaches the gate, before awaiting it.
    /// </summary>
    public Action? MessageProcessingStarted { get; set; }

    public Action? AfterUnhandledErrorCaptured { get; set; }

    private TaskCompletionSource unhandledErrorCapturedSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets or sets the number of remaining calls to <see cref="DeserializeMessage"/>
    /// that should throw an <see cref="InvalidOperationException"/> instead of
    /// deserializing normally. Each throwing call decrements the counter.
    /// </summary>
    public int DeserializeThrowCount { get => this.deserializeThrowCount; set => this.deserializeThrowCount = value; }

    public override async Task<Command> SendCommandAsync(CommandParameters commandParameters, CancellationToken cancellationToken = default)
    {
        if (this.ReturnUncompletedCommand)
        {
            return new TestCommand(this.LastCommandId, commandParameters) { CompletionBehavior = this.UncompletedCommandBehavior };
        }

        if (this.ShouldCancelCommand)
        {
            Command returnedCommand = new(this.LastCommandId, commandParameters, this.TimeProvider);
            returnedCommand.Cancel();
            return returnedCommand;
        }

        if (this.ReturnCustomValue)
        {
            Command returnedCommand = new(this.LastCommandId, commandParameters, this.TimeProvider);
            if (this.CustomReturnValue is null)
            {
                returnedCommand.SetResult(null!);
            }
            else
            {
                returnedCommand.SetResult(this.CustomReturnValue);
            }

            return returnedCommand;
        }

        return await base.SendCommandAsync(commandParameters, cancellationToken);
    }

    /// <summary>
    /// Replaces the pending command collection with one that remembers at most the specified number
    /// of canceled commands. Must be called before <see cref="Transport.ConnectAsync"/>.
    /// </summary>
    /// <param name="maxTrackedCanceledCommands">The maximum number of canceled commands to remember.</param>
    public void UseCanceledCommandTrackerCapacity(uint maxTrackedCanceledCommands)
    {
        this.PendingCommands = new PendingCommandCollection(maxTrackedCanceledCommands);
    }

    public Connection GetConnection()
    {
        return this.Connection;
    }

    /// <summary>
    /// Registers an arbitrary type for an event name, bypassing the normal
    /// EventMessage&lt;T&gt; wrapping. Used to test deserialization failure paths
    /// where the deserialized type is not an EventMessage.
    /// </summary>
    public void RegisterInvalidEventMessageType(string eventName, Type type)
    {
        this.AddEventMessageType(eventName, type);
    }

    /// <summary>
    /// Waits until the late-fault continuation has recorded an exception into the transport's
    /// collected-error store for the given behavior. Waiting only for the handler body to complete is not
    /// sufficient, because the capture happens on a continuation after the handler returns.
    /// </summary>
    /// <param name="timeout">A safety bound. The wait ends when the error is captured, not when this elapses.</param>
    /// <param name="errorBehavior">The behavior whose collected errors are of interest.</param>
    /// <returns><see langword="true"/> if an error was captured for that behavior.</returns>
    /// <remarks>
    /// Each capture completes the current signal and installs a fresh one, so a wait resumes on the next
    /// capture rather than on a timer. The signal is snapshotted <em>before</em> the predicate is tested: a
    /// capture landing between the two would otherwise complete a signal this loop has already replaced,
    /// and the wait would hang until the safety bound even though the condition it wanted had been met.
    /// </remarks>
    public async Task<bool> WaitForCollectedEventHandlerExceptionAsync(TimeSpan timeout, TransportErrorBehavior errorBehavior)
    {
        UnhandledErrorCollection unhandledErrors = this.UnhandledErrors;
        using CancellationTokenSource safetyBound = new(timeout);
        while (true)
        {
            Task nextCapture = Volatile.Read(ref this.unhandledErrorCapturedSignal).Task;
            if (unhandledErrors.HasUnhandledErrors(errorBehavior))
            {
                return true;
            }

            try
            {
                await nextCapture.WaitAsync(safetyBound.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return unhandledErrors.HasUnhandledErrors(errorBehavior);
            }
        }
    }

    /// <summary>
    /// COnfigures the transport to simulate concurrent acquisition of the connection lock
    /// by ConnectAsync and DisconnectAsync, which can occur when a connection error
    /// happens during connection or disconnection. This is used to test for correct
    /// handling of such concurrency, such as ensuring that only one method actually
    /// acquires the lock and processes, and that the other method correctly observes
    /// the post-lock state (connected vs. disconnected) when it acquires the lock after
    /// the first method releases it. The orchestration of the concurrency is tricky,
    /// and is done here to centralize the logic and ensure correct timing.
    /// </summary>
    public Task EnableConnectLockConcurrencyTesting()
    {
        TaskCompletionSource firstCallerReadyTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondCallerReadyTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource firstCallerAcquiredLockTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        // Signal once caller 1 has actually acquired the semaphore so caller 2 can
        // proceed deterministically rather than relying on a fixed-duration delay.
        this.AfterAcquireLockCallback = () => firstCallerAcquiredLockTaskCompletionSource.TrySetResult();

        this.BeforeAcquireLockCallback = async () =>
        {
            int currentCallCount = Interlocked.Increment(ref this.concurrentConnectLockAcquisitions);
            if (currentCallCount == 1)
            {
                // ConnectAsync or DisconnectAsync is first into the AcquireConnectionLockAsync,
                // and is about to acquire the lock. Signal it, then wait for the other method
                // to also enter the callback.
                firstCallerReadyTaskCompletionSource.TrySetResult();
                await secondCallerReadyTaskCompletionSource.Task;
            }
            else if (currentCallCount == 2)
            {
                // The other method has entered AcquireConnectionLockAsync; it will block.
                // Signal that it's here, then wait until caller 1 has acquired the semaphore
                // before proceeding — this is deterministic unlike a fixed-duration delay.
                secondCallerReadyTaskCompletionSource.TrySetResult();
                await firstCallerAcquiredLockTaskCompletionSource.Task;
            }
        };

        // Return the task that completes when the first caller has entered the callback.
        // Tests that care about which caller is "first" should start their first task,
        // await this returned task, then start their second task.
        return firstCallerReadyTaskCompletionSource.Task;
    }

    protected override IncomingMessage CreateIncomingMessage(IMemoryOwner<byte> owner, int length)
    {
        bool throwOnDeserialization = Interlocked.Decrement(ref this.deserializeThrowCount) >= 0;
        return new TestIncomingMessage(owner, length, throwOnDeserialization);
    }

    protected override async Task ProcessMessageAsync(IncomingMessage packet)
    {
        if (this.MessageProcessingGate is not null)
        {
            // Hold the message-processing loop deterministically, without a timed delay, so a test can
            // observe the transport's behavior while a message is still in flight.
            this.MessageProcessingStarted?.Invoke();
            await this.MessageProcessingGate().ConfigureAwait(false);
        }

        await base.ProcessMessageAsync(packet).ConfigureAwait(false);
    }

    protected override async Task DisconnectAsync(bool throwCollectedExceptions, CancellationToken cancellationToken = default)
    {
        if (this.ThrowOnDisconnect)
        {
            throw new WebDriverBiDiException("Simulated disconnect failure");
        }

        await base.DisconnectAsync(throwCollectedExceptions, cancellationToken).ConfigureAwait(false);

        // Only increment after base.DisconnectAsync completes, which means the disconnect
        // logic actually executed (didn't return early via fast-path or double-check)
        Interlocked.Increment(ref this.disconnectCallCount);
    }

    protected override async Task AcquireConnectionLockAsync(CancellationToken cancellationToken = default)
    {
        if (this.BeforeAcquireLockCallback is not null)
        {
            await this.BeforeAcquireLockCallback().ConfigureAwait(false);
        }

        try
        {
            await base.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (WebDriverBiDiTimeoutException)
        {
            // Signalled before the rethrow, so a test observes the abandonment before the code under
            // test does.
            this.AcquireLockFailedCallback?.Invoke();
            throw;
        }

        this.AfterAcquireLockCallback?.Invoke();

        Func<Task>? afterAcquireAsyncCallback = Interlocked.Exchange(ref this.afterAcquireLockAsyncCallback, null);
        if (afterAcquireAsyncCallback is not null)
        {
            await afterAcquireAsyncCallback().ConfigureAwait(false);
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        this.IsDisposed = true;
        await base.DisposeAsyncCore().ConfigureAwait(false);
    }

    /// <summary>
    /// Gets or sets one or more exceptions that should fault the message-processing
    /// loop immediately. Used to exercise the fault continuation attached in
    /// Transport.ConnectAsync, including both its single-inner and multi-inner
    /// AggregateException branches.
    /// </summary>
    /// <remarks>
    /// When a single exception is provided, the processing task is faulted via
    /// <see cref="Task.FromException(Exception)"/>, producing an AggregateException
    /// with a single inner exception. When two or more exceptions are provided,
    /// a <see cref="TaskCompletionSource"/> is faulted with the full set, producing
    /// an AggregateException whose InnerExceptions contains every supplied
    /// exception, which triggers the aggregate-unwrapping branch in
    /// Transport.LogMessageProcessingFault.
    /// </remarks>
    public Exception[]? ReadLoopOuterFault { get; set; }

    protected override Task ReadIncomingMessagesAsync()
    {
        if (this.ReadLoopOuterFault is { Length: > 0 })
        {
            if (this.ReadLoopOuterFault.Length == 1)
            {
                return Task.FromException(this.ReadLoopOuterFault[0]);
            }

            // TaskCompletionSource.SetException(IEnumerable<Exception>) produces a
            // faulted task whose Exception.InnerExceptions contains every supplied
            // exception, which is what we need to drive the Count != 1 branch.
            TaskCompletionSource taskCompletionSource = new();
            taskCompletionSource.SetException(this.ReadLoopOuterFault);
            return taskCompletionSource.Task;
        }

        return base.ReadIncomingMessagesAsync();
    }

    protected override void CaptureUnhandledError(UnhandledErrorKind errorType, Exception ex, string terminalReason)
    {
        base.CaptureUnhandledError(errorType, ex, terminalReason);

        // Release anyone waiting on the previous signal and arm a fresh one, so a later capture can be
        // awaited too. Exchanging before completing means a waiter that snapshotted the old signal is
        // woken, while one arriving afterwards waits on the new one.
        TaskCompletionSource capturedSignal = Interlocked.Exchange(
            ref this.unhandledErrorCapturedSignal,
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
        capturedSignal.TrySetResult();

        this.AfterUnhandledErrorCaptured?.Invoke();
    }
}
