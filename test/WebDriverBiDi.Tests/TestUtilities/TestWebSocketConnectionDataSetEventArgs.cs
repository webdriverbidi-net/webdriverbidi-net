namespace WebDriverBiDi.TestUtilities;

using Newtonsoft.Json.Linq;

public record TestWebSocketConnectionDataSentEventArgs : WebDriverBiDiEventArgs
{
    private readonly long? sentCommandId;
    private readonly string? sentCommandName;

    public TestWebSocketConnectionDataSentEventArgs(string? dataSent)
    {
        if (dataSent is not null)
        {
            JObject jsonObject = JObject.Parse(dataSent);
            JToken? id = jsonObject["id"];
            JToken? method = jsonObject["method"];
            this.sentCommandId = id?.Value<long>();
            this.sentCommandName = method?.Value<string>();
        }
    }

    public long SentCommandId => this.sentCommandId ?? -1;

    public string? SentCommandName => this.sentCommandName;
}
