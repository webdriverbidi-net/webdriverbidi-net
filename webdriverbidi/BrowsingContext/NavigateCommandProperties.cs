namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class NavigateCommandProperties : CommandProperties
{
    private string browsingContextId;
    private string url;
    private ReadinessState? wait;

    public NavigateCommandProperties(string browsingContextId, string url)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
    }

    [JsonProperty("context")]
    [JsonRequired]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    [JsonProperty("url")]
    [JsonRequired]
    public string Url { get => this.url; set => this.url = value; }

    public ReadinessState? Wait { get => this.wait; set => this.wait = value; }

    [JsonProperty("wait", NullValueHandling = NullValueHandling.Ignore)]
    internal string? SerializableWait
    {
        get
        {
            if (this.wait is null)
            {
                return null;
            }

            return this.wait.Value.ToString().ToLowerInvariant();
        }
    }

    public override string MethodName => "browsingContext.navigate";
}