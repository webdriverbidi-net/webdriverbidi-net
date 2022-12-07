namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StackFrame
{
    private int lineNumber = -1;
    private int columnNumber = -1;
    private string functionName = string.Empty;
    private string url = string.Empty;

    [JsonConstructor]
    private StackFrame()
    {
    }

    [JsonProperty("functionName")]
    [JsonRequired]
    public string FunctionName { get => this.functionName; internal set => this.functionName = value; }

    [JsonProperty("lineNumber")]
    [JsonRequired]
    public int LineNumber { get => this.lineNumber; internal set => this.lineNumber = value; }

    [JsonProperty("columnNumber")]
    [JsonRequired]
    public int ColumnNumber { get => this.columnNumber; internal set => this.columnNumber = value; }

    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }
}