namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class RemovePreloadScriptCommandSettings : CommandSettings
{
    private string loadScriptId;

    public RemovePreloadScriptCommandSettings(string loadScriptId)
    {
        this.loadScriptId = loadScriptId;
    }

    public override string MethodName => "script.removePreloadScript";

    public override Type ResultType => typeof(EmptyResult);

    [JsonProperty("script")]
    public string LoadScriptId { get => this.loadScriptId; set => this.loadScriptId = value; }
}