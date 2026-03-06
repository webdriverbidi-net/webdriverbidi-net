namespace WebDriverBiDi.TestUtilities;

using System.IO;
using System.IO.Pipes;
using WebDriverBiDi.Protocol;

public class TestPipeConnection : PipeConnection
{
    private int receiveCallCount;

    public TestPipeConnection(IPipeServerProcessProvider pipeServerProcessProvider)
        : base(pipeServerProcessProvider)
    {
    }

    public bool BypassDataSend { get; set; } = true;

    public bool ThrowOnStop { get; set; }

    public bool ThrowIOExceptionOnSend { get; set; }

    public bool ThrowObjectDisposedExceptionOnSend { get; set; }

    public bool ThrowIOExceptionOnReceive { get; set; }

    public bool ThrowObjectDisposedExceptionOnReceive { get; set; }

    public TimeSpan? DataSendDelay { get; set; }

    public Func<bool>? IsActiveOverride { get; set; }

    public bool Disposed => this.IsDisposed;

    public bool PipesDisposed
    {
        get => this.AreConnectionPipesDisposed;
        set => this.AreConnectionPipesDisposed = value;
    }

    public override bool IsActive
    {
        get
        {
            if (this.IsActiveOverride is not null)
            {
                return this.IsActiveOverride();
            }

            if (this.ThrowOnStop)
            {
                return true;
            }

            return base.IsActive;
        }
    }

    public event EventHandler? DataSendStarting;

    public override Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (this.ThrowOnStop)
        {
            throw new WebDriverBiDiException("Simulated stop failure");
        }

        return base.StopAsync(cancellationToken);
    }

    protected override Task SendPipeDataAsync(byte[] messageBuffer, CancellationToken cancellationToken = default)
    {
        this.OnDataSendStarting();

        if (this.ThrowIOExceptionOnSend)
        {
            throw new IOException("Simulated pipe write failure");
        }

        if (this.ThrowObjectDisposedExceptionOnSend)
        {
            throw new ObjectDisposedException("Simulated pipe disposed");
        }

        Task result = Task.CompletedTask;
        if (!this.BypassDataSend)
        {
            result = base.SendPipeDataAsync(messageBuffer, cancellationToken);
        }

        if (this.DataSendDelay.HasValue)
        {
            Task.Delay(this.DataSendDelay.Value).Wait();
        }

        return result;
    }

    protected override Task<int> ReadPipeDataAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        int currentCall = Interlocked.Increment(ref this.receiveCallCount);

        // For exception tests, return fake data on first call to allow second call to happen
        if (currentCall == 1 && (this.ThrowIOExceptionOnReceive || this.ThrowObjectDisposedExceptionOnReceive))
        {
            byte[] fakeData = System.Text.Encoding.UTF8.GetBytes("test\0");
            Array.Copy(fakeData, 0, buffer, offset, fakeData.Length);
            return Task.FromResult(fakeData.Length);
        }

        // Throw on second call
        if (currentCall > 1)
        {
            if (this.ThrowIOExceptionOnReceive)
            {
                throw new IOException("Simulated pipe read failure");
            }

            if (this.ThrowObjectDisposedExceptionOnReceive)
            {
                throw new ObjectDisposedException("Simulated pipe disposed during read");
            }
        }

        return base.ReadPipeDataAsync(buffer, offset, count, cancellationToken);
    }

    protected virtual void OnDataSendStarting()
    {
        if (this.DataSendStarting is not null)
        {
            this.DataSendStarting(this, new EventArgs());
        }
    }
}
