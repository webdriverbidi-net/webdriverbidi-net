namespace WebDriverBidi;

using Newtonsoft.Json.Linq;

public class ProtocolErrorReceivedEventArgs : EventArgs
{
    public ProtocolErrorReceivedEventArgs(JToken errorData)
    {
        this.ErrorData = errorData;
    }

    public JToken ErrorData { get; }
}