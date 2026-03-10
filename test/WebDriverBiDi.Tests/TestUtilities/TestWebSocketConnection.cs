namespace WebDriverBiDi.TestUtilities;

using System.Net.WebSockets;
using System.Text;
using WebDriverBiDi.Protocol;

public class TestWebSocketConnection : WebSocketConnection
{
    private int receiveCallCount;
    private int stopCallCount;

    public bool BypassStart { get; set; } = true;

    public bool BypassStop { get; set; } = true;

    public bool BypassDataSend { get; set; } = true;

    public bool ThrowOnStop { get; set; }

    public int StopCallCount => this.stopCallCount;

    public string? DataSent { get; set; }

    public TimeSpan? DataSendDelay { get; set; }

    public TimeSpan? StopDelay { get; set; }

    public TaskCompletionSource? StartBarrier { get; set; }

    public Func<ArraySegment<byte>, CancellationToken, int, Task<WebSocketReceiveResult>>? ReceiveHandler { get; set; }

    public Func<bool>? IsActiveOverride { get; set; }

    public Func<ArraySegment<byte>, Task>? SendWebSocketDataOverride { get; set; }

    public bool Disposed => this.IsDisposed;

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

    public event EventHandler<TestWebSocketConnectionDataSentEventArgs>? DataSendComplete;

    public async Task RaiseDataReceivedEventAsync(string data)
    {
        await this.OnDataReceived.NotifyObserversAsync(new ConnectionDataReceivedEventArgs(Encoding.UTF8.GetBytes(data)));
    }

    public async Task RaiseLogMessageEventAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, level, "TestWebSocketConnection"));
    }

    public async Task RaiseConnectionErrorEventAsync(Exception exception)
    {
        await this.OnConnectionError.NotifyObserversAsync(new ConnectionErrorEventArgs(exception));
    }

    public async Task RaiseRemoteDisconnectedEventAsync()
    {
        await this.OnRemoteDisconnected.NotifyObserversAsync(new ConnectionDisconnectedEventArgs());
    }

    public override async Task StartAsync(string url, CancellationToken cancellationToken = default)
    {
        this.ConnectionString = url;
        if (this.StartBarrier is not null)
        {
            await this.StartBarrier.Task.ConfigureAwait(false);
        }

        if (!this.BypassStart)
        {
            await base.StartAsync(url, cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref this.stopCallCount);

        if (this.BypassStop)
        {
            return;
        }
        else if (this.ThrowOnStop)
        {
            throw new WebDriverBiDiException("Simulated stop failure");
        }
        else
        {
            if (this.StopDelay.HasValue && this.StopDelay.Value > TimeSpan.Zero)
            {
                await Task.Delay(this.StopDelay.Value, cancellationToken).ConfigureAwait(false);
            }

            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public override Task SendDataAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (this.BypassStart)
        {
            // Bypass the check to see if the connection has been started,
            // so that we can test the plumbing without needing an actual
            // WebSocket server active.
            return this.SendWebSocketDataAsync(data, cancellationToken);
        }

        return base.SendDataAsync(data, cancellationToken);
    }

    protected override Task SendWebSocketDataAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default)
    {
        if (this.SendWebSocketDataOverride is not null)
        {
            return this.SendWebSocketDataOverride(data);
        }

        this.OnDataSendStarting();
        this.DataSent = Encoding.UTF8.GetString(data);
        Task result = Task.CompletedTask;
        if (!this.BypassDataSend)
        {
            result = base.SendWebSocketDataAsync(data, cancellationToken);
        }

        if (this.DataSendDelay.HasValue)
        {
            Task.Delay(this.DataSendDelay.Value).Wait();
        }

        this.OnDataSendComplete();
        return result;
    }

    protected override async Task<WebSocketReceiveResult> ReceiveWebSocketDataAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        int currentCall = Interlocked.Increment(ref this.receiveCallCount);
        if (this.ReceiveHandler is not null)
        {
            return await this.ReceiveHandler(buffer, cancellationToken, currentCall);
        }

        return await base.ReceiveWebSocketDataAsync(buffer, cancellationToken);
    }

    protected override Task CloseClientWebSocketAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnDataSendStarting()
    {
        if (this.DataSendStarting is not null)
        {
            this.DataSendStarting(this, new EventArgs());
        }
    }

    protected virtual void OnDataSendComplete()
    {
        if (this.DataSendComplete is not null)
        {
            this.DataSendComplete(this, new TestWebSocketConnectionDataSentEventArgs(this.DataSent));
        }
    }
}
