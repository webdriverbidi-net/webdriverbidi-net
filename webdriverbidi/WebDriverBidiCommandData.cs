namespace WebDriverBidi;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class WebDriverBidiCommandData
{
    private CommandSettings commandSettings;

    public WebDriverBidiCommandData(long commandId, CommandSettings commandSettings)
    {
        this.CommandId = commandId;
        this.commandSettings = commandSettings;
        this.SynchronizationEvent = new ManualResetEvent(false);
    }

    [JsonProperty("id")]
    public long CommandId { get; }

    [JsonProperty("method")]
    public string CommandName => this.commandSettings.MethodName;

    [JsonProperty("params")]
    public CommandSettings CommandParameters => this.commandSettings;

    public Type ResultType => this.commandSettings.ResultType;

    public ManualResetEvent SynchronizationEvent { get; }

    public CommandResult? Result { get; set; }

    public Exception? ThrownException { get; set; }
}