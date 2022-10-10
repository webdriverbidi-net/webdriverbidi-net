namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class ReloadCommandSettings : CommandSettings
{
    private string browsingContextId;
    private bool? ignoreCache;
    private ReadinessState? wait;

    public ReloadCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    [JsonProperty("ignoreCache", NullValueHandling = NullValueHandling.Ignore)]
    public bool? IgnoreCache { get => this.ignoreCache; set => this.ignoreCache = value; }

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