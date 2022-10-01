namespace WebDriverBidi;

using Newtonsoft.Json.Linq;

public class ProtocolUnknownMessageReceivedEventArgs : EventArgs
{
    public ProtocolUnknownMessageReceivedEventArgs(JToken message)
    {
        this.Message = message;
    }

    public JToken Message { get; }
}