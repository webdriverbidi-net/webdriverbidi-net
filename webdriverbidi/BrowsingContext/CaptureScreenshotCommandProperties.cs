namespace WebDriverBidi.BrowsingContext;

using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptIn)]
public class CaptureScreenshotCommandProperties : CommandProperties
{
    private string browsingContextId;

    public CaptureScreenshotCommandProperties(string browsingContextId)
    {
        this.browsingContextId = browsingContextId;
    }

    [JsonProperty("context")]
    public string BrowsingContextId { get => this.browsingContextId; set => this.browsingContextId = value; }

    public override string MethodName => "browsingContext.captureScreenshot";
}