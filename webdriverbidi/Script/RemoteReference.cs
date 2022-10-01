namespace WebDriverBidi.Script;

using Newtonsoft.Json;
using JsonConverters;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(RemoteReferenceJsonConverter))]
public class RemoteReference : ArgumentValue
{
    private string handle;
    private Dictionary<string, object?> additionalData = new Dictionary<string, object?>();

    public RemoteReference(string handle)
    {
        this.handle = handle;
    }

    [JsonProperty("handle")]
    public string Handle { get => this.handle; internal set => this.handle = value; }

    public Dictionary<string, object?> AdditionalData => this.additionalData;
}