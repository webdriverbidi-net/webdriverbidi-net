namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class StackFrame
{
    private int lineNumber;
    private int columnNumber;
    private string functionName;
    private string url;

    [JsonConstructor]
    private StackFrame(string functionName, int lineNumber, int columnNumber, string url)
    {
        this.functionName = functionName;
        this.lineNumber = lineNumber;
        this.columnNumber = columnNumber;
        this.url = url;
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