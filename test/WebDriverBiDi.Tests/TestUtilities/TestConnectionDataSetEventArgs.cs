namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json.Linq;

public class TestConnectionDataSentEventArgs : EventArgs
{
    private readonly long? sentCommandId;
    private readonly string? sentCommandName;

    public TestConnectionDataSentEventArgs(string? dataSent)
    {
        if (dataSent is not null)
        {
            JObject jsonObject = JObject.Parse(dataSent);
            this.sentCommandId = jsonObject["id"]!.Value<long>();
            this.sentCommandName = jsonObject["method"]!.Value<string>();
        }
    }

    public long SentCommandId => this.sentCommandId!.Value;

    public string? SentCommandName => this.sentCommandName;
}
