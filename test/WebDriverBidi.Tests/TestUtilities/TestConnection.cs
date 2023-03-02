namespace WebDriverBidi.TestUtilities;

using WebDriverBidi.Protocol;

public class TestConnection : Connection
{
    public string? DataSent { get; set; }

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
        return Task.CompletedTask;
    }

    public override Task SendData(string data)
    {
        this.DataSent = data;
        this.OnDataSendComplete();
        return Task.CompletedTask;
    }

    public override Task Stop()
    {
        return Task.CompletedTask;
    }

    protected virtual void OnDataSendComplete()
    {
        if (this.DataSendComplete is not null)
        {
            this.DataSendComplete(this, new TestConnectionDataSentEventArgs(this.DataSent));
        }
    }
}