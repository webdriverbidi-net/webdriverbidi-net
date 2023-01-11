namespace WebDriverBidi.Log;

using Newtonsoft.Json;
using JsonConverters;
using Script;

[JsonObject(MemberSerialization.OptIn)]
[JsonConverter(typeof(LogEntryJsonConverter))]
public class LogEntry
{
    private string type = string.Empty;
    private LogLevel level = LogLevel.Error;
    private Source source = new();
    private string? text;
    private long timestamp = -1;
    private StackTrace? stacktrace;

    internal LogEntry()
    {
    }

    [JsonProperty("type")]
    [JsonRequired]
    public string Type { get => this.type; internal set => this.type = value; }

    public LogLevel Level => this.level;

    [JsonProperty("source")]
    [JsonRequired]
    public Source Source { get => this.source; internal set => this.source = value; }

    [JsonProperty("text", NullValueHandling = NullValueHandling.Include)]
    public string? Text { get => this.text; internal set => this.text = value; }

    [JsonProperty("timestamp")]
    [JsonRequired]
    public long Timestamp { get => this.timestamp; internal set => this.timestamp = value; }

    [JsonProperty("stacktrace", NullValueHandling = NullValueHandling.Ignore)]
    public StackTrace? StackTrace { get => this.stacktrace; internal set => this.stacktrace = value; }

    [JsonProperty("level")]
    [JsonRequired]
    internal string SerializableLevel
    {
        get => this.level.ToString().ToLowerInvariant();
        set
        {
            if (!Enum.TryParse<LogLevel>(value, true, out LogLevel logLevelValue))
            {
                throw new WebDriverBidiException($"Invalid value for log level {value}");
            }

            this.level = logLevelValue;
        }
    }
}
