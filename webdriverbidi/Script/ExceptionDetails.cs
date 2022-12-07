namespace WebDriverBidi.Script;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class ExceptionDetails
{
    private int columnNumber = -1;
    private int lineNumber = -1;
    private string text = "";
    private StackTrace stackTrace = new StackTrace();
    private RemoteValue exception = new RemoteValue("null");

    [JsonConstructor]
    internal ExceptionDetails()
    {
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