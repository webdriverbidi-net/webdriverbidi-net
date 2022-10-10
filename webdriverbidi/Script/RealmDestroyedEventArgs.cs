namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class RealmDestroyedEventArgs : EventArgs
{
    private string realmId;

    [JsonConstructor]
    public RealmDestroyedEventArgs(string realmId)
    {
        this.realmId = realmId;
    }

    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }
}
