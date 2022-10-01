namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class HandleUserPromptCommandProperties : CommandProperties
{
    private string browsingContextId;
    private bool? accept;
    private string? userText;

    public HandleUserPromptCommandProperties(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    [JsonProperty("accept", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Accept { get => this.accept; set => this.accept = value; }

    [JsonProperty("userText", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserText { get => this.userText; set => this.userText = value; }

    public override string MethodName => "browsingContext.handleUserPrompt";
}