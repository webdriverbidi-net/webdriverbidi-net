namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GetTreeCommandSettings : CommandSettings
{
    private int? maxDepth;
    private string? rootBrowsingContextId;

    public GetTreeCommandSettings()
    {
    }

    [JsonProperty("maxDepth", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxDepth { get => this.maxDepth; set => this.maxDepth = value; }

    [JsonProperty("root", NullValueHandling = NullValueHandling.Ignore)]
    public string? RootBrowsingContextId { get => rootBrowsingContextId; set => this.rootBrowsingContextId = value; }

    public override string MethodName => "browsingContext.getTree";
}