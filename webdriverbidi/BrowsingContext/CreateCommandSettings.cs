namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CreateCommandSettings : CommandSettings
{
    private BrowsingContextCreateType createType;

    private string? referenceContextId;
    
    public CreateCommandSettings(BrowsingContextCreateType createType)
    {
        this.createType = createType;
    }

    public override string MethodName => "browsingContext.create";

    public override Type ResultType => typeof(CreateCommandResult);

    public BrowsingContextCreateType CreateType { get => this.createType; set => this.createType = value; }

    [JsonProperty("referenceContext", NullValueHandling = NullValueHandling.Ignore)]
    public string? ReferenceContextId { get => this.referenceContextId; set => this.referenceContextId = value; }

    [JsonProperty("type")]
    internal string SerializableCreateType
    {
        get => this.createType.ToString().ToLowerInvariant();
    }
}