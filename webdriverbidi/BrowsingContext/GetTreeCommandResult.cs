namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GetTreeCommandResult : CommandResult
{
    private List<BrowsingContextInfo> contextTree = new List<BrowsingContextInfo>();

    [JsonConstructor]
    private GetTreeCommandResult()
    {
    }

    public IList<BrowsingContextInfo> ContextTree => this.contextTree.AsReadOnly();

    [JsonProperty("contexts")]
    [JsonRequired]
    internal List<BrowsingContextInfo> SerializableContextTree { get => this.contextTree; set => this.contextTree = value; }
}