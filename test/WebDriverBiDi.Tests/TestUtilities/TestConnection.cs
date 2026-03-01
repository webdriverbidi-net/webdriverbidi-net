namespace WebDriverBiDi.TestUtilities;

using System.Net.WebSockets;
using System.Text;
using WebDriverBiDi.Protocol;

public class TestConnection : Connection
{
    private int receiveCallCount;

    public bool BypassStart { get; set; } = true;

    public bool BypassStop { get; set; } = true;

    public bool BypassDataSend { get; set; } = true;

    public bool ThrowOnStop { get; set; }

    public string? DataSent { get; set; }

    public TimeSpan? DataSendDelay { get; set; }

    public TaskCompletionSource? StartBarrier { get; set; }

    public Func<ArraySegment<byte>, CancellationToken, int, Task<WebSocketReceiveResult>>? ReceiveHandler { get; set; }

    public override bool IsActive
    {
        get
        {
            if (this.ThrowOnStop)
            {
                return true;
            }

            return base.IsActive;
        }
    }

    public event EventHandler? DataSendStarting;

    public event EventHandler<TestConnectionDataSentEventArgs>? DataSendComplete;

    public async Task RaiseDataReceivedEventAsync(string data)
    {
        await this.OnDataReceived.NotifyObserversAsync(new ConnectionDataReceivedEventArgs(Encoding.UTF8.GetBytes(data)));
    }

    public async Task RaiseLogMessageEventAsync(string message, WebDriverBiDiLogLevel level)
    {
        await this.OnLogMessage.NotifyObserversAsync(new LogMessageEventArgs(message, level, "TestConnection"));
    }

    public override async Task StartAsync(string url)
    {
        this.ConnectedUrl = url;
        if (this.StartBarrier is not null)
        {
            await this.StartBarrier.Task.ConfigureAwait(false);
        }

        if (!this.BypassStart)
        {
            await base.StartAsync(url).ConfigureAwait(false);
        }
    }

    public override Task StopAsync()
    {
        if (this.BypassStop)
        {
            return Task.CompletedTask;
        }
        else if (this.ThrowOnStop)
        {
            throw new WebDriverBiDiException("Simulated stop failure");
        }
        else
        {
            return base.StopAsync();
        }
    }

    public override Task SendDataAsync(byte[] data)
    {
        if (this.BypassStart)
        {
            // Bypass the check to see if the connection has been started,
            // so that we can test the plumbing without needing an actual
            // WebSocket server active.
            return this.SendWebSocketDataAsync(data);
        }

        return base.SendDataAsync(data);
    }

    protected override Task SendWebSocketDataAsync(ArraySegment<byte> data)
    {
        this.OnDataSendStarting();
        this.DataSent = Encoding.UTF8.GetString(data);
        Task result = Task.CompletedTask;
        if (!this.BypassDataSend)
        {
            result = base.SendWebSocketDataAsync(data);
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

    protected override Task CloseClientWebSocketAsync()
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
            this.DataSendComplete(this, new TestConnectionDataSentEventArgs(this.DataSent));
        }
    }
}
