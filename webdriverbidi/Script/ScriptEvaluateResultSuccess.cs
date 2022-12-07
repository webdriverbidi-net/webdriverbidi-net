namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class ScriptEvaluateResultSuccess: ScriptEvaluateResult
{
    private RemoteValue result = new RemoteValue("null");

    [JsonConstructor]
    internal ScriptEvaluateResultSuccess() : base()
    {
    }

    [JsonProperty("result")]
    public RemoteValue Result { get => this.result; internal set => this.result = value; }
}