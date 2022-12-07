namespace WebDriverBidi.Log;

using Newtonsoft.Json;
using Script;

public class ConsoleLogEntry : LogEntry
{
    private string method = string.Empty;
    private List<RemoteValue> args = new List<RemoteValue>();

    internal ConsoleLogEntry() : base()
    {
    }

    [JsonProperty("method")]
    [JsonRequired]
    public string Method { get => this.method; internal set => this.method = value; }

    public IList<RemoteValue> Args => this.args.AsReadOnly();

    [JsonProperty("args")]
    [JsonRequired]
    internal List<RemoteValue> SerializableArgs { get => this.args; set => this.args = value; }
}