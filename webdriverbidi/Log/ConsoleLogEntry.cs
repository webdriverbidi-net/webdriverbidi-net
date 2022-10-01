namespace WebDriverBidi.Log;

using Newtonsoft.Json;
using Script;

public class ConsoleLogEntry : LogEntry
{
    private string method;
    private List<RemoteValue> args = new List<RemoteValue>();

    internal ConsoleLogEntry(string type, LogLevel level, string? text, Source source, long timestamp, string method)
        : base(type, level, text, source, timestamp)
    {
        this.method = method;
    }

    [JsonProperty("method")]
    [JsonRequired]
    public string Method { get => this.method; internal set => this.method = value; }

    public IList<RemoteValue> Args => this.args.AsReadOnly();

    [JsonProperty("args")]
    [JsonRequired]
    internal List<RemoteValue> SerializableArgs { get => this.args; set => this.args = value; }
}