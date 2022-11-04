namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class BrowsingContextInfo
{
    private string id;
    private string url;
    private List<BrowsingContextInfo> children = new List<BrowsingContextInfo>();
    private string? parentId;

    [JsonConstructor]
    private BrowsingContextInfo(string id, string url)
    {
        this.id = id;
        this.url = url;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.id; internal set => this.id = value; }

    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }

    public IList<BrowsingContextInfo> Children => this.children.AsReadOnly();

    [JsonProperty("parent")]
    public string? Parent { get => this.parentId; internal set => this.parentId = value; }

    [JsonProperty("children")]
    [JsonRequired]
    internal List<BrowsingContextInfo> SerializableChildren { get => this.children; set => this.children = value; }
}
