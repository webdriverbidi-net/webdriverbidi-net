namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class NewCommandSettings : CommandSettings
{
    private CapabilitiesRequest? alwaysMatch;
    private List<CapabilitiesRequest> firstMatch = new List<CapabilitiesRequest>();

    public override string MethodName => "session.new";

    [JsonProperty("alwaysMatch", NullValueHandling = NullValueHandling.Ignore)]
    public CapabilitiesRequest? AlwaysMatch { get => this.alwaysMatch; set => this.alwaysMatch = value; }

    public List<CapabilitiesRequest> FirstMatch => this.firstMatch;

    [JsonProperty("firstMatch", NullValueHandling = NullValueHandling.Ignore)]
    internal IList<CapabilitiesRequest>? SerializableFirstMatch
    {
        get
        {
            if (this.firstMatch.Count == 0)
            {
                return null;
            }

            return this.firstMatch.AsReadOnly();
        }
    }
}