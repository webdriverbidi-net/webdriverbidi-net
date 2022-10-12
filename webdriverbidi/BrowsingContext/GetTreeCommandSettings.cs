namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GetTreeCommandSettings : CommandSettings
{
    private int? maxDepth;
    private string? rootBrowsingContextId;

    public GetTreeCommandSettings()
    {
    }

    public override string MethodName => "browsingContext.getTree";

    public override Type ResultType => typeof(GetTreeCommandResult);

    [JsonProperty("maxDepth", NullValueHandling = NullValueHandling.Ignore)]
    public int? MaxDepth { get => this.maxDepth; set => this.maxDepth = value; }

    [JsonProperty("root", NullValueHandling = NullValueHandling.Ignore)]
    public string? RootBrowsingContextId { get => rootBrowsingContextId; set => this.rootBrowsingContextId = value; }
}