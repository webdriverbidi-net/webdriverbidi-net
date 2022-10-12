namespace WebDriverBidi.BrowsingContext;

using System;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class NavigateCommandSettings : CommandSettings
{
    private string browsingContextId;
    private string url;
    private ReadinessState? wait;

    public NavigateCommandSettings(string browsingContextId, string url)
    {
        this.browsingContextId = browsingContextId;
        this.url = url;
    }

    public override string MethodName => "browsingContext.navigate";

    public override Type ResultType => typeof(BrowsingContextNavigateResult);

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
}