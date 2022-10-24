namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class AddPreloadScriptCommandResult : CommandResult
{
    private string loadScriptId = string.Empty;

    [JsonConstructor]
    internal AddPreloadScriptCommandResult()
    {
    }

    [JsonProperty("script")]
    public string LoadScriptId { get => this.loadScriptId; internal set => this.loadScriptId = value; }
}