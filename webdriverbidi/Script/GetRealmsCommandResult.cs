namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GetRealmsCommandResult : CommandResult
{
    private List<RealmInfo> realms = new();

    public IList<RealmInfo> Realms => this.realms.AsReadOnly();

    [JsonProperty("realms")]
    internal List<RealmInfo> SerializableRealms { get => this.realms; set => this.realms = value; }
}