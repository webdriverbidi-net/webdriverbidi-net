namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class ScriptEvaluateResultSuccess: ScriptEvaluateResult
{
    private RemoteValue result;

    internal ScriptEvaluateResultSuccess(string realmId, RemoteValue result) : base(realmId)
    {
        this.result = result;
    }

    [JsonProperty("result")]
    public RemoteValue Result { get => this.result; internal set => this.result = value; }
}