namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandSettings : CommandSettings
{
    private string browsingContextId;

    public CaptureScreenshotCommandSettings(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    public override string MethodName => "browsingContext.captureScreenshot";
}