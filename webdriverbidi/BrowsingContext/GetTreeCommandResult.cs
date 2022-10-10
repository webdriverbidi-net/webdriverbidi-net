namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GetTreeCommandResult : CommandResult
{
    [JsonConstructor]
    private GetTreeCommandResult()
    {
    }

    private List<BrowsingContextInfo> contextTree = new List<BrowsingContextInfo>();

    public IList<BrowsingContextInfo> ContextTree => this.contextTree.AsReadOnly();

    [JsonProperty("contexts")]
    [JsonRequired]
    internal List<BrowsingContextInfo> SerializableContextTree { get => this.contextTree; set => this.contextTree = value; }
}