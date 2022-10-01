namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class ScriptEvaluateResultException: ScriptEvaluateResult
{
    private ExceptionDetails result;

    internal ScriptEvaluateResultException(string realmId, ExceptionDetails result) : base(realmId)
    {
        this.result = result;
    }

    [JsonProperty("exceptionDetails")]
    public ExceptionDetails ExceptionDetails{ get => this.result; set => this.result = value; }
}