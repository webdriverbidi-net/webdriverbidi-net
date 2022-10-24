namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class AddLoadScriptCommandSettings : CommandSettings
{
    private string expression;
    private string? sandbox;

    public AddLoadScriptCommandSettings(string expression)
    {
        this.expression = expression;
    }

    public override string MethodName => "script.addLoadScript";

    public override Type ResultType => typeof(AddLoadScriptCommandResult);

    [JsonProperty("expression")]
    public string Expression { get => this.expression; set => this.expression = value; }

    [JsonProperty("sandbox", NullValueHandling = NullValueHandling.Ignore)]
    public string? Sandbox { get => this.sandbox; set => this.sandbox = value; }
}
