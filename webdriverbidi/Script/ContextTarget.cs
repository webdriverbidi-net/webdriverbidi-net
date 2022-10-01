namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class ContextTarget : ScriptTarget
{
    private string browsingContextId;
    private string? sandbox;

    public ContextTarget(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    [JsonProperty("sandbox", NullValueHandling = NullValueHandling.Ignore)]
    public string? Sandbox { get => this.sandbox; internal set => this.sandbox = value; }

    public override Dictionary<string, object?> ToDictionary()
    {
        var result = new Dictionary<string, object?>();
        result["context"] = this.browsingContextId;
        if (this.sandbox is not null)
        {
            result["sandbox"] = this.sandbox;
        }
        
        return result;
    }
}