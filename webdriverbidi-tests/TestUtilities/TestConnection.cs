namespace WebDriverBidi.TestUtilities;

public class TestConnection : Connection
{
    public string? DataSent { get; set; }

    public void RaiseDataReceivedEvent(string data)
    {
        this.OnDataReceived(new DataReceivedEventArgs(data));
    }

    public void RaiseLogMessageEvent(string message, WebDriverBidiLogLevel level)
    {
        this.OnLogMessage(new LogMessageEventArgs(message, level));
    }

    public override Task Start(string url)
    {
        return Task.CompletedTask;
    }

    public override Task SendData(string data)
    {
        this.DataSent = data;
        return Task.CompletedTask;
    }

    public override Task Stop()
    {
        return Task.CompletedTask;
    }
}