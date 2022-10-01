namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class Source
{
    private string realmId;
    private string? browsingContextId;

    [JsonConstructor]
    internal Source(string realmId)
    {
        this.realmId = realmId;
    }

    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }

    [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
    public string? Context { get => this.browsingContextId; internal set => this.browsingContextId = value; }
}