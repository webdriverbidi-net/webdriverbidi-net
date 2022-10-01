namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(RealmInfoJsonConverter))]
public class RealmInfo
{
    private string realmId;
    private string origin;
    private RealmType realmType;

    internal RealmInfo(string realmId, string origin, RealmType realmType)
    {
        this.realmId = realmId;
        this.origin = origin;
        this.realmType = realmType;
    }

    [JsonProperty("realm")]
    [JsonRequired]
    public string RealmId { get => this.realmId; internal set => this.realmId = value; }

    [JsonProperty("origin")]
    [JsonRequired]
    public string Origin { get => this.origin; internal set => this.origin = value; }

    public RealmType Type { get => this.realmType; }

    [JsonProperty("type")]
    [JsonRequired]
    internal string SerializableType
    {
        get
        {
            string typeString = this.realmType.ToString().ToLowerInvariant();
            if (typeString.IndexOf("worker") > 0)
            {
                return typeString.Insert(typeString.IndexOf("worker"), "-");
            }

            if (typeString.IndexOf("worklet") > 0)
            {
                return typeString.Insert(typeString.IndexOf("worklet"), "-");
            }
            
            return typeString;
        }
        set
        {
            string sanitizedType = value.Replace("-", "");
            RealmType type;
            if (!Enum.TryParse<RealmType>(sanitizedType, true, out type))
            {
                throw new WebDriverBidiException($"Malformed response: Invalid value {type} for RealmInfo 'type' property");
            }

            this.realmType = type;
        }
    }
}