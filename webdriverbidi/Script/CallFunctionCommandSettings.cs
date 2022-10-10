namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CallFunctionCommandSettings : CommandSettings
{   
    private string functionDeclaration;
    private ScriptTarget scriptTarget;
    private bool awaitPromise;
    private List<ArgumentValue> arguments = new List<ArgumentValue>();
    private ArgumentValue? thisObject;
    private OwnershipModel? ownershipModel;

    public CallFunctionCommandSettings(string functionDeclaration, ScriptTarget scriptTarget, bool awaitPromise)
    {
        this.functionDeclaration = functionDeclaration;
        this.scriptTarget = scriptTarget;
        this.awaitPromise = awaitPromise;
    }

    [JsonProperty("functionDeclaration")]
    public string FunctionDeclaration { get => this.functionDeclaration; set => this.functionDeclaration = value; }

    [JsonProperty("target")]
    public ScriptTarget ScriptTarget { get => this.scriptTarget; set => this.scriptTarget = value; }

    [JsonProperty("awaitPromise")]
    public bool AwaitPromise { get => this.awaitPromise; set => this.awaitPromise = value; }

    [JsonProperty("this", NullValueHandling = NullValueHandling.Ignore)]
    public ArgumentValue? ThisObject { get => this.thisObject; set => this.thisObject = value; }

    public List<ArgumentValue> Arguments => this.arguments;

    public OwnershipModel? OwnershipModel { get => this.ownershipModel; set => this.ownershipModel = value; }

    [JsonProperty("resultOwnership", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableOwnershipModel
    {
        get
        {
            if (this.ownershipModel is null)
            {
                return null;
            }

            return this.ownershipModel.Value.ToString().ToLowerInvariant();
        }
    }

    [JsonProperty("arguments", NullValueHandling = NullValueHandling.Ignore)]
    internal IList<ArgumentValue>? SerializableArguments
    {
        get
        {
            if (this.arguments.Count == 0)
            {
                return null;
            }

            return this.arguments.AsReadOnly();
        }
    }

    public override string MethodName => "script.callFunction";
}