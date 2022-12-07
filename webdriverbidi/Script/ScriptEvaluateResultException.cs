namespace WebDriverBidi.Script;

using Newtonsoft.Json;

public class ScriptEvaluateResultException: ScriptEvaluateResult
{
    private ExceptionDetails result = new ExceptionDetails();

    [JsonConstructor]
    internal ScriptEvaluateResultException() : base()
    {
    }

    [JsonProperty("exceptionDetails")]
    public ExceptionDetails ExceptionDetails{ get => this.result; internal set => this.result = value; }
}