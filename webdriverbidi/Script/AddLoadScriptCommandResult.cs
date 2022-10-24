namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class AddLoadScriptCommandResult : CommandResult
{
    private string loadScriptId;

    [JsonConstructor]
    internal AddLoadScriptCommandResult(string loadScriptId)
    {
        this.loadScriptId = loadScriptId;
    }

    [JsonProperty("script")]
    public string LoadScriptId { get => this.loadScriptId; internal set => this.loadScriptId = value; }
}