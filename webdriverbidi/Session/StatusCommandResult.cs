namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StatusCommandResult : CommandResult
{
    private bool ready;
    private string message;

    [JsonConstructor]
    internal StatusCommandResult(bool ready, string message)
    {
        this.ready = ready;
        this.message = message;
    }

    [JsonProperty("ready")]
    [JsonRequired]
    public bool IsReady { get => this.ready; internal set => this.ready = value; }

    [JsonProperty("message")]
    [JsonRequired]
    public string Message { get => this.message; internal set => this.message = value; }
}