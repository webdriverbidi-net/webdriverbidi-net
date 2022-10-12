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

    public override string MethodName => "browsingContext.create";

    public override Type ResultType => typeof(CreateCommandResult);

    public BrowsingContextCreateType CreateType { get => this.createType; set => this.createType = value; }

    [JsonProperty("type")]
    internal string SerializableCreateType
    {
        get => this.createType.ToString().ToLowerInvariant();
    }
}