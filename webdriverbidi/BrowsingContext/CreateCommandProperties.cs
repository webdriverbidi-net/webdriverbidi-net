namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CreateCommandProperties : CommandProperties
{
    private BrowsingContextCreateType createType;
    
    public CreateCommandProperties(BrowsingContextCreateType createType)
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