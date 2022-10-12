namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class GetRealmsCommandSettings : CommandSettings
{
    private string? browsingContextId;
    private RealmType? realmType;

    public override string MethodName => "script.getRealms";

    public override Type ResultType => typeof(GetRealmsCommandResult);

    public GetRealmsCommandSettings()
    {
    }

    [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
    public string? BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    public RealmType? RealmType { get => this.realmType; set => this.realmType = value; }

    [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableRealmType
    {
        get
        {
            if (this.realmType is null)
            {
                return null;
            }

            string typeValue = this.realmType.Value.ToString().ToLowerInvariant();
            if (typeValue.IndexOf("worker") > 0)
            {
                typeValue = typeValue.Insert(typeValue.IndexOf("worker"), "-");
            }

            if (typeValue.IndexOf("worklet") > 0)
            {
                typeValue = typeValue.Insert(typeValue.IndexOf("worklet"), "-");
            }

            return typeValue;
        }
    }
}