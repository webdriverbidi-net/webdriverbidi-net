namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CreateCommandResult : CommandResult
{
    private string contextId = string.Empty;

    [JsonConstructor]
    private CreateCommandResult()
    {
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.contextId; internal set => this.contextId = value; }
}

