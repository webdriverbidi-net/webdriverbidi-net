namespace WebDriverBiDi.TestUtilities;

using System.Text;
using WebDriverBiDi.Protocol;

public class TestConnection : Connection
{
    public bool BypassStart { get; set; } = true;

    public bool BypassStop { get; set; } = true;

    public bool BypassDataSend { get; set; } = true;

    public string? DataSent { get; set; }

    public TimeSpan? DataSendDelay { get; set; }

    public event EventHandler? DataSendStarting;

    public event EventHandler<TestConnectionDataSentEventArgs>? DataSendComplete;

    public void RaiseDataReceivedEvent(string data)
    {
        this.OnDataReceived(new ConnectionDataReceivedEventArgs(data));
    }

    public void RaiseLogMessageEvent(string message, WebDriverBiDiLogLevel level)
    {
        this.OnLogMessage(new LogMessageEventArgs(message, level, "TestConnection"));
    }

    public override Task StartAsync(string url)
    {
        this.ConnectedUrl = url;
        if (this.BypassStart)
        {
            return Task.CompletedTask;
        }
        else
        {
            return base.StartAsync(url);
        }
    }

    public override Task StopAsync()
    {
        if (this.BypassStop)
        {
            return Task.CompletedTask;
        }
        else
        {
            return base.StopAsync();
        }
    }

    public override Task SendDataAsync(string data)
    {
        if (this.BypassStart)
        {
            // Bypass the check to see if the connection has been started,
            // so that we can test the plumbing without needing an actual
            // WebSocket server active.
            return this.SendWebSocketDataAsync(Encoding.UTF8.GetBytes(data));
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