namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json.Linq;

public class TestConnectionDataSentEventArgs : EventArgs
{
    private readonly long? sentCommandId;
    private readonly string? dataSent;
    private readonly string? sentCommandName;

    public TestConnectionDataSentEventArgs(string? dataSent)
    {
        if (dataSent is not null)
        {
            this.dataSent = dataSent;
            JObject jsonObject = JObject.Parse(dataSent);
            this.sentCommandId = jsonObject["id"]!.Value<long>();
            this.sentCommandName = jsonObject["method"]!.Value<string>();
        }
    }

    public bool IsValidCommand => this.sentCommandId is not null;

    public long SentCommandId => this.sentCommandId!.Value;

    public string? SentCommandName => this.sentCommandName;

    public string DataSent => this.dataSent ?? string.Empty;
}
