namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(RealmInfoJsonConverter))]
public class RealmInfo
{
    private string realmId = string.Empty;
    private string origin = string.Empty;
    private RealmType realmType = RealmType.Window;

    internal RealmInfo()
    {
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
            if (!Enum.TryParse<RealmType>(sanitizedType, true, out RealmType type))
            {
                throw new WebDriverBidiException($"Malformed response: Invalid value {type} for RealmInfo 'type' property");
            }

            this.realmType = type;
        }
    }
}