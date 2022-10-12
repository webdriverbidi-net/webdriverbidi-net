namespace WebDriverBidi;

public class ProtocolErrorReceivedEventArgs : EventArgs
{
    public ProtocolErrorReceivedEventArgs(ErrorResponse? errorData)
    {
        this.ErrorData = errorData;
    }

    public ErrorResponse? ErrorData { get; }
}