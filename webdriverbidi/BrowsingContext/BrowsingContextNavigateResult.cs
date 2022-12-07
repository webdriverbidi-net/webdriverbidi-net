namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class BrowsingContextNavigateResult : CommandResult
{
    private string? id;
    private string url = string.Empty;

    [JsonConstructor]
    private BrowsingContextNavigateResult()
    {
    }

    [JsonProperty("navigation")]
    public string? NavigationId { get => this.id; internal set => this.id = value; }

    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }
}