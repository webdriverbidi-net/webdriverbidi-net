namespace WebDriverBidi;

public class ProtocolUnknownMessageReceivedEventArgs : EventArgs
{
    public ProtocolUnknownMessageReceivedEventArgs(string message)
    {
        this.Message = message;
    }

    public string Message { get; }
}