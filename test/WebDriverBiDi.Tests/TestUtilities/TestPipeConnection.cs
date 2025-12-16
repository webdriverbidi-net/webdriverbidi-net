namespace WebDriverBiDi.TestUtilities;

using WebDriverBiDi.Protocol;

public class TestPipeConnection: PipeConnection
{
    public bool BypassDataSend { get; set; } = true;

    public TimeSpan? DataSendDelay { get; set; }
    
    public event EventHandler? DataSendStarting;

    protected override Task SendPipeDataAsync(byte[] messageBuffer)
    {
        this.OnDataSendStarting();
        Task result = Task.CompletedTask;
        if (!this.BypassDataSend)
        {
            result = base.SendPipeDataAsync(messageBuffer);
        }

        if (this.DataSendDelay.HasValue)
        {
            Task.Delay(this.DataSendDelay.Value).Wait();
        }

        return result;
    }

    protected virtual void OnDataSendStarting()
    {
        if (this.DataSendStarting is not null)
        {
            this.DataSendStarting(this, new EventArgs());
        }
    }
}