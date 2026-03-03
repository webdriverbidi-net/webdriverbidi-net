namespace WebDriverBiDi.TestUtilities;

using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Protocol;
using WebDriverBiDi;

public class TestTransport : Transport
{
    private TimeSpan messageProcessingDelay = TimeSpan.Zero;
    private int deserializeThrowCount;
    private int disconnectCallCount;

    public TestTransport(Connection connection) : base(connection)
    {
    }

    public long LastTestCommandId => this.LastCommandId;

    public bool IsDisposed { get; private set; }

    public bool ThrowOnDisconnect { get; set; }

    public bool ReturnCustomValue { get; set; }

    public bool ShouldCancelCommand { get; set; }

    public bool ReturnUncompletedCommand { get; set; }

    public TimeSpan MessageProcessingDelay { get => this.messageProcessingDelay; set => this.messageProcessingDelay = value; }

    public CommandResult? CustomReturnValue { get; set; }

    public int DisconnectCallCount => this.disconnectCallCount;

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
            return new TestCommand(LastCommandId, commandParameters);
        }

        if (this.ShouldCancelCommand)
        {
            Command returnedCommand = new Command(LastCommandId, commandParameters);
            returnedCommand.Cancel();
            return returnedCommand;
        }

        if (this.ReturnCustomValue)
        {
            Command returnedCommand = new Command(LastCommandId, commandParameters);
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
}
