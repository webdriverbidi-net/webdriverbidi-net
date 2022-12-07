namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(ScriptEvaluateResultJsonConverter))]
public class ScriptEvaluateResult : CommandResult
{
    private string realmId = "";
    private string type = "";

    protected ScriptEvaluateResult()
    {
    }

    [JsonProperty("type")]
    [JsonRequired]
    public string Type { get => this.type; internal set => this.type = value; }

    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }
}