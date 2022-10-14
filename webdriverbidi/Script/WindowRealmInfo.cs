namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class WindowRealmInfo : RealmInfo
{
    private string browsingContextId;
    private string? sandbox;

    internal WindowRealmInfo(string realmId, string origin, RealmType realmType, string browsingContextId)
        : base (realmId, origin, realmType)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContext { get => this.browsingContextId; internal set => this.browsingContextId = value; }

    [JsonProperty("sandbox", NullValueHandling = NullValueHandling.Ignore)]
    public string? Sandbox { get => this.sandbox; internal set => this.sandbox = value; }
}