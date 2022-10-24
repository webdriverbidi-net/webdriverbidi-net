namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class AddPreloadScriptCommandSettings : CommandSettings
{
    private string expression;
    private string? sandbox;

    public AddPreloadScriptCommandSettings(string expression)
    {
        this.expression = expression;
    }

    public override string MethodName => "script.addPreoadScript";

    public override Type ResultType => typeof(AddPreloadScriptCommandResult);

    [JsonProperty("expression")]
    public string Expression { get => this.expression; set => this.expression = value; }

    [JsonProperty("sandbox", NullValueHandling = NullValueHandling.Ignore)]
    public string? Sandbox { get => this.sandbox; set => this.sandbox = value; }
}
