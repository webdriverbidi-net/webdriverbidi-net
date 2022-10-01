namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(ScriptEvaluateResultJsonConverter))]
public class ScriptEvaluateResult
{
    private string realmId;

    protected ScriptEvaluateResult(string realmId)
    {
        this.realmId = realmId;
    }

    [JsonProperty("realm")]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }
}