namespace WebDriverBidi;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonObject(MemberSerialization.OptIn)]
public class WebDriverBidiCommandData
{
    public WebDriverBidiCommandData(long commandId, string commandName, JToken commandParameters)
    {
        this.CommandId = commandId;
        this.CommandName = commandName;
        this.CommandParameters = commandParameters;
        this.SynchronizationEvent = new ManualResetEvent(false);
    }

    [JsonProperty("id")]
    public long CommandId { get; }

    [JsonProperty("method")]
    public string CommandName { get; }

    [JsonProperty("params")]
    public JToken CommandParameters { get; }

    public ManualResetEvent SynchronizationEvent { get; }

    public WebDriverBidiCommandResultData? Result { get; set; }
}