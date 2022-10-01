namespace WebDriverBidi.Session;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class NewCommandResult
{
    private string sessionId = string.Empty;
    
    private CapabilitiesResult capabilitiesResult = new CapabilitiesResult();

    [JsonConstructor]
    private NewCommandResult()
    {
    }

    [JsonProperty("sessionId")]
    [JsonRequired]
    public string SessionId { get => this.sessionId; internal set => this.sessionId = value; }

    [JsonProperty("capabilities")]
    [JsonRequired]
    public CapabilitiesResult Capabilities { get => this.capabilitiesResult; internal set => this.capabilitiesResult = value; }
}