namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class RealmTarget : ScriptTarget
{
    private string realmId;

    public RealmTarget(string realmId)
    {
        this.realmId = realmId;
    }

    [JsonProperty("realm")]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }
}