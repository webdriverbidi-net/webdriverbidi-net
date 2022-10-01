namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class NavigationEventArgs : EventArgs
{
    private string? id;

    private string browsingContextId;

    private string url;

    [JsonConstructor]
    private NavigationEventArgs(string browsingContextId, string url, string? navigationId)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
        this.id = navigationId;
    }

    [JsonProperty("navigation")]
    public string? NavigationId { get => this.id; internal set => this.id = value; }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }
    
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }
}