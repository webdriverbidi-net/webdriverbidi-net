namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CreateCommandResult
{
    private string contextId;

    [JsonConstructor]
    private CreateCommandResult(string contextId)
    {
        this.contextId = contextId;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.contextId; internal set => this.contextId = value; }
}

