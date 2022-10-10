namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CreateCommandSettings : CommandSettings
{
    private BrowsingContextCreateType createType;
    
    public CreateCommandSettings(BrowsingContextCreateType createType)
    {
        this.createType = createType;
    }

    public BrowsingContextCreateType CreateType { get => this.createType; set => this.createType = value; }

    public override string MethodName => "browsingContext.create";

    [JsonProperty("type")]
    internal string SerializableCreateType
    {
        get => this.createType.ToString().ToLowerInvariant();
    }
}