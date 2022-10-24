namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class RemoveLoadScriptCommandSettings : CommandSettings
{
    private string loadScriptId;

    public RemoveLoadScriptCommandSettings(string loadScriptId)
    {
        this.loadScriptId = loadScriptId;
    }

    public override string MethodName => "script.removeLoadScript";

    public override Type ResultType => typeof(EmptyResult);

    [JsonProperty("script")]
    public string LoadScriptId { get => this.loadScriptId; set => this.loadScriptId = value; }
}