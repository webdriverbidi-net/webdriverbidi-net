namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class UserPromptClosedEventArgs : EventArgs
{
     private string browsingContextId;
    private bool isAccepted;
    private string? userText;

    [JsonConstructor]
    private UserPromptClosedEventArgs(string browsingContextId, bool isAccepted)
    {
        this.browsingContextId = browsingContextId;
        this.isAccepted = isAccepted;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    [JsonProperty("accepted")]
    [JsonRequired]
    public bool IsAccepted { get => this.isAccepted; set => isAccepted = value; }

    [JsonProperty("userText", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserText { get => this.userText; internal set => this.userText = value; }
}
