namespace WebDriverBiDi.TestUtilities;

using System.Text.Json;
using System.Threading.Tasks;
using Protocol;
using WebDriverBiDi;

public class TestTransport : Transport
{
    private TimeSpan messageProcessingDelay = TimeSpan.Zero;
    private int deserializeThrowCount;
    private int disconnectCallCount;
    private int concurrentConnectLockAcquisitions = 0;

    public TestTransport(WebSocketConnection connection) : base(connection)
    {
    }

    public long LastTestCommandId => this.LastCommandId;

    public int TestPendingCommandCount => this.PendingCommands.PendingCommandCount;

    public bool IsDisposed { get; private set; }

    public bool ThrowOnDisconnect { get; set; }

    public bool ReturnCustomValue { get; set; }

    public bool ShouldCancelCommand { get; set; }

    public bool ReturnUncompletedCommand { get; set; }

    public TimeSpan MessageProcessingDelay { get => this.messageProcessingDelay; set => this.messageProcessingDelay = value; }

    public CommandResult? CustomReturnValue { get; set; }

    public int DisconnectCallCount => this.disconnectCallCount;

    public int ConcurrentConnectLockAcquisitions => this.concurrentConnectLockAcquisitions;

    public TimeSpan DisconnectDelay { get; set; } = TimeSpan.Zero;

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
    /// Gets or sets the number of remaining calls to <see cref="DeserializeMessage"/>
    /// that should throw an <see cref="InvalidOperationException"/> instead of
    /// deserializing normally. Each throwing call decrements the counter.
    /// </summary>
    public int DeserializeThrowCount { get => this.deserializeThrowCount; set => this.deserializeThrowCount = value; }

    public override async Task<Command> SendCommandAsync(CommandParameters commandParameters, CancellationToken cancellationToken = default)
    {
        if (this.ReturnUncompletedCommand)
        {
            return new TestCommand(this.LastCommandId, commandParameters);
        }

        if (this.ShouldCancelCommand)
        {
            Command returnedCommand = new Command(this.LastCommandId, commandParameters);
            returnedCommand.Cancel();
            return returnedCommand;
        }

        if (this.ReturnCustomValue)
        {
            Command returnedCommand = new Command(this.LastCommandId, commandParameters);
            returnedCommand.SetResult(this.CustomReturnValue!);
            return returnedCommand;
        }

        return await base.SendCommandAsync(commandParameters, cancellationToken);
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

    public async Task<bool> WaitForCollectedEventHandlerExceptionAsync(TimeSpan timeout, TransportErrorBehavior errorBehavior)
    {
        // This test needs to wait until the late-fault continuation has actually
        // recorded the exception into the transport's collected-error store.
        // Waiting only for the handler body to complete is not sufficient.
        UnhandledErrorCollection unhandledErrors = this.UnhandledErrors;
        DateTime endTime = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < endTime)
        {
            if (unhandledErrors.HasUnhandledErrors(errorBehavior))
            {
                return true;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10)).ConfigureAwait(false);
        }

        return unhandledErrors.HasUnhandledErrors(errorBehavior);
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
    public void EnableConnectLockConcurrencyTesting()
    {
        ManualResetEventSlim connectionMethodHasLock = new(false);
        ManualResetEventSlim otherMethodBlocked = new(false);
        this.BeforeAcquireLockCallback = async () =>
        {
            int currentCallCount = Interlocked.Increment(ref this.concurrentConnectLockAcquisitions);
            if (currentCallCount == 1)
            {
                // ConnectAsync or DisconnectAsync is first into the AcquireConnectionLockAsync,
                // and is about to acquire the lock. Signal it, then wait for the other method
                // to also enter the callback.
                connectionMethodHasLock.Set();
                await Task.Run(() => otherMethodBlocked.Wait());
            }
            else if (currentCallCount == 2)
            {
                // The other method has entered AcquireConnectionLockAsync; it will block.
                // Signal that it's here, then wait for ConnectAsync or DisconnectAsync
                // to signal it is acquiring the lock, and add a small delay so the
                // semaphore is held by ConnectAsync or DisconnectAsync.
                otherMethodBlocked.Set();
                await Task.Run(() => connectionMethodHasLock.Wait());
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }
        };
    }

    protected override JsonElement DeserializeMessage(byte[] messageData)
    {
        if (Interlocked.Decrement(ref this.deserializeThrowCount) >= 0)
        {
            throw new InvalidOperationException("Simulated deserialization failure");
        }

        return base.DeserializeMessage(messageData);
    }

    protected override async Task DisconnectAsync(bool throwCollectedExceptions, CancellationToken cancellationToken = default)
    {
        if (this.ThrowOnDisconnect)
        {
            throw new WebDriverBiDiException("Simulated disconnect failure");
        }

        if (this.DisconnectDelay > TimeSpan.Zero)
        {
            await Task.Delay(this.DisconnectDelay, cancellationToken).ConfigureAwait(false);
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

        await base.AcquireConnectionLockAsync(cancellationToken).ConfigureAwait(false);

        this.AfterAcquireLockCallback?.Invoke();
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
            TaskCompletionSource tcs = new();
            tcs.SetException(this.ReadLoopOuterFault);
            return tcs.Task;
        }

        return base.ReadIncomingMessagesAsync();
    }
}
