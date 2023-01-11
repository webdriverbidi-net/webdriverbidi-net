namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CloseCommandSettings : CommandSettings
{
    private string browsingContextId;

    public CloseCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    public override string MethodName => "browsingContext.close";

    public override Type ResultType => typeof(EmptyResult);

    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }
}