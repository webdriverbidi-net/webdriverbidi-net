namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class ExceptionDetails
{
    private int columnNumber;
    private int lineNumber;
    private string text;
    private StackTrace stackTrace;
    private RemoteValue exception;

    [JsonConstructor]
    internal ExceptionDetails(string text, int lineNumber, int columnNumber, StackTrace stackTrace, RemoteValue exception)
    {
        this.text = text;
        this.columnNumber = columnNumber;
        this.lineNumber = lineNumber;
        this.stackTrace = stackTrace;
        this.exception = exception;
    }

    [JsonProperty("text")]
    [JsonRequired]
    public string Text { get => this.text; internal set => this.text = value; }

    [JsonProperty("columnNumber")]
    [JsonRequired]
    public int ColumnNumber { get => this.columnNumber; internal set => this.columnNumber = value; }

    [JsonProperty("lineNumber")]
    [JsonRequired]
    public int LineNumber { get => this.lineNumber; internal set => this.lineNumber = value; }

    [JsonProperty("stackTrace")]
    [JsonRequired]
    public StackTrace StackTrace { get => this.stackTrace; internal set => this.stackTrace = value; }

    [JsonProperty("exception")]
    [JsonRequired]
    public RemoteValue Exception { get => this.exception; internal set => this.exception = value; }
}