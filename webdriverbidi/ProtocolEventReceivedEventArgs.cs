namespace WebDriverBidi;

public class ProtocolEventReceivedEventArgs : EventArgs
{
    public ProtocolEventReceivedEventArgs(string methodName, object? eventData)
    {
        this.EventName = methodName;
        this.EventData = eventData;
    }

    public string EventName { get; }

    public object? EventData { get; }
}