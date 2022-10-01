namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CloseCommandProperties : CommandProperties
{
    private string browsingContextId;

    public CloseCommandProperties(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    public override string MethodName => "browsingContext.close";
}