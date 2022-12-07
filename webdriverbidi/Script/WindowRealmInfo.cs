namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class WindowRealmInfo : RealmInfo
{
    private string browsingContextId = string.Empty;
    private string? sandbox;

    internal WindowRealmInfo() : base ()
    {
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContext { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    [JsonProperty("sandbox", NullValueHandling = NullValueHandling.Ignore)]
    public string? Sandbox { get => this.sandbox; internal set => this.sandbox = value; }
}