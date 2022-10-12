namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class HandleUserPromptCommandSettings : CommandSettings
{
    private string browsingContextId;
    private bool? accept;
    private string? userText;

    public HandleUserPromptCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    public override string MethodName => "browsingContext.handleUserPrompt";

    public override Type ResultType => typeof(EmptyResult);

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    [JsonProperty("accept", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Accept { get => this.accept; set => this.accept = value; }

    [JsonProperty("userText", NullValueHandling = NullValueHandling.Ignore)]
    public string? UserText { get => this.userText; set => this.userText = value; }
}