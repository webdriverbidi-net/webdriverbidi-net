namespace WebDriverBidi.TestUtilities;

using System.Text;
using WebDriverBidi.Protocol;

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

    public void RaiseLogMessageEvent(string message, WebDriverBidiLogLevel level)
    {
        this.OnLogMessage(new LogMessageEventArgs(message, level, "TestConnection"));
    }

    public override Task Start(string url)
    {
        this.ConnectedUrl = url;
        if (this.BypassStart)
        {
            return Task.CompletedTask;
        }
        else
        {
            return base.Start(url);
        }
    }

    public override Task Stop()
    {
        if (this.BypassStop)
        {
            return Task.CompletedTask;
        }
        else
        {
            return base.Stop();
        }
    }

    public override Task SendData(string data)
    {
        if (this.BypassStart)
        {
            // Bypass the check to see if the connection has been started,
            // so that we can test the plumbing without needing an actual
            // WebSocket server active.
            return this.SendWebSocketData(Encoding.UTF8.GetBytes(data));
        }

        return base.SendData(data);
    }

    protected override Task SendWebSocketData(ArraySegment<byte> data)
    {
        this.OnDataSendStarting();
        this.DataSent = Encoding.UTF8.GetString(data);
        Task result = Task.CompletedTask;
        if (!this.BypassDataSend)
        {
            result = base.SendWebSocketData(data);
        }

        if (this.DataSendDelay.HasValue)
        {
            Task.Delay(this.DataSendDelay.Value).Wait();
        }

        this.OnDataSendComplete();
        return result;
    }

    protected override Task CloseClientWebSocket()
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