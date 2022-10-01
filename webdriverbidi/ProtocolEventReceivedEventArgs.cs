namespace WebDriverBidi;

using Newtonsoft.Json.Linq;

public class ProtocolEventReceivedEventArgs : EventArgs
{
    public ProtocolEventReceivedEventArgs(string methodName, JToken eventData)
    {
        this.EventName = methodName;
        this.EventData = eventData;
    }

    public string EventName { get; }

    public JToken EventData { get; }
}