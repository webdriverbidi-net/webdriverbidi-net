namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(ScriptEvaluateResultJsonConverter))]
public class ScriptEvaluateResult : CommandResult
{
    private string realmId = "";
    private ScriptEvaluateResultType resultType = ScriptEvaluateResultType.Success;

    protected ScriptEvaluateResult()
    {
    }

    public ScriptEvaluateResultType ResultType => resultType;

    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }
    
    [JsonProperty("type")]
    [JsonRequired]
    internal string SerializableResultType
    {
        get
        {
            return this.resultType.ToString().ToLowerInvariant();
        }

        set
        {
            ScriptEvaluateResultType type;
            if (!Enum.TryParse<ScriptEvaluateResultType>(value, true, out type))
            {
                throw new WebDriverBidiException($"Malformed response: Invalid value {value} for RealmInfo 'type' property");
            }

            this.resultType = type;
        }
    }
}