namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class WindowRealmInfo : RealmInfo
{
    private string browsingContextId;

    internal WindowRealmInfo(string realmId, string origin, RealmType realmType, string browsingContextId)
        : base (realmId, origin, realmType)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContext { get => this.browsingContextId; internal set => this.browsingContextId = value; }
}