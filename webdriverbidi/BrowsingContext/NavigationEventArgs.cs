namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class NavigationEventArgs : EventArgs
{
    private string? id;

    private string browsingContextId;

    private string url;

    private long epochTimestamp;

    private DateTime timestamp;

    [JsonConstructor]
    public NavigationEventArgs(string browsingContextId, string url, long timestamp, string? navigationId)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
        this.EpochTimestamp = timestamp;
        this.id = navigationId;
    }

    [JsonProperty("navigation")]
    public string? NavigationId { get => this.id; internal set => this.id = value; }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; internal set => this.browsingContextId = value; }
    
    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; internal set => this.url = value; }

    [JsonProperty("timestamp")]
    [JsonRequired]
    public long EpochTimestamp
    {
        get { return this.epochTimestamp; }
        private set
        {
            this.epochTimestamp = value;
            this.timestamp = DateTime.UnixEpoch.AddMilliseconds(value);
        }
    }

    public DateTime Timestamp => this.timestamp;
}